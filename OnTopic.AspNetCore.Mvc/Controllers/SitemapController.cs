/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml.Linq;
using OnTopic.Attributes;

namespace OnTopic.AspNetCore.Mvc.Controllers;

/*==============================================================================================================================
| CLASS: SITEMAP CONTROLLER
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Responds to requests for a sitemap according to sitemap.org's schema. The view is expected to recursively loop over
///   child topics to generate the appropriate markup.
/// </summary>
/// <remarks>
///   By default, some <see cref="Topic"/>s are <i>excluded</i> based on their content types—which includes not only the
///   <see cref="Topic"/>, but also all of its descendents. Other <see cref="Topic"/>s are <i>skipped</i>, also based on
///   their content types; in this case, the <see cref="Topic"/> is excluded, but its descendents are not. What content
///   types are excluded or skipped can be configured, respectively, by modifying the static <see cref="ExcludedContentTypes"
///   /> and <see cref="SkippedContentTypes"/> collections.
/// </remarks>
/// <param name="topicRepository">
///   The <see cref="ISitemapTopicRepository"/> used to retrieve the minimal <see cref="Topic"/> graph for the sitemap.
/// </param>
public class SitemapController(ISitemapTopicRepository topicRepository) : Controller {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              ISitemapTopicRepository         _topicRepository                = Contract.Requires(topicRepository);

  /*============================================================================================================================
  | CONSTANTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  private static readonly XNamespace _sitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

  /*============================================================================================================================
  | EXCLUDED CONTENT TYPES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Specifies what content types should not be listed in the sitemap, including any descendents.
  /// </summary>
  public static Collection<string> ExcludedContentTypes { get; } = ["List"];

  /*============================================================================================================================
  | SKIPPED CONTENT TYPES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Specifies what content types should not be listed in the sitemap—but whose descendents should still be evaluated.
  /// </summary>
  public static Collection<string> SkippedContentTypes { get; } = [
    "PageGroup",
    "Container"
  ];

  /*============================================================================================================================
  | GET: /SITEMAP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides the Sitemap.org sitemap for the site.
  /// </summary>
  /// <param name="indent">Optionally enables indentation of XML elements in output for human readability.</param>
  /// <returns>A Sitemap.org sitemap.</returns>
  public ActionResult Index(bool indent = false) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Ensure topics are loaded
    \-------------------------------------------------------------------------------------------------------------------------*/
    var rootTopic               = _topicRepository.Load().GetAwaiter().GetResult();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish sitemap
    \-------------------------------------------------------------------------------------------------------------------------*/
    var declaration             = new XDeclaration("1.0", "utf-8", "no");
    var sitemap                 = GenerateSitemap(rootTopic);
    var settings                = indent? SaveOptions.None : SaveOptions.DisableFormatting;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return the homepage view
    \-------------------------------------------------------------------------------------------------------------------------*/
    return Content(declaration.ToString() + sitemap.ToString(settings), "text/xml");

  }

  /*============================================================================================================================
  | METHOD: GENERATE SITEMAP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a root topic, generates an XML-formatted sitemap.
  /// </summary>
  /// <param name="rootTopic">The topic to add to the sitemap.</param>
  /// <returns>A Sitemap.org sitemap.</returns>
  private XDocument GenerateSitemap(Topic rootTopic) =>
    new(
      new XElement(_sitemapNamespace + "urlset",
        from topic in rootTopic.Children
        select AddTopic(topic)
      )
    );

  /*============================================================================================================================
  | METHOD: ADD TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a <see cref="Topic"/>, returns the sitemap <c>url</c> elements for it and its descendants.
  /// </summary>
  /// <param name="topic">The topic to add to the sitemap.</param>
  private List<XElement> AddTopic(Topic topic) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish return collection
    \-------------------------------------------------------------------------------------------------------------------------*/
    List<XElement> topics       = [];

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate topic
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (
      topic.Attributes.GetBoolean("IsPrivateBranch") ||
      topic.ContentType.Equals("Container", StringComparison.OrdinalIgnoreCase) &&
      topic.Attributes.GetBoolean("NoIndex")
    ) return topics;
    if (ExcludedContentTypes.Any(c => topic.ContentType.Equals(c, StringComparison.OrdinalIgnoreCase))) return topics;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish variables
    \-------------------------------------------------------------------------------------------------------------------------*/
    var domain                  = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
    var lastModified            = new DateTime(Math.Max(topic.LastModified.Ticks, new DateTime(2000, 1, 1).Ticks));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish root element
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topicElement            = new XElement(_sitemapNamespace + "url",
      new XElement(_sitemapNamespace + "loc", domain + topic.GetWebPath()),
      new XElement(_sitemapNamespace + "changefreq", "monthly"),
      new XElement(_sitemapNamespace + "lastmod", lastModified.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
      new XElement(_sitemapNamespace + "priority", 1)
    );
    if (
      !SkippedContentTypes.Any(c => topic.ContentType?.Equals(c, StringComparison.OrdinalIgnoreCase)?? false) &&
      String.IsNullOrWhiteSpace(topic.Attributes.GetValue("Url")) &&
      !topic.Attributes.GetBoolean("NoIndex") &&
      !topic.Attributes.GetBoolean("IsDisabled")
    ) {
      topics.Add(topicElement);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Iterate over children
    \-------------------------------------------------------------------------------------------------------------------------*/
    foreach (var childTopic in  topic.Children) {
      topics.AddRange(AddTopic(childTopic));
    }

    return topics;

  }

} //Class
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using OnTopic.Internal.Diagnostics;
using OnTopic.Repositories;

namespace OnTopic.AspNetCore.Mvc.Controllers {

  /*============================================================================================================================
  | CLASS: SITEMAP CONTROLLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Responds to requests for a sitemap according to sitemap.org's schema. The view is expected to recursively loop over
  ///   child topics to generate the appropriate markup.
  /// </summary>
  public class SitemapController : Controller {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicRepository                _topicRepository;

    /*==========================================================================================================================
    | CONSTANTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    private static readonly XNamespace _sitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly XNamespace _pagemapNamespace = "http://www.google.com/schemas/sitemap-pagemap/1.0";

    /*==========================================================================================================================
    | EXCLUDE CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Specifies what content types should not be listed in the sitemap.
    /// </summary>
    private static string[] ExcludeContentTypes { get; } = { "List" };

    /*==========================================================================================================================
    | EXCLUDE ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Specifies what attributes should not be listed in the sitemap.
    /// </summary>
    private static string[] ExcludeAttributes { get; } = {
      "Body",
      "IsActive",
      "IsDisabled",
      "ParentID",
      "TopicID",
      "IsHidden",
      "NoIndex",
      "URL",
      "SortOrder"
    };

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic Controller with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    public SitemapController(ITopicRepository topicRepository) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicRepository, nameof(topicRepository));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository = topicRepository;

    }

    /*==========================================================================================================================
    | GET: /SITEMAP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the Sitemap.org sitemap for the site.
    /// </summary>
    /// <param name="indent">Optionally enables indentation of XML elements in output for human readability.</param>
    /// <param name="includeMetadata">Optionally enables extended metadata associated with each topic.</param>
    /// <returns>A Sitemap.org sitemap.</returns>
    public virtual ActionResult Index(bool indent = false, bool includeMetadata = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure topics are loaded
      \-----------------------------------------------------------------------------------------------------------------------*/
      var rootTopic = _topicRepository.Load();

      Contract.Assume(
        rootTopic,
        $"The topic graph could not be successfully loaded from the {nameof(ITopicRepository)} instance. The " +
        $"{nameof(SitemapController)} is unable to establish a local copy to work off of."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish sitemap
      \-----------------------------------------------------------------------------------------------------------------------*/
      var declaration           = new XDeclaration("1.0", "utf-8", "no");
      var sitemap               = GenerateSitemap(rootTopic, includeMetadata);
      var settings              = indent? SaveOptions.None : SaveOptions.DisableFormatting;

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the homepage view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return Content(declaration.ToString() + sitemap.ToString(settings), "text/xml");

    }

    /*==========================================================================================================================
    | GET: /SITEMAP/EXTENDED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the Sitemap.org sitemap for the site, including extended metadata attributes.
    /// </summary>
    /// <remarks>
    ///   Introducing the metadata makes the sitemap considerably larger. However, it also means that some agents will index the
    ///   additional information and make it available for querying. For instance, the (now defunct) Google Custom Search Engine
    ///   (CSE) would previously allow queries to be filtered based on metadata attributes exposed via the sitemap.
    /// </remarks>
    /// <param name="indent">Optionally enables indentation of XML elements in output for human readability.</param>
    /// <returns>A Sitemap.org sitemap.</returns>
    public virtual ActionResult Extended(bool indent = false) => Index(indent, true);

    /*==========================================================================================================================
    | METHOD: GENERATE SITEMAP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a root topic, generates an XML-formatted sitemap.
    /// </summary>
    /// <param name="topic">The topic to add to the sitemap.</param>
    /// <param name="includeMetadata">Optionally enables extended metadata associated with each topic.</param>
    /// <returns>A Sitemap.org sitemap.</returns>
    private XDocument GenerateSitemap(Topic rootTopic, bool includeMetadata = false) =>
      new(
        new XElement(_sitemapNamespace + "urlset",
          from topic in rootTopic?.Children
          select AddTopic(topic, includeMetadata)
        )
      );

    /*==========================================================================================================================
    | METHOD: ADD TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="Topic"/>, adds it to a given <see cref="XmlNode"/>.
    /// </summary>
    /// <param name="topic">The topic to add to the sitemap.</param>
    /// <param name="includeMetadata">Optionally enables extended metadata associated with each topic.</param>
    private IEnumerable<XElement> AddTopic(Topic topic, bool includeMetadata = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish return collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topics = new List<XElement>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic is null) return topics;
      if (topic.Attributes.GetValue("NoIndex") is "1") return topics;
      if (topic.Attributes.GetValue("IsDisabled") is "1") return topics;
      if (ExcludeContentTypes.Any(c => topic.ContentType.Equals(c, StringComparison.OrdinalIgnoreCase))) return topics;

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var domain = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
      var lastModified = new DateTime(Math.Max(topic.LastModified.Ticks, new DateTime(2000, 1, 1).Ticks));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish root element
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicElement = new XElement(_sitemapNamespace + "url",
        new XElement(_sitemapNamespace + "loc", domain + topic.GetWebPath()),
        new XElement(_sitemapNamespace + "changefreq", "monthly"),
        new XElement(_sitemapNamespace + "lastmod", lastModified.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
        new XElement(_sitemapNamespace + "priority", 1),
        includeMetadata? new XElement(_pagemapNamespace + "PageMap",
          new XElement(_pagemapNamespace + "DataObject",
            new XAttribute("type", topic.ContentType?? "Page"),
            getAttributes()
          ),
          getRelationships()
        ) : null
      );
      if (!topic.ContentType!.Equals("Container", StringComparison.OrdinalIgnoreCase)) {
        topics.Add(topicElement);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Iterate over children
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var childTopic in topic.Children) {
        topics.AddRange(AddTopic(childTopic, includeMetadata));
      }

      return topics;

      /*------------------------------------------------------------------------------------------------------------------------
      | Get attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      IEnumerable<XElement> getAttributes() =>
        from attribute in topic.Attributes
        where !ExcludeAttributes.Contains(attribute.Key, StringComparer.OrdinalIgnoreCase)
        where topic.Attributes.GetValue(attribute.Key)?.Length < 256
        select new XElement(_pagemapNamespace + "Attribute",
          new XAttribute("name", attribute.Key),
          new XText(topic.Attributes.GetValue(attribute.Key))
        );

      /*------------------------------------------------------------------------------------------------------------------------
      | Get relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      IEnumerable<XElement> getRelationships() =>
        from relationship in topic.Relationships
        select new XElement(_pagemapNamespace + "DataObject",
          new XAttribute("type", relationship.Name),
          from relatedTopic in topic.Relationships[relationship.Name]
          select new XElement(_pagemapNamespace + "Attribute",
            new XAttribute("name", "TopicKey"),
            new XText(relatedTopic.GetUniqueKey().Replace("Root:", "", StringComparison.InvariantCultureIgnoreCase))
          )
        );

    }

  } //Class
} //Namespace
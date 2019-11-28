/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Microsoft.AspNetCore.Mvc;
using Ignia.Topics.Repositories;
using Ignia.Topics.AspNetCore.Mvc.Models;
using Ignia.Topics.Internal.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;

namespace Ignia.Topics.AspNetCore.Mvc.Controllers {

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
    /// <returns>The site's homepage view.</returns>
    public virtual ActionResult Index() {

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
      var sitemap = GenerateSitemap(rootTopic);
      var sitemapFile = new StringBuilder();
      using (var writer = new StringWriter(sitemapFile)) {
        sitemap.Save(writer);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the homepage view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return Content(sitemapFile.ToString(), "text/xml");

    }

    /*==========================================================================================================================
    | METHOD: GENERATE SITEMAP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a root topic, generates an XML-formatted sitemap.
    /// </summary>
    /// <returns>The site's homepage view.</returns>
    public virtual XDocument GenerateSitemap(Topic rootTopic) =>
      new XDocument(
        new XDeclaration("1.0", "utf-8", String.Empty),
        new XElement(_sitemapNamespace + "urlset",
          from topic in rootTopic?.Children
          select AddTopic(topic)
        )
      );

    /*==========================================================================================================================
    | METHOD: ADD TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="Topic"/>, adds it to a given <see cref="XmlNode"/>.
    /// </summary>
    public IEnumerable<XElement> AddTopic(Topic topic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish return collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topics = new List<XElement>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) return topics;
      if (topic.Attributes.GetValue("NoIndex") == "1") return topics;
      if (topic.Attributes.GetValue("IsDisabled") == "1") return topics;
      if (ExcludeContentTypes.Any(c => topic.ContentType.Equals(c, StringComparison.InvariantCultureIgnoreCase))) return topics;

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
        new XElement(_pagemapNamespace + "PageMap",
          new XElement(_pagemapNamespace + "DataObject",
            new XAttribute("type", topic.ContentType?? "Page"),
            getAttributes()
          ),
          getRelationships()
        )
      );
      topics.Add(topicElement);

      /*------------------------------------------------------------------------------------------------------------------------
      | Iterate over children
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var childTopic in topic.Children) {
        topics.AddRange(AddTopic(childTopic));
      }

      return topics;

      /*------------------------------------------------------------------------------------------------------------------------
      | Get attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      IEnumerable<XElement> getAttributes() =>
        from attribute in topic.Attributes
        where !ExcludeAttributes.Contains(attribute.Key, StringComparer.InvariantCultureIgnoreCase)
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

  } // Class

} // Namespace
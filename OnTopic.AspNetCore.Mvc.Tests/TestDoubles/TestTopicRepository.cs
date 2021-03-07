/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Diagnostics.CodeAnalysis;
using OnTopic.AspNetCore.Mvc.Controllers;
using OnTopic.Attributes;
using OnTopic.Internal.Diagnostics;
using OnTopic.Repositories;
using OnTopic.TestDoubles;

namespace OnTopic.AspNetCore.Mvc.Tests.TestDoubles {

  /*============================================================================================================================
  | STUB: TEST TOPIC REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a basic version of an <see cref="ITopicRepository"/> with <see cref="Topic"/> instances meant to evaluate the
  ///   business logic of various controllers, such as the <see cref="TopicController"/>, <see cref="RedirectController"/>, and
  ///   <see cref="SitemapController"/>.
  /// </summary>
  [ExcludeFromCodeCoverage]
  class TestTopicRepository: DummyTopicRepository {

    /*==========================================================================================================================
    | VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            Topic                           _cache;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the StubTopicRepository.
    /// </summary>
    /// <returns>A new instance of the StubTopicRepository.</returns>
    public TestTopicRepository() : base() {
      _cache = CreateFakeData();
      Contract.Assume(_cache);
    }

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override Topic? Load(string? uniqueKey = null, Topic? referenceTopic = null, bool isRecursive = true) => _cache;

    /*==========================================================================================================================
    | METHOD: CREATE FAKE DATA
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a collection of fake data that loosely mimics a bare bones database.
    /// </summary>
    private static Topic CreateFakeData() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Define topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      var rootTopic             = new Topic("Root", "Container");
      var webTopic              = new Topic("Web", "Page", rootTopic);
      var validTopic            = new Topic("Valid", "Page", webTopic);
      var childTopic            = new Topic("Child", "Page", validTopic, 5);
      var nestedTopics          = new Topic("NestedTopics", "List", childTopic);
      _                         = new Topic("NestedTopic", "Page", nestedTopics);
      var redirectPage          = new Topic("Redirect", "Page", webTopic);
      var noIndexPage           = new Topic("NoIndex", "Page", webTopic);
      _                         = new Topic("NoIndexChild", "Page", noIndexPage);
      var disabledPage          = new Topic("Disabled", "Page", webTopic);
      var pageGroup             = new Topic("PageGroup", "PageGroup", rootTopic);
      _                         = new Topic("PageGroupChild", "Page", pageGroup);

      /*------------------------------------------------------------------------------------------------------------------------
      | Define attributes
      \-----------------------------------------------------------------------------------------------------------------------*/

      //Associations
      childTopic.References.SetValue("Reference", redirectPage);
      childTopic.Relationships.SetValue("Relationships", redirectPage);

      //Valid attributes
      validTopic.Attributes.SetValue("Attribute", "Value");
      validTopic.Attributes.SetValue("Title", "Title");

      //Excluded attributes
      validTopic.Attributes.SetBoolean("NoIndex", false);
      validTopic.Attributes.SetBoolean("IsHidden", true);
      validTopic.Attributes.SetValue("Body", "Body");
      validTopic.Attributes.SetValue("SortOrder", "5");

      //Excluded topics
      redirectPage.Attributes.SetValue("Url", "https://www.ignia.com/");
      noIndexPage.Attributes.SetBoolean("NoIndex", true);
      disabledPage.Attributes.SetBoolean("IsDisabled", true);

      /*------------------------------------------------------------------------------------------------------------------------
      | Return data
      \-----------------------------------------------------------------------------------------------------------------------*/
      return rootTopic;

    }

  }
}

/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ignia.Topics.Data.Caching;
using Ignia.Topics.Mapping;
using Ignia.Topics.Repositories;
using Ignia.Topics.Tests.TestDoubles;
using Ignia.Topics.Tests.ViewModels;
using Ignia.Topics.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC MAPPING SERVICE TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicMappingService"/> using local DTOs.
  /// </summary>
  [TestClass]
  public class TopicMappingServiceTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    ITopicRepository            _topicRepository                = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicMappingServiceTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="FakeTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph.
    /// </remarks>
    public TopicMappingServiceTest() {
      _topicRepository = new CachedTopicRepository(new FakeTopicRepository());
    }

    /*==========================================================================================================================
    | TEST: MAP (INSTANCE)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests setting basic scalar values by providing it with an already
    ///   constructed instance of a DTO.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapGeneric() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Test", "Page");

      topic.Attributes.SetValue("MetaTitle", "ValueA");
      topic.Attributes.SetValue("Title", "Value1");
      topic.Attributes.SetValue("IsHidden", "1");

      var target                = (PageTopicViewModel)mappingService.Map(topic, new PageTopicViewModel());

      Assert.AreEqual<string>("ValueA", target.MetaTitle);
      Assert.AreEqual<string>("Value1", target.Title);
      Assert.AreEqual<bool>(true, target.IsHidden);

    }

    /*==========================================================================================================================
    | TEST: MAP (DYNAMIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests setting basic scalar values by allowing it to dynamically
    ///   determine the instance type.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapDynamic() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Test", "Page");

      topic.Attributes.SetValue("MetaTitle", "ValueA");
      topic.Attributes.SetValue("Title", "Value1");
      topic.Attributes.SetValue("IsHidden", "1");

      var target                = (PageTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<string>("ValueA", target.MetaTitle);
      Assert.AreEqual<string>("Value1", target.Title);
      Assert.AreEqual<bool>(true, target.IsHidden);

    }

    /*==========================================================================================================================
    | TEST: MAP PARENTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the parent tree.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapParents() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var grandParent           = TopicFactory.Create("Grandparent", "Sample");
      var parent                = TopicFactory.Create("Parent", "Page", grandParent);
      var topic                 = TopicFactory.Create("Test", "Page", parent);

      topic.Attributes.SetValue("MetaTitle", "ValueA");
      topic.Attributes.SetValue("Title", "Value1");
      topic.Attributes.SetValue("IsHidden", "1");

      parent.Attributes.SetValue("Title", "Value2");
      parent.Attributes.SetValue("IsHidden", "1");

      grandParent.Attributes.SetValue("Title", "Value3");
      grandParent.Attributes.SetValue("IsHidden", "1");
      grandParent.Attributes.SetValue("Property", "ValueB");

      var viewModel             = (PageTopicViewModel)mappingService.Map(topic);
      var parentViewModel       = viewModel?.Parent;
      var grandParentViewModel  = parentViewModel?.Parent as SampleTopicViewModel;

      Assert.IsNotNull(viewModel);
      Assert.IsNotNull(parentViewModel);
      Assert.IsNotNull(grandParentViewModel);
      Assert.AreEqual<string>("ValueA", viewModel.MetaTitle);
      Assert.AreEqual<string>("Value1", viewModel.Title);
      Assert.AreEqual<bool>(true, viewModel.IsHidden);
      Assert.AreEqual<string>("Value2", parentViewModel.Title);
      Assert.AreEqual<string>("Value3", grandParentViewModel.Title);
      Assert.AreEqual<string>("ValueB", grandParentViewModel.Property);

    }

    /*==========================================================================================================================
    | TEST: INHERIT VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully inherits values as specified by
    ///   <see cref="InheritAttribute"/>.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_InheritValues() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var grandParent           = TopicFactory.Create("Grandparent", "Page");
      var parent                = TopicFactory.Create("Parent", "Page", grandParent);
      var topic                 = TopicFactory.Create("Test", "Sample", parent);

      grandParent.Attributes.SetValue("Property", "ValueA");
      grandParent.Attributes.SetValue("InheritedProperty", "ValueB");

      var viewModel             = (SampleTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<string>(null, viewModel.Property);
      Assert.AreEqual<string>("ValueB", viewModel.InheritedProperty);

    }

    /*==========================================================================================================================
    | TEST: ALTERNATE ATTRIBUTE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully derives values from the key
    ///   specified by <see cref="AttributeKeyAttribute"/>.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_AlternateAttributeKey() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Test", "Sample");

      topic.Attributes.SetValue("Property", "ValueA");
      topic.Attributes.SetValue("PropertyAlias", "ValueB");

      var viewModel             = (SampleTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<string>("ValueA", viewModel.PropertyAlias);

    }

    /*==========================================================================================================================
    | TEST: MAP RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the relationships.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapRelationships() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var relatedTopic1         = TopicFactory.Create("RelatedTopic1", "Page");
      var relatedTopic2         = TopicFactory.Create("RelatedTopic2", "Index");
      var relatedTopic3         = TopicFactory.Create("RelatedTopic3", "Page");
      var topic                 = TopicFactory.Create("Test", "Sample");

      topic.Relationships.SetTopic("Cousins", relatedTopic1);
      topic.Relationships.SetTopic("Cousins", relatedTopic2);
      topic.Relationships.SetTopic("Siblings", relatedTopic3);

      var target                = (SampleTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<int>(2, target.Cousins.Count);
      Assert.IsNotNull(target.Cousins.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic1")));
      Assert.IsNotNull(target.Cousins.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic2")));
      Assert.IsNull(target.Cousins.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic3")));

    }

    /*==========================================================================================================================
    | TEST: ALTERNATE RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully derives values from the key and
    ///   type specified by <see cref="RelationshipAttribute"/>.
    /// </summary>
    /// <remarks>
    ///   The <see cref="SampleTopicViewModel.RelationshipAlias"/> uses a <see cref="RelationshipAttribute"/> to set the
    ///   relationship key to <c>AmbiguousRelationship</c> and the <see cref="RelationshipType"/> to <see
    ///   cref="RelationshipType.IncomingRelationship"/>. <c>AmbiguousRelationship</c> refers to a relationship that is both
    ///   outgoing and incoming. It should be smart enough to a) look for the <c>AmbigousRelationship</c> instead of the
    ///   <c>RelationshipAlias</c>, and b) source from the <see cref="Topic.IncomingRelationships"/> collection.
    /// </remarks>
    [TestMethod]
    public void TopicMappingService_AlternateRelationship() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var relatedTopic1         = TopicFactory.Create("RelatedTopic1", "Page");
      var relatedTopic2         = TopicFactory.Create("RelatedTopic2", "Index");
      var relatedTopic3         = TopicFactory.Create("RelatedTopic3", "Page");
      var relatedTopic4         = TopicFactory.Create("RelatedTopic4", "Page");
      var relatedTopic5         = TopicFactory.Create("RelatedTopic5", "Page");
      var relatedTopic6         = TopicFactory.Create("RelatedTopic6", "Page");
      var topic                 = TopicFactory.Create("Test", "Sample");

      //Set outgoing relationships
      topic.Relationships.SetTopic("RelationshipAlias", relatedTopic1);
      topic.Relationships.SetTopic("AmbiguousRelationship", relatedTopic2);
      topic.Relationships.SetTopic("AmbiguousRelationship", relatedTopic3);

      //Set incoming relationships
      relatedTopic4.Relationships.SetTopic("RelationshipAlias", topic);
      relatedTopic5.Relationships.SetTopic("AmbiguousRelationship", topic);
      relatedTopic6.Relationships.SetTopic("AmbiguousRelationship", topic);

      var target = (SampleTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<int>(2, target.RelationshipAlias.Count);
      Assert.IsNotNull(target.RelationshipAlias.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic5")));
      Assert.IsNotNull(target.RelationshipAlias.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic6")));

    }

    /*==========================================================================================================================
    | TEST: MAP NESTED TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the nested topics.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapNestedTopics() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Test", "Sample");
      var childTopic            = TopicFactory.Create("ChildTopic", "Page", topic);
      var topicList             = TopicFactory.Create("Categories", "List", topic);
      var nestedTopic1          = TopicFactory.Create("NestedTopic1", "Page", topicList);
      var nestedTopic2          = TopicFactory.Create("NestedTopic2", "Index", topicList);

      var target                = (SampleTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<int>(2, target.Categories.Count);
      Assert.IsNotNull(target.Categories.FirstOrDefault((t) => t.Key.StartsWith("NestedTopic1")));
      Assert.IsNotNull(target.Categories.FirstOrDefault((t) => t.Key.StartsWith("NestedTopic2")));
      Assert.IsNull(target.Categories.FirstOrDefault((t) => t.Key.StartsWith("Categories")));
      Assert.IsNull(target.Categories.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic")));

    }

    /*==========================================================================================================================
    | TEST: MAP CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the nested topics.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapChildren() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Test", "Sample");
      var childTopic1           = TopicFactory.Create("ChildTopic1", "Page", topic);
      var childTopic2           = TopicFactory.Create("ChildTopic2", "Page", topic);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "Sample", topic);
      var childTopic4           = TopicFactory.Create("ChildTopic4", "Index", childTopic3);

      var target                = (SampleTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<int>(3, target.Children.Count);
      Assert.IsNotNull(target.Children.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic1")));
      Assert.IsNotNull(target.Children.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic2")));
      Assert.IsNotNull(target.Children.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic3")));
      Assert.IsNull(target.Children.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic4")));
      Assert.AreEqual<int>(0, ((SampleTopicViewModel)target.Children.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic3"))).Children.Count);

    }

    /*==========================================================================================================================
    | TEST: RECURSIVE RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully follows relationships per the
    ///   instructions of each model class.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_RecursiveRelationships() {

      var mappingService        = new TopicMappingService(_topicRepository);

      //Self
      var topic                 = TopicFactory.Create("Test", "Sample");

      //First cousins
      var cousinTopic1          = TopicFactory.Create("CousinTopic1", "Page");
      var cousinTopic2          = TopicFactory.Create("CousinTopic2", "Index");
      var cousinTopic3          = TopicFactory.Create("CousinTopic3", "Sample");

      //Children of cousins
      var childTopic1           = TopicFactory.Create("ChildTopic1", "Page", cousinTopic3);
      var childTopic2           = TopicFactory.Create("ChildTopic2", "Page", cousinTopic3);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "Sample", cousinTopic3);

      //Other cousins
      var secondCousin          = TopicFactory.Create("SecondCousin", "Page");
      var cousinTwiceRemoved    = TopicFactory.Create("CousinOnceRemoved", "Page", childTopic3);

      //Set first cousins
      topic.Relationships.SetTopic("Cousins", cousinTopic1);
      topic.Relationships.SetTopic("Cousins", cousinTopic2);
      topic.Relationships.SetTopic("Cousins", cousinTopic3);

      //Set ancillary relationships
      cousinTopic3.Relationships.SetTopic("Cousins", secondCousin);

      var target                = (SampleTopicViewModel)mappingService.Map(topic);
      var cousinTarget          = (SampleTopicViewModel)target.Cousins.FirstOrDefault((t) => t.Key.StartsWith("CousinTopic3"));
      var distantCousinTarget   = (SampleTopicViewModel)cousinTarget.Children.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic3"));

      //Because Cousins is set to recurse over Children, its children should be set
      Assert.AreEqual<int>(3, cousinTarget.Children.Count);

      //Because Cousins is not set to recurse over Cousins, its cousins should NOT be set (even though there is one cousin)
      Assert.AreEqual<int>(0, cousinTarget.Cousins.Count);

      //Because Children is not set to recurse over Children, the grandchildren of a cousin should NOT be set
      Assert.AreEqual<int>(0, distantCousinTarget.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP SLIDE SHOW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls a <see
    ///   cref="SlideshowTopicViewModel"/>, even though the <see cref="ContentListTopicViewModel.ContentItems"/> list is a
    ///   collection of <see cref="ContentItemTopicViewModel"/> objects (from which <see cref="SlideTopicViewModel"/>.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapSlideshow() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Test", "Slideshow");
      var slides                = TopicFactory.Create("ContentItems", "List", topic);
      var childTopic1           = TopicFactory.Create("ChildTopic1", "Slide", slides);
      var childTopic2           = TopicFactory.Create("ChildTopic2", "Slide", slides);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "Slide", slides);
      var childTopic4           = TopicFactory.Create("ChildTopic4", "ContentItem", slides);

      var target                = (SlideshowTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<int>(4, target.ContentItems.Count);
      Assert.IsNotNull(target.ContentItems.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic1")));
      Assert.IsNotNull(target.ContentItems.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic2")));
      Assert.IsNotNull(target.ContentItems.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic3")));
      Assert.IsNotNull(target.ContentItems.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic4")));

    }

    /*==========================================================================================================================
    | TEST: MAP TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully includes <see cref="Topic"/>
    ///   instances if called for by the model. This isn't a best practice, but is maintained for edge cases.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapTopics() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var relatedTopic1         = TopicFactory.Create("RelatedTopic1", "Page");
      var relatedTopic2         = TopicFactory.Create("RelatedTopic2", "Index");
      var relatedTopic3         = TopicFactory.Create("RelatedTopic3", "Page");
      var topic                 = TopicFactory.Create("Test", "Sample");

      topic.Relationships.SetTopic("Related", relatedTopic1);
      topic.Relationships.SetTopic("Related", relatedTopic2);
      topic.Relationships.SetTopic("Related", relatedTopic3);

      var target                = (SampleTopicViewModel)mappingService.Map(topic);
      var relatedTopic3copy     = ((Topic)target.Related.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic3")));

      Assert.AreEqual<int>(3, target.Related.Count);
      Assert.IsNotNull(target.Related.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic1")));
      Assert.IsNotNull(target.Related.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic2")));
      Assert.IsNotNull(target.Related.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic3")));
      Assert.AreEqual(relatedTopic3.Key, relatedTopic3copy.Key);

    }

    /*==========================================================================================================================
    | TEST: MAP METADATA
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether is able to lookup metadata successfully using the
    ///   <see cref="MetadataAttribute"/>.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapMetadata() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Test", "MetadataLookup");

      var target                = (MetadataLookupTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<int>(5, target.Categories.Count);

    }

    /*==========================================================================================================================
    | TEST: FILTER BY CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see
    ///   cref="SampleTopicViewModel.Children"/> property can be filtered by <see cref="TopicViewModel.ContentType"/>.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_FilterByContentType() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Test", "Sample");
      var childTopic1           = TopicFactory.Create("ChildTopic1", "Page", topic);
      var childTopic2           = TopicFactory.Create("ChildTopic2", "Index", topic);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "Index", topic);
      var childTopic4           = TopicFactory.Create("ChildTopic4", "Index", childTopic3);

      var target                = (SampleTopicViewModel)mappingService.Map(topic);

      var indexes               = target.Children.GetByContentType("Index");

      Assert.AreEqual<int>(2, indexes.Count);
      Assert.IsNotNull(indexes.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic2")));
      Assert.IsNotNull(indexes.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic3")));
      Assert.IsNull(indexes.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic4")));

    }

    /*==========================================================================================================================
    | TEST: MAP GETTER METHODS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a property corresponding to a getter method on <see cref="Topic"/> to ensure that it is
    ///   correctly populated.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapGetterMethods() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Topic", "Sample");
      var childTopic            = TopicFactory.Create("Child", "Page", topic);
      var grandChildTopic       = TopicFactory.Create("GrandChild", "Index", childTopic);

      var target = (IndexTopicViewModel)mappingService.Map(grandChildTopic);

      Assert.AreEqual<string>("Topic:Child:GrandChild", target.UniqueKey);

    }

    /*==========================================================================================================================
    | TEST: MAP REQUIRED PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is not thrown if it is set.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapRequiredProperty() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Topic", "Required");

      topic.Attributes.SetValue("RequiredAttribute", "Required");

      var target = (RequiredTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<string>("Required", target.RequiredAttribute);

    }

    /*==========================================================================================================================
    | TEST: MAP REQUIRED PROPERTY EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is thrown if it isn't set.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public void TopicMappingService_MapRequiredPropertyException() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Topic", "Required");

      var target = (RequiredTopicViewModel)mappingService.Map(topic);

    }

    /*==========================================================================================================================
    | TEST: REQUIRED OBJECT PROPERTY EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is thrown if it isn't set.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public void TopicMappingService_MapRequiredObjectPropertyException() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Topic", "RequiredObject");

      var target = (RequiredTopicViewModel)mappingService.Map(topic);

    }

    /*==========================================================================================================================
    | TEST: DEFAULT VALUE PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has default properties. Ensures that each is set appropriately.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapDefaultValueProperties() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Topic", "DefaultValue");

      var target                = (DefaultValueTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<string>("Default", target.DefaultString);
      Assert.AreEqual<int>(10, target.DefaultInt);
      Assert.IsTrue(target.DefaultBool);

    }

    /*==========================================================================================================================
    | TEST: MINIMUM VALUE PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has minimum value properties. Ensures that an error is thrown if the minimum is not met.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public void TopicMappingService_MapMinimumValueProperties() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Topic", "MinimumLengthProperty");

      topic.Attributes.SetValue("MinimumLength", "Hello World");

      var target = (MinimumLengthPropertyTopicViewModel)mappingService.Map(topic);

    }

    /*==========================================================================================================================
    | TEST: FILTER BY ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see
    ///   cref="SampleTopicViewModel.Children"/> property can be filtered using stacked <see cref="FilterByAttributeAttribute"/>
    ///   instances.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_FilterByAttribute() {

      var mappingService        = new TopicMappingService(_topicRepository);
      var topic                 = TopicFactory.Create("Test", "Filtered");
      var childTopic1           = TopicFactory.Create("ChildTopic1", "Page", topic);
      var childTopic2           = TopicFactory.Create("ChildTopic2", "Index", topic);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "Page", topic);
      var childTopic4           = TopicFactory.Create("ChildTopic4", "Page", childTopic3);

      childTopic1.Attributes.SetValue("SomeAttribute", "ValueA");
      childTopic2.Attributes.SetValue("SomeAttribute", "ValueA");
      childTopic3.Attributes.SetValue("SomeAttribute", "ValueA");
      childTopic4.Attributes.SetValue("SomeAttribute", "ValueB");

      var target = (FilteredTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<int>(2, target.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: CACHING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> as well as a <see cref="CachedTopicMappingService"/> and ensures that
    ///   the same instance of a mapped object is turned after two calls.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_Caching() {

      var mappingService = new TopicMappingService(_topicRepository);
      var cachedMappingService = new CachedTopicMappingService(mappingService);

      var topic = TopicFactory.Create("Test", "Filtered", 5);

      var target1 = (FilteredTopicViewModel)cachedMappingService.Map(topic);
      var target2 = (FilteredTopicViewModel)cachedMappingService.Map(topic);

      Assert.AreEqual<FilteredTopicViewModel>(target1, target2);

    }


  } //Class

} //Namespace


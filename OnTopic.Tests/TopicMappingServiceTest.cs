/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Attributes;
using OnTopic.Data.Caching;
using OnTopic.Mapping;
using OnTopic.Mapping.Annotations;
using OnTopic.Metadata;
using OnTopic.Metadata.AttributeTypes;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.Tests.TestDoubles;
using OnTopic.Tests.ViewModels;
using OnTopic.Tests.ViewModels.Metadata;
using OnTopic.ViewModels;

namespace OnTopic.Tests {

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
    readonly                    ITopicRepository                _topicRepository;
    readonly                    ITopicMappingService            _mappingService;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicMappingServiceTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="StubTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph.
    /// </remarks>
    public TopicMappingServiceTest() {
      _topicRepository = new CachedTopicRepository(new StubTopicRepository());
      _mappingService = new TopicMappingService(new DummyTopicRepository(), new FakeViewModelLookupService());
    }

    /*==========================================================================================================================
    | TEST: MAP: GENERIC: RETURNS NEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests setting basic scalar values by specifying an explicit type.
    /// </summary>
    [TestMethod]
    public async Task Map_Generic_ReturnsNewModel() {

      var topic                 = TopicFactory.Create("Test", "Page");

      topic.Attributes.SetValue("MetaTitle", "ValueA");
      topic.Attributes.SetValue("Title", "Value1");
      topic.Attributes.SetValue("IsHidden", "1");

      var target                = await _mappingService.MapAsync<PageTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<string>("ValueA", target.MetaTitle);
      Assert.AreEqual<string>("Value1", target.Title);
      Assert.AreEqual<bool>(true, target.IsHidden);

    }

    /*==========================================================================================================================
    | TEST: MAP: DYNAMIC: RETURNS NEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests setting basic scalar values by allowing it to dynamically
    ///   determine the instance type.
    /// </summary>
    [TestMethod]
    public async Task Map_Dynamic_ReturnsNewModel() {

      var topic                 = TopicFactory.Create("Test", "Page");

      topic.Attributes.SetValue("MetaTitle", "ValueA");
      topic.Attributes.SetValue("Title", "Value1");
      topic.Attributes.SetValue("IsHidden", "1");

      var target                = (PageTopicViewModel?)await _mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.AreEqual<string>("ValueA", target.MetaTitle);
      Assert.AreEqual<string>("Value1", target.Title);
      Assert.AreEqual<bool>(true, target.IsHidden);

    }

    /*==========================================================================================================================
    | TEST: MAP: DISABLED PROPERTY: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it maps a property decorated with the <see
    ///   cref="DisableMappingAttribute"/>.
    /// </summary>
    [TestMethod]
    public async Task Map_DisabledProperty_ReturnsNull() {

      var topic                 = TopicFactory.Create("Test", "DisableMapping");

      var viewModel             = await _mappingService.MapAsync<DisableMappingTopicViewModel>(topic).ConfigureAwait(false);

      Assert.IsNotNull(viewModel);
      Assert.IsNull(viewModel.Key);

    }

    /*==========================================================================================================================
    | TEST: MAP: PARENTS: RETURNS ASCENDENTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the parent tree.
    /// </summary>
    [TestMethod]
    public async Task Map_Parents_ReturnsAscendents() {

      var grandParent           = TopicFactory.Create("Grandparent", "AscendentSpecialized");
      var parent                = TopicFactory.Create("Parent", "Ascendent", grandParent);
      var topic                 = TopicFactory.Create("Test", "Ascendent", parent);

      grandParent.Attributes.SetValue("IsRoot", "1");

      var viewModel             = await _mappingService.MapAsync<AscendentTopicViewModel>(topic).ConfigureAwait(false);
      var parentViewModel       = viewModel?.Parent;
      var grandParentViewModel  = parentViewModel?.Parent as AscendentSpecializedTopicViewModel;

      Assert.IsNotNull(viewModel);
      Assert.IsNotNull(parentViewModel);
      Assert.IsNotNull(grandParentViewModel);
      Assert.AreEqual<string>("Test", viewModel.Key);
      Assert.AreEqual<string>("Parent", parentViewModel.Key);
      Assert.AreEqual<string>("Grandparent", grandParentViewModel.Key);
      Assert.IsTrue(grandParentViewModel.IsRoot);
      Assert.IsNull(grandParentViewModel.Parent);

    }

    /*==========================================================================================================================
    | TEST: MAP: INHERIT ATTRIBUTE: INHERITS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully inherits values as specified by
    ///   <see cref="InheritAttribute"/>.
    /// </summary>
    [TestMethod]
    public async Task Map_InheritAttribute_InheritsValue() {

      var grandParent           = TopicFactory.Create("Grandparent", "Page");
      var parent                = TopicFactory.Create("Parent", "Page", grandParent);
      var topic                 = TopicFactory.Create("Test", "InheritedProperty", parent);

      grandParent.Attributes.SetValue("Property", "ValueA");
      grandParent.Attributes.SetValue("InheritedProperty", "ValueB");

      var viewModel             = await _mappingService.MapAsync<InheritedPropertyTopicViewModel>(topic).ConfigureAwait(false);

      Assert.IsNull(viewModel.Property);
      Assert.AreEqual<string>("ValueB", viewModel.InheritedProperty);

    }

    /*==========================================================================================================================
    | TEST: MAP: NULLABLE PROPERTIES WITH INVALID VALUES: RETURNS NULL OR DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests setting nullable scalar values with invalid values with the
    ///   expectation that they will be returned as null.
    /// </summary>
    [TestMethod]
    public async Task Map_NullablePropertiesWithInvalidValues_ReturnsNullOrDefault() {

      var topic                 = TopicFactory.Create("Test", "Page");

      topic.Attributes.SetValue("NullableInteger", "A");
      topic.Attributes.SetValue("NullableBoolean", "43");
      topic.Attributes.SetValue("NullableDateTime", "Hello World");
      topic.Attributes.SetValue("NullableUrl", "invalid://Web\\Path\\File!?@Query=String?");

      var target                = await _mappingService.MapAsync<NullablePropertyTopicViewModel>(topic).ConfigureAwait(false);

      Assert.IsNull(target.NullableInteger);
      Assert.IsNull(target.NullableBoolean);
      Assert.IsNull(target.NullableDateTime);
      Assert.IsNull(target.NullableUrl);

      //The following should not be null since they map to non-nullable properties which will have default values
      Assert.AreEqual(topic.Title, target.Title);
      Assert.AreEqual<bool?>(topic.IsHidden, target.IsHidden);
      Assert.AreEqual<DateTime?>(topic.LastModified, target.LastModified);

    }

    /*==========================================================================================================================
    | TEST: MAP: NULLABLE PROPERTIES WITH VALID VALUES: RETURNS VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests setting nullable scalar values with valid values with the
    ///   expectation that they will be mapped correctly.
    /// </summary>
    [TestMethod]
    public async Task Map_NullablePropertiesWithInvalidValues_ReturnsValues() {

      var topic                 = TopicFactory.Create("Test", "Page");

      topic.Attributes.SetValue("NullableString", "Hello World.");
      topic.Attributes.SetValue("NullableInteger", "43");
      topic.Attributes.SetValue("NullableDouble", "3.14159265359");
      topic.Attributes.SetValue("NullableBoolean", "tRuE");
      topic.Attributes.SetValue("NullableDateTime", "10/15/1976");
      topic.Attributes.SetValue("NullableUrl", "/Web/Path/File?Query=String");

      topic.Attributes.SetValue("Title", "Hello World.");
      topic.Attributes.SetValue("IsHidden", "true");
      topic.Attributes.SetValue("LastModified", "10/15/1976");

      var target                = await _mappingService.MapAsync<NullablePropertyTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<string?>("Hello World.", target.NullableString);
      Assert.AreEqual<int?>(43, target.NullableInteger);
      Assert.AreEqual<double?>(3.14159265359, target.NullableDouble);
      Assert.AreEqual<bool?>(true, target.NullableBoolean);
      Assert.AreEqual<DateTime?>(new(1976, 10, 15), target.NullableDateTime);
      Assert.AreEqual<Uri?>(new("/Web/Path/File?Query=String", UriKind.RelativeOrAbsolute), target.NullableUrl);

      Assert.AreEqual<string?>(topic.Title, target.Title);
      Assert.AreEqual<bool?>(target.IsHidden, target.IsHidden);
      Assert.AreEqual<DateTime?>(topic.LastModified, target.LastModified);

    }

    /*==========================================================================================================================
    | TEST: MAP: ALTERNATE ATTRIBUTE KEY: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully derives values from the key
    ///   specified by <see cref="AttributeKeyAttribute"/>.
    /// </summary>
    [TestMethod]
    public async Task Map_AlternateAttributeKey_ReturnsMappedModel() {

      var topic                 = TopicFactory.Create("Test", "PropertyAlias");

      topic.Attributes.SetValue("Property", "ValueA");
      topic.Attributes.SetValue("PropertyAlias", "ValueB");

      var viewModel             = await _mappingService.MapAsync<PropertyAliasTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<string>("ValueA", viewModel.PropertyAlias);

    }

    /*==========================================================================================================================
    | TEST: MAP: RELATIONSHIPS: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the relationships.
    /// </summary>
    [TestMethod]
    public async Task Map_Relationships_ReturnsMappedModel() {

      var relatedTopic1         = TopicFactory.Create("Cousin1", "Relation");
      var relatedTopic2         = TopicFactory.Create("Cousin2", "Relation");
      var relatedTopic3         = TopicFactory.Create("Sibling", "Relation");
      var topic                 = TopicFactory.Create("Test", "Relation");

      topic.Relationships.SetTopic("Cousins", relatedTopic1);
      topic.Relationships.SetTopic("Cousins", relatedTopic2);
      topic.Relationships.SetTopic("Siblings", relatedTopic3);

      var target                = await _mappingService.MapAsync<RelationTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int>(2, target.Cousins.Count);
      Assert.IsNotNull(GetChildTopic(target.Cousins, "Cousin1"));
      Assert.IsNotNull(GetChildTopic(target.Cousins, "Cousin2"));
      Assert.IsNull(GetChildTopic(target.Cousins, "Sibling"));

    }

    /*==========================================================================================================================
    | TEST: MAP: ALTERNATE RELATIONSHIP: RETURNS CORRECT RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully derives values from the key and
    ///   type specified by <see cref="Mapping.Annotations.RelationshipAttribute"/>.
    /// </summary>
    /// <remarks>
    ///   The <see cref="AmbiguousRelationTopicViewModel.RelationshipAlias"/> uses <see
    ///   cref="Mapping.Annotations.RelationshipAttribute"/> to set the relationship key to <c>AmbiguousRelationship</c> and the
    ///   <see cref="RelationshipType"/> to <see cref="RelationshipType.IncomingRelationship"/>. <c>AmbiguousRelationship</c>
    ///   refers to a relationship that is both outgoing and incoming. It should be smart enough to a) look for the
    ///   <c>AmbigousRelationship</c> instead of the <c>RelationshipAlias</c>, and b) source from the <see
    ///   cref="Topic.IncomingRelationships"/> collection.
    /// </remarks>
    [TestMethod]
    public async Task Map_AlternateRelationship_ReturnsCorrectRelationship() {

      var outgoingRelation      = TopicFactory.Create("OutgoingRelation", "KeyOnly");
      var incomingRelation      = TopicFactory.Create("IncomingRelation", "KeyOnly");
      var ambiguousRelation     = TopicFactory.Create("AmbiguousRelation", "KeyOnly");

      var topic                 = TopicFactory.Create("Test", "AmbiguousRelation");

      //Set outgoing relationships
      topic.Relationships.SetTopic("RelationshipAlias", ambiguousRelation);
      topic.Relationships.SetTopic("AmbiguousRelationship", outgoingRelation);

      //Set incoming relationships
      ambiguousRelation.Relationships.SetTopic("RelationshipAlias", topic);
      incomingRelation.Relationships.SetTopic("AmbiguousRelationship", topic);

      var target = await _mappingService.MapAsync<AmbiguousRelationTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int>(1, target.RelationshipAlias.Count);
      Assert.IsNotNull(GetChildTopic(target.RelationshipAlias, "IncomingRelation"));

    }

    /*==========================================================================================================================
    | TEST: MAP: CUSTOM COLLECTION: RETURNS COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully derives values from a custom source
    ///   collection compatible with <see cref="IList{T}"/>, where <c>{T}</c> is a <see cref="Topic"/>, or derivative.
    /// </summary>
    [TestMethod]
    public async Task Map_CustomCollection_ReturnsCollection() {

      var topic                 = _topicRepository.Load("Root:Configuration:ContentTypes:Page");
      var target                = await _mappingService.MapAsync<ContentTypeDescriptorTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int>(8, target.AttributeDescriptors.Count);
      Assert.AreEqual<int>(2, target.PermittedContentTypes.Count);

      //Ensure custom collections are not recursively followed without instruction
      Assert.AreEqual<int>(0, target.PermittedContentTypes.FirstOrDefault()?.PermittedContentTypes.Count?? 0);

    }

    /*==========================================================================================================================
    | TEST: MAP: NESTED TOPICS: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the nested topics.
    /// </summary>
    [TestMethod]
    public async Task Map_NestedTopics_ReturnsMappedModel() {

      var topic                 = TopicFactory.Create("Test", "Nested");
      var childTopic            = TopicFactory.Create("ChildTopic", "KeyOnly", topic);
      var topicList             = TopicFactory.Create("Categories", "List", topic);
      var nestedTopic1          = TopicFactory.Create("NestedTopic1", "KeyOnly", topicList);
      var nestedTopic2          = TopicFactory.Create("NestedTopic2", "KeyOnly", topicList);

      topicList.IsHidden        = true;

      var target                = await _mappingService.MapAsync<NestedTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int>(2, target.Categories.Count);

      Assert.IsNotNull(GetChildTopic(target.Categories, "NestedTopic1"));
      Assert.IsNotNull(GetChildTopic(target.Categories, "NestedTopic2"));
      Assert.IsNull(GetChildTopic(target.Categories, "Categories"));
      Assert.IsNull(GetChildTopic(target.Categories, "ChildTopic"));

    }

    /*==========================================================================================================================
    | TEST: MAP: CHILDREN: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls child topics.
    /// </summary>
    [TestMethod]
    public async Task Map_Children_ReturnsMappedModel() {

      var topic                 = TopicFactory.Create("Test", "Descendent");
      var childTopic1           = TopicFactory.Create("ChildTopic1", "Descendent", topic);
      var childTopic2           = TopicFactory.Create("ChildTopic2", "Descendent", topic);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "Descendent", topic);
      var childTopic4           = TopicFactory.Create("ChildTopic4", "DescendentSpecialized", topic);
      var invalidChildTopic     = TopicFactory.Create("invalidChildTopic", "KeyOnly", topic);
      var grandchildTopic       = TopicFactory.Create("GrandchildTopic", "Descendent", childTopic3);

      childTopic4.Attributes.SetBoolean("IsLeaf", true);

      var target                = await _mappingService.MapAsync<DescendentTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int>(4, target.Children.Count);
      Assert.IsNotNull(GetChildTopic(target.Children, "ChildTopic1"));
      Assert.IsNotNull(GetChildTopic(target.Children, "ChildTopic2"));
      Assert.IsNotNull(GetChildTopic(target.Children, "ChildTopic3"));
      Assert.IsNotNull(GetChildTopic(target.Children, "ChildTopic4"));
      Assert.IsTrue(((DescendentSpecializedTopicViewModel)GetChildTopic(target.Children, "ChildTopic4")).IsLeaf);
      Assert.IsNull(GetChildTopic(target.Children, "invalidChildTopic"));
      Assert.IsNull(GetChildTopic(target.Children, "GrandchildTopic"));
      Assert.IsNotNull(GetChildTopic(
        ((DescendentTopicViewModel)GetChildTopic(target.Children, "ChildTopic3")).Children,
        "GrandchildTopic"
      ));
    }

    /*==========================================================================================================================
    | TEST: MAP: MAP TO PARENT: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's nested complex objects are
    ///   property mapped with attribute values from the parent, based on their <see cref="MapToParentAttribute"/>
    ///   configuration.
    /// </summary>
    [TestMethod]
    public async Task Map_MapToParent_ReturnsMappedModel() {

      var topic                 = TopicFactory.Create("Test", "FlattenChildren");

      topic.Attributes.SetValue("PrimaryKey", "Primary Key");
      topic.Attributes.SetValue("AlternateKey", "Alternate Key");
      topic.Attributes.SetValue("AncillaryKey", "Ancillary Key");
      topic.Attributes.SetValue("AliasedKey", "Aliased Key");

      var target = await _mappingService.MapAsync<MapToParentTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<string>("Test", target.Primary.Key);
      Assert.AreEqual<string>("Aliased Key", target.Alternate.Key);
      Assert.AreEqual<string>("Ancillary Key", target.Ancillary.Key);

    }

    /*==========================================================================================================================
    | TEST: MAP: TOPIC REFERENCES: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully maps referenced topics.
    /// </summary>
    [TestMethod]
    public async Task Map_TopicReferences_ReturnsMappedModel() {

      var mappingService        = new TopicMappingService(_topicRepository, new FakeViewModelLookupService());
      var topicReference        = _topicRepository.Load(11111);

      var topic                 = TopicFactory.Create("Test", "TopicReference");

      topic.Attributes.SetInteger("TopicReferenceId", topicReference.Id);

      var target                = (TopicReferenceTopicViewModel?)await mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.IsNotNull(target.TopicReference);
      Assert.AreEqual<string>(topicReference.Key, target.TopicReference.Key);

    }

    /*==========================================================================================================================
    | TEST: MAP: RECURSIVE RELATIONSHIPS: RETURNS GRAPH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully follows relationships per the
    ///   instructions of each model class.
    /// </summary>
    [TestMethod]
    public async Task Map_RecursiveRelationships_ReturnsGraph() {

      //Self
      var topic                 = TopicFactory.Create("Test", "Relation");

      //First cousins
      var cousinTopic1          = TopicFactory.Create("CousinTopic1", "Relation");
      var cousinTopic2          = TopicFactory.Create("CousinTopic2", "Relation");
      var cousinTopic3          = TopicFactory.Create("CousinTopic3", "RelationWithChildren");

      //First cousins once removed
      var childTopic1           = TopicFactory.Create("ChildTopic1", "RelationWithChildren", cousinTopic3);
      var childTopic2           = TopicFactory.Create("ChildTopic2", "RelationWithChildren", cousinTopic3);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "RelationWithChildren", cousinTopic3);

      //Other cousins
      var secondCousin          = TopicFactory.Create("SecondCousin", "Relation");
      var cousinOnceRemoved     = TopicFactory.Create("CousinOnceRemoved", "Relation", childTopic3);

      //Set first cousins
      topic.Relationships.SetTopic("Cousins", cousinTopic1);
      topic.Relationships.SetTopic("Cousins", cousinTopic2);
      topic.Relationships.SetTopic("Cousins", cousinTopic3);

      //Set ancillary relationships
      cousinTopic3.Relationships.SetTopic("Cousins", secondCousin);

      var target                = await _mappingService.MapAsync<RelationTopicViewModel>(topic).ConfigureAwait(false);

      var cousinTarget          = GetChildTopic(target.Cousins, "CousinTopic3") as RelationWithChildrenTopicViewModel;
      var distantCousinTarget   = GetChildTopic(cousinTarget.Children, "ChildTopic3") as RelationWithChildrenTopicViewModel;

      //Because Cousins is set to recurse over Children, its children should be set
      Assert.AreEqual<int>(3, cousinTarget.Children.Count);

      //Because Cousins is not set to recurse over Cousins, its cousins should NOT be set (even though there is one cousin)
      Assert.AreEqual<int>(0, cousinTarget.Cousins.Count);

      //Because Children is not set to recurse over Children, the grandchildren of a cousin should NOT be set
      Assert.AreEqual<int>(0, distantCousinTarget.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: SLIDE SHOW: RETURNS DERIVED VIEW MODELS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls a <see
    ///   cref="SlideshowTopicViewModel"/>, even though the <see cref="ContentListTopicViewModel.ContentItems"/> list is a
    ///   collection of <see cref="ContentItemTopicViewModel"/> objects (from which <see cref="SlideTopicViewModel"/>.
    /// </summary>
    [TestMethod]
    public async Task Map_Slideshow_ReturnsDerivedViewModels() {

      var topic                 = TopicFactory.Create("Test", "Slideshow");
      var slides                = TopicFactory.Create("ContentItems", "List", topic);
      var childTopic1           = TopicFactory.Create("ChildTopic1", "Slide", slides);
      var childTopic2           = TopicFactory.Create("ChildTopic2", "Slide", slides);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "Slide", slides);
      var childTopic4           = TopicFactory.Create("ChildTopic4", "ContentItem", slides);

      var target                = await _mappingService.MapAsync<SlideshowTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int>(4, target.ContentItems.Count);
      Assert.IsNotNull(GetChildTopic(target.ContentItems, "ChildTopic1"));
      Assert.IsNotNull(GetChildTopic(target.ContentItems, "ChildTopic2"));
      Assert.IsNotNull(GetChildTopic(target.ContentItems, "ChildTopic3"));
      Assert.IsNotNull(GetChildTopic(target.ContentItems, "ChildTopic4"));

    }

    /*==========================================================================================================================
    | TEST: MAP: TOPIC ENTITIES: RETURNS TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully includes <see cref="Topic"/>
    ///   instances if called for by the model. This isn't a best practice, but is maintained for edge cases.
    /// </summary>
    [TestMethod]
    public async Task Map_TopicEntities_ReturnsTopics() {

      var relatedTopic1         = TopicFactory.Create("RelatedTopic1", "KeyOnly");
      var relatedTopic2         = TopicFactory.Create("RelatedTopic2", "KeyOnly");
      var relatedTopic3         = TopicFactory.Create("RelatedTopic3", "KeyOnly");
      var topic                 = TopicFactory.Create("Test", "RelatedEntity");

      topic.Relationships.SetTopic("RelatedTopics", relatedTopic1);
      topic.Relationships.SetTopic("RelatedTopics", relatedTopic2);
      topic.Relationships.SetTopic("RelatedTopics", relatedTopic3);

      var target                = await _mappingService.MapAsync<RelatedEntityTopicViewModel>(topic).ConfigureAwait(false);
      var relatedTopic3copy     = (getRelatedTopic(target, "RelatedTopic3"));

      Assert.AreEqual<int>(3, target.RelatedTopics.Count);

      Assert.IsNotNull(getRelatedTopic(target, "RelatedTopic1"));
      Assert.IsNotNull(getRelatedTopic(target, "RelatedTopic2"));
      Assert.IsNotNull(getRelatedTopic(target, "RelatedTopic3"));

      Assert.AreEqual(relatedTopic3.Key, relatedTopic3copy.Key);

      Topic getRelatedTopic(RelatedEntityTopicViewModel topic, string key)
        => topic.RelatedTopics.FirstOrDefault((t) => t.Key.StartsWith(key, StringComparison.InvariantCulture));

    }

    /*==========================================================================================================================
    | TEST: MAP: METADATA LOOKUP: RETURNS LOOKUP ITEMS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether is able to lookup metadata successfully using the
    ///   <see cref="MetadataAttribute"/>.
    /// </summary>
    [TestMethod]
    public async Task Map_MetadataLookup_ReturnsLookupItems() {

      var mappingService        = new TopicMappingService(_topicRepository, new FakeViewModelLookupService());
      var topic                 = TopicFactory.Create("Test", "MetadataLookup");

      var target                = (MetadataLookupTopicViewModel?)await mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.AreEqual<int>(5, target.Categories.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: CIRCULAR REFERENCE: RETURNS MAPPED PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully handles a circular reference by
    ///   taking advantage of its internal caching mechanism.
    /// </summary>
    [TestMethod]
    public async Task Map_CircularReference_ReturnsCachedParent() {

      var topic                 = TopicFactory.Create("Test", "Circular", 1);
      var childTopic            = TopicFactory.Create("ChildTopic", "Circular", 2, topic);

      var mappedTopic           = await _mappingService.MapAsync<CircularTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<CircularTopicViewModel>(mappedTopic, mappedTopic.Children.First().Parent);

    }

    /*==========================================================================================================================
    | TEST: MAP: FILTER BY CONTENT TYPE: RETURNS FILTERED COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see
    ///   cref="DescendentTopicViewModel.Children"/> property can be filtered by <see cref="TopicViewModel.ContentType"/>.
    /// </summary>
    [TestMethod]
    public async Task Map_FilterByContentType_ReturnsFilteredCollection() {

      var topic                 = TopicFactory.Create("Test", "Descendent");
      var childTopic1           = TopicFactory.Create("ChildTopic1", "Descendent", topic);
      var childTopic2           = TopicFactory.Create("ChildTopic2", "DescendentSpecialized", topic);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "DescendentSpecialized", topic);
      var childTopic4           = TopicFactory.Create("ChildTopic4", "DescendentSpecialized", childTopic3);

      var target                = await _mappingService.MapAsync<DescendentTopicViewModel>(topic).ConfigureAwait(false);

      var specialized           = target.Children.GetByContentType("DescendentSpecialized");

      Assert.AreEqual<int>(2, specialized.Count);
      Assert.IsNotNull(GetChildTopic(specialized, "ChildTopic2"));
      Assert.IsNotNull(GetChildTopic(specialized, "ChildTopic3"));
      Assert.IsNull(GetChildTopic(specialized, "ChildTopic4"));

    }

    /*==========================================================================================================================
    | TEST: MAP: GETTER METHODS: MAP METHOD OUTPUT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a property corresponding to a getter method on <see cref="Topic"/> to ensure that it is
    ///   correctly populated.
    /// </summary>
    [TestMethod]
    public async Task Map_GetterMethods_MapMethodOutput() {

      var topic                 = TopicFactory.Create("Topic", "Sample");
      var childTopic            = TopicFactory.Create("Child", "Page", topic);
      var grandChildTopic       = TopicFactory.Create("GrandChild", "Index", childTopic);

      var target = await _mappingService.MapAsync<IndexTopicViewModel>(grandChildTopic).ConfigureAwait(false);

      Assert.AreEqual<string>("Topic:Child:GrandChild", target.UniqueKey);

    }

    /*==========================================================================================================================
    | TEST: MAP: COMPATIBLE PROPERTIES: MAP OBJECT REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps two properties with compatible types without attempting a conversion. This can be used for mapping types that are
    ///   appropriate for the target view model, such as enums.
    /// </summary>
    [TestMethod]
    public async Task Map_CompatibleProperties_MapObjectReference() {

      var topic                 = (TextAttribute)TopicFactory.Create("Attribute", "TextAttribute");

      topic.VersionHistory.Add(new(1976, 10, 15, 9, 30, 00));

      var target                = await _mappingService.MapAsync<CompatiblePropertyTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<ModelType>(topic.ModelType, target.ModelType);
      Assert.AreEqual<int>(1, target.VersionHistory.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: VALID REQUIRED PROPERTY: IS MAPPED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is not thrown if it is set.
    /// </summary>
    [TestMethod]
    public async Task Map_ValidRequiredProperty_IsMapped() {

      var topic                 = TopicFactory.Create("Topic", "Required");

      topic.Attributes.SetValue("RequiredAttribute", "Required");

      var target = await _mappingService.MapAsync<RequiredTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<string>("Required", target.RequiredAttribute);

    }

    /*==========================================================================================================================
    | TEST: MAP: INVALID REQUIRED PROPERTY: THROWS VALIDATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is thrown if it isn't set.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task Map_InvalidRequiredProperty_ThrowsValidationException() {

      var topic                 = TopicFactory.Create("Topic", "Required");

      var target = await _mappingService.MapAsync<RequiredTopicViewModel>(topic).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: INVALID REQUIRED OBJECT: THROWS VALIDATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is thrown if it isn't set.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task Map_InvalidRequiredObject_ThrowsValidationException() {

      var topic                 = TopicFactory.Create("Topic", "RequiredObject");

      var target                = await _mappingService.MapAsync<RequiredTopicViewModel>(topic).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: NULL PROPERTY: MAPS DEFAULT VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has default properties. Ensures that each is set appropriately.
    /// </summary>
    [TestMethod]
    public async Task Map_NullProperty_MapsDefaultValue() {

      var topic                 = TopicFactory.Create("Topic", "DefaultValue");

      var target                = await _mappingService.MapAsync<DefaultValueTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<string>("Default", target.DefaultString);
      Assert.AreEqual<int>(10, target.DefaultInt);
      Assert.IsTrue(target.DefaultBool);

    }

    /*==========================================================================================================================
    | TEST: MAP: EXCEEDS MINIMUM VALUE: THROWS VALIDATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has minimum value properties. Ensures that an error is thrown if the minimum is not met.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task Map_ExceedsMinimumValue_ThrowsValidationException() {

      var topic                 = TopicFactory.Create("Topic", "MinimumLengthProperty");

      topic.Attributes.SetValue("MinimumLength", "Hello World");

      var target = await _mappingService.MapAsync<MinimumLengthPropertyTopicViewModel>(topic).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: FILTER BY ATTRIBUTE: RETURNS FILTERED COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see
    ///   cref="FilteredTopicViewModel.Children"/> property can be filtered using stacked <see cref="FilterByAttributeAttribute"/>
    ///   instances.
    /// </summary>
    [TestMethod]
    public async Task Map_FilterByAttribute_ReturnsFilteredCollection() {

      var topic                 = TopicFactory.Create("Test", "Filtered");
      var childTopic1           = TopicFactory.Create("ChildTopic1", "Page", topic);
      var childTopic2           = TopicFactory.Create("ChildTopic2", "Index", topic);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "Page", topic);
      var childTopic4           = TopicFactory.Create("ChildTopic4", "Page", childTopic3);

      childTopic1.Attributes.SetValue("SomeAttribute", "ValueA");
      childTopic2.Attributes.SetValue("SomeAttribute", "ValueA");
      childTopic3.Attributes.SetValue("SomeAttribute", "ValueA");
      childTopic4.Attributes.SetValue("SomeAttribute", "ValueB");

      var target = await _mappingService.MapAsync<FilteredTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int>(2, target.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: FLATTEN ATTRIBUTE: RETURNS FLAT COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see
    ///   cref="FlattenChildrenTopicViewModel.Children"/> property is properly flattened.
    /// </summary>
    [TestMethod]
    public async Task Map_FlattenAttribute_ReturnsFlatCollection() {

      var topic                 = TopicFactory.Create("Test", "FlattenChildren");

      for (var i = 0; i < 5; i++) {
        var childTopic          = TopicFactory.Create("Child" + i, "Page", topic);
        for (var j = 0; j < 5; j++) {
          TopicFactory.Create("GrandChild" + i + j, "FlattenChildren", childTopic);
        }
      }

      var target = await _mappingService.MapAsync<FlattenChildrenTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int>(25, target.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: CACHED TOPIC: RETURNS CACHED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> as well as a <see cref="CachedTopicMappingService"/> and ensures that
    ///   the same instance of a mapped object is turned after two calls.
    /// </summary>
    [TestMethod]
    public async Task Map_CachedTopic_ReturnsCachedModel() {

      var cachedMappingService = new CachedTopicMappingService(_mappingService);

      var topic = TopicFactory.Create("Test", "Filtered", 5);

      var target1 = (FilteredTopicViewModel?)await cachedMappingService.MapAsync(topic).ConfigureAwait(false);
      var target2 = (FilteredTopicViewModel?)await cachedMappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.AreEqual<FilteredTopicViewModel>(target1, target2);

    }

    /*==========================================================================================================================
    | METHOD: GET CHILD TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A helper function which retrieves a child topic based on the key.
    /// </summary>
    public static KeyOnlyTopicViewModel GetChildTopic(IEnumerable<KeyOnlyTopicViewModel> topicCollection, string key)
      => topicCollection.FirstOrDefault((t) => t.Key.StartsWith(key, StringComparison.InvariantCulture));

    public static TopicViewModel GetChildTopic(IEnumerable<TopicViewModel> topicCollection, string key)
      => topicCollection.FirstOrDefault((t) => t.Key.StartsWith(key, StringComparison.InvariantCulture));

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Attributes;
using OnTopic.Data.Caching;
using OnTopic.Internal.Diagnostics;
using OnTopic.Lookup;
using OnTopic.Mapping;
using OnTopic.Mapping.Annotations;
using OnTopic.Mapping.Internal;
using OnTopic.Metadata;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.TestDoubles.Metadata;
using OnTopic.Tests.Entities;
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
  [ExcludeFromCodeCoverage]
  public class TopicMappingServiceTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITypeLookupService              _typeLookupService;
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

      /*------------------------------------------------------------------------------------------------------------------------
      | Create composite topic lookup service
      \-----------------------------------------------------------------------------------------------------------------------*/
      _typeLookupService        = new CompositeTypeLookupService(
        new TopicViewModelLookupService(),
        new FakeViewModelLookupService()
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Assemble dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = new CachedTopicRepository(new StubTopicRepository());
      _mappingService           = new TopicMappingService(new DummyTopicRepository(), _typeLookupService);

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

      var target                = await _mappingService.MapAsync<PageTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<string?>("ValueA", target?.MetaTitle);
      Assert.AreEqual<string?>("Value1", target?.Title);

    }

    /*==========================================================================================================================
    | TEST: MAP: GENERIC: RETURNS NEW RECORD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and confirms that a basic <c>record</c> type can be mapped by
    ///   explicitly setting defining the target type.
    /// </summary>
    [TestMethod]
    public async Task Map_Generic_ReturnsNewRecord() {

      var topic                 = TopicFactory.Create("Test", "Page");

      var target                = await _mappingService.MapAsync<RecordTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<string?>(topic.Key, target?.Key);

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

      var target                = (PageTopicViewModel?)await _mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.AreEqual<string?>("ValueA", target?.MetaTitle);
      Assert.AreEqual<string?>("Value1", target?.Title);

    }

    /*==========================================================================================================================
    | TEST: MAP: GENERIC: RETURNS CONVERTED PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and confirms that a basic source property of type string—e.g., <see
    ///   cref="CustomTopic.BooleanAsStringAttribute"/>—can successfully be converted to a target type of an integer—e.g., <see
    ///   cref="ConvertPropertyViewModel.BooleanAttribute"/>.
    /// </summary>
    [TestMethod]
    public async Task Map_Generic_ReturnsConvertedProperty() {

      var topic                 = new CustomTopic("Test", "CustomTopic");

      topic.NumericAttribute = 1;
      topic.BooleanAsStringAttribute = "1";

      var target                = await _mappingService.MapAsync<ConvertPropertyViewModel>(topic).ConfigureAwait(false);

      Assert.IsTrue(target?.BooleanAttribute);
      Assert.AreEqual<string?>(topic.NumericAttribute.ToString(CultureInfo.InvariantCulture), target?.NumericAsStringAttribute);

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
      Assert.AreEqual<string?>("Test", viewModel.Key);
      Assert.AreEqual<string?>("Parent", parentViewModel.Key);
      Assert.AreEqual<string?>("Grandparent", grandParentViewModel.Key);
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

      Assert.IsNull(viewModel?.Property);
      Assert.AreEqual<string?>("ValueB", viewModel?.InheritedProperty);

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

      Assert.IsNull(target?.NullableInteger);
      Assert.IsNull(target?.NullableBoolean);
      Assert.IsNull(target?.NullableDateTime);
      Assert.IsNull(target?.NullableUrl);

      //The following should not be null since they map to non-nullable properties which will have default values
      Assert.AreEqual(topic.Title, target?.Title);
      Assert.AreEqual<bool?>(topic.IsHidden, target?.IsHidden);
      Assert.AreEqual<DateTime?>(topic.LastModified, target?.LastModified);

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

      Assert.AreEqual<string?>("Hello World.", target?.NullableString);
      Assert.AreEqual<int?>(43, target?.NullableInteger);
      Assert.AreEqual<double?>(3.14159265359, target?.NullableDouble);
      Assert.AreEqual<bool?>(true, target?.NullableBoolean);
      Assert.AreEqual<DateTime?>(new(1976, 10, 15), target?.NullableDateTime);
      Assert.AreEqual<Uri?>(new("/Web/Path/File?Query=String", UriKind.RelativeOrAbsolute), target?.NullableUrl);

      Assert.AreEqual<string?>(topic.Title, target?.Title);
      Assert.AreEqual<bool?>(topic.IsHidden, target?.IsHidden);
      Assert.AreEqual<DateTime?>(topic.LastModified, target?.LastModified);

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

      Assert.AreEqual<string?>("ValueA", viewModel?.PropertyAlias);

    }

    /*==========================================================================================================================
    | TEST: MAPPED TOPIC CACHE ENTRY: GET MISSING ASSOCIATIONS: RETURNS DIFFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MappedTopicCacheEntry"/> with a set of <see cref="AssociationTypes"/>, and then confirms that
    ///   its <see cref="MappedTopicCacheEntry.GetMissingAssociations(AssociationTypes)"/> correctly returns the missing
    ///   associations.
    /// </summary>
    [TestMethod]
    public void MappedTopicCacheEntry_GetMissingAssociations_ReturnsDifference() {

      var cacheEntry            = new MappedTopicCacheEntry() {
        Associations            = AssociationTypes.Children | AssociationTypes.Parents
      };
      var associations          = AssociationTypes.Children | AssociationTypes.References;

      var difference            = cacheEntry.GetMissingAssociations(associations);

      cacheEntry.AddMissingAssociations(difference);

      Assert.IsTrue(difference.HasFlag(AssociationTypes.References));
      Assert.IsFalse(difference.HasFlag(AssociationTypes.Children));
      Assert.IsFalse(difference.HasFlag(AssociationTypes.Parents));

    }

    /*==========================================================================================================================
    | TEST: MAPPED TOPIC CACHE ENTRY: ADD MISSING ASSOCIATIONS: SETS UNION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MappedTopicCacheEntry"/> with a set of <see cref="AssociationTypes"/>, and then confirms that
    ///   its <see cref="MappedTopicCacheEntry.AddMissingAssociations(AssociationTypes)"/> correctly extends the missing
    ///   associations.
    /// </summary>
    [TestMethod]
    public void MappedTopicCacheEntry_AddMissingAssociations_SetsUnion() {

      var cacheEntry            = new MappedTopicCacheEntry() {
        Associations            = AssociationTypes.Children
      };
      var associations          = AssociationTypes.Children | AssociationTypes.Parents;

      cacheEntry.AddMissingAssociations(associations);

      Assert.AreEqual<AssociationTypes>(AssociationTypes.Children | AssociationTypes.Parents, cacheEntry.Associations);

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

      topic.Relationships.SetValue("Cousins", relatedTopic1);
      topic.Relationships.SetValue("Cousins", relatedTopic2);
      topic.Relationships.SetValue("Siblings", relatedTopic3);

      var target                = await _mappingService.MapAsync<RelationTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(2, target?.Cousins.Count);
      Assert.IsNotNull(GetChildTopic(target?.Cousins, "Cousin1"));
      Assert.IsNotNull(GetChildTopic(target?.Cousins, "Cousin2"));
      Assert.IsNull(GetChildTopic(target?.Cousins, "Sibling"));

    }

    /*==========================================================================================================================
    | TEST: MAP: RELATIONSHIPS: SKIPS DISABLED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully skips disabled.
    /// </summary>
    [TestMethod]
    public async Task Map_Relationships_SkipsDisabled() {

      var relatedTopic1         = TopicFactory.Create("Cousin1", "Relation");
      var relatedTopic2         = TopicFactory.Create("Cousin2", "Relation");
      var topic                 = TopicFactory.Create("Test", "Relation");

      topic.Relationships.SetValue("Cousins", relatedTopic1);
      topic.Relationships.SetValue("Cousins", relatedTopic2);

      topic.IsDisabled          = true;
      relatedTopic2.IsDisabled  = true;

      var target                = await _mappingService.MapAsync<RelationTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(1, target?.Cousins.Count);
      Assert.IsNotNull(GetChildTopic(target?.Cousins, "Cousin1"));
      Assert.IsNull(GetChildTopic(target?.Cousins, "Cousin2"));

    }

    /*==========================================================================================================================
    | TEST: MAP: ALTERNATE RELATIONSHIP: RETURNS CORRECT RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully derives values from the key and
    ///   type specified by <see cref="Mapping.Annotations.CollectionAttribute"/>.
    /// </summary>
    /// <remarks>
    ///   The <see cref="AmbiguousRelationTopicViewModel.RelationshipAlias"/> uses <see cref="Mapping.Annotations.
    ///   CollectionAttribute"/> to set the relationship key to <c>AmbiguousRelationship</c> and the <see cref="CollectionType"
    ///   /> to <see cref="CollectionType.IncomingRelationship"/>. <c>AmbiguousRelationship</c> refers to a relationship that is
    ///   both outgoing and incoming. It should be smart enough to a) look for the <c>AmbigousRelationship</c> instead of the
    ///   <c>RelationshipAlias</c>, and b) source from the <see cref="Topic.IncomingRelationships"/> collection.
    /// </remarks>
    [TestMethod]
    public async Task Map_AlternateRelationship_ReturnsCorrectRelationship() {

      var outgoingRelation      = TopicFactory.Create("OutgoingRelation", "KeyOnly");
      var incomingRelation      = TopicFactory.Create("IncomingRelation", "KeyOnly");
      var ambiguousRelation     = TopicFactory.Create("AmbiguousRelation", "KeyOnly");

      var topic                 = TopicFactory.Create("Test", "AmbiguousRelation");

      //Set outgoing relationships
      topic.Relationships.SetValue("RelationshipAlias", ambiguousRelation);
      topic.Relationships.SetValue("AmbiguousRelationship", outgoingRelation);

      //Set incoming relationships
      ambiguousRelation.Relationships.SetValue("RelationshipAlias", topic);
      incomingRelation.Relationships.SetValue("AmbiguousRelationship", topic);

      var target = await _mappingService.MapAsync<AmbiguousRelationTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(1, target?.RelationshipAlias.Count);
      Assert.IsNotNull(GetChildTopic(target?.RelationshipAlias, "IncomingRelation"));

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

      Assert.AreEqual<int?>(8, target?.AttributeDescriptors.Count);
      Assert.AreEqual<int?>(2, target?.PermittedContentTypes.Count);

      //Ensure custom collections are not recursively followed without instruction
      Assert.AreEqual<int?>(0, target?.PermittedContentTypes.FirstOrDefault()?.PermittedContentTypes.Count?? 0);

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
      var topicList             = TopicFactory.Create("Categories", "List", topic);
      _                         = TopicFactory.Create("ChildTopic", "KeyOnly", topic);
      _                         = TopicFactory.Create("NestedTopic1", "KeyOnly", topicList);
      _                         = TopicFactory.Create("NestedTopic2", "KeyOnly", topicList);

      topicList.IsHidden        = true;

      var target                = await _mappingService.MapAsync<NestedTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(2, target?.Categories.Count);

      Assert.IsNotNull(GetChildTopic(target?.Categories, "NestedTopic1"));
      Assert.IsNotNull(GetChildTopic(target?.Categories, "NestedTopic2"));
      Assert.IsNull(GetChildTopic(target?.Categories, "Categories"));
      Assert.IsNull(GetChildTopic(target?.Categories, "ChildTopic"));

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
      _                         = TopicFactory.Create("ChildTopic1", "Descendent", topic);
      _                         = TopicFactory.Create("ChildTopic2", "Descendent", topic);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "Descendent", topic);
      var childTopic4           = TopicFactory.Create("ChildTopic4", "DescendentSpecialized", topic);
      _                         = TopicFactory.Create("invalidChildTopic", "KeyOnly", topic);
      _                         = TopicFactory.Create("GrandchildTopic", "Descendent", childTopic3);

      childTopic4.Attributes.SetBoolean("IsLeaf", true);

      var target                = await _mappingService.MapAsync<DescendentTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(4, target?.Children.Count);
      Assert.IsNotNull(GetChildTopic(target?.Children, "ChildTopic1"));
      Assert.IsNotNull(GetChildTopic(target?.Children, "ChildTopic2"));
      Assert.IsNotNull(GetChildTopic(target?.Children, "ChildTopic3"));
      Assert.IsNotNull(GetChildTopic(target?.Children, "ChildTopic4"));
      Assert.IsTrue(((DescendentSpecializedTopicViewModel?)GetChildTopic(target?.Children, "ChildTopic4"))?.IsLeaf);
      Assert.IsNull(GetChildTopic(target?.Children, "invalidChildTopic"));
      Assert.IsNull(GetChildTopic(target?.Children, "GrandchildTopic"));
      Assert.IsNotNull(GetChildTopic(
        ((DescendentTopicViewModel?)GetChildTopic(target?.Children, "ChildTopic3"))?.Children,
        "GrandchildTopic"
      ));
    }

    /*==========================================================================================================================
    | TEST: MAP: WITH DISABLED: SKIPS DISABLED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> with children and tests whether it successfully skips disabled child
    ///   topics.
    /// </summary>
    [TestMethod]
    public async Task Map_Children_SkipsDisabled() {

      var topic                 = TopicFactory.Create("Test", "Descendent");
      _                         = TopicFactory.Create("ChildTopic1", "Descendent", topic);
      _                         = TopicFactory.Create("ChildTopic2", "Descendent", topic);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "Descendent", topic);

      topic.IsDisabled          = true;
      childTopic3.IsDisabled    = true;

      var target                = await _mappingService.MapAsync<DescendentTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(2, target?.Children.Count);
      Assert.IsNotNull(GetChildTopic(target?.Children, "ChildTopic1"));
      Assert.IsNotNull(GetChildTopic(target?.Children, "ChildTopic2"));
      Assert.IsNull(GetChildTopic(target?.Children, "ChildTopic3"));

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

      Assert.AreEqual<string?>("Test", target?.Primary?.Key);
      Assert.AreEqual<string?>("Aliased Key", target?.Alternate?.Key);
      Assert.AreEqual<string?>("Ancillary Key", target?.Ancillary?.Key);

    }

    /*==========================================================================================================================
    | TEST: MAP: TOPIC REFERENCES AS ATTRIBUTE: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully maps referenced topics stored in
    ///   <see cref="Topic.Attributes"/>.
    /// </summary>
    [TestMethod]
    public async Task Map_TopicReferencesAsAttribute_ReturnsMappedModel() {

      var mappingService        = new TopicMappingService(_topicRepository, _typeLookupService);
      var topicReference        = _topicRepository.Load(11111);

      Contract.Assume(topicReference);

      var topic                 = TopicFactory.Create("Test", "TopicReference");

      topic.Attributes.SetInteger("TopicReferenceId", topicReference.Id);

      var target                = (TopicReferenceTopicViewModel?)await mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.IsNotNull(target?.TopicReference);
      Assert.AreEqual<string?>(topicReference.Key, target?.TopicReference.Key);

    }

    /*==========================================================================================================================
    | TEST: MAP: TOPIC REFERENCES: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully maps referenced topics.
    /// </summary>
    [TestMethod]
    public async Task Map_TopicReferences_ReturnsMappedModel() {

      var mappingService        = new TopicMappingService(_topicRepository, _typeLookupService);
      var topicReference        = _topicRepository.Load(11111);

      var topic                 = TopicFactory.Create("Test", "TopicReference");

      topic.References.SetValue("TopicReference", topicReference);

      var target                = (TopicReferenceTopicViewModel?)await mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.IsNotNull(target?.TopicReference);
      Assert.AreEqual<string?>(topicReference?.Key, target?.TopicReference.Key);

    }

    /*==========================================================================================================================
    | TEST: MAP: TOPIC REFERENCES: SKIPS DISABLED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully skips disabled topics.
    /// </summary>
    [TestMethod]
    public async Task Map_TopicReferences_SkipsDisabled() {

      var mappingService        = new TopicMappingService(_topicRepository, _typeLookupService);

      var topic                 = TopicFactory.Create("Test", "TopicReference");
      var topicReference        = TopicFactory.Create("Reference", "Page");

      topicReference.IsDisabled = true;

      topic.References.SetValue("TopicReference", topicReference);

      var target                = (TopicReferenceTopicViewModel?)await mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.IsNull(target?.TopicReference);

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
      _                         = TopicFactory.Create("ChildTopic1", "RelationWithChildren", cousinTopic3);
      _                         = TopicFactory.Create("ChildTopic2", "RelationWithChildren", cousinTopic3);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "RelationWithChildren", cousinTopic3);

      //Other cousins
      var secondCousin          = TopicFactory.Create("SecondCousin", "Relation");
      _                         = TopicFactory.Create("CousinOnceRemoved", "Relation", childTopic3);

      //Set first cousins
      topic.Relationships.SetValue("Cousins", cousinTopic1);
      topic.Relationships.SetValue("Cousins", cousinTopic2);
      topic.Relationships.SetValue("Cousins", cousinTopic3);

      //Set ancillary relationships
      cousinTopic3.Relationships.SetValue("Cousins", secondCousin);

      var target                = await _mappingService.MapAsync<RelationTopicViewModel>(topic).ConfigureAwait(false);

      var cousinTarget          = GetChildTopic(target?.Cousins, "CousinTopic3") as RelationWithChildrenTopicViewModel;
      var distantCousinTarget   = GetChildTopic(cousinTarget?.Children, "ChildTopic3") as RelationWithChildrenTopicViewModel;

      //Because Cousins is set to recurse over Children, its children should be set
      Assert.AreEqual<int?>(3, cousinTarget?.Children.Count);

      //Because Cousins is not set to recurse over Cousins, its cousins should NOT be set (even though there is one cousin)
      Assert.AreEqual<int?>(0, cousinTarget?.Cousins.Count);

      //Because Children is not set to recurse over Children, the grandchildren of a cousin should NOT be set
      Assert.AreEqual<int?>(0, distantCousinTarget?.Children.Count);

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
      _                         = TopicFactory.Create("ChildTopic1", "Slide", slides);
      _                         = TopicFactory.Create("ChildTopic2", "Slide", slides);
      _                         = TopicFactory.Create("ChildTopic3", "Slide", slides);
      _                         = TopicFactory.Create("ChildTopic4", "ContentItem", slides);

      var target                = await _mappingService.MapAsync<SlideshowTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(4, target?.ContentItems.Count);
      Assert.IsNotNull(GetChildTopic(target?.ContentItems, "ChildTopic1"));
      Assert.IsNotNull(GetChildTopic(target?.ContentItems, "ChildTopic2"));
      Assert.IsNotNull(GetChildTopic(target?.ContentItems, "ChildTopic3"));
      Assert.IsNotNull(GetChildTopic(target?.ContentItems, "ChildTopic4"));

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

      topic.Relationships.SetValue("RelatedTopics", relatedTopic1);
      topic.Relationships.SetValue("RelatedTopics", relatedTopic2);
      topic.Relationships.SetValue("RelatedTopics", relatedTopic3);

      var target                = await _mappingService.MapAsync<RelatedEntityTopicViewModel>(topic).ConfigureAwait(false);
      var relatedTopic3copy     = (getRelatedTopic(target, "RelatedTopic3"));

      Assert.AreEqual<int?>(3, target?.RelatedTopics.Count);

      Assert.IsNotNull(getRelatedTopic(target, "RelatedTopic1"));
      Assert.IsNotNull(getRelatedTopic(target, "RelatedTopic2"));
      Assert.IsNotNull(getRelatedTopic(target, "RelatedTopic3"));

      Assert.AreEqual(relatedTopic3.Key, relatedTopic3copy?.Key);

      Topic? getRelatedTopic(RelatedEntityTopicViewModel? topic, string key)
        => topic?.RelatedTopics.FirstOrDefault((t) => t.Key.StartsWith(key, StringComparison.Ordinal));

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

      var mappingService        = new TopicMappingService(_topicRepository, _typeLookupService);
      var topic                 = TopicFactory.Create("Test", "MetadataLookup");

      var target                = (MetadataLookupTopicViewModel?)await mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(5, target?.Categories.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: CACHED TOPIC: RETURNS SAME REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and binds two properties to the same association, each with the same
    ///   <c>[Include()]</c> attribute. Ensures that the result of each property are the same, and only map the specified
    ///   association.
    /// </summary>
    [TestMethod]
    public async Task Map_CachedTopic_ReturnsSameReference() {

      var topic                 = TopicFactory.Create("Test", "Redundant", 1);
      var child                 = TopicFactory.Create("ChildTopic", "RedundantItem", topic, 2);

      child.References.SetValue("Reference", topic);

      var mappedTopic           = await _mappingService.MapAsync<RedundantTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<TopicAssociationsViewModel?>(mappedTopic?.FirstItem, mappedTopic?.SecondItem);
      Assert.AreEqual<TopicViewModel?>(mappedTopic?.FirstItem?.Parent, mappedTopic?.SecondItem?.Parent);
      Assert.IsNull(mappedTopic?.FirstItem?.Reference);
      Assert.IsNull(mappedTopic?.SecondItem?.Reference);

    }

    /*==========================================================================================================================
    | TEST: MAP: CACHED TOPIC: RETURNS PROGRESSIVE REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and binds two properties to the same association, each with a
    ///   different <c>[Include()]</c> attribute. Ensures that the result of each property contains both assocations.
    /// </summary>
    [TestMethod]
    public async Task Map_CachedTopic_ReturnsProgressiveReference() {

      var topic                 = TopicFactory.Create("Test", "Progressive", 1);
      var child                 = TopicFactory.Create("ChildTopic", "RedundantItem", topic, 2);

      child.References.SetValue("Reference", topic);

      var mappedTopic           = await _mappingService.MapAsync<ProgressiveTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<TopicAssociationsViewModel?>(mappedTopic?.FirstItem, mappedTopic?.SecondItem);
      Assert.AreEqual<TopicViewModel?>(mappedTopic?.FirstItem?.Parent, mappedTopic?.SecondItem?.Reference);

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
      _                         = TopicFactory.Create("ChildTopic", "Circular", topic, 2);

      var mappedTopic           = await _mappingService.MapAsync<CircularTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<CircularTopicViewModel?>(mappedTopic, mappedTopic?.Children.First().Parent);

    }

    /*==========================================================================================================================
    | TEST: MAP: FILTER BY COLLECTION TYPE: RETURNS FILTERED COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see
    ///   cref="DescendentTopicViewModel.Children"/> property can be filtered by <see cref="TopicViewModel.ContentType"/>.
    /// </summary>
    [TestMethod]
    public async Task Map_FilterByCollectionType_ReturnsFilteredCollection() {

      var topic                 = TopicFactory.Create("Test", "Descendent");
      _                         = TopicFactory.Create("ChildTopic1", "Descendent", topic);
      _                         = TopicFactory.Create("ChildTopic2", "DescendentSpecialized", topic);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "DescendentSpecialized", topic);
      _                         = TopicFactory.Create("ChildTopic4", "DescendentSpecialized", childTopic3);

      var target                = await _mappingService.MapAsync<DescendentTopicViewModel>(topic).ConfigureAwait(false);

      var specialized           = target?.Children.GetByContentType("DescendentSpecialized");

      Assert.AreEqual<int?>(2, specialized?.Count);
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

      Assert.AreEqual<string?>("Topic:Child:GrandChild", target?.UniqueKey);

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

      var topic                 = (TextAttributeDescriptor)TopicFactory.Create("Attribute", "TextAttributeDescriptor");

      topic.VersionHistory.Add(new(1976, 10, 15, 9, 30, 00));

      var target                = await _mappingService.MapAsync<CompatiblePropertyTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<ModelType?>(topic.ModelType, target?.ModelType);
      Assert.AreEqual<int?>(1, target?.VersionHistory?.Count);

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

      Assert.AreEqual<string?>("Required", target?.RequiredAttribute);

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

      await _mappingService.MapAsync<RequiredTopicViewModel>(topic).ConfigureAwait(false);

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

      await _mappingService.MapAsync<RequiredTopicViewModel>(topic).ConfigureAwait(false);

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

      Assert.AreEqual<string?>("Default", target?.DefaultString);
      Assert.AreEqual<int?>(10, target?.DefaultInt);
      Assert.IsTrue(target?.DefaultBool);

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

      await _mappingService.MapAsync<MinimumLengthPropertyTopicViewModel>(topic).ConfigureAwait(false);

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
      childTopic4.Attributes.SetValue("SomeAttribute", "ValueA");

      childTopic1.Attributes.SetValue("SomeOtherAttribute", "ValueB");
      childTopic2.Attributes.SetValue("SomeOtherAttribute", "ValueB");
      childTopic3.Attributes.SetValue("SomeOtherAttribute", "ValueA");
      childTopic4.Attributes.SetValue("SomeOtherAttribute", "ValueA");


      var target = await _mappingService.MapAsync<FilteredTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(2, target?.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: FILTER BY INVALID ATTRIBUTE: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to map a view model that has an invalid <see cref="FilterByAttributeAttribute.Key"/> value of <c>ContentType
    ///   </c>; throws an <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task Map_FilterByInvalidAttribute_ThrowsExceptions() {

      var topic                 = TopicFactory.Create("Test", "FilteredInvalid");

      var target = await _mappingService.MapAsync<FilteredInvalidTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(2, target?.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: FILTER BY CONTENT TYPE: RETURNS FILTERED COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see
    ///   cref="FilteredTopicViewModel.Children"/> property can be filtered using a <see cref="FilterByContentTypeAttribute"/>
    ///   instances.
    /// </summary>
    [TestMethod]
    public async Task Map_FilterByContentType_ReturnsFilteredCollection() {

      var topic                 = TopicFactory.Create("Test", "Filtered");
      _                         = TopicFactory.Create("ChildTopic1", "Page", topic);
      _                         = TopicFactory.Create("ChildTopic2", "Index", topic);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "Page", topic);
      _                         = TopicFactory.Create("ChildTopic4", "Page", childTopic3);

      var target = await _mappingService.MapAsync<FilteredContentTypeTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(2, target?.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: FLATTEN ATTRIBUTE: RETURNS FLAT COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see cref="
    ///   FlattenChildrenTopicViewModel.Children"/> property is properly flattened.
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

      Assert.AreEqual<int?>(25, target?.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: FLATTEN ATTRIBUTE: EXCLUDE TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see cref="
    ///   FlattenChildrenTopicViewModel.Children"/> property excludes any <see cref="Topic.IsDisabled"/> or nested topics.
    /// </summary>
    [TestMethod]
    public async Task Map_FlattenAttribute_ExcludeTopics() {

      var topic                 = TopicFactory.Create("Test", "FlattenChildren");
      var childTopic            = TopicFactory.Create("Child", "FlattenChildren", topic);
      var grandChildTopic       = TopicFactory.Create("Grandchild", "FlattenChildren", childTopic);
      var listTopic             = TopicFactory.Create("List", "List", childTopic);
      _                         = TopicFactory.Create("Nested", "FlattenChildren", listTopic);

      grandChildTopic.IsDisabled = true;

      var target = await _mappingService.MapAsync<FlattenChildrenTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(1, target?.Children.Count);

    }
    /*==========================================================================================================================
    | TEST: MAP: CACHED TOPIC: RETURNS CACHED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> as well as a <see cref="CachedTopicMappingService"/> and ensures that
    ///   the same instance of a mapped object is returned after two calls.
    /// </summary>
    [TestMethod]
    public async Task Map_CachedTopic_ReturnsCachedModel() {

      var cachedMappingService = new CachedTopicMappingService(_mappingService);

      var topic = TopicFactory.Create("Test", "Filtered", 5);

      var target1 = (FilteredTopicViewModel?)await cachedMappingService.MapAsync(topic).ConfigureAwait(false);
      var target2 = (FilteredTopicViewModel?)await cachedMappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.AreEqual<FilteredTopicViewModel?>(target1, target2);

    }

    /*==========================================================================================================================
    | TEST: MAP: CACHED TOPIC: RETURNS UNIQUE REFERENCE PER TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> as well as a <see cref="CachedTopicMappingService"/> and ensures that
    ///   a different instance of a mapped object is returned after two calls, assuming the types are different.
    /// </summary>
    [TestMethod]
    public async Task Map_CachedTopic_ReturnsUniqueReferencePerType() {

      var cachedMappingService = new CachedTopicMappingService(_mappingService);

      var topic = TopicFactory.Create("Test", "Filtered", 5);

      var target1 = (FilteredTopicViewModel?)await cachedMappingService.MapAsync<FilteredTopicViewModel>(topic).ConfigureAwait(false);
      var target2 = (FilteredTopicViewModel?)await cachedMappingService.MapAsync<FilteredTopicViewModel>(topic).ConfigureAwait(false);
      var target3 = (TopicViewModel?)await cachedMappingService.MapAsync<PageTopicViewModel>(topic).ConfigureAwait(false);

      Assert.AreEqual<FilteredTopicViewModel?>(target1, target2);
      Assert.AreNotEqual(target1, target3);

    }

    /*==========================================================================================================================
    | METHOD: GET CHILD TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A helper function which retrieves a child topic based on the key.
    /// </summary>
    public static KeyOnlyTopicViewModel? GetChildTopic(IEnumerable<KeyOnlyTopicViewModel>? topicCollection, string key)
      => topicCollection?.FirstOrDefault((t) => t.Key?.StartsWith(key, StringComparison.Ordinal)?? false);

    public static TopicViewModel? GetChildTopic(IEnumerable<TopicViewModel>? topicCollection, string key)
      => topicCollection?.FirstOrDefault((t) => t.Key.StartsWith(key, StringComparison.Ordinal));

  } //Class
} //Namespace
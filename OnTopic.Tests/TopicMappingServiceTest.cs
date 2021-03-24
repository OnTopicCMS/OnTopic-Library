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
using OnTopic.Attributes;
using OnTopic.Data.Caching;
using OnTopic.Internal.Diagnostics;
using OnTopic.Mapping;
using OnTopic.Mapping.Annotations;
using OnTopic.Mapping.Internal;
using OnTopic.Metadata;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.TestDoubles.Metadata;
using OnTopic.Tests.Entities;
using OnTopic.Tests.Fixtures;
using OnTopic.Tests.ViewModels;
using OnTopic.Tests.ViewModels.Metadata;
using OnTopic.ViewModels;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC MAPPING SERVICE TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicMappingService"/> using local DTOs.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [Xunit.Collection("Shared Repository")]
  public class TopicMappingServiceTest: IClassFixture<TopicInfrastructureFixture<StubTopicRepository>> {

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
    public TopicMappingServiceTest(TopicInfrastructureFixture<StubTopicRepository> fixture) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(fixture, nameof(fixture));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set services
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = fixture.CachedTopicRepository;
      _mappingService           = fixture.MappingService;

    }

    /*==========================================================================================================================
    | TEST: MAP: GENERIC: RETURNS NEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests setting basic scalar values by specifying an explicit type.
    /// </summary>
    [Fact]
    public async Task Map_Generic_ReturnsNewModel() {

      var topic                 = new Topic("Test", "Page");

      topic.Attributes.SetValue("MetaTitle", "ValueA");
      topic.Attributes.SetValue("Title", "Value1");

      var target                = await _mappingService.MapAsync<PageTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal("ValueA", target?.MetaTitle);
      Assert.Equal("Value1", target?.Title);

    }

    /*==========================================================================================================================
    | TEST: MAP: GENERIC: RETURNS NEW RECORD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and confirms that a basic <c>record</c> type can be mapped by
    ///   explicitly setting defining the target type.
    /// </summary>
    [Fact]
    public async Task Map_Generic_ReturnsNewRecord() {

      var topic                 = new Topic("Test", "Page");

      var target                = await _mappingService.MapAsync<RecordTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal(topic.Key, target?.Key);

    }

    /*==========================================================================================================================
    | TEST: MAP: DYNAMIC: RETURNS NEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests setting basic scalar values by allowing it to dynamically
    ///   determine the instance type.
    /// </summary>
    [Fact]
    public async Task Map_Dynamic_ReturnsNewModel() {

      var topic                 = new Topic("Test", "Page");

      topic.Attributes.SetValue("MetaTitle", "ValueA");
      topic.Attributes.SetValue("Title", "Value1");

      var target                = (PageTopicViewModel?)await _mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.Equal("ValueA", target?.MetaTitle);
      Assert.Equal("Value1", target?.Title);

    }

    /*==========================================================================================================================
    | TEST: MAP: GENERIC: RETURNS CONVERTED PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and confirms that a basic source property of type string—e.g., <see
    ///   cref="CustomTopic.BooleanAsStringAttribute"/>—can successfully be converted to a target type of an integer—e.g., <see
    ///   cref="ConvertPropertyViewModel.BooleanAttribute"/>.
    /// </summary>
    [Fact]
    public async Task Map_Generic_ReturnsConvertedProperty() {

      var topic                 = new CustomTopic("Test", "CustomTopic") {
        NumericAttribute        = 1,
        BooleanAsStringAttribute = "1"
      };

      var target                = await _mappingService.MapAsync<ConvertPropertyViewModel>(topic).ConfigureAwait(false);

      Assert.True(target?.BooleanAttribute);
      Assert.Equal(topic.NumericAttribute.ToString(CultureInfo.InvariantCulture), target?.NumericAsStringAttribute);

    }

    /*==========================================================================================================================
    | TEST: MAP: CONSTRUCTOR: RETURNS NEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and attempts to map a view model with a constructor containing a
    ///   scalar value, topic reference, relationship, and an optional parameter. Confirms that the expected model is returned.
    /// </summary>
    [Fact]
    public async Task Map_Constructor_ReturnsNewModel() {

      var topic                 = new Topic("Topic", "Constructed", null, 1);
      var related1              = new Topic("Related1", "Constructed", null, 2);
      var related2              = new Topic("Related2", "Constructed", null, 3);
      var related3              = new Topic("Related3", "Constructed", null, 4);

      topic.Attributes.SetValue("Value", "Foo");
      topic.Attributes.SetValue("ScalarValue", "Invalid");
      topic.Attributes.SetValue("OptionalValue", "3");
      topic.References.SetValue("TopicReference", related1);
      topic.Relationships.SetValue("Related", related2);
      topic.Relationships.SetValue("Relationships", related2); //Should not be mapped
      topic.Relationships.SetValue("Relationships", related3); //Should not be mapped

      related1.Attributes.SetValue("Value", "Bar");
      related1.References.SetValue("TopicReference", related3);

      related3.Attributes.SetValue("Value", "Baz");

      var target                = await _mappingService.MapAsync<ConstructedTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal("Foo", target?.ScalarValue);
      Assert.NotNull(target?.TopicReference);
      Assert.NotNull(target?.Relationships);
      Assert.Equal<int?>(5, target?.OptionalValue);
      Assert.Single(target?.Relationships);

      Assert.Equal("Bar", target?.TopicReference?.ScalarValue);

      Assert.NotNull(target?.TopicReference?.TopicReference);
      Assert.Equal("Baz", target?.TopicReference!.TopicReference!.ScalarValue);

      Assert.Null(target?.TopicReference!.TopicReference!.TopicReference);

    }

    /*==========================================================================================================================
    | TEST: MAP: CONSTRUCTOR: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and attempts to map a view model with a constructor containing a
    ///   circular reference, and confirms that a <see cref="TopicMappingException"/> is correctly thrown.
    /// </summary>
    [Fact]
    public async Task Map_Constructor_ThrowsException() {

      var topic                 = new Topic("Topic", "Constructed", null, 1);
      var related               = new Topic("Related", "Constructed", null, 2);

      topic.References.SetValue("TopicReference", related);
      related.References.SetValue("TopicReference", topic);

      await Assert.ThrowsAsync<TopicMappingException>(async () =>
        await _mappingService.MapAsync<ConstructedTopicViewModel>(topic).ConfigureAwait(false)
      ).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: DISABLED PROPERTY: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it maps a property decorated with the <see
    ///   cref="DisableMappingAttribute"/>.
    /// </summary>
    [Fact]
    public async Task Map_DisabledProperty_ReturnsNull() {

      var topic                 = new Topic("Test", "DisableMapping");

      var viewModel             = await _mappingService.MapAsync<DisableMappingTopicViewModel>(topic).ConfigureAwait(false);

      Assert.NotNull(viewModel);
      Assert.Null(viewModel?.Key);

    }

    /*==========================================================================================================================
    | TEST: MAP: PARENTS: RETURNS ASCENDENTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the parent tree.
    /// </summary>
    [Fact]
    public async Task Map_Parents_ReturnsAscendents() {

      var grandParent           = new Topic("Grandparent", "AscendentSpecialized");
      var parent                = new Topic("Parent", "Ascendent", grandParent);
      var topic                 = new Topic("Test", "Ascendent", parent);

      grandParent.Attributes.SetValue("IsRoot", "1");

      var viewModel             = await _mappingService.MapAsync<AscendentTopicViewModel>(topic).ConfigureAwait(false);
      var parentViewModel       = viewModel?.Parent;
      var grandParentViewModel  = parentViewModel?.Parent as AscendentSpecializedTopicViewModel;

      Assert.NotNull(viewModel);
      Assert.NotNull(parentViewModel);
      Assert.NotNull(grandParentViewModel);
      Assert.Equal("Test", viewModel?.Key);
      Assert.Equal("Parent", parentViewModel?.Key);
      Assert.Equal("Grandparent", grandParentViewModel?.Key);
      Assert.True(grandParentViewModel?.IsRoot);
      Assert.Null(grandParentViewModel?.Parent);

    }

    /*==========================================================================================================================
    | TEST: MAP: INHERIT ATTRIBUTE: INHERITS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully inherits values as specified by
    ///   <see cref="InheritAttribute"/>.
    /// </summary>
    [Fact]
    public async Task Map_InheritAttribute_InheritsValue() {

      var grandParent           = new Topic("Grandparent", "Page");
      var parent                = new Topic("Parent", "Page", grandParent);
      var topic                 = new Topic("Test", "InheritedProperty", parent);

      grandParent.Attributes.SetValue("Property", "ValueA");
      grandParent.Attributes.SetValue("InheritedProperty", "ValueB");

      var viewModel             = await _mappingService.MapAsync<InheritedPropertyTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Null(viewModel?.Property);
      Assert.Equal("ValueB", viewModel?.InheritedProperty);

    }

    /*==========================================================================================================================
    | TEST: MAP: NULLABLE PROPERTIES WITH INVALID VALUES: RETURNS NULL OR DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests setting nullable scalar values with invalid values with the
    ///   expectation that they will be returned as null.
    /// </summary>
    [Fact]
    public async Task Map_NullablePropertiesWithInvalidValues_ReturnsNullOrDefault() {

      var topic                 = new Topic("Test", "Page");

      topic.Attributes.SetValue("NullableInteger", "A");
      topic.Attributes.SetValue("NullableBoolean", "43");
      topic.Attributes.SetValue("NullableDateTime", "Hello World");
      topic.Attributes.SetValue("NullableUrl", "invalid://Web\\Path\\File!?@Query=String?");

      var target                = await _mappingService.MapAsync<NullablePropertyTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Null(target?.NullableInteger);
      Assert.Null(target?.NullableBoolean);
      Assert.Null(target?.NullableDateTime);
      Assert.Null(target?.NullableUrl);

      //The following should not be null since they map to non-nullable properties which will have default values
      Assert.Equal(topic.Title, target?.Title);
      Assert.Equal<bool?>(topic.IsHidden, target?.IsHidden);
      Assert.Equal<DateTime?>(topic.LastModified, target?.LastModified);

    }

    /*==========================================================================================================================
    | TEST: MAP: NULLABLE PROPERTIES WITH VALID VALUES: RETURNS VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests setting nullable scalar values with valid values with the
    ///   expectation that they will be mapped correctly.
    /// </summary>
    [Fact]
    public async Task Map_NullablePropertiesWithInvalidValues_ReturnsValues() {

      var topic                 = new Topic("Test", "Page");

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

      Assert.Equal("Hello World.", target?.NullableString);
      Assert.Equal<int?>(43, target?.NullableInteger);
      Assert.Equal<double?>(3.14159265359, target?.NullableDouble);
      Assert.Equal<bool?>(true, target?.NullableBoolean);
      Assert.Equal<DateTime?>(new(1976, 10, 15), target?.NullableDateTime);
      Assert.Equal<Uri?>(new("/Web/Path/File?Query=String", UriKind.RelativeOrAbsolute), target?.NullableUrl);

      Assert.Equal(topic.Title, target?.Title);
      Assert.Equal<bool?>(topic.IsHidden, target?.IsHidden);
      Assert.Equal<DateTime?>(topic.LastModified, target?.LastModified);

    }

    /*==========================================================================================================================
    | TEST: MAP: ALTERNATE ATTRIBUTE KEY: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully derives values from the key
    ///   specified by <see cref="AttributeKeyAttribute"/>.
    /// </summary>
    [Fact]
    public async Task Map_AlternateAttributeKey_ReturnsMappedModel() {

      var topic                 = new Topic("Test", "PropertyAlias");

      topic.Attributes.SetValue("Property", "ValueA");
      topic.Attributes.SetValue("PropertyAlias", "ValueB");

      var viewModel             = await _mappingService.MapAsync<PropertyAliasTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal("ValueA", viewModel?.PropertyAlias);

    }

    /*==========================================================================================================================
    | TEST: MAPPED TOPIC CACHE: TRY GET VALUE: RETURNS ENTRY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MappedTopicCacheEntry"/> and then confirms that it is returned via <see cref="
    ///   MappedTopicCache.TryGetValue(Int32, Type, out MappedTopicCacheEntry)"/>.
    /// </summary>
    [Fact]
    public void MappedTopicCache_TryGetValue_ReturnsEntry() {

      var cache                 = new MappedTopicCache();
      var topicId               = 1;
      var viewModel             = new EmptyViewModel();

      cache.Register(topicId, AssociationTypes.None, viewModel);

      var isSuccess             = cache.TryGetValue(topicId, viewModel.GetType(), out var result);

      Assert.True(isSuccess);
      Assert.Equal<object?>(viewModel, result?.MappedTopic);

    }

    /*==========================================================================================================================
    | TEST: MAPPED TOPIC CACHE: TRY GET VALUE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MappedTopicCache"/> and then confirms that it is not returned via <see cref="MappedTopicCache
    ///   .TryGetValue(Int32, Type, out MappedTopicCacheEntry)"/> if the <see cref="Type"/> doesn't match.
    /// </summary>
    [Fact]
    public void MappedTopicCache_TryGetValue_ReturnsNull() {

      var cache                 = new MappedTopicCache();
      var topicId               = 1;

      cache.Register(topicId, AssociationTypes.None, new EmptyViewModel());

      var isSuccess             = cache.TryGetValue(topicId, typeof(TopicViewModel), out var result);

      Assert.False(isSuccess);
      Assert.Null(result);

    }

    /*==========================================================================================================================
    | TEST: MAPPED TOPIC CACHE: GET OR ADD: RETURNS EXISTING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MappedTopicCache"/> and then confirms that the existing entry is returned when calling <see
    ///   cref="MappedTopicCache.Register(Int32, AssociationTypes, Object)"/> with duplicate parameters.
    /// </summary>
    [Fact]
    public void MappedTopicCache_GetOrAdd_ReturnsExisting() {

      var cache                 = new MappedTopicCache();
      var initialViewModel      = new EmptyViewModel();
      var newViewModel          = new EmptyViewModel();

      cache.Register(1, AssociationTypes.None, initialViewModel);
      cache.Register(1, AssociationTypes.None, newViewModel);

      var isSuccess             = cache.TryGetValue(1, newViewModel.GetType(), out var result);

      Assert.True(isSuccess);
      Assert.Equal<object?>(initialViewModel, result?.MappedTopic);

    }

    /*==========================================================================================================================
    | TEST: MAPPED TOPIC CACHE: GET OR ADD: NEW TOPIC: IS NOT CACHED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MappedTopicCache"/> and then confirms that entries are <i>not</i> added when using <see cref=
    ///   "MappedTopicCache.Register(Int32, AssociationTypes, Object)"/> doesn't add entries with <see cref="Topic.IsNew"/>.
    /// </summary>
    [Fact]
    public void MappedTopicCache_GetOrAdd_NewTopic_IsNotCached() {

      var cache                 = new MappedTopicCache();

      cache.Register(-1, AssociationTypes.None, new EmptyViewModel());

      var isSuccess             = cache.TryGetValue(-1, typeof(EmptyViewModel), out var result);

      Assert.False(isSuccess);
      Assert.Null(result);

    }

    /*==========================================================================================================================
    | TEST: MAPPED TOPIC CACHE: GET OR ADD: OBJECT: IS NOT CACHED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MappedTopicCache"/> and then confirms that entries are <i>not</i> added when using <see cref=
    ///   "MappedTopicCache.Register(Int32, AssociationTypes, Object)"/> doesn't add entries with view models of type <see cref=
    ///   "Object"/>.
    /// </summary>
    [Fact]
    public void MappedTopicCache_GetOrAdd_IsNotCached() {

      var cache                 = new MappedTopicCache();

      cache.Register(1, AssociationTypes.None, new object());

      var isSuccess             = cache.TryGetValue(-1, typeof(object), out var result);

      Assert.False(isSuccess);
      Assert.Null(result);
      Assert.Null(result);

    }

    /*==========================================================================================================================
    | TEST: MAPPED TOPIC CACHE ENTRY: GET MISSING ASSOCIATIONS: RETURNS DIFFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MappedTopicCacheEntry"/> with a set of <see cref="AssociationTypes"/>, and then confirms that
    ///   its <see cref="MappedTopicCacheEntry.GetMissingAssociations(AssociationTypes)"/> correctly returns the missing
    ///   associations.
    /// </summary>
    [Fact]
    public void MappedTopicCacheEntry_GetMissingAssociations_ReturnsDifference() {

      var cacheEntry            = new MappedTopicCacheEntry() {
        Associations            = AssociationTypes.Children | AssociationTypes.Parents
      };
      var associations          = AssociationTypes.Children | AssociationTypes.References;

      var difference            = cacheEntry.GetMissingAssociations(associations);

      cacheEntry.AddMissingAssociations(difference);

      Assert.True(difference.HasFlag(AssociationTypes.References));
      Assert.False(difference.HasFlag(AssociationTypes.Children));
      Assert.False(difference.HasFlag(AssociationTypes.Parents));

    }

    /*==========================================================================================================================
    | TEST: MAPPED TOPIC CACHE ENTRY: ADD MISSING ASSOCIATIONS: SETS UNION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MappedTopicCacheEntry"/> with a set of <see cref="AssociationTypes"/>, and then confirms that
    ///   its <see cref="MappedTopicCacheEntry.AddMissingAssociations(AssociationTypes)"/> correctly extends the missing
    ///   associations.
    /// </summary>
    [Fact]
    public void MappedTopicCacheEntry_AddMissingAssociations_SetsUnion() {

      var cacheEntry            = new MappedTopicCacheEntry() {
        Associations            = AssociationTypes.Children
      };
      var associations          = AssociationTypes.Children | AssociationTypes.Parents;

      cacheEntry.AddMissingAssociations(associations);

      Assert.Equal<AssociationTypes>(AssociationTypes.Children | AssociationTypes.Parents, cacheEntry.Associations);

    }

    /*==========================================================================================================================
    | TEST: MAP: RELATIONSHIPS: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the relationships.
    /// </summary>
    [Fact]
    public async Task Map_Relationships_ReturnsMappedModel() {

      var relatedTopic1         = new Topic("Cousin1", "Relation");
      var relatedTopic2         = new Topic("Cousin2", "Relation");
      var relatedTopic3         = new Topic("Sibling", "Relation");
      var topic                 = new Topic("Test", "Relation");

      topic.Relationships.SetValue("Cousins", relatedTopic1);
      topic.Relationships.SetValue("Cousins", relatedTopic2);
      topic.Relationships.SetValue("Siblings", relatedTopic3);

      var target                = await _mappingService.MapAsync<RelationTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<int?>(2, target?.Cousins.Count);
      Assert.NotNull(GetChildTopic(target?.Cousins, "Cousin1"));
      Assert.NotNull(GetChildTopic(target?.Cousins, "Cousin2"));
      Assert.Null(GetChildTopic(target?.Cousins, "Sibling"));

    }

    /*==========================================================================================================================
    | TEST: MAP: RELATIONSHIPS: SKIPS DISABLED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully skips disabled.
    /// </summary>
    [Fact]
    public async Task Map_Relationships_SkipsDisabled() {

      var relatedTopic1         = new Topic("Cousin1", "Relation");
      var relatedTopic2         = new Topic("Cousin2", "Relation");
      var topic                 = new Topic("Test", "Relation");

      topic.Relationships.SetValue("Cousins", relatedTopic1);
      topic.Relationships.SetValue("Cousins", relatedTopic2);

      topic.IsDisabled          = true;
      relatedTopic2.IsDisabled  = true;

      var target                = await _mappingService.MapAsync<RelationTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Single(target?.Cousins);
      Assert.NotNull(GetChildTopic(target?.Cousins, "Cousin1"));
      Assert.Null(GetChildTopic(target?.Cousins, "Cousin2"));

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
    [Fact]
    public async Task Map_AlternateRelationship_ReturnsCorrectRelationship() {

      var outgoingRelation      = new Topic("OutgoingRelation", "KeyOnly");
      var incomingRelation      = new Topic("IncomingRelation", "KeyOnly");
      var ambiguousRelation     = new Topic("AmbiguousRelation", "KeyOnly");

      var topic                 = new Topic("Test", "AmbiguousRelation");

      //Set outgoing relationships
      topic.Relationships.SetValue("RelationshipAlias", ambiguousRelation);
      topic.Relationships.SetValue("AmbiguousRelationship", outgoingRelation);

      //Set incoming relationships
      ambiguousRelation.Relationships.SetValue("RelationshipAlias", topic);
      incomingRelation.Relationships.SetValue("AmbiguousRelationship", topic);

      var target = await _mappingService.MapAsync<AmbiguousRelationTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Single(target?.RelationshipAlias);
      Assert.NotNull(GetChildTopic(target?.RelationshipAlias, "IncomingRelation"));

    }

    /*==========================================================================================================================
    | TEST: MAP: CUSTOM COLLECTION: RETURNS COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully derives values from a custom source
    ///   collection compatible with <see cref="IList{T}"/>, where <c>{T}</c> is a <see cref="Topic"/>, or derivative.
    /// </summary>
    [Fact]
    public async Task Map_CustomCollection_ReturnsCollection() {

      var topic                 = _topicRepository.Load("Root:Configuration:ContentTypes:Page");
      var target                = await _mappingService.MapAsync<ContentTypeDescriptorTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<int?>(8, target?.AttributeDescriptors.Count);
      Assert.Equal<int?>(2, target?.PermittedContentTypes.Count);

      //Ensure custom collections are not recursively followed without instruction
      Assert.Empty(target?.PermittedContentTypes.FirstOrDefault()?.PermittedContentTypes);

    }

    /*==========================================================================================================================
    | TEST: MAP: NESTED TOPICS: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the nested topics.
    /// </summary>
    [Fact]
    public async Task Map_NestedTopics_ReturnsMappedModel() {

      var topic                 = new Topic("Test", "Nested");
      var topicList             = new Topic("Categories", "List", topic);
      _                         = new Topic("ChildTopic", "KeyOnly", topic);
      _                         = new Topic("NestedTopic1", "KeyOnly", topicList);
      _                         = new Topic("NestedTopic2", "KeyOnly", topicList);

      topicList.IsHidden        = true;

      var target                = await _mappingService.MapAsync<NestedTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<int?>(2, target?.Categories.Count);

      Assert.NotNull(GetChildTopic(target?.Categories, "NestedTopic1"));
      Assert.NotNull(GetChildTopic(target?.Categories, "NestedTopic2"));
      Assert.Null(GetChildTopic(target?.Categories, "Categories"));
      Assert.Null(GetChildTopic(target?.Categories, "ChildTopic"));

    }

    /*==========================================================================================================================
    | TEST: MAP: CHILDREN: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls child topics.
    /// </summary>
    [Fact]
    public async Task Map_Children_ReturnsMappedModel() {

      var topic                 = new Topic("Test", "Descendent");
      _                         = new Topic("ChildTopic1", "Descendent", topic);
      _                         = new Topic("ChildTopic2", "Descendent", topic);
      var childTopic3           = new Topic("ChildTopic3", "Descendent", topic);
      var childTopic4           = new Topic("ChildTopic4", "DescendentSpecialized", topic);
      _                         = new Topic("invalidChildTopic", "KeyOnly", topic);
      _                         = new Topic("GrandchildTopic", "Descendent", childTopic3);

      childTopic4.Attributes.SetBoolean("IsLeaf", true);

      var target                = await _mappingService.MapAsync<DescendentTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<int?>(4, target?.Children.Count);
      Assert.NotNull(GetChildTopic(target?.Children, "ChildTopic1"));
      Assert.NotNull(GetChildTopic(target?.Children, "ChildTopic2"));
      Assert.NotNull(GetChildTopic(target?.Children, "ChildTopic3"));
      Assert.NotNull(GetChildTopic(target?.Children, "ChildTopic4"));
      Assert.True(((DescendentSpecializedTopicViewModel?)GetChildTopic(target?.Children, "ChildTopic4"))?.IsLeaf);
      Assert.Null(GetChildTopic(target?.Children, "invalidChildTopic"));
      Assert.Null(GetChildTopic(target?.Children, "GrandchildTopic"));
      Assert.NotNull(GetChildTopic(
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
    [Fact]
    public async Task Map_Children_SkipsDisabled() {

      var topic                 = new Topic("Test", "Descendent");
      _                         = new Topic("ChildTopic1", "Descendent", topic);
      _                         = new Topic("ChildTopic2", "Descendent", topic);
      var childTopic3           = new Topic("ChildTopic3", "Descendent", topic);

      topic.IsDisabled          = true;
      childTopic3.IsDisabled    = true;

      var target                = await _mappingService.MapAsync<DescendentTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<int?>(2, target?.Children.Count);
      Assert.NotNull(GetChildTopic(target?.Children, "ChildTopic1"));
      Assert.NotNull(GetChildTopic(target?.Children, "ChildTopic2"));
      Assert.Null(GetChildTopic(target?.Children, "ChildTopic3"));

    }

    /*==========================================================================================================================
    | TEST: MAP: MAP TO PARENT: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's nested complex objects are
    ///   property mapped with attribute values from the parent, based on their <see cref="MapToParentAttribute"/>
    ///   configuration.
    /// </summary>
    [Fact]
    public async Task Map_MapToParent_ReturnsMappedModel() {

      var topic                 = new Topic("Test", "FlattenChildren");

      topic.Attributes.SetValue("PrimaryKey", "Primary Key");
      topic.Attributes.SetValue("AlternateKey", "Alternate Key");
      topic.Attributes.SetValue("AncillaryKey", "Ancillary Key");
      topic.Attributes.SetValue("AliasedKey", "Aliased Key");

      var target = await _mappingService.MapAsync<MapToParentTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal("Test", target?.Primary?.Key);
      Assert.Equal("Aliased Key", target?.Alternate?.Key);
      Assert.Equal("Ancillary Key", target?.Ancillary?.Key);

    }

    /*==========================================================================================================================
    | TEST: MAP: MAP AS: RETURNS TOPIC REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully maps a topic reference using the
    ///   type specified in the <see cref="MapAsAttribute"/>.
    /// </summary>
    [Fact]
    public async Task Map_MapAs_ReturnsTopicReference() {

      var topicReference        = _topicRepository.Load(11111);

      Contract.Assume(topicReference);

      var topic                 = new Topic("Test", "MapAs");

      topic.References.SetValue("TopicReference", topicReference);

      var target = (MapAsTopicViewModel?)await _mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.NotNull(target?.TopicReference);
      Assert.Equal<Type?>(typeof(AscendentTopicViewModel), target?.TopicReference?.GetType());

    }

    /*==========================================================================================================================
    | TEST: MAP: MAP AS: RETURNS RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully maps a relationship collection using
    ///   the type specified in the <see cref="MapAsAttribute"/>.
    /// </summary>
    [Fact]
    public async Task Map_MapAs_ReturnsRelationships() {

      var relatedTopic          = _topicRepository.Load(11111);

      Contract.Assume(relatedTopic);

      var topic                 = new Topic("Test", "MapAs");

      topic.Relationships.SetValue("Relationships", relatedTopic);

      var target = (MapAsTopicViewModel?)await _mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.Single(target?.Relationships);
      Assert.Equal<Type?>(typeof(AscendentTopicViewModel), target?.Relationships.FirstOrDefault()?.GetType());

    }

    /*==========================================================================================================================
    | TEST: MAP: TOPIC REFERENCES AS ATTRIBUTE: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully maps referenced topics stored in
    ///   <see cref="Topic.Attributes"/>.
    /// </summary>
    [Fact]
    public async Task Map_TopicReferencesAsAttribute_ReturnsMappedModel() {

      var topicReference        = _topicRepository.Load(11111);

      Contract.Assume(topicReference);

      var topic                 = new Topic("Test", "TopicReference");

      topic.Attributes.SetInteger("TopicReferenceId", topicReference.Id);

      var target                = (TopicReferenceTopicViewModel?)await _mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.NotNull(target?.TopicReference);
      Assert.Equal(topicReference.Key, target?.TopicReference?.Key);

    }

    /*==========================================================================================================================
    | TEST: MAP: TOPIC REFERENCES: RETURNS MAPPED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully maps referenced topics.
    /// </summary>
    [Fact]
    public async Task Map_TopicReferences_ReturnsMappedModel() {

      var topicReference        = _topicRepository.Load(11111);

      var topic                 = new Topic("Test", "TopicReference");

      topic.References.SetValue("TopicReference", topicReference);

      var target                = (TopicReferenceTopicViewModel?)await _mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.NotNull(target?.TopicReference);
      Assert.Equal(topicReference?.Key, target?.TopicReference?.Key);

    }

    /*==========================================================================================================================
    | TEST: MAP: TOPIC REFERENCES: SKIPS DISABLED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully skips disabled topics.
    /// </summary>
    [Fact]
    public async Task Map_TopicReferences_SkipsDisabled() {

      var topic                 = new Topic("Test", "TopicReference");
      var topicReference        = new Topic("Reference", "Page") {
        IsDisabled              = true
      };

      topic.References.SetValue("TopicReference", topicReference);

      var target                = (TopicReferenceTopicViewModel?)await _mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.Null(target?.TopicReference);

    }

    /*==========================================================================================================================
    | TEST: MAP: RECURSIVE RELATIONSHIPS: RETURNS GRAPH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully follows relationships per the
    ///   instructions of each model class.
    /// </summary>
    [Fact]
    public async Task Map_RecursiveRelationships_ReturnsGraph() {

      //Self
      var topic                 = new Topic("Test", "Relation");

      //First cousins
      var cousinTopic1          = new Topic("CousinTopic1", "Relation");
      var cousinTopic2          = new Topic("CousinTopic2", "Relation");
      var cousinTopic3          = new Topic("CousinTopic3", "RelationWithChildren");

      //First cousins once removed
      _                         = new Topic("ChildTopic1", "RelationWithChildren", cousinTopic3);
      _                         = new Topic("ChildTopic2", "RelationWithChildren", cousinTopic3);
      var childTopic3           = new Topic("ChildTopic3", "RelationWithChildren", cousinTopic3);

      //Other cousins
      var secondCousin          = new Topic("SecondCousin", "Relation");
      _                         = new Topic("CousinOnceRemoved", "Relation", childTopic3);

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
      Assert.Equal<int?>(3, cousinTarget?.Children.Count);

      //Because Cousins is not set to recurse over Cousins, its cousins should NOT be set (even though there is one cousin)
      Assert.Empty(cousinTarget?.Cousins);

      //Because Children is not set to recurse over Children, the grandchildren of a cousin should NOT be set
      Assert.Empty(distantCousinTarget?.Children);

    }

    /*==========================================================================================================================
    | TEST: MAP: SLIDE SHOW: RETURNS DERIVED VIEW MODELS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls a <see
    ///   cref="SlideshowTopicViewModel"/>, even though the <see cref="ContentListTopicViewModel.ContentItems"/> list is a
    ///   collection of <see cref="ContentItemTopicViewModel"/> objects (from which <see cref="SlideTopicViewModel"/>.
    /// </summary>
    [Fact]
    public async Task Map_Slideshow_ReturnsDerivedViewModels() {

      var topic                 = new Topic("Test", "Slideshow");
      var slides                = new Topic("ContentItems", "List", topic);
      _                         = new Topic("ChildTopic1", "Slide", slides);
      _                         = new Topic("ChildTopic2", "Slide", slides);
      _                         = new Topic("ChildTopic3", "Slide", slides);
      _                         = new Topic("ChildTopic4", "ContentItem", slides);

      var target                = await _mappingService.MapAsync<SlideshowTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<int?>(4, target?.ContentItems.Count);
      Assert.NotNull(GetChildTopic(target?.ContentItems, "ChildTopic1"));
      Assert.NotNull(GetChildTopic(target?.ContentItems, "ChildTopic2"));
      Assert.NotNull(GetChildTopic(target?.ContentItems, "ChildTopic3"));
      Assert.NotNull(GetChildTopic(target?.ContentItems, "ChildTopic4"));

    }

    /*==========================================================================================================================
    | TEST: MAP: TOPIC ENTITIES: RETURNS TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully includes <see cref="Topic"/>
    ///   instances if called for by the model. This isn't a best practice, but is maintained for edge cases.
    /// </summary>
    [Fact]
    public async Task Map_TopicEntities_ReturnsTopics() {

      var relatedTopic1         = new Topic("RelatedTopic1", "KeyOnly");
      var relatedTopic2         = new Topic("RelatedTopic2", "KeyOnly");
      var relatedTopic3         = new Topic("RelatedTopic3", "KeyOnly");
      var topic                 = new Topic("Test", "RelatedEntity");

      topic.Relationships.SetValue("RelatedTopics", relatedTopic1);
      topic.Relationships.SetValue("RelatedTopics", relatedTopic2);
      topic.Relationships.SetValue("RelatedTopics", relatedTopic3);

      var target                = await _mappingService.MapAsync<RelatedEntityTopicViewModel>(topic).ConfigureAwait(false);
      var relatedTopic3copy     = (getRelatedTopic(target, "RelatedTopic3"));

      Assert.Equal<int?>(3, target?.RelatedTopics.Count);

      Assert.NotNull(getRelatedTopic(target, "RelatedTopic1"));
      Assert.NotNull(getRelatedTopic(target, "RelatedTopic2"));
      Assert.NotNull(getRelatedTopic(target, "RelatedTopic3"));

      Assert.Equal(relatedTopic3.Key, relatedTopic3copy?.Key);

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
    [Fact]
    public async Task Map_MetadataLookup_ReturnsLookupItems() {

      var topic                 = new Topic("Test", "MetadataLookup");

      var target                = (MetadataLookupTopicViewModel?)await _mappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.Equal<int?>(5, target?.Categories.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: CACHED TOPIC: RETURNS SAME REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and binds two properties to the same association, each with the same
    ///   <c>[Include()]</c> attribute. Ensures that the result of each property are the same, and only map the specified
    ///   association.
    /// </summary>
    [Fact]
    public async Task Map_CachedTopic_ReturnsSameReference() {

      var topic                 = new Topic("Test", "Redundant", null, 1);
      var child                 = new Topic("ChildTopic", "RedundantItem", topic, 2);

      child.References.SetValue("Reference", topic);

      var mappedTopic           = await _mappingService.MapAsync<RedundantTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<TopicAssociationsViewModel?>(mappedTopic?.FirstItem, mappedTopic?.SecondItem);
      Assert.Equal<TopicViewModel?>(mappedTopic?.FirstItem?.Parent, mappedTopic?.SecondItem?.Parent);
      Assert.Null(mappedTopic?.FirstItem?.Reference);
      Assert.Null(mappedTopic?.SecondItem?.Reference);

    }

    /*==========================================================================================================================
    | TEST: MAP: CACHED TOPIC: RETURNS PROGRESSIVE REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and binds two properties to the same association, each with a
    ///   different <c>[Include()]</c> attribute. Ensures that the result of each property contains both assocations.
    /// </summary>
    [Fact]
    public async Task Map_CachedTopic_ReturnsProgressiveReference() {

      var topic                 = new Topic("Test", "Progressive", null, 1);
      var child                 = new Topic("ChildTopic", "RedundantItem", topic, 2);

      child.References.SetValue("Reference", topic);

      var mappedTopic           = await _mappingService.MapAsync<ProgressiveTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<TopicAssociationsViewModel?>(mappedTopic?.FirstItem, mappedTopic?.SecondItem);
      Assert.Equal<TopicViewModel?>(mappedTopic?.FirstItem?.Parent, mappedTopic?.SecondItem?.Reference);

    }

    /*==========================================================================================================================
    | TEST: MAP: CIRCULAR REFERENCE: RETURNS MAPPED PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully handles a circular reference by
    ///   taking advantage of its internal caching mechanism.
    /// </summary>
    [Fact]
    public async Task Map_CircularReference_ReturnsCachedParent() {

      var topic                 = new Topic("Test", "Circular", null, 1);
      _                         = new Topic("ChildTopic", "Circular", topic, 2);

      var mappedTopic           = await _mappingService.MapAsync<CircularTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<CircularTopicViewModel?>(mappedTopic, mappedTopic?.Children.First().Parent);

    }

    /*==========================================================================================================================
    | TEST: MAP: FILTER BY COLLECTION TYPE: RETURNS FILTERED COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see
    ///   cref="DescendentTopicViewModel.Children"/> property can be filtered by <see cref="TopicViewModel.ContentType"/>.
    /// </summary>
    [Fact]
    public async Task Map_FilterByCollectionType_ReturnsFilteredCollection() {

      var topic                 = new Topic("Test", "Descendent");
      _                         = new Topic("ChildTopic1", "Descendent", topic);
      _                         = new Topic("ChildTopic2", "DescendentSpecialized", topic);
      var childTopic3           = new Topic("ChildTopic3", "DescendentSpecialized", topic);
      _                         = new Topic("ChildTopic4", "DescendentSpecialized", childTopic3);

      var target                = await _mappingService.MapAsync<DescendentTopicViewModel>(topic).ConfigureAwait(false);

      var specialized           = target?.Children.GetByContentType("DescendentSpecialized");

      Assert.Equal<int?>(2, specialized?.Count);
      Assert.NotNull(GetChildTopic(specialized, "ChildTopic2"));
      Assert.NotNull(GetChildTopic(specialized, "ChildTopic3"));
      Assert.Null(GetChildTopic(specialized, "ChildTopic4"));

    }

    /*==========================================================================================================================
    | TEST: MAP: GETTER METHODS: MAP METHOD OUTPUT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a property corresponding to a getter method on <see cref="Topic"/> to ensure that it is
    ///   correctly populated.
    /// </summary>
    [Fact]
    public async Task Map_GetterMethods_MapMethodOutput() {

      var topic                 = new Topic("Topic", "Sample");
      var childTopic            = new Topic("Child", "Page", topic);
      var grandChildTopic       = new Topic("GrandChild", "Index", childTopic);

      var target = await _mappingService.MapAsync<IndexTopicViewModel>(grandChildTopic).ConfigureAwait(false);

      Assert.Equal("Topic:Child:GrandChild", target?.UniqueKey);

    }

    /*==========================================================================================================================
    | TEST: MAP: COMPATIBLE PROPERTIES: MAP OBJECT REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps two properties with compatible types without attempting a conversion. This can be used for mapping types that are
    ///   appropriate for the target view model, such as enums.
    /// </summary>
    [Fact]
    public async Task Map_CompatibleProperties_MapObjectReference() {

      var topic                 = new TextAttributeDescriptor("Attribute", "TextAttributeDescriptor");

      topic.VersionHistory.Add(new(1976, 10, 15, 9, 30, 00));

      var target                = await _mappingService.MapAsync<CompatiblePropertyTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<ModelType?>(topic.ModelType, target?.ModelType);
      Assert.Single(target?.VersionHistory);

    }

    /*==========================================================================================================================
    | TEST: MAP: VALID REQUIRED PROPERTY: IS MAPPED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is not thrown if it is set.
    /// </summary>
    [Fact]
    public async Task Map_ValidRequiredProperty_IsMapped() {

      var topic                 = new Topic("Topic", "Required");

      topic.Attributes.SetValue("RequiredAttribute", "Required");

      var target = await _mappingService.MapAsync<RequiredTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal("Required", target?.RequiredAttribute);

    }

    /*==========================================================================================================================
    | TEST: MAP: INVALID REQUIRED PROPERTY: THROWS VALIDATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is thrown if it isn't set.
    /// </summary>
    [Fact]
    public async Task Map_InvalidRequiredProperty_ThrowsValidationException() {

      var topic                 = new Topic("Topic", "Required");

      await Assert.ThrowsAsync<ValidationException>(async () =>
        await _mappingService.MapAsync<RequiredTopicViewModel>(topic).ConfigureAwait(false)
      ).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: INVALID REQUIRED OBJECT: THROWS VALIDATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is thrown if it isn't set.
    /// </summary>
    [Fact]
    public async Task Map_InvalidRequiredObject_ThrowsValidationException() {

      var topic                 = new Topic("Topic", "RequiredObject");

      await Assert.ThrowsAsync<ValidationException>(async () =>
        await _mappingService.MapAsync<RequiredTopicViewModel>(topic).ConfigureAwait(false)
      ).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: NULL PROPERTY: MAPS DEFAULT VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has default properties. Ensures that each is set appropriately.
    /// </summary>
    [Fact]
    public async Task Map_NullProperty_MapsDefaultValue() {

      var topic                 = new Topic("Topic", "DefaultValue");

      var target                = await _mappingService.MapAsync<DefaultValueTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal("Default", target?.DefaultString);
      Assert.Equal<int?>(10, target?.DefaultInt);
      Assert.True(target?.DefaultBool);

    }

    /*==========================================================================================================================
    | TEST: MAP: EXCEEDS MINIMUM VALUE: THROWS VALIDATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has minimum value properties. Ensures that an error is thrown if the minimum is not met.
    /// </summary>
    [Fact]
    public async Task Map_ExceedsMinimumValue_ThrowsValidationException() {

      var topic                 = new Topic("Topic", "MinimumLengthProperty");

      topic.Attributes.SetValue("MinimumLength", "Hello World");

      await Assert.ThrowsAsync<ValidationException>(async () =>
        await _mappingService.MapAsync<MinimumLengthPropertyTopicViewModel>(topic).ConfigureAwait(false)
      ).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: FILTER BY ATTRIBUTE: RETURNS FILTERED COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see
    ///   cref="FilteredTopicViewModel.Children"/> property can be filtered using stacked <see cref="FilterByAttributeAttribute"/>
    ///   instances.
    /// </summary>
    [Fact]
    public async Task Map_FilterByAttribute_ReturnsFilteredCollection() {

      var topic                 = new Topic("Test", "Filtered");
      var childTopic1           = new Topic("ChildTopic1", "Page", topic);
      var childTopic2           = new Topic("ChildTopic2", "Index", topic);
      var childTopic3           = new Topic("ChildTopic3", "Page", topic);
      var childTopic4           = new Topic("ChildTopic4", "Page", childTopic3);

      childTopic1.Attributes.SetValue("SomeAttribute", "ValueA");
      childTopic2.Attributes.SetValue("SomeAttribute", "ValueA");
      childTopic3.Attributes.SetValue("SomeAttribute", "ValueA");
      childTopic4.Attributes.SetValue("SomeAttribute", "ValueA");

      childTopic1.Attributes.SetValue("SomeOtherAttribute", "ValueB");
      childTopic2.Attributes.SetValue("SomeOtherAttribute", "ValueB");
      childTopic3.Attributes.SetValue("SomeOtherAttribute", "ValueA");
      childTopic4.Attributes.SetValue("SomeOtherAttribute", "ValueA");


      var target = await _mappingService.MapAsync<FilteredTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<int?>(2, target?.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: FILTER BY INVALID ATTRIBUTE: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to map a view model that has an invalid <see cref="FilterByAttributeAttribute.Key"/> value of <c>ContentType
    ///   </c>; throws an <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public async Task Map_FilterByInvalidAttribute_ThrowsExceptions() {

      var topic                 = new Topic("Test", "FilteredInvalid");

      await Assert.ThrowsAsync<ArgumentException>(async () =>
        await _mappingService.MapAsync<FilteredInvalidTopicViewModel>(topic).ConfigureAwait(false)
      ).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: FILTER BY CONTENT TYPE: RETURNS FILTERED COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see
    ///   cref="FilteredTopicViewModel.Children"/> property can be filtered using a <see cref="FilterByContentTypeAttribute"/>
    ///   instances.
    /// </summary>
    [Fact]
    public async Task Map_FilterByContentType_ReturnsFilteredCollection() {

      var topic                 = new Topic("Test", "Filtered");
      _                         = new Topic("ChildTopic1", "Page", topic);
      _                         = new Topic("ChildTopic2", "Index", topic);
      var childTopic3           = new Topic("ChildTopic3", "Page", topic);
      _                         = new Topic("ChildTopic4", "Page", childTopic3);

      var target = await _mappingService.MapAsync<FilteredContentTypeTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<int?>(2, target?.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: FLATTEN ATTRIBUTE: RETURNS FLAT COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see cref="
    ///   FlattenChildrenTopicViewModel.Children"/> property is properly flattened.
    /// </summary>
    [Fact]
    public async Task Map_FlattenAttribute_ReturnsFlatCollection() {

      var topic                 = new Topic("Test", "FlattenChildren");

      for (var i = 0; i < 5; i++) {
        var childTopic          = new Topic("Child" + i, "Page", topic);
        for (var j = 0; j < 5; j++) {
          _ = new Topic("GrandChild" + i + j, "FlattenChildren", childTopic);
        }
      }

      var target = await _mappingService.MapAsync<FlattenChildrenTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<int?>(25, target?.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP: FLATTEN ATTRIBUTE: EXCLUDE TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see cref="
    ///   FlattenChildrenTopicViewModel.Children"/> property excludes any <see cref="Topic.IsDisabled"/> or nested topics.
    /// </summary>
    [Fact]
    public async Task Map_FlattenAttribute_ExcludeTopics() {

      var topic                 = new Topic("Test", "FlattenChildren");
      var childTopic            = new Topic("Child", "FlattenChildren", topic);
      var grandChildTopic       = new Topic("Grandchild", "FlattenChildren", childTopic);
      var listTopic             = new Topic("List", "List", childTopic);
      _                         = new Topic("Nested", "FlattenChildren", listTopic);

      grandChildTopic.IsDisabled = true;

      var target = await _mappingService.MapAsync<FlattenChildrenTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Single(target?.Children);

    }
    /*==========================================================================================================================
    | TEST: MAP: CACHED TOPIC: RETURNS CACHED MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> as well as a <see cref="CachedTopicMappingService"/> and ensures that
    ///   the same instance of a mapped object is returned after two calls.
    /// </summary>
    [Fact]
    public async Task Map_CachedTopic_ReturnsCachedModel() {

      var cachedMappingService = new CachedTopicMappingService(_mappingService);

      var topic = new Topic("Test", "Filtered", null, 5);

      var target1 = (FilteredTopicViewModel?)await cachedMappingService.MapAsync(topic).ConfigureAwait(false);
      var target2 = (FilteredTopicViewModel?)await cachedMappingService.MapAsync(topic).ConfigureAwait(false);

      Assert.Equal<FilteredTopicViewModel?>(target1, target2);

    }

    /*==========================================================================================================================
    | TEST: MAP: CACHED TOPIC: RETURNS UNIQUE REFERENCE PER TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> as well as a <see cref="CachedTopicMappingService"/> and ensures that
    ///   a different instance of a mapped object is returned after two calls, assuming the types are different.
    /// </summary>
    [Fact]
    public async Task Map_CachedTopic_ReturnsUniqueReferencePerType() {

      var cachedMappingService = new CachedTopicMappingService(_mappingService);

      var topic = new Topic("Test", "Filtered", null, 5);

      var target1 = (FilteredTopicViewModel?)await cachedMappingService.MapAsync<FilteredTopicViewModel>(topic).ConfigureAwait(false);
      var target2 = (FilteredTopicViewModel?)await cachedMappingService.MapAsync<FilteredTopicViewModel>(topic).ConfigureAwait(false);
      var target3 = (TopicViewModel?)await cachedMappingService.MapAsync<PageTopicViewModel>(topic).ConfigureAwait(false);

      Assert.Equal<FilteredTopicViewModel?>(target1, target2);
      Assert.NotEqual<object?>(target1, target3);

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
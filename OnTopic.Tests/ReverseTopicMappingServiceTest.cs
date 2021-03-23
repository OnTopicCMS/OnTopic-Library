/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using OnTopic.Data.Caching;
using OnTopic.Internal.Diagnostics;
using OnTopic.Mapping;
using OnTopic.Mapping.Annotations;
using OnTopic.Mapping.Reverse;
using OnTopic.Metadata;
using OnTopic.Models;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.TestDoubles.Metadata;
using OnTopic.Tests.BindingModels;
using OnTopic.ViewModels;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: REVERSE TOPIC MAPPING SERVICE TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="ReverseTopicMappingService"/> using local DTOs.
  /// </summary>
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class ReverseTopicMappingServiceTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ReverseTopicMappingServiceTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="StubTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph.
    /// </remarks>
    public ReverseTopicMappingServiceTest() {
      _topicRepository = new CachedTopicRepository(new StubTopicRepository());
    }

    /*==========================================================================================================================
    | TEST: MAP: GENERIC: RETURNS NEW TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests setting basic scalar values by specifying an explicit
    ///   type.
    /// </summary>
    [TestMethod]
    public async Task Map_Generic_ReturnsNewTopic() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new TextAttributeTopicBindingModel("Test") {
        ContentType             = "TextAttributeDescriptor",
        Title                   = "Test Attribute",
        DefaultValue            = "Hello",
        IsRequired              = true
      };

      var target                = await mappingService.MapAsync<TextAttributeDescriptor>(bindingModel).ConfigureAwait(false);

      Assert.AreEqual<string?>("Test", target?.Key);
      Assert.AreEqual<string?>("TextAttributeDescriptor", target?.ContentType);
      Assert.AreEqual<string?>("Test Attribute", target?.Title);
      Assert.AreEqual<string?>("Hello", target?.DefaultValue);
      Assert.AreEqual<bool?>(true, target?.IsRequired);

    }

    /*==========================================================================================================================
    | TEST: MAP: DYNAMIC: RETURNS NEW TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests setting basic scalar values by allowing it to
    ///   dynamically determine the instance type.
    /// </summary>
    [TestMethod]
    public async Task Map_Dynamic_ReturnsNewTopic() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new TextAttributeTopicBindingModel("Test") {
        ContentType             = "TextAttributeDescriptor",
        Title                   = "Test Attribute",
        DefaultValue            = "Hello",
        IsRequired              = true
      };

      var target                = (TextAttributeDescriptor?)await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

      Assert.IsNotNull(target);
      Assert.AreEqual<string?>("Test", target?.Key);
      Assert.AreEqual<string?>("TextAttributeDescriptor", target?.ContentType);
      Assert.AreEqual<string?>("Test Attribute", target?.Title);
      Assert.AreEqual<string?>("Hello", target?.DefaultValue);
      Assert.AreEqual<bool?>(true, target?.IsRequired);

    }

    /*==========================================================================================================================
    | TEST: MAP: EXISTING: RETURNS UPDATED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests setting basic scalar values on an existing object,
    ///   ensuring that all mapped values are overwritten, and unmapped valued are not.
    /// </summary>
    [TestMethod]
    public async Task Map_Existing_ReturnsUpdatedTopic() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new TextAttributeTopicBindingModel("Test") {
        ContentType             = "TextAttributeDescriptor",
        Title                   = null,
        DefaultValue            = "World",
        IsRequired              = false
      };

      var target                = (TextAttributeDescriptor?)TopicFactory.Create("Test", "TextAttributeDescriptor");

      Contract.Assume(target);

      target.Title              = "Original Attribute";
      target.DefaultValue       = "Hello";
      target.IsRequired         = true;
      target.IsExtendedAttribute= false;

      target.Attributes.SetValue("Description", "Original Description");

      target                    = (TextAttributeDescriptor?)await mappingService.MapAsync(bindingModel, target).ConfigureAwait(false);

      Assert.AreEqual<string?>("Test", target?.Key);
      Assert.AreEqual<string?>("TextAttributeDescriptor", target?.ContentType);
      Assert.AreEqual<string?>("Test", target?.Title); //Should inherit from "Key" since it will be null
      Assert.AreEqual<string?>("World", target?.DefaultValue);
      Assert.AreEqual<bool?>(false, target?.IsRequired);
      Assert.AreEqual<bool?>(false, target?.IsExtendedAttribute);
      Assert.AreEqual<string?>("Original Description", target?.Attributes.GetValue("Description"));

    }

    /*==========================================================================================================================
    | TEST: MAP: RECORD: RETURNS NEW TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests mapping a binding model that's based on a C# 9.0
    ///   record type.
    /// </summary>
    [TestMethod]
    public async Task Map_Record_ReturnsNewTopic() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new RecordTopicBindingModel() {
        Key                     = "Test",
        ContentType             = "TextAttributeDescriptor"
      };

      var target                = await mappingService.MapAsync<TextAttributeDescriptor>(bindingModel).ConfigureAwait(false);

      Assert.IsNotNull(target);
      Assert.AreEqual<string>("Test", target.Key);
      Assert.AreEqual<string>("TextAttributeDescriptor", target.ContentType);

    }

    /*==========================================================================================================================
    | TEST: MAP: COMPLEX OBJECT: RETURNS FLATTENED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests setting values from complex objects.
    /// </summary>
    [TestMethod]
    public async Task Map_ComplexObject_ReturnsFlattenedTopic() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new MapToParentTopicBindingModel {
        Key                     = "Test",
        ContentType             = "Contact"
      };

      bindingModel.PrimaryContact.Name                          = "Jeremy";
      bindingModel.AlternateContact.Email                       = "AlternateContact@Ignia.com";
      bindingModel.BillingContact.Email                         = "BillingContact@Ignia.com";

      var target                = (Topic?)await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

      Assert.IsNotNull(target);
      Assert.AreEqual<string?>("Jeremy", target.Attributes.GetValue("Name"));
      Assert.AreEqual<string?>("AlternateContact@Ignia.com", target.Attributes.GetValue("AlternateEmail"));
      Assert.AreEqual<string?>("BillingContact@Ignia.com", target.Attributes.GetValue("BillingContactEmail"));

    }

    /*==========================================================================================================================
    | TEST: MAP: ALTERNATE ATTRIBUTE KEY: RETURNS MAPPED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests whether it successfully derives values from the key
    ///   specified by <see cref="AttributeKeyAttribute"/>.
    /// </summary>
    [TestMethod]
    public async Task Map_AlternateAttributeKey_ReturnsMappedTopic() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new PageTopicBindingModel("Test") {
        ContentType             = "Page",
        Title                   = "Test Page",
        BrowserTitle            = "Browser Title"
      };

      var target                = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

      Assert.AreEqual<string?>("Browser Title", target?.Attributes.GetValue("MetaTitle"));

    }

    /*==========================================================================================================================
    | TEST: MAP: RELATIONSHIPS: RETURNS MAPPED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests whether it successfully crawls the relationships.
    /// </summary>
    [TestMethod]
    public async Task Map_Relationships_ReturnsMappedTopic() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new ContentTypeDescriptorTopicBindingModel("Test");
      var contentTypes          = _topicRepository.GetContentTypeDescriptors();
      var topic                 = (ContentTypeDescriptor)TopicFactory.Create("Test", "ContentTypeDescriptor");

      topic.Relationships.SetValue("ContentTypes", contentTypes[4]);

      for (var i = 0; i < 3; i++) {
        bindingModel.ContentTypes.Add(
          new() {
            UniqueKey = contentTypes[i].GetUniqueKey()
          }
        );
      }

      var target                = (ContentTypeDescriptor?)await mappingService.MapAsync(bindingModel, topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(3, target?.PermittedContentTypes.Count);
      Assert.IsTrue(target?.PermittedContentTypes.Contains(contentTypes[0]));
      Assert.IsTrue(target?.PermittedContentTypes.Contains(contentTypes[1]));
      Assert.IsTrue(target?.PermittedContentTypes.Contains(contentTypes[2]));
      Assert.IsFalse(target?.PermittedContentTypes.Contains(contentTypes[3]));

    }


    /*==========================================================================================================================
    | TEST: MAP: RELATIONSHIPS: THROW EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests whether it correctly throws an exception if the
    ///   <see cref="IAssociatedTopicBindingModel.UniqueKey"/> cannot be located in the repository.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(MappingModelValidationException))]
    public async Task Map_Relationships_ThrowException() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new ContentTypeDescriptorTopicBindingModel("Test");
      var topic                 = (ContentTypeDescriptor)TopicFactory.Create("Test", "ContentTypeDescriptor");

      bindingModel.ContentTypes.Add(
        new() {
          UniqueKey = "Root:Configuration:InvalidKey"
        }
      );

      await mappingService.MapAsync(bindingModel, topic).ConfigureAwait(false);

    }


    /*==========================================================================================================================
    | TEST: MAP: NESTED TOPICS: RETURNS MAPPED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests whether it successfully crawls the nested topics.
    /// </summary>
    [TestMethod]
    public async Task Map_NestedTopics_ReturnsMappedTopic() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new ContentTypeDescriptorTopicBindingModel("Test");

      bindingModel.Attributes.Add(new TextAttributeTopicBindingModel("Attribute1"));
      bindingModel.Attributes.Add(new TextAttributeTopicBindingModel("Attribute2"));
      bindingModel.Attributes.Add(new TextAttributeTopicBindingModel("Attribute3") { DefaultValue = "New Value" });

      var topic                 = TopicFactory.Create("Test", "ContentTypeDescriptor");
      var attributes            = TopicFactory.Create("Attributes", "List", topic);

      var attribute3            = (AttributeDescriptor)TopicFactory.Create("Attribute3", "TextAttributeDescriptor", attributes);
      _                         = TopicFactory.Create("Attribute4", "TextAttributeDescriptor", attributes);

      attribute3.DefaultValue   = "Original Value";

      var target                = (ContentTypeDescriptor?)await mappingService.MapAsync(bindingModel, topic).ConfigureAwait(false);

      Assert.AreEqual<int?>(3, target?.AttributeDescriptors.Count);
      Assert.IsNotNull(target?.AttributeDescriptors.GetValue("Attribute1"));
      Assert.IsNotNull(target?.AttributeDescriptors.GetValue("Attribute2"));
      Assert.IsNotNull(target?.AttributeDescriptors.GetValue("Attribute3"));
      Assert.AreEqual<string?>("New Value", target?.AttributeDescriptors.GetValue("Attribute3")?.DefaultValue);
      Assert.IsNull(target?.AttributeDescriptors.GetValue("Attribute4"));

    }

    /*==========================================================================================================================
    | TEST: MAP: TOPIC REFERENCES: RETURNS MAPPED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests whether it successfully maps referenced topics.
    /// </summary>
    [TestMethod]
    public async Task Map_TopicReferences_ReturnsMappedTopic() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var topic                 = _topicRepository.Load("Root:Configuration:ContentTypes:Attributes:Title");

      Contract.Assume(topic);

      var bindingModel          = new ReferenceTopicBindingModel("Test") {
        BaseTopic               = new() {
          UniqueKey             = topic.GetUniqueKey()
        }
      };

      var target                = (TextAttributeDescriptor?)await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

      Assert.IsNotNull(target?.BaseTopic);
      Assert.AreEqual<string?>("Title", target?.BaseTopic.Key);
      Assert.AreEqual<string?>("Text", target?.EditorType);

    }

    /*==========================================================================================================================
    | TEST: MAP: NULL TOPIC REFERENCE: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests whether it correctly ovewrite any topic references if
    ///   the <see cref="IAssociatedTopicBindingModel"/> has a <see cref="IAssociatedTopicBindingModel.UniqueKey"/> set to null.
    /// </summary>
    [TestMethod]
    public async Task Map_NullTopicReference_Delete() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var topic                 = _topicRepository.Load("Root:Configuration:ContentTypes:Attributes:Title");
      var baseTopic             = _topicRepository.Load("Root:Configuration:ContentTypes:Attributes:Key");

      Contract.Assume(topic);

      topic.BaseTopic           = baseTopic;

      var bindingModel          = new ReferenceTopicBindingModel(topic.Key) {
        ContentType             = topic.ContentType,
        BaseTopic               = new() {
          UniqueKey             = ""
        }
      };

      await mappingService.MapAsync(bindingModel, topic).ConfigureAwait(false);

      Assert.IsNull(topic.BaseTopic);

    }

    /*==========================================================================================================================
    | TEST: MAP: NULL TOPIC REFERENCE: THROW EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests whether it correctly throws an exception if the <see
    ///   cref="IAssociatedTopicBindingModel"/> is set to null.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(MappingModelValidationException))]
    public async Task Map_NullTopicReference_ThrowException() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new ReferenceTopicBindingModel("AttributeDescriptor") {
        ContentType             = "AttributeDescriptor"
      };

      await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: NULL TOPIC REFERENCE KEY: THROW EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests whether it correctly throws an exception if the <see
    ///   cref="IAssociatedTopicBindingModel.UniqueKey"/> is set to null.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(MappingModelValidationException))]
    public async Task Map_NullTopicReferenceKey_ThrowException() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new ReferenceTopicBindingModel("AttributeDescriptor") {
        ContentType             = "AttributeDescriptor",
        BaseTopic               = new()
      };

      await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: TOPIC REFERENCES: THOWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests whether it correctly throws an exception if the
    ///   <see cref="IAssociatedTopicBindingModel.UniqueKey"/> cannot be resolved in the supplied <see cref="ITopicRepository"
    ///   />.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(MappingModelValidationException))]
    public async Task Map_TopicReferences_ThrowException() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new ReferenceTopicBindingModel("Test") {
        BaseTopic               = new() {
          UniqueKey             = "Root:InvalidKey"
        }
      };

      await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: VALID REQUIRED PROPERTY: IS MAPPED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is thrown if it is not set.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task Map_ValidRequiredProperty_IsMapped() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new PageTopicBindingModel("Test");

      await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: NULL PROPERTY: MAPS DEFAULT VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has default properties. Ensures that each is set appropriately.
    /// </summary>
    [TestMethod]
    public async Task Map_NullProperty_MapsDefaultValue() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new PageTopicBindingModel("Test") {
        Title                   = "Required Title"
      };

      var target                = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

      Assert.AreEqual<string?>("Default page description", target?.Attributes.GetValue("MetaDescription"));

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

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new MinimumLengthPropertyTopicBindingModel("Test") {
        Title                   = "Hello World"
      };

      await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: INVALID CHILDREN PROPERTY: THROWS INVALID OPERATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has children property. This is invalid, and expected to throw an <see
    ///   cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(MappingModelValidationException))]
    public async Task Map_InvalidChildrenProperty_ThrowsInvalidOperationException() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidChildrenTopicBindingModel("Test");

      await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: INVALID PARENT PROPERTY: THROWS INVALID OPERATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has parent property. This is invalid, and expected to throw an <see
    ///   cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(MappingModelValidationException))]
    public async Task Map_InvalidParentProperty_ThrowsInvalidOperationException() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new InvalidParentTopicBindingModel("Test") {
        Parent                  = new("Test", "Page")
      };

      await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: INVALID ATTRIBUTE: THROWS INVALID OPERATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a property that doesn't map to any attributes. This is invalid, and expected to throw an
    ///   <see cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(MappingModelValidationException))]
    public async Task Map_InvalidAttribute_ThrowsInvalidOperationException() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidAttributeTopicBindingModel("Test");

      await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: INVALID RELATIONSHIP BASE TYPE: THROWS INVALID OPERATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a relationship whose type doesn't implement <see cref="IAssociatedTopicBindingModel"/>.
    ///   This is invalid, and expected to throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(MappingModelValidationException))]
    public async Task Map_InvalidRelationshipBaseType_ThrowsInvalidOperationException() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidRelationshipBaseTypeTopicBindingModel("Test");

      await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: INVALID RELATIONSHIP TYPE: THROWS INVALID OPERATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a relationship with an invalid <see cref="CollectionType"/>—i.e., it refers to <see cref=
    ///   "CollectionType.NestedTopics"/>, even though the property is associated with a <see cref="CollectionType.Relationship"
    ///   />. This is invalid, and expected to throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(MappingModelValidationException))]
    public async Task Map_InvalidRelationshipType_ThrowsInvalidOperationException() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidRelationshipTypeTopicBindingModel("Test");

      await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: INVALID RELATIONSHIP LIST TYPE: THROWS INVALID OPERATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a relationship that implements an invalid collection type—i.e., it implements a <see
    ///   cref="Dictionary{TKey, TValue}"/>, even though relationships are expected to return a type implementing <see
    ///   cref="IList"/>. This is invalid, and expected to throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(MappingModelValidationException))]
    public async Task Map_InvalidRelationshipListType_ThrowsInvalidOperationException() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidRelationshipListTypeTopicBindingModel("Test");

      await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: INVALID NESTED TOPIC LIST TYPE: THROWS INVALID OPERATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a nested topic that implements an invalid collection type—i.e., it implements a <see
    ///   cref="Dictionary{TKey, TValue}"/>, even though nestd topics are expected to return a type implementing <see cref="
    ///   IList"/>. This is invalid, and expected to throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(MappingModelValidationException))]
    public async Task Map_InvalidNestedTopicListType_ThrowsInvalidOperationException() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidNestedTopicListTypeTopicBindingModel("Test");

      await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: INVALID TOPIC REFERENCE TYPE: THROWS INVALID OPERATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a reference that implements an invalid type—i.e., it implements a <see cref="
    ///   TopicViewModel"/>, even though references are expected to return a type implementing <see cref="
    ///   IAssociatedTopicBindingModel"/>. This is invalid, and expected to throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(MappingModelValidationException))]
    public async Task Map_InvalidTopicReferenceType_ThrowsInvalidOperationException() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidReferenceTypeTopicBindingModel("Test");

      await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: MAP: DISABLED PROPERTY: IS NOT MAPPED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a property that doesn't map to any attributes. This is invalid. However, the property
    ///   is also decorated with the <see cref="DisableMappingAttribute"/>, which should prevent the <see
    ///   cref="ReverseTopicMappingService"/> from validating or mapping the property.
    /// </summary>
    [TestMethod]
    public async Task Map_DisabledProperty_IsNotMapped() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new DisabledAttributeTopicBindingModel("Test") {
        UnmappedAttribute       = "Hello World"
      };

      var target = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

      Assert.IsNull(target?.Attributes.GetValue("UnmappedAttribute", null));

    }

  } //Class
} //Namespace
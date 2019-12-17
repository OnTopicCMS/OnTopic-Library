/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Ignia.Topics.Attributes;
using Ignia.Topics.Data.Caching;
using Ignia.Topics.Mapping;
using Ignia.Topics.Mapping.Annotations;
using Ignia.Topics.Repositories;
using Ignia.Topics.TestFixtures;
using Ignia.Topics.Tests.BindingModels;
using Ignia.Topics.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ignia.Topics.Metadata;
using Ignia.Topics.Models;
using System.Collections.Generic;
using System.Collections;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: REVERSE TOPIC MAPPING SERVICE TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="ReverseTopicMappingService"/> using local DTOs.
  /// </summary>
  [TestClass]
  public class ReverseReverseTopicMappingServiceTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ReverseReverseTopicMappingServiceTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="StubTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph.
    /// </remarks>
    public ReverseReverseTopicMappingServiceTest() {
      _topicRepository = new CachedTopicRepository(new StubTopicRepository());
    }

    /*==========================================================================================================================
    | TEST: MAP (GENERIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests setting basic scalar values by specifying an explicit
    ///   type.
    /// </summary>
    [TestMethod]
    public async Task MapGeneric() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new AttributeDescriptorTopicBindingModel() {
        Key                     = "Test",
        ContentType             = "AttributeDescriptor",
        Title                   = "Test Attribute",
        DefaultValue            = "Hello",
        IsRequired              = true
      };

      var target                = await mappingService.MapAsync<AttributeDescriptor>(bindingModel).ConfigureAwait(false);

      Assert.AreEqual<string>("Test", target.Key);
      Assert.AreEqual<string>("AttributeDescriptor", target.ContentType);
      Assert.AreEqual<string>("Test Attribute", target.Title);
      Assert.AreEqual<string>("Hello", target.DefaultValue);
      Assert.AreEqual<bool>(true, target.IsRequired);

    }

    /*==========================================================================================================================
    | TEST: MAP (DYNAMIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests setting basic scalar values by allowing it to
    ///   dynamically determine the instance type.
    /// </summary>
    [TestMethod]
    public async Task MapDynamic() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new AttributeDescriptorTopicBindingModel {
        Key                     = "Test",
        ContentType             = "AttributeDescriptor",
        Title                   = "Test Attribute",
        DefaultValue            = "Hello",
        IsRequired              = true
      };

      var target                = (AttributeDescriptor?)await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

      Assert.IsNotNull(target);
      Assert.AreEqual<string>("Test", target.Key);
      Assert.AreEqual<string>("AttributeDescriptor", target.ContentType);
      Assert.AreEqual<string>("Test Attribute", target.Title);
      Assert.AreEqual<string>("Hello", target.DefaultValue);
      Assert.AreEqual<bool>(true, target.IsRequired);

    }

    /*==========================================================================================================================
    | TEST: MAP (EXISTING)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests setting basic scalar values on an existing object,
    ///   ensuring that all mapped values are overwritten, and unmapped valued are not.
    /// </summary>
    [TestMethod]
    public async Task MapExisting() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new AttributeDescriptorTopicBindingModel() {
        Key                     = "Test",
        ContentType             = "AttributeDescriptor",
        Title                   = null,
        DefaultValue            = "World",
        IsRequired              = false
      };

      var target                = (AttributeDescriptor?)TopicFactory.Create("Test", "AttributeDescriptor");

      target.Title              = "Original Attribute";
      target.DefaultValue       = "Hello";
      target.IsRequired         = true;
      target.IsExtendedAttribute        = false;
      target.Description        = "Original Description";

      target                    = (AttributeDescriptor?)await mappingService.MapAsync(bindingModel, target).ConfigureAwait(false);

      Assert.AreEqual<string>("Test", target.Key);
      Assert.AreEqual<string>("AttributeDescriptor", target.ContentType);
      Assert.AreEqual<string>("Test", target.Title); //Should inherit from "Key" since it will be null
      Assert.AreEqual<string>("World", target.DefaultValue);
      Assert.AreEqual<bool>(false, target.IsRequired);
      Assert.AreEqual<bool>(false, target.IsExtendedAttribute);
      Assert.AreEqual<string>("Original Description", target.Description);

    }

    /*==========================================================================================================================
    | TEST: MAP COMPLEX OBJECTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests setting values from complex objects.
    /// </summary>
    [TestMethod]
    public async Task MapComplexObjects() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new MapToParentTopicBindingModel {
        Key                     = "Test",
        ContentType             = "Contact"
      };

      bindingModel.PrimaryContact.Email = "PrimaryContact@Ignia.com";
      bindingModel.AlternateContact.Email = "AlternateContact@Ignia.com";
      bindingModel.BillingContact.Email = "BillingContact@Ignia.com";

      var target                = (Topic?)await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

      Assert.IsNotNull(target);
      Assert.AreEqual<string>("PrimaryContact@Ignia.com", target.Attributes.GetValue("Email"));
      Assert.AreEqual<string>("AlternateContact@Ignia.com", target.Attributes.GetValue("AlternateEmail"));
      Assert.AreEqual<string>("BillingContact@Ignia.com", target.Attributes.GetValue("BillingContactEmail"));

    }

    /*==========================================================================================================================
    | TEST: ALTERNATE ATTRIBUTE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests whether it successfully derives values from the key
    ///   specified by <see cref="AttributeKeyAttribute"/>.
    /// </summary>
    [TestMethod]
    public async Task AlternateAttributeKey() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new PageTopicBindingModel {
        Key                     = "Test",
        ContentType             = "Page",
        Title                   = "Test Page",
        BrowserTitle            = "Browser Title"
      };

      var target                = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

      Assert.AreEqual<string>("Browser Title", target.Attributes.GetValue("MetaTitle"));

    }

    /*==========================================================================================================================
    | TEST: MAP RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests whether it successfully crawls the relationships.
    /// </summary>
    [TestMethod]
    public async Task MapRelationships() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new ContentTypeDescriptorTopicBindingModel("Test");
      var contentTypes          = _topicRepository.GetContentTypeDescriptors();
      var topic                 = (ContentTypeDescriptor)TopicFactory.Create("Test", "ContentTypeDescriptor");

      topic.Relationships.SetTopic("ContentTypes", contentTypes[4]);

      for (var i = 0; i < 3; i++) {
        bindingModel.ContentTypes.Add(
          new Models.RelatedTopicBindingModel() {
            UniqueKey = contentTypes[i].GetUniqueKey()
          }
        );
      }

      var target                = (ContentTypeDescriptor?)await mappingService.MapAsync(bindingModel, topic).ConfigureAwait(false);

      Assert.AreEqual<int>(3, target.PermittedContentTypes.Count);
      Assert.IsTrue(target.PermittedContentTypes.Contains(contentTypes[0]));
      Assert.IsTrue(target.PermittedContentTypes.Contains(contentTypes[1]));
      Assert.IsTrue(target.PermittedContentTypes.Contains(contentTypes[2]));
      Assert.IsFalse(target.PermittedContentTypes.Contains(contentTypes[3]));

    }

    /*==========================================================================================================================
    | TEST: MAP NESTED TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests whether it successfully crawls the nested topics.
    /// </summary>
    [TestMethod]
    public async Task MapNestedTopics() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new ContentTypeDescriptorTopicBindingModel("Test");

      bindingModel.Attributes.Add(new AttributeDescriptorTopicBindingModel("Attribute1"));
      bindingModel.Attributes.Add(new AttributeDescriptorTopicBindingModel("Attribute2"));
      bindingModel.Attributes.Add(new AttributeDescriptorTopicBindingModel("Attribute3") { DefaultValue = "New Value" } );

      var topic                 = TopicFactory.Create("Test", "ContentTypeDescriptor");
      var attributes            = TopicFactory.Create("Attributes", "List", topic);

      var attribute3            = (AttributeDescriptor)TopicFactory.Create("Attribute3", "AttributeDescriptor", attributes);
      var attribute4            = TopicFactory.Create("Attribute4", "AttributeDescriptor", attributes);

      attribute3.DefaultValue   = "Original Value";

      var target                = (ContentTypeDescriptor?)await mappingService.MapAsync(bindingModel, topic).ConfigureAwait(false);

      Assert.AreEqual<int>(3, target.AttributeDescriptors.Count);
      Assert.IsNotNull(target.AttributeDescriptors.GetTopic("Attribute1"));
      Assert.IsNotNull(target.AttributeDescriptors.GetTopic("Attribute2"));
      Assert.IsNotNull(target.AttributeDescriptors.GetTopic("Attribute3"));
      Assert.AreEqual<string>("New Value", target.AttributeDescriptors.GetTopic("Attribute3").DefaultValue);
      Assert.IsNull(target.AttributeDescriptors.GetTopic("Attribute4"));

    }

    /*==========================================================================================================================
    | TEST: MAP TOPIC REFERENCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReverseTopicMappingService"/> and tests whether it successfully maps referenced topics.
    /// </summary>
    [TestMethod]
    public async Task MapTopicReferences() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new ReferenceTopicBindingModel("Test") {
        DerivedTopic            = new RelatedTopicBindingModel() {
          UniqueKey             = _topicRepository.Load("Root:Configuration:ContentTypes:Attributes:Title").GetUniqueKey()
        }
      };

      var target                = (AttributeDescriptor?)await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

      target.DerivedTopic       = _topicRepository.Load(target.Attributes.GetInteger("TopicId", -5));

      Assert.IsNotNull(target.DerivedTopic);
      Assert.AreEqual<string>("FormField", target.EditorType);

    }

    /*==========================================================================================================================
    | TEST: MAP REQUIRED PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is thrown if it is not set.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task MapRequiredProperty() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new PageTopicBindingModel("Test");

      var target                = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: DEFAULT VALUE PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has default properties. Ensures that each is set appropriately.
    /// </summary>
    [TestMethod]
    public async Task MapDefaultValueProperties() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new PageTopicBindingModel("Test") {
        Title                   = "Required Title"
      };

      var target                = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

      Assert.AreEqual<string>("Default page description", target.Attributes.GetValue("MetaDescription"));

    }

    /*==========================================================================================================================
    | TEST: MINIMUM VALUE PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has minimum value properties. Ensures that an error is thrown if the minimum is not met.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task MapMinimumValueProperties() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new MinimumLengthPropertyTopicBindingModel("Test") {
        Title                   = "Hello World"
      };

      var target = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: CHILDREN PROPERTY (INVALID)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has children property. This is invalid, and expected to throw an <see
    ///   cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task InvalidChildrenProperty() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidChildrenTopicBindingModel("Test");

      var target = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: PARENT PROPERTY (INVALID)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has parent property. This is invalid, and expected to throw an <see
    ///   cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task InvalidParentProperty() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new InvalidParentTopicBindingModel("Test") {
        Parent                  = new BasicTopicBindingModel("Test", "Page")
      };

      var target = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: ATTRIBUTE PROPERTY (INVALID)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a property that doesn't map to any attributes. This is invalid, and expected to throw an
    ///   <see cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task InvalidAttributeProperty() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidAttributeTopicBindingModel("Test");

      var target = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: RELATIONSHIP BASE TYPE PROPERTY (INVALID)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a relationship whose type doesn't implement <see cref="IRelatedTopicBindingModel"/>. This
    ///   is invalid, and expected to throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task InvalidRelationshipBaseTypeProperty() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidRelationshipBaseTypeTopicBindingModel("Test");

      var target = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: REFERENCE NAME PROPERTY (INVALID)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a reference that does not end in <c>Id</c>. This is invalid, and expected to throw an
    ///   <see cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task InvalidReferenceNameProperty() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidReferenceNameTopicBindingModel("Test");

      var target = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: RELATIONSHIP TYPE PROPERTY (INVALID)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a relationship an invalid <see cref="RelationshipType"/>—i.e., it refers to <see
    ///   cref="RelationshipType.NestedTopics"/>, even though the property is associated with a <see
    ///   cref="RelationshipType.Relationship"/>. This is invalid, and expected to throw an <see
    ///   cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task InvalidRelationshipTypeProperty() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidRelationshipTypeTopicBindingModel("Test");

      var target = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: RELATIONSHIP LIST TYPE PROPERTY (INVALID)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a relationship that implements an invalid collection type—i.e., it implements a <see
    ///   cref="Dictionary{TKey, TValue}"/>, even though relationships are expected to return a type implementing <see
    ///   cref="IList"/>. This is invalid, and expected to throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task InvalidRelationshipListTypeProperty() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidRelationshipListTypeTopicBindingModel("Test");

      var target = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: REFERENCE TYPE PROPERTY (INVALID)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a reference that implements an invalid type—i.e., it implements a <see
    ///   cref="TopicViewModel"/>, even though references are expected to return a type implementing <see
    ///   cref="IRelatedTopicBindingModel"/>. This is invalid, and expected to throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task InvalidReferenceTypeProperty() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);
      var bindingModel          = new InvalidReferenceTypeTopicBindingModel("Test");

      var target = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | TEST: ATTRIBUTE PROPERTY (DISABLED)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a property that doesn't map to any attributes. This is invalid. However, the property
    ///   is also decorated with the <see cref="DisableMappingAttribute"/>, which should prevent the <see
    ///   cref="ReverseTopicMappingService"/> from validating or mapping the property.
    /// </summary>
    [TestMethod]
    public async Task DisabledAttributeProperty() {

      var mappingService        = new ReverseTopicMappingService(_topicRepository);

      var bindingModel          = new DisabledAttributeTopicBindingModel("Test") {
        UnmappedAttribute       = "Hello World"
      };

      var target = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

      Assert.IsNull(target.Attributes.GetValue("UnmappedAttribute", null));

    }

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Ignia.Topics.Data.Caching;
using Ignia.Topics.Mapping;
using Ignia.Topics.Repositories;
using Ignia.Topics.Tests.TestDoubles;
using Ignia.Topics.Tests.BindingModels;
using Ignia.Topics.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ignia.Topics.Metadata;
using Ignia.Topics.Models;

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
    readonly                    ITopicRepository                _topicRepository                = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ReverseReverseTopicMappingServiceTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="FakeTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph.
    /// </remarks>
    public ReverseReverseTopicMappingServiceTest() {
      _topicRepository = new CachedTopicRepository(new FakeTopicRepository());
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

      var mappingService        = new ReverseTopicMappingService(_topicRepository, new FakeViewModelLookupService());
      var bindingModel          = new AttributeDescriptorTopicBindingModel();

      bindingModel.Key          = "Test";
      bindingModel.ContentType  = "AttributeDescriptor";
      bindingModel.Title        = "Test Attribute";
      bindingModel.DefaultValue = "Hello";
      bindingModel.IsRequired   = true;

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

      var mappingService        = new ReverseTopicMappingService(_topicRepository, new FakeViewModelLookupService());
      var bindingModel          = new AttributeDescriptorTopicBindingModel();

      bindingModel.Key          = "Test";
      bindingModel.ContentType  = "AttributeDescriptor";
      bindingModel.Title        = "Test Attribute";
      bindingModel.DefaultValue = "Hello";
      bindingModel.IsRequired   = true;

      var target                = (AttributeDescriptor)await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

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

      var mappingService        = new ReverseTopicMappingService(_topicRepository, new FakeViewModelLookupService());
      var bindingModel          = new AttributeDescriptorTopicBindingModel();
      var target                = (AttributeDescriptor)TopicFactory.Create("Test", "AttributeDescriptor");

      target.Title              = "Original Attribute";
      target.DefaultValue       = "Hello";
      target.IsRequired         = true;
      target.StoreInBlob        = false;
      target.Description        = "Original Description";

      bindingModel.Key          = "Test";
      bindingModel.ContentType  = "AttributeDescriptor";
      bindingModel.Title        = null;
      bindingModel.DefaultValue = "World";
      bindingModel.IsRequired   = false;

      target                    = (AttributeDescriptor)await mappingService.MapAsync(bindingModel, target).ConfigureAwait(false);

      Assert.AreEqual<string>("Test", target.Key);
      Assert.AreEqual<string>("AttributeDescriptor", target.ContentType);
      Assert.AreEqual<string>("Test", target.Title); //Should inherit from "Key" since it will be null
      Assert.AreEqual<string>("World", target.DefaultValue);
      Assert.AreEqual<bool>(false, target.IsRequired);
      Assert.AreEqual<bool>(false, target.StoreInBlob);
      Assert.AreEqual<string>("Original Description", target.Description);

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

      var mappingService        = new ReverseTopicMappingService(_topicRepository, new FakeViewModelLookupService());
      var bindingModel          = new PageTopicBindingModel();

      bindingModel.Key          = "Test";
      bindingModel.ContentType  = "Page";
      bindingModel.Title        = "Test Page";
      bindingModel.BrowserTitle = "Browser Title";

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

      var mappingService        = new ReverseTopicMappingService(_topicRepository, new FakeViewModelLookupService());
      var contentTypes          = _topicRepository.GetContentTypeDescriptors();
      var bindingModel          = new ContentTypeDescriptorTopicBindingModel();

      bindingModel.Key          = "Test";
      bindingModel.ContentType  = "ContentTypeDescriptor";

      bindingModel.ContentTypes.Add(new Models.RelatedTopicBindingModel() { UniqueKey = contentTypes[0].GetUniqueKey() });
      bindingModel.ContentTypes.Add(new Models.RelatedTopicBindingModel() { UniqueKey = contentTypes[1].GetUniqueKey() });
      bindingModel.ContentTypes.Add(new Models.RelatedTopicBindingModel() { UniqueKey = contentTypes[2].GetUniqueKey() });

      var target                = (ContentTypeDescriptor)await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

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

      var mappingService        = new ReverseTopicMappingService(_topicRepository, new FakeViewModelLookupService());
      var bindingModel          = new ContentTypeDescriptorTopicBindingModel("Test");

      bindingModel.Attributes.Add(new AttributeDescriptorTopicBindingModel("Attribute1"));
      bindingModel.Attributes.Add(new AttributeDescriptorTopicBindingModel("Attribute2"));
      bindingModel.Attributes.Add(new AttributeDescriptorTopicBindingModel("Attribute3") { DefaultValue = "New Value" } );

      var topic                 = TopicFactory.Create("Test", "ContentTypeDescriptor");
      var attributes            = TopicFactory.Create("Attributes", "List", topic);

      var attribute3            = (AttributeDescriptor)TopicFactory.Create("Attribute3", "AttributeDescriptor", attributes);
      var attribute4            = TopicFactory.Create("Attribute4", "AttributeDescriptor", attributes);

      attribute3.DefaultValue   = "Original Value";

      var target                = (ContentTypeDescriptor)await mappingService.MapAsync(bindingModel, topic).ConfigureAwait(false);

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

      var mappingService        = new ReverseTopicMappingService(_topicRepository, new FakeViewModelLookupService());
      var bindingModel          = new AttributeDescriptorTopicBindingModel("Test");

      bindingModel.DerivedTopic = new RelatedTopicBindingModel() {
        UniqueKey               = _topicRepository.Load("Root:Configuration:ContentTypes:Attributes:Title").GetUniqueKey()
      };

      var target                = (AttributeDescriptor)await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

      var test = _topicRepository.Load("Root:Configuration:ContentTypes");

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

      var mappingService        = new ReverseTopicMappingService(_topicRepository, new FakeViewModelLookupService());
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

      var mappingService        = new ReverseTopicMappingService(_topicRepository, new FakeViewModelLookupService());
      var bindingModel          = new PageTopicBindingModel("Test");

      bindingModel.Title        = "Required Title";

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

      var mappingService        = new ReverseTopicMappingService(_topicRepository, new FakeViewModelLookupService());
      var bindingModel          = new MinimumLengthPropertyTopicBindingModel("Test");

      bindingModel.Title        = "Hello World";

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

      var mappingService        = new ReverseTopicMappingService(_topicRepository, new FakeViewModelLookupService());
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

      var mappingService        = new ReverseTopicMappingService(_topicRepository, new FakeViewModelLookupService());
      var bindingModel          = new InvalidParentTopicBindingModel("Test");

      bindingModel.Parent       = new BasicTopicBindingModel("Test", "Page");

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

      var mappingService        = new ReverseTopicMappingService(_topicRepository, new FakeViewModelLookupService());
      var bindingModel          = new InvalidAttributeTopicBindingModel("Test");

      var target = await mappingService.MapAsync(bindingModel).ConfigureAwait(false);

    }



  } //Class

} //Namespace


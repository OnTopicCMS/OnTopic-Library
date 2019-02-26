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


  } //Class

} //Namespace


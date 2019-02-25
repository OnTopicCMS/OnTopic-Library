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


  } //Class

} //Namespace


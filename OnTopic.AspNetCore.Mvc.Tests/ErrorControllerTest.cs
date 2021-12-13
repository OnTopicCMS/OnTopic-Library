/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Microsoft.AspNetCore.Mvc;
using OnTopic.AspNetCore.Mvc;
using OnTopic.AspNetCore.Mvc.Controllers;
using OnTopic.AspNetCore.Mvc.Tests.TestDoubles;
using OnTopic.Data.Caching;
using OnTopic.Mapping;
using OnTopic.Repositories;
using OnTopic.ViewModels;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: ERROR CONTROLLER TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="ErrorController"/>.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class ErrorControllerTest: IClassFixture<TestTopicRepository> {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository;
    readonly                    ITopicMappingService            _topicMappingService;
    readonly                    ControllerContext               _context;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ErrorControllerTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="StubTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph. In addition, it initializes a shared <see cref="Topic"/> reference to use for the various
    ///   tests.
    /// </remarks>
    public ErrorControllerTest(TestTopicRepository topicRepository) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = new CachedTopicRepository(topicRepository);
      _topicMappingService      = new TopicMappingService(_topicRepository, new TopicViewModelLookupService());
      _context                  = FakeControllerContext.GetControllerContext("Error");

    }

    /*==========================================================================================================================
    | TEST: ERROR CONTROLLER: HTTP: RETURNS EXPECTED ERROR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="ErrorController.HttpAsync(Int32)" /> action with different status codes, and ensures that the
    ///   expected <see cref="Topic"/> is returned in the <see cref="TopicViewResult"/>.
    /// </summary>
    [Theory]
    [InlineData(405, "405")]                               // Exact match
    [InlineData(412, "400")]                               // Fallback to category
    [InlineData(512, "Error")]                             // Fallback to root topic
    public async void ErrorController_Http_ReturnsExpectedError(int errorCode, string expectedContent) {

      var controller            = new ErrorController(_topicRepository, _topicMappingService) {
        ControllerContext       = new(_context)
      };
      var result                = await controller.HttpAsync(errorCode).ConfigureAwait(false) as TopicViewResult;
      var model                 = result?.Model as PageTopicViewModel;

      controller.Dispose();

      Assert.NotNull(result);
      Assert.Equal(expectedContent, model?.Title);

    }

  } //Class
} //Namespace
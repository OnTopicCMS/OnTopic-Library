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
  | CLASS: TOPIC CONTROLLER TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicController"/>, and other <see cref="Controller"/> classes that are part of
  ///   the <see cref="OnTopic.AspNetCore.Mvc"/> namespace.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class TopicControllerTest: IClassFixture<TestTopicRepository> {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository;
    readonly                    ITopicMappingService            _topicMappingService;
    readonly                    Topic                           _topic;
    readonly                    ControllerContext               _context;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicControllerTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="StubTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph. In addition, it initializes a shared <see cref="Topic"/> reference to use for the various
    ///   tests.
    /// </remarks>
    public TopicControllerTest(TestTopicRepository topicRepository) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = new CachedTopicRepository(topicRepository);
      _topic                    = _topicRepository.Load("Root:Web:Valid:Child")!;
      _topicMappingService      = new TopicMappingService(_topicRepository, new TopicViewModelLookupService());
      _context                  = FakeControllerContext.GetControllerContext("Web", "Web/Valid/Child/");

    }

    /*==========================================================================================================================
    | TEST: TOPIC CONTROLLER: INDEX ASYNC: RETURNS TOPIC VIEW RESULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="TopicController.IndexAsync(String)" /> action.
    /// </summary>
    [Fact]
    public async Task TopicController_IndexAsync_ReturnsTopicViewResult() {

      var controller            = new TopicController(_topicRepository, _topicMappingService) {
        ControllerContext       = _context
      };

      var result                = await controller.IndexAsync(_topic.GetWebPath()).ConfigureAwait(false) as TopicViewResult;
      var model                 = result?.Model as PageTopicViewModel;

      controller.Dispose();

      Assert.NotNull(model);
      Assert.Equal("Child", model?.Title);

    }

    /*==========================================================================================================================
    | TEST: REDIRECT CONTROLLER: REDIRECT: RETURNS REDIRECT RESULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="RedirectController.Redirect(Int32)" /> action.
    /// </summary>
    [Fact]
    public void RedirectController_TopicRedirect_ReturnsRedirectResult() {

      var controller            = new RedirectController(_topicRepository);
      var result                = controller.Redirect(_topic.Id) as RedirectResult;

      controller.Dispose();

      Assert.NotNull(result);
      Assert.True(result?.Permanent?? false);
      Assert.Equal(_topic.GetWebPath(), result?.Url);

    }

    /*==========================================================================================================================
    | TEST: REDIRECT CONTROLLER: REDIRECT: RETURNS NOT FOUND OBJECT RESULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="RedirectController.Redirect(Int32)" /> action with an invalid <see cref="Topic.Id"/> and
    ///   confirms that a <see cref="NotFoundResult"/> is returned.
    /// </summary>
    [Fact]
    public void RedirectController_TopicRedirect_ReturnsNotFoundObjectResult() {

      var controller            = new RedirectController(_topicRepository);
      var result                = controller.Redirect(12345);

      controller.Dispose();

      Assert.NotNull(result);
      Assert.IsType<NotFoundObjectResult>(result);

    }

  } //Class
} //Namespace
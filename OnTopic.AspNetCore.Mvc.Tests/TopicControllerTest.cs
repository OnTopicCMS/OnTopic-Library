﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using OnTopic.AspNetCore.Mvc;
using OnTopic.AspNetCore.Mvc.Controllers;
using OnTopic.AspNetCore.Mvc.Tests.TestDoubles;
using OnTopic.Data.Caching;
using OnTopic.Mapping;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.ViewModels;
using Xunit;

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

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish view model context
      \-----------------------------------------------------------------------------------------------------------------------*/
      var routes                = new RouteData();

      routes.Values.Add("rootTopic", "Web");
      routes.Values.Add("path", "Web/Valid/Child/");

      var actionContext         = new ActionContext {
        HttpContext             = new DefaultHttpContext(),
        RouteData               = routes,
        ActionDescriptor        = new ControllerActionDescriptor()
      };
      _context                  = new(actionContext);

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

    /*==========================================================================================================================
    | TEST: SITEMAP CONTROLLER: INDEX: RETURNS SITEMAP XML
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the index action of the <see cref="SitemapController.Index(Boolean, Boolean)" /> action.
    /// </summary>
    [Fact]
    public void SitemapController_Index_ReturnsSitemapXml() {

      var controller            = new SitemapController(_topicRepository) {
        ControllerContext       = new(_context)
      };
      var result                = controller.Index() as ContentResult;
      var model                 = result?.Content as string;

      controller.Dispose();

      Assert.NotNull(model);

      Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>", model, StringComparison.Ordinal);
      Assert.Contains("/Web/Valid/Child/</loc>", model!, StringComparison.Ordinal);

    }

    /*==========================================================================================================================
    | TEST: SITEMAP CONTROLLER: INDEX: EXCLUDES CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the index action of the <see cref="SitemapController.Index(Boolean, Boolean)" /> action and verifies that it
    ///   properly excludes <c>List</c> content types, and skips over <c>Container</c> and <c>PageGroup</c>.
    /// </summary>
    [Fact]
    public void SitemapController_Index_ExcludesContentTypes() {

      var controller            = new SitemapController(_topicRepository) {
        ControllerContext       = new(_context)
      };
      var result                = controller.Extended(true) as ContentResult;
      var model                 = result?.Content as string;

      controller.Dispose();

      Assert.NotNull(model);
      Assert.False(model!.Contains("NestedTopics/</loc>", StringComparison.Ordinal));
      Assert.False(model!.Contains("NestedTopic/</loc>", StringComparison.Ordinal));
      Assert.False(model!.Contains("Redirect/</loc>", StringComparison.Ordinal));
      Assert.False(model!.Contains("NoIndex/</loc>", StringComparison.Ordinal));
      Assert.False(model!.Contains("Disabled/</loc>", StringComparison.Ordinal));
      Assert.False(model!.Contains("PageGroup/</loc>", StringComparison.Ordinal));

      Assert.True(model!.Contains("PageGroupChild/</loc>", StringComparison.Ordinal));
      Assert.True(model!.Contains("NoIndexChild/</loc>", StringComparison.Ordinal));

    }

    /*==========================================================================================================================
    | TEST: SITEMAP CONTROLLER: INDEX: EXCLUDES CONTAINER DESCENDANTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the index action of the <see cref="SitemapController.Index(Boolean, Boolean)" /> action and verifies that it
    ///   properly excludes the children of <c>Container</c> topics that are marked as <c>NoIndex</c>.
    /// </summary>
    [Fact]
    public void SitemapController_Index_ExcludesContainerDescendants() {

      var controller            = new SitemapController(_topicRepository) {
        ControllerContext       = new(_context)
      };
      var result                = controller.Extended(true) as ContentResult;
      var model                 = result?.Content as string;

      controller.Dispose();

      Assert.NotNull(model);
      Assert.False(model!.Contains("NoIndexContainer/</loc>", StringComparison.Ordinal));
      Assert.False(model!.Contains("NoIndexContainerChild/</loc>", StringComparison.Ordinal));

    }

    /*==========================================================================================================================
    | TEST: SITEMAP CONTROLLER: INDEX: EXCLUDES PRIVATE BRANCHES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the index action of the <see cref="SitemapController.Index(Boolean, Boolean)" /> action and verifies that it
    ///   properly excludes the topics that are marked as <c>IsPrivateBranch</c>, including their descendants.
    /// </summary>
    [Fact]
    public void SitemapController_Index_ExcludesPrivateBranches() {

      var controller            = new SitemapController(_topicRepository) {
        ControllerContext       = new(_context)
      };
      var result                = controller.Extended(true) as ContentResult;
      var model                 = result?.Content as string;

      controller.Dispose();

      Assert.NotNull(model);
      Assert.False(model!.Contains("PrivateBranch/</loc>", StringComparison.Ordinal));
      Assert.False(model!.Contains("PrivateBranchChild/</loc>", StringComparison.Ordinal));

    }

    /*==========================================================================================================================
    | TEST: SITEMAP CONTROLLER: EXTENDED: INCLUDES ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the extended action of the <see cref="SitemapController.Extended(Boolean)" /> action and ensures that the
    ///   results include the expected attributes.
    /// </summary>
    [Fact]
    public void SitemapController_Extended_IncludesAttributes() {

      var controller            = new SitemapController(_topicRepository) {
        ControllerContext       = new(_context)
      };
      var result                = controller.Extended(true) as ContentResult;
      var model                 = result?.Content as string;

      controller.Dispose();

      Assert.NotNull(model);

      Assert.Contains("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>", model, StringComparison.Ordinal);
      Assert.Contains("/Web/Valid/Child/</loc>", model, StringComparison.Ordinal);

      Assert.Contains("<Attribute name=\"Attribute\">Value</Attribute>", model, StringComparison.Ordinal);
      Assert.Contains("<Attribute name=\"Title\">Title</Attribute>", model, StringComparison.Ordinal);
      Assert.Contains("<DataObject type=\"Relationships\">", model, StringComparison.Ordinal);
      Assert.Contains("<Attribute name=\"TopicKey\">Web:Redirect</Attribute>", model, StringComparison.Ordinal);
      Assert.Contains("<DataObject type=\"References\">", model, StringComparison.Ordinal);
      Assert.Contains("<Attribute name=\"Reference\">Web:Redirect</Attribute>", model, StringComparison.Ordinal);

    }

    /*==========================================================================================================================
    | TEST: SITEMAP CONTROLLER: EXTENDED: EXCLUDES ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the index action of the <see cref="SitemapController.Extended(Boolean)" /> action and verifies that it
    ///   properly excludes e.g. the <c>Body</c> and <c>IsHidden</c> attributes.
    /// </summary>
    [Fact]
    public void SitemapController_Index_ExcludesAttributes() {

      var controller            = new SitemapController(_topicRepository) {
        ControllerContext       = new(_context)
      };
      var result                = controller.Extended(true) as ContentResult;
      var model                 = result?.Content as string;

      controller.Dispose();

      Assert.NotNull(model);

      Assert.False(model!.Contains("<Attribute name=\"Body\">", StringComparison.Ordinal));
      Assert.False(model!.Contains("<Attribute name=\"IsHidden\">", StringComparison.Ordinal));
      Assert.False(model!.Contains("<Attribute name=\"SortOrder\">", StringComparison.Ordinal));
      Assert.False(model!.Contains("<Attribute name=\"ContentType\">List</Attribute>", StringComparison.Ordinal));

    }

  } //Class
} //Namespace
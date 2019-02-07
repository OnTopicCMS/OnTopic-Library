/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Ignia.Topics.ViewModels;

namespace Ignia.Topics.AspNetCore.Mvc {

  /*============================================================================================================================
  | CLASS: TOPIC VIEW RESULT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom version of <see cref="ViewResult" /> capable of determining the most appropriate view.
  /// </summary>
  public class TopicViewResult : ViewResult {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a new instances of a <see cref="TopicViewResult"/> based on the <see cref="ITopicViewModelCore.View"/> and
    ///   <see cref="ITopicViewModelCore.ContentType"/>.
    /// </summary>
    /// <remarks>
    ///   If the <see cref="ITopicViewModelCore.ContentType"/> is unavailable, it is assumed to be <c>Page</c>. If the <see
    ///   cref="ITopicViewModelCore.View"/> is unavailable, it is assumed to be the same as the <see
    ///   cref="ITopicViewModelCore.ContentType"/>.
    /// </remarks>
    public TopicViewResult(ITopicViewModel viewModel, ICompositeViewEngine viewEngine) : base() {
      ViewData.Model = viewModel;
      TopicContentType = viewModel.ContentType ?? "Page";
      TopicView = viewModel.View ?? TopicContentType;
      ViewEngine = viewEngine;
    }

    /// <summary>
    ///   Constructs a new instances of a <see cref="TopicViewResult"/> based on a supplied <paramref name="contentType"/> and,
    ///   optionally, <paramref name="view"/>.
    /// </summary>
    /// <remarks>
    ///   If the <paramref name="contentType"/> is not provided, it is assumed to be <c>Page</c>. If the <paramref name="view"/>
    ///   is not provided, it is assumed to be <paramref name="contentType"/>.
    /// </remarks>
    public TopicViewResult(object viewModel, string contentType = "Page", string view = null) : base() {
      ViewData.Model = viewModel;
      ContentType = contentType;
      TopicView = view ?? ContentType;
    }

    /*==========================================================================================================================
    | PUBLIC PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    public string TopicContentType { get; }
    public string TopicView { get; }


  } //Class

} //Namespace

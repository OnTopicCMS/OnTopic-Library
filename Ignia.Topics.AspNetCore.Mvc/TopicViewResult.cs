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
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ignia.Topics.ViewModels;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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
    ///   Constructs a new instances of a <see cref="TopicViewResult"/> based on a supplied <paramref name="contentType"/> and,
    ///   optionally, <paramref name="view"/>.
    /// </summary>
    /// <remarks>
    ///   If the <paramref name="contentType"/> is not provided, it is assumed to be <c>Page</c>. If the <paramref name="view"/>
    ///   is not provided, it is assumed to be <paramref name="contentType"/>.
    /// </remarks>
    public TopicViewResult(
      ViewDataDictionary viewData,
      ITempDataDictionary tempData,
      object viewModel,
      string contentType = "Page",
      string view = null
    ) : base() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(viewData != null, nameof(viewData));
      Contract.Requires<ArgumentNullException>(tempData != null, nameof(tempData));
      Contract.Requires<ArgumentNullException>(viewModel != null, nameof(viewModel));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set local variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      ViewData = viewData;
      TempData = tempData;
      ViewData.Model = viewModel;
      TopicContentType = contentType;
      TopicView = view ?? ContentType;
      //ViewName = TopicView;

    }

    /*==========================================================================================================================
    | PROPERTY: TOPIC CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents the <see cref="ContentTypeDescriptor"/> name associated with the current <see cref="Topic"/>.
    /// </summary>
    /// <remarks>
    ///   The preferred nomenclature for the name of a <see cref="ContentTypeDescriptor"/> is simply <c>ContentType</c>. The
    ///   base <see cref="ViewResult"/> class has an existing <see cref="ContentType"/> property representing the HTTP response
    ///   value, however. As such, <see cref="TopicContentType"/> is used to disambiguate the terms.
    /// </remarks>
    public string TopicContentType { get; }

    /*==========================================================================================================================
    | PROPERTY: TOPIC VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents the <see cref="Topic.View"/> property. This may be overwritten later by a variety of sources.
    /// </summary>
    /// <remarks>
    ///   The associated <see cref="TopicViewResultExecutor"/> will fall back to the <see cref="TopicView"/> if the view isn't
    ///   set via other sources, such as the HTTP <c>accepts</c> header, the query string, &c.
    /// </remarks>
    public string TopicView { get; }

    /*==========================================================================================================================
    | METHOD: EXECUTE RESULT (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Called by the framework to execute the current <see cref="TopicViewResult"/>.
    /// </summary>
    /// <param name="context">The <see cref="ActionContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> representing the current state of execution.</returns>
    public override async Task ExecuteResultAsync(ActionContext context) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(context != null, nameof(context));

      /*------------------------------------------------------------------------------------------------------------------------
      | Call associated executor
      \-----------------------------------------------------------------------------------------------------------------------*/
      var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<TopicViewResult>>();
      await executor.ExecuteAsync(context, this);

    }

  } //Class
} //Namespace

/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using OnTopic.Internal.Diagnostics;
using OnTopic.Metadata;

namespace OnTopic.AspNetCore.Mvc {

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
      ViewDataDictionary        viewData,
      ITempDataDictionary?      tempData,
      object                    viewModel,
      string?                   contentType                     = null,
      string?                   view                            = null
    ) : base() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(viewData, nameof(viewData));
      Contract.Requires(viewModel, nameof(viewModel));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set local variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      ViewData                  = viewData;
      TempData                  = tempData;
      ViewData.Model            = viewModel;
      TopicContentType          = contentType?? TopicContentType;
      TopicView                 = view ?? ContentType;
      //ViewName                = TopicView;

    }

    /*==========================================================================================================================
    | PROPERTY: TOPIC CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents the <see cref="ContentTypeDescriptor"/> name associated with the current <see cref="Topic"/>.
    /// </summary>
    /// <remarks>
    ///   The preferred nomenclature for the name of a <see cref="ContentTypeDescriptor"/> is simply <c>ContentType</c>. The
    ///   base <see cref="ViewResult"/> class has an existing <see cref="ViewResult.ContentType"/> property representing the
    ///   HTTP response value, however. As such, <see cref="TopicContentType"/> is used to disambiguate the terms.
    /// </remarks>
    public string TopicContentType { get; } = "Page";

    /*==========================================================================================================================
    | PROPERTY: TOPIC VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents the <see cref="Topic.View"/> property. This may be overwritten later by a variety of sources.
    /// </summary>
    /// <remarks>
    ///   The associated <see cref="TopicViewResultExecutor"/> will fall back to the <see cref="TopicView"/> if the view isn't
    ///   set via other sources, such as the HTTP <c>accepts</c> header, the query string, etc.
    /// </remarks>
    public string TopicView { get; }

    /*==========================================================================================================================
    | METHOD: EXECUTE RESULT (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public override async Task ExecuteResultAsync(ActionContext context) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(context, nameof(context));

      /*------------------------------------------------------------------------------------------------------------------------
      | Call associated executor
      \-----------------------------------------------------------------------------------------------------------------------*/
      var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<TopicViewResult>>();
      await executor.ExecuteAsync(context, this).ConfigureAwait(false);

    }

  } //Class
} //Namespace
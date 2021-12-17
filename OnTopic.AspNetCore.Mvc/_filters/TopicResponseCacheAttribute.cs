/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using OnTopic.AspNetCore.Mvc.Controllers;
using OnTopic.Attributes;

namespace OnTopic.AspNetCore.Mvc {

  /*============================================================================================================================
  | CLASS: RESPONSE CACHE FILTER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   When applied to a <see cref="TopicController"/>—or derived class—will apply any configured <c>CacheProfile</c> reference
  ///   associated with the current topic.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The <c>Page</c> content type has a topic reference to a <c>CacheProfile</c> content type, which contains settings for
  ///     configuring HTTP response headers. The <see cref="TopicResponseCacheAttribute"/> evaluates the current <see cref="Topic"/> to
  ///     determine which, if any, <c>CacheProfile</c> it is associated with, and applies the settings to the HTTP response
  ///     headers. If a <c>CacheProfile</c> is not configured, it will default to the <c>CacheProfile</c> with the <see cref="
  ///     Topic.Key"/> of <c>Default</c>.
  ///   </para>
  ///   <para>
  ///     This filter is enabled automatically when <see cref="ServiceCollectionExtensions.AddTopicSupport(IMvcBuilder)"/> is
  ///     configured. It is only applied to actions on controllers derived from <see cref="TopicController"/>, which is needed
  ///     in order to ensure access to the current <see cref="Topic"/> and a configured <see cref="ITopicRepository"/>.
  ///   </para>
  ///   <para>
  ///     If the ASP.NET Core Response Caching Middleware is configured via e.g. <see cref="ResponseCachingServicesExtensions
  ///     .AddResponseCaching(IServiceCollection)"/>, then the page content may be eligible for output caching, depending on the
  ///     configuration used. This allows the same processed page content to be used for multiple clients without the server
  ///     needing to rerender them. This reduces response time and CPU usages at a cost of increased memory.
  ///   </para>
  /// </remarks>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
  public sealed class TopicResponseCacheAttribute : ActionFilterAttribute {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private static              Topic?                          _defaultCacheProfile;

    /*==========================================================================================================================
    | EVENT: ON ACTION EXECUTING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void OnActionExecuting(ActionExecutingContext context) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(context, nameof(context));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate controller
      \-----------------------------------------------------------------------------------------------------------------------*/
      var controller            = context.Controller as TopicController;

      if (controller is null) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure default cache profile
      \-----------------------------------------------------------------------------------------------------------------------*/

      // Lookup the default cache profile for reference
      if (_defaultCacheProfile is null) {
        _defaultCacheProfile    = controller.TopicRepository.Load("Configuration:CacheProfiles:Default");
      }

      // Ensure the above lookup is only performed once per application
      if (_defaultCacheProfile is null) {
        _defaultCacheProfile    = new Topic("ImplicitDefault", "CacheProfile");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify cache profile
      \-----------------------------------------------------------------------------------------------------------------------*/
      var cacheProfile          = controller.CurrentTopic?.References.GetValue("CacheProfile")?? _defaultCacheProfile;

      // If the empty cache profile is returned
      if (cacheProfile.Key is "ImplicitDefault") {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var headers               = context.HttpContext.Response.Headers;
      var duration              = cacheProfile.Attributes.GetInteger("Duration");
      var location              = Enum.Parse<ResponseCacheLocation>(cacheProfile.Attributes.GetValue("Location")?? "None");
      var noStore               = cacheProfile.Attributes.GetBoolean("NoStore");
      var varyByHeader          = cacheProfile.Attributes.GetValue("VaryByHeader");
      var varyByQueryKeys       = cacheProfile.Attributes.GetValue("VaryByQueryKeys");

      /*------------------------------------------------------------------------------------------------------------------------
      | Exit if the cache profile is effectively empty
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (duration is 0 && location is 0 && !noStore && String.IsNullOrEmpty(varyByHeader + varyByQueryKeys)) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate metadata
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!noStore && duration is 0) {
        throw new InvalidOperationException(
          $"The {nameof(duration)} attribute must be set to a positive value if the {nameof(noStore)} attribute is not enabled."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Clear existing headers
      \-----------------------------------------------------------------------------------------------------------------------*/
      headers.Remove(HeaderNames.Vary);
      headers.Remove(HeaderNames.CacheControl);
      headers.Remove(HeaderNames.Pragma);

      /*------------------------------------------------------------------------------------------------------------------------
      | Vary by keys, if appropriate
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (varyByQueryKeys is not null) {
        var responseCachingFeature = context.HttpContext.Features.Get<IResponseCachingFeature>();
        if (responseCachingFeature == null) {
          throw new InvalidOperationException(
            "VaryByQueryKeys depends on the ASP.NET Response Caching Middleware, which is not currently configured."
          );
        }
        responseCachingFeature.VaryByQueryKeys = varyByQueryKeys.Split(',', StringSplitOptions.RemoveEmptyEntries);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set standard HTTP headers
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!string.IsNullOrEmpty(varyByHeader)) {
        headers[HeaderNames.Vary]                               = varyByHeader;
      }

      if (noStore) {
        headers[HeaderNames.CacheControl]                       = "no-store";
        if (location is ResponseCacheLocation.None) {
          headers.AppendCommaSeparatedValues(HeaderNames.CacheControl, "no-cache");
          headers[HeaderNames.Pragma]                           = "no-cache";
        }
        return;
      }

      if (location is ResponseCacheLocation.None) {
        headers[HeaderNames.Pragma]                             = "no-cache";
      }

      string? cacheControl                                      = location switch {
        ResponseCacheLocation.Any                               => "public",
        ResponseCacheLocation.Client                            => "private",
        ResponseCacheLocation.None                              => "no-cache",
        _                                                       => null
      };

      headers[HeaderNames.CacheControl]                         = $"{cacheControl},max-age={duration}";

    }

  } //Class
} //Namespace
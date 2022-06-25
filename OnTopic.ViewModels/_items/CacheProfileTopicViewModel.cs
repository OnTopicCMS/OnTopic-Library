/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: CACHE PROFILE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed model for feeding views with information about a <c>CacheProfile</c> topic, as referenced by
  ///   the <see cref="PageTopicViewModel"/> and its derivatives.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public record CacheProfileTopicViewModel: ItemTopicViewModel {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="CacheProfileTopicViewModel"/> with an <paramref name="attributes"/> dictionary.
    /// </summary>
    /// <param name="attributes">An <see cref="AttributeValueDictionary"/> of attribute values.</param>
    public CacheProfileTopicViewModel(AttributeValueDictionary attributes): base(attributes) {
      Contract.Requires(attributes, nameof(attributes));
      Duration                  = attributes.GetInteger(nameof(Duration));
      Location                  = attributes.GetValue(nameof(Location));
      NoStore                   = attributes.GetBoolean(nameof(NoStore));
      VaryByHeader              = attributes.GetValue(nameof(VaryByHeader));
      VaryByQueryKeys           = attributes.GetValue(nameof(VaryByQueryKeys));
    }

    /// <summary>
    ///   Initializes a new <see cref="CacheProfileTopicViewModel"/> with no parameters.
    /// </summary>
    public CacheProfileTopicViewModel() { }

    /*==========================================================================================================================
    | DURATION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the duration in seconds for which the response is cached. If this property is set, the "max-age" in the
    ///   "Cache-control" header is set in the response.
    /// </summary>
    public int? Duration { get; init; }

    /*==========================================================================================================================
    | LOCATION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the location where the data from a particular URL must be cached. If this property is set, the
    ///   "Cache-control" header is set in the response.
    /// </summary>
    public string? Location { get; init; }

    /*==========================================================================================================================
    | NO STORE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the value which determines whether the data should be stored or not. When set to true, it sets the
    ///   "Cache-control" header in the response to "no-store". Ignores the "Location" parameter for values other than "None".
    ///   Ignores the "Duration" parameter.
    /// </summary>
    public bool? NoStore { get; init; }

    /*==========================================================================================================================
    | VARY BY HEADER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets which header the cache should vary by, if appropriate.
    /// </summary>
    public string? VaryByHeader { get; init; }

    /*==========================================================================================================================
    | VARY BY QUERY KEYS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets which query string parameter keys the cache should vary by, if appropriate. Should be comma delimited.
    /// </summary>
    public string? VaryByQueryKeys { get; init; }

  } //Class
} //Namespace
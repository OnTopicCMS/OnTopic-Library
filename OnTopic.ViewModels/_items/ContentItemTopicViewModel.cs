/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: CONTENT ITEM TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed model for feeding views with information about a <c>ContentItem</c> topic, as used in the <see
  ///   cref="ContentListTopicViewModel"/> model, and its derivatives.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public record ContentItemTopicViewModel: ItemTopicViewModel {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="ContentItemTopicViewModel"/> with an <paramref name="attributes"/> dictionary.
    /// </summary>
    /// <param name="attributes">An <see cref="AttributeValueDictionary"/> of attribute values.</param>
    public ContentItemTopicViewModel(AttributeValueDictionary attributes): base(attributes) {
      Contract.Requires(attributes, nameof(attributes));
      Description               = attributes.GetValue(nameof(Description))!;
      LearnMoreUrl              = attributes.GetUri(nameof(LearnMoreUrl));
      ThumbnailImage            = attributes.GetUri(nameof(ThumbnailImage));
      Category                  = attributes.GetValue(nameof(Category));
    }

    /// <summary>
    ///   Initializes a new <see cref="ContentItemTopicViewModel"/> with no parameters.
    /// </summary>
    public ContentItemTopicViewModel() { }

    /*==========================================================================================================================
    | DESCRIPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the description; for Content Items, this is effectively the body.
    /// </summary>
    public string Description { get; init; } = default!;

    /*==========================================================================================================================
    | LEARN MORE URL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets an optional URL for additional information that should be linked to.
    /// </summary>
    public Uri? LearnMoreUrl { get; init; }

    /*==========================================================================================================================
    | THUMBNAIL IMAGE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets an optional path to a thumbnail image that should accompany the content item.
    /// </summary>
    public Uri? ThumbnailImage { get; init; }

    /*==========================================================================================================================
    | CATEGORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the category that the content item should be grouped under.
    /// </summary>
    public string? Category { get; init; }

  } //Class
} //Namespace
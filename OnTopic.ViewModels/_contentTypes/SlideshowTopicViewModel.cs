/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: SLIDESHOW TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed model for feeding views with information about a <c>Slideshow</c> item.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public record SlideshowTopicViewModel: ContentListTopicViewModel {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="SlideshowTopicViewModel"/> with an <paramref name="attributes"/> dictionary.
    /// </summary>
    /// <param name="attributes">An <see cref="AttributeValueDictionary"/> of attribute values.</param>
    public SlideshowTopicViewModel(AttributeValueDictionary attributes): base(attributes) {
      Contract.Requires(attributes, nameof(attributes));
      TransitionEffect          = attributes.GetValue(nameof(TransitionEffect));
    }

    /// <summary>
    ///   Initializes a new <see cref="SlideshowTopicViewModel"/> with no parameters.
    /// </summary>
    public SlideshowTopicViewModel() { }

    /*==========================================================================================================================
    | TRANSITION EFFECT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the transition effect to use when flipping between slides.
    /// </summary>
    /// <remarks>
    ///   The effective values for <see cref="TransitionEffect"/> are dependent on the library used for presenting the
    ///   slideshow. Typically, they will map to standard HTML5/CSS3 transition effects, but they could differ depending on the
    ///   implementation.
    /// </remarks>
    public string? TransitionEffect { get; init; }

  } //Class
} //Namespace
﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: LOOKUP LIST ITEM TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed model for feeding views with information about <c>LookupListItem</c> topic.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public record LookupListItemTopicViewModel: ItemTopicViewModel {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="LookupListItemTopicViewModel"/> with an <paramref name="attributes"/> dictionary.
    /// </summary>
    /// <param name="attributes">An <see cref="AttributeDictionary"/> of attribute values.</param>
    public LookupListItemTopicViewModel(AttributeDictionary attributes): base(attributes) { }

    /// <summary>
    ///   Initializes a new <see cref="LookupListItemTopicViewModel"/> with no parameters.
    /// </summary>
    public LookupListItemTopicViewModel() { }

  } //Class
} //Namespace
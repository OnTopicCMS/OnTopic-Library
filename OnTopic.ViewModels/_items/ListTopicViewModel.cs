/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using OnTopic.Mapping.Annotations;

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: LIST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed model for modeling a <c>List</c> topic, as used for nested topics.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Generally, the preferred way to model a nested topic list is to map it to an <see cref="IList"/>. In fact, when
  ///     mapping children, any topics of type <c>List</c> will be skipped. That said, there remain a couple of scenarios where
  ///     mapping a <c>List</c> may be necessary or appropriate, such as when cross-referencing nested topic located on another
  ///     topic.
  ///   </para>
  ///   <para>
  ///     Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///     default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///     are supplied for convenience to model factory default settings for out-of-the-box content types.
  ///   </para>
  /// </remarks>
  [Obsolete(
    $"There should not be a need for a LookupListItem view model. If these must be referenced, prefer using e.g. " +
    $"{nameof(MapAsAttribute)} to specify a common view model.",
    false
  )]
  public record ListTopicViewModel: ContentItemTopicViewModel {

  } //Class
} //Namespace
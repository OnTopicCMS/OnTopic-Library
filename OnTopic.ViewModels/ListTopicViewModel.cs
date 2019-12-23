/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: LIST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for modeling a nested topic list.
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
  public class ListTopicViewModel: ContentItemTopicViewModel {

  } //Class
} //Namespace
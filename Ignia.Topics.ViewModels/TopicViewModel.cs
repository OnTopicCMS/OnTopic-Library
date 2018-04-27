/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Mapping;

namespace Ignia.Topics.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for feeding views.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentative level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public class TopicViewModel: ITopicViewModelCore {

    public int Id { get; set; }
    public string Key { get; set; }
    public string ContentType { get; set; }
    [Recurse(Relationships.Parents)]
    public TopicViewModel Parent { get; set; }
    public string UniqueKey { get; set; }
    public string View { get; set; }
    public string Title { get; set; }
    public bool IsHidden { get; set; }
    public DateTime LastModified { get; set; }

  } //Class
} //Namespace

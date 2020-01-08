/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: MAP TO PARENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing views with the <see cref="MapToParentAttribute"/>.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class MapToParentTopicViewModel {

    [MapToParent(AttributePrefix = "")]
    public KeyOnlyTopicViewModel? Primary { get; set; } = new KeyOnlyTopicViewModel();

    [MapToParent(AttributePrefix = "Aliased")]
    public KeyOnlyTopicViewModel? Alternate { get; set; } = new KeyOnlyTopicViewModel();

    [MapToParent]
    public KeyOnlyTopicViewModel? Ancillary { get; set; } = new KeyOnlyTopicViewModel();

  } //Class
} //Namespace
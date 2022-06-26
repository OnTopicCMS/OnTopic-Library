/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

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
    public KeyOnlyTopicViewModel? Primary { get; set; } = new();

    [MapToParent(AttributePrefix = "Aliased")]
    public KeyOnlyTopicViewModel? Alternate { get; set; } = new();

    [MapToParent]
    public KeyOnlyTopicViewModel? Ancillary { get; set; } = new();

  } //Class
} //Namespace
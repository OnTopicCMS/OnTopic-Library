/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: INHERITED PROPERTY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple view model with two properties—one annotated with <see cref="InheritAttribute"/>, and the other
  ///   without.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class InheritedPropertyTopicViewModel {
    public string? Property { get; set; }

    [Inherit]
    public string? InheritedProperty { get; set; }

  } //Class
} //Namespace
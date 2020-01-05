/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: PROPERTY ALIAS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple view model with a single property (<see cref="PropertyAlias"/>) for mapping a property annotated with
  ///   a <see cref="AttributeKeyAttribute"/>.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class PropertyAliasTopicViewModel {

    [AttributeKey("Property")]
    public string? PropertyAlias { get; set; }

  } //Class
} //Namespace
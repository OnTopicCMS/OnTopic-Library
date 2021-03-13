/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: CONVERT PROPERTY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a data transfer object with two properties which are intended to map to source properties of data types which
  ///   are different, but where the <see cref="TopicMappingService"/> is capable of converting between them.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class ConvertPropertyViewModel {

    [AttributeKey("BooleanAsStringAttribute")]
    public bool BooleanAttribute { get; set; }

    [AttributeKey("NumericAttribute")]
    public string? NumericAsStringAttribute { get; set; }

  } //Class
} //Namespace
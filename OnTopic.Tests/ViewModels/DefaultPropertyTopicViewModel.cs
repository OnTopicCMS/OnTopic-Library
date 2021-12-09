/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: DEFAULT VALUE TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing views properties annotated with default value attributes.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class DefaultValueTopicViewModel {

    [DefaultValue("Default")]
    public string? DefaultString { get; set; }

    [DefaultValue(10)]
    public int DefaultInt { get; set; }

    [DefaultValue(true)]
    public bool DefaultBool { get; set; }

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: ATTRIBUTE TOPIC (INVALID)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom binding model with an invalid property—i.e., one that doesn't map to a valid attribute. An <see
  ///   cref="InvalidOperationException"/> should be thrown when it is mapped.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class InvalidAttributeTopicBindingModel : BasicTopicBindingModel {

    public InvalidAttributeTopicBindingModel(string key) : base(key, "Page") { }

    public string? UnmappedAttribute { get; set; }

  } //Class
} //Namespace
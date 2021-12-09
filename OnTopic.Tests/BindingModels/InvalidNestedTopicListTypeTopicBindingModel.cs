/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections;

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: NESTED TOPIC LIST TYPE TOPIC (INVALID)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom binding model with an invalid collection type—i.e., it implements a <see cref="Dictionary{TKey,
  ///   TValue}"/>, even though nested topics are expected to return a type implementing <see cref="IList"/>. An <see cref="
  ///   InvalidOperationException"/> should be thrown when it is mapped.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class InvalidNestedTopicListTypeTopicBindingModel : BasicTopicBindingModel {

    public InvalidNestedTopicListTypeTopicBindingModel(string key) : base(key, "ContentTypeDescriptor") { }

    public Dictionary<string, BasicTopicBindingModel> Attributes { get; } = new();

  } //Class
} //Namespace
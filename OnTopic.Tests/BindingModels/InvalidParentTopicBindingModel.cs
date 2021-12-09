/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: PARENT TOPIC (INVALID)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom binding model with an invalid <see cref="Parent"/> property. An <see
  ///   cref="InvalidOperationException"/> should be thrown when it is mapped.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class InvalidParentTopicBindingModel : BasicTopicBindingModel {

    public InvalidParentTopicBindingModel(string key) : base(key, "Page") { }

    public BasicTopicBindingModel? Parent { get; set; }

  } //Class
} //Namespace
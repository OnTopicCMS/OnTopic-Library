/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: DISABLED ATTRIBUTE TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom binding model with an invalid property—i.e., one that doesn't map to a valid attribute. It is,
  ///   however, decorated with the <see cref="DisableMappingAttribute"/> and, thus, should simply be ignored, instead of
  ///   throwing an <see cref="InvalidOperationException"/>.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class DisabledAttributeTopicBindingModel : BasicTopicBindingModel {

    public DisabledAttributeTopicBindingModel(string key) : base(key, "Page") { }

    [DisableMapping]
    public string? UnmappedAttribute { get; set; }

  } //Class
} //Namespace
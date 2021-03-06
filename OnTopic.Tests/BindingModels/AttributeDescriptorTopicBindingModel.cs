/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Metadata;

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: ATTRIBUTE DESCRIPTOR TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a minimal implementation of a custom topic binding model with a couple of scalar values mapping to properties
  ///   on the <see cref="AttributeDescriptor"/> content type.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class AttributeDescriptorTopicBindingModel : BasicTopicBindingModel {

    public AttributeDescriptorTopicBindingModel(string key, string attributeType) : base(key, attributeType) { }

    public string? Title { get; set; }

    public string? DefaultValue { get; set; }

    public bool IsRequired { get; set; }

  } //Class
} //Namespace
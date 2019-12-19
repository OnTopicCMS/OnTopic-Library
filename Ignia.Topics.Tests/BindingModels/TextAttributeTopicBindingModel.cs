/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Ignia.Topics.Metadata;
using Ignia.Topics.Metadata.AttributeTypes;

namespace Ignia.Topics.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: TEXT ATTRIBUTE (DESCRIPTOR)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a minimal implementation of a custom topic binding model with a couple of scalar values mapping to properties
  ///   on the <see cref="TextAttribute"/> content type.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class TextAttributeTopicBindingModel : AttributeDescriptorTopicBindingModel {

    public TextAttributeTopicBindingModel(string? key = null) : base(key, "TextAttribute") { }

  } //Class
} //Namespace
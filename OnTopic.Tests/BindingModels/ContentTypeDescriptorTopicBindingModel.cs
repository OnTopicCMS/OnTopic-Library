/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using OnTopic.Models;

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: CONTENT TYPE DESCRIPTOR TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a minimal implementation of a custom topic binding model with a relationship property.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class ContentTypeDescriptorTopicBindingModel : BasicTopicBindingModel {

    public ContentTypeDescriptorTopicBindingModel(string? key = null) : base(key, "ContentTypeDescriptor") { }

    public List<RelatedTopicBindingModel> ContentTypes { get; } = new List<RelatedTopicBindingModel>();

    public List<AttributeDescriptorTopicBindingModel> Attributes { get; } = new List<AttributeDescriptorTopicBindingModel>();

  } //Class
} //Namespace
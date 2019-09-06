/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ignia.Topics.Mapping;
using Ignia.Topics.Models;
using Ignia.Topics.Metadata;

namespace Ignia.Topics.Tests.BindingModels {

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

    public AttributeDescriptorTopicBindingModel(string? key = null) : base(key, "AttributeDescriptor") {}

    public string? Title { get; set; }

    public string? DefaultValue { get; set; }

    public bool IsRequired { get; set; }

  } //Class

} //Namespace

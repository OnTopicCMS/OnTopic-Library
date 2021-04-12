/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
using OnTopic.Models;
using OnTopic.Tests.ViewModels;

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: RELATIONSHIP BASE TYPE TOPIC (INVALID)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom binding model with an invalid base type for an association—i.e., one that doesn't implement the <see
  ///   cref="IAssociatedTopicBindingModel"/>. An <see cref="InvalidOperationException"/> should be thrown when it is mapped.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class InvalidRelationshipBaseTypeTopicBindingModel : BasicTopicBindingModel {

    public InvalidRelationshipBaseTypeTopicBindingModel(string key) : base(key, "ContentTypeDescriptor") { }

    public Collection<EmptyViewModel> ContentTypes { get; } = new();

  } //Class
} //Namespace
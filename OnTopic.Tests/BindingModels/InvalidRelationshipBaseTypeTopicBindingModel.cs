/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
using OnTopic.Models;
using OnTopic.ViewModels;

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: RELATIONSHIP BASE TYPE TOPIC (INVALID)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom binding model with an invalid base type for a relationship—i.e., one that doesn't implement the <see
  ///   cref="IRelatedTopicBindingModel"/>. An <see cref="InvalidOperationException"/> should be thrown when it is mapped.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class InvalidRelationshipBaseTypeTopicBindingModel : BasicTopicBindingModel {

    public InvalidRelationshipBaseTypeTopicBindingModel(string? key = null) : base(key, "ContentTypeDescriptor") { }

    public Collection<TopicViewModel> ContentTypes { get; } = new();

  } //Class
} //Namespace
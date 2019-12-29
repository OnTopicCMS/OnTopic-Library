/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Models;

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: REFERENCE NAME TOPIC (INVALID)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom binding model with an invalid reference name—i.e., one that doesn't end in <c>Id</c>. An <see
  ///   cref="InvalidOperationException"/> should be thrown when it is mapped.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class InvalidReferenceNameTopicBindingModel : BasicTopicBindingModel {

    public InvalidReferenceNameTopicBindingModel(string? key = null) : base(key, "Page") { }

    public RelatedTopicBindingModel TopicReference { get; } = new RelatedTopicBindingModel();

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Mapping.Annotations;
using OnTopic.Models;
using OnTopic.ViewModels;

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: REFERENCE TYPE TOPIC (INVALID)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom binding model with an invalid reference type—i.e., one that doesn't implement <see cref="
  ///   IAssociatedTopicBindingModel"/>. An <see cref="InvalidOperationException"/> should be thrown when it is mapped.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class InvalidReferenceTypeTopicBindingModel : BasicTopicBindingModel {

    public InvalidReferenceTypeTopicBindingModel(string? key = null) : base(key, "Page") { }

    public TopicViewModel BaseTopic { get; } = new();

  } //Class
} //Namespace
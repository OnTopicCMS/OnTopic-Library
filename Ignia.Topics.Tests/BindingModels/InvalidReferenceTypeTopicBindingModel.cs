/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ignia.Topics.Mapping.Annotations;
using Ignia.Topics.Models;
using Ignia.Topics.ViewModels;

namespace Ignia.Topics.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: REFERENCE TYPE TOPIC (INVALID)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom binding model with an invalid reference type—i.e., one that doesn't implement <see
  ///   cref="IRelatedTopicBindingModel"/>. An <see cref="InvalidOperationException"/> should be thrown when it is mapped.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class InvalidReferenceTypeTopicBindingModel : BasicTopicBindingModel {

    public InvalidReferenceTypeTopicBindingModel(string? key = null) : base(key, "Page") { }

    [AttributeKey("TopicId")]
    public TopicViewModel DerivedTopic { get; } = new TopicViewModel();

  } //Class

} //Namespace

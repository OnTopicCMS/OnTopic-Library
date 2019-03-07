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
using Ignia.Topics.Mapping;
using Ignia.Topics.Models;
using Ignia.Topics.ViewModels;

namespace Ignia.Topics.Tests.BindingModels {

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

    public InvalidRelationshipBaseTypeTopicBindingModel(string key = null) : base(key, "ContentTypeDescriptor") { }

    public List<TopicViewModel> ContentTypes { get; } = new List<TopicViewModel>();

  } //Class

} //Namespace

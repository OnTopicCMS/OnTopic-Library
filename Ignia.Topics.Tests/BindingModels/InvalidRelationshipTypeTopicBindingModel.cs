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
  | BINDING MODEL: RELATIONSHIP TYPE TOPIC (INVALID)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom binding model with an invalid <see cref="RelationshipType"/>—i.e., it refers to <see
  ///   cref="RelationshipType.NestedTopics"/>, even though the property is associated with a <see
  ///   cref="RelationshipType.Relationship"/>. An <see cref="InvalidOperationException"/> should be thrown when it is mapped.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class InvalidRelationshipTypeTopicBindingModel : BasicTopicBindingModel {

    public InvalidRelationshipTypeTopicBindingModel(string? key = null) : base(key, "ContentTypeDescriptor") { }

    [Relationship(RelationshipType.NestedTopics)]
    public List<RelatedTopicBindingModel> ContentTypes { get; } = new List<RelatedTopicBindingModel>();

  } //Class

} //Namespace

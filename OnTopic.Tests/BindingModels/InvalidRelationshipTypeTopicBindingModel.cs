/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
using OnTopic.Mapping.Annotations;
using OnTopic.ViewModels.BindingModels;

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: RELATIONSHIP TYPE TOPIC (INVALID)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom binding model with an invalid <see cref="CollectionType"/>—i.e., it refers to <see cref="
  ///   CollectionType.NestedTopics"/>, even though the property is associated with a <see cref="CollectionType.Relationship"/>.
  ///   An <see cref="InvalidOperationException"/> should be thrown when it is mapped.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class InvalidRelationshipTypeTopicBindingModel : BasicTopicBindingModel {

    public InvalidRelationshipTypeTopicBindingModel(string? key = null) : base(key, "ContentTypeDescriptor") { }

    [Collection(CollectionType.NestedTopics)]
    public Collection<RelatedTopicBindingModel> ContentTypes { get; } = new();

  } //Class
} //Namespace
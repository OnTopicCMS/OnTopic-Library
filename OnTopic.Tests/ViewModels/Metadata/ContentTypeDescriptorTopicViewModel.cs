/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels.Metadata {

  /*============================================================================================================================
  | VIEW MODEL: CONTENT TYPE DESCRIPTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing view models with properties that map to custom collections
  ///   on the source <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class ContentTypeDescriptorTopicViewModel {

    public List<AttributeDescriptorTopicViewModel> AttributeDescriptors { get; } = new();

    [Relationship(RelationshipType.MappedCollection)]
    [Follow(Relationships.None)]
    public List<ContentTypeDescriptorTopicViewModel> PermittedContentTypes { get; } = new();

  } //Class
} //Namespace
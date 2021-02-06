/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
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

    public Collection<AttributeDescriptorTopicViewModel> AttributeDescriptors { get; } = new();

    [Collection(CollectionType.MappedCollection)]
    [Follow(AssociationTypes.None)]
    public Collection<ContentTypeDescriptorTopicViewModel> PermittedContentTypes { get; } = new();

  } //Class
} //Namespace
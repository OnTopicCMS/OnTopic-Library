/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Ignia.Topics.Mapping.Annotations;
using Ignia.Topics.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Ignia.Topics.Tests.ViewModels {

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
  public class ContentTypeDescriptorTopicViewModel : PageTopicViewModel {

    public TopicViewModelCollection<TopicViewModel> AttributeDescriptors
      { get; } = new TopicViewModelCollection<TopicViewModel>();

    [Relationship(RelationshipType.MappedCollection)]
    [Follow(Relationships.None)]
    public TopicViewModelCollection<ContentTypeDescriptorTopicViewModel> PermittedContentTypes
      { get; } = new TopicViewModelCollection<ContentTypeDescriptorTopicViewModel>();

  } //Class
} //Namespace
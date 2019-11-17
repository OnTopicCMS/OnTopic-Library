/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using Ignia.Topics.Mapping.Annotations;
using Ignia.Topics.Models;
using Ignia.Topics.ViewModels;

namespace Ignia.Topics.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: SAMPLE TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing basic mapping rules, including scalar and collection-based
  ///   properties.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class SampleTopicViewModel : PageTopicViewModel {

    public string? Property { get; set; }

    [Inherit]
    public string? InheritedProperty { get; set; }

    [AttributeKey("Property")]
    public string? PropertyAlias { get; set; }

    public TopicViewModel? TopicReference { get; set; }

    [Follow(Relationships.Relationships)]
    public TopicViewModelCollection<PageTopicViewModel> Children { get; } = new TopicViewModelCollection<PageTopicViewModel>();

    [Follow(Relationships.Children)]
    public TopicViewModelCollection<PageTopicViewModel> Cousins { get; } = new TopicViewModelCollection<PageTopicViewModel>();

    public TopicViewModelCollection<PageTopicViewModel> Categories { get; } = new TopicViewModelCollection<PageTopicViewModel>();

    public Collection<Topic> Related { get; } = new Collection<Topic>();

    [Relationship("AmbiguousRelationship", Type = RelationshipType.IncomingRelationship)]
    public TopicViewModelCollection<TopicViewModel> RelationshipAlias { get; } = new TopicViewModelCollection<TopicViewModel>();

  } //Class

} //Namespace

/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Ignia.Topics.Mapping;
using Ignia.Topics.ViewModels;

namespace Ignia.Topics.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: MINIMUM VALUE PROPERTY TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing view properties annotated with minimum value attributes.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class MetadataLookupTopicViewModel : TopicViewModel {

    [Metadata("Categories")]
    public TopicViewModelCollection<TopicViewModel> Categories { get; } = new TopicViewModelCollection<TopicViewModel>();

  } //Class
} //Namespace

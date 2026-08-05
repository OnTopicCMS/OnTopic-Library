/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping;

namespace OnTopic.Tests.ViewModels;

/*==============================================================================================================================
| VIEW MODEL: CONCURRENT REFERENCE TOPIC
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a strongly typed data transfer object with two topic references, both intended to resolve to the same shared <see
///   cref="SharedConcurrentTopicViewModel"/>.
/// </summary>
/// <remarks>
///   <para>
///     Both references are mapped as properties, so the <see cref="TopicMappingService"/> resolves them concurrently within a
///     single mapping pass. When both point at the same topic, this drives two branches to map that topic to the same type at
///     once, which is a supported sibling concurrency scenario. The <see cref="MapAsAttribute"/> pins the mapped view model
///     type so the scenario does not depend on the shared topic's content type.
///   </para>
///   <para>
///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
///   </para>
/// </remarks>
public class ConcurrentReferenceTopicViewModel {

  [MapAs(typeof(SharedConcurrentTopicViewModel))]
  public SharedConcurrentTopicViewModel? FirstReference { get; set; }

  [MapAs(typeof(SharedConcurrentTopicViewModel))]
  public SharedConcurrentTopicViewModel? SecondReference { get; set; }

} //Class
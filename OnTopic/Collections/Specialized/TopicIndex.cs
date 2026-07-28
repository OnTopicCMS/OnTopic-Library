/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Concurrent;
using OnTopic.Repositories;

namespace OnTopic.Collections.Specialized;

/*==============================================================================================================================
| CLASS: TOPIC INDEX
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Represents a collection of <see cref="Topic"/> objects indexed by <see cref="Topic.Id"/>.
/// </summary>
/// <remarks>
///   Backed by <see cref="ConcurrentDictionary{TKey, TValue}"/>, allowing an <see cref="ITopicRepository"/> implementation to
///   share a single topic graph across concurrent callers, and most notably when caching is involved, since lazy loading
///   mutates the graph as callers read properties that aren't yet loaded. The index must tolerate concurrent reads and writes
///   regardless of which implementation is being used.
/// </remarks>
public class TopicIndex : ConcurrentDictionary<int, Topic> {

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="TopicCollection"/>.
  /// </summary>
  /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
  /// <remarks>
  ///   Unsaved <see cref="Topic"/> instances (<see cref="Topic.IsNew"/>) are skipped, since their <see cref="Topic.Id"/> is a
  ///   placeholder shared by every other unsaved topic, not a real identity, and so isn't a genuine collision. Any other
  ///   colliding <see cref="Topic.Id"/> reflects corrupt data and continues to throw; e.g., a bulk seed of corrupt data should
  ///   fail clearly. This is deliberately stricter than the tolerate semantics of the live index's attach and detach methods,
  ///   which must not throw in the middle of an operation.
  /// </remarks>
  public TopicIndex(IEnumerable<Topic>? topics = null) {
    if (topics is not null) {
      foreach (var topic in topics) {
        if (topic.IsNew) {
          continue;
        }
        if (!TryAdd(topic.Id, topic)) {
          throw new ArgumentException($"An item with the same key has already been added. Key: {topic.Id}", nameof(topics));
        }
      }
    }
  }

} //Class
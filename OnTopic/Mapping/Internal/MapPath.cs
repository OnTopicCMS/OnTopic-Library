/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Mapping.Internal;

/*==============================================================================================================================
| CLASS: MAP PATH
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Represents a single frame in the depth-first path of an in-progress mapping operation, tracking the <see cref="Topic.Id"/>
///   and target <see cref="Type"/> currently being constructed, along with a reference to the frame that preceded it.
/// </summary>
/// <remarks>
///   The <see cref="MapPath"/> allows the <see cref="TopicMappingService"/> to distinguish a genuine constructor cycle, in
///   which a topic is mapped to a type that is already being constructed higher up the same call chain, from sibling
///   concurrency, in which two independent branches happen to map the same topic to the same type at the same time. The former
///   is a true circular reference and must throw; the latter is benign, and the joiner should await the in-progress result.
/// </remarks>
/// <param name="topicId">The <see cref="Topic.Id"/> of the topic being mapped at this frame.</param>
/// <param name="type">The target <see cref="Type"/> being constructed at this frame.</param>
/// <param name="parent">The preceding <see cref="MapPath"/> frame, or <c>null</c> if this frame is the path root.</param>
internal sealed class MapPath(int topicId, Type type, MapPath? parent) {

  /*============================================================================================================================
  | PROPERTY: TOPIC ID
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="Topic.Id"/> of the topic being mapped at this frame.
  /// </summary>
  internal int                  TopicId                         { get; } = topicId;

  /*============================================================================================================================
  | PROPERTY: TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The target <see cref="Type"/> being constructed at this frame.
  /// </summary>
  internal Type                 Type                            { get; } = type;

  /*============================================================================================================================
  | PROPERTY: PARENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The preceding <see cref="MapPath"/> frame, or <c>null</c> if this frame is the root of the path.
  /// </summary>
  internal MapPath?             Parent                          { get; } = parent;

  /*============================================================================================================================
  | METHOD: CONTAINS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Determines whether the supplied <paramref name="topicId"/> and <paramref name="type"/> pair already appears anywhere on
  ///   the current path, indicating a constructor cycle.
  /// </summary>
  /// <param name="topicId">The <see cref="Topic.Id"/> to search for.</param>
  /// <param name="type">The target <see cref="Type"/> to search for.</param>
  /// <returns>Returns <c>true</c> if the pair is already on the path, and otherwise <c>false</c>.</returns>
  internal bool Contains(int topicId, Type type) {
    // Walk up the parent chain, comparing each frame against the requested pair
    for (var frame = this; frame is not null; frame = frame.Parent) {
      if (frame.TopicId == topicId && frame.Type == type) {
        return true;
      }
    }
    return false;
  }

} //Class
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Concurrent;

namespace OnTopic.Mapping.Internal {

  /*============================================================================================================================
  | CLASS: MAPPED TOPIC CACHE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a collection intended to track local caching of objects mapped using the <see cref="TopicMappingService"/>.
  /// </summary>
  internal class MappedTopicCache: ConcurrentDictionary<int, MappedTopicCacheEntry> {


  } //Class
} //Namespace
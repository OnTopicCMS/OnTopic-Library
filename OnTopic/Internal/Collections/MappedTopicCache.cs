/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Concurrent;
using OnTopic.Mapping;
using OnTopic.Internal.Mapping;

namespace OnTopic.Internal.Collections {

  /*============================================================================================================================
  | CLASS: MAPPED TOPIC CACHE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a collection intended to track local caching of objects mapped using the <see cref="TopicMappingService"/>.
  /// </summary>
  public class MappedTopicCache: ConcurrentDictionary<int, MappedTopicCacheEntry> {


  } //Class
} //Namespace
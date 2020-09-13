/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Internal.Mapping {

  /*============================================================================================================================
  | CLASS: MAPPED TOPIC CACHE ENTRY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides an entry to tracking an object mapped using the <see cref="TopicMappingService"/>.
  /// </summary>
  /// <remarks>
  ///   In addition to the actual <see cref="MappedTopic"/>, this also includes a <see cref="Relationships"/> property for
  ///   tracking what relationships were mapped to the <see cref="MappedTopic"/>.
  /// </remarks>
  public class MappedTopicCacheEntry {

    /*==========================================================================================================================
    | PROPERTY: MAPPED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the mapped object.
    /// </summary>
    public object MappedTopic { get; set; } = null!;

    /*==========================================================================================================================
    | PROPERTY: RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the relationships that the <see cref="MappedTopic"/> was mapped with.
    /// </summary>
    public Relationships Relationships { get; set; } = Relationships.None;

  } //Class
} //Namespace
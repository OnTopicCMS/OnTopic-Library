/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Repositories;

/*==============================================================================================================================
| ENUM: TOPIC PAYLOAD
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Specifies which data ensure is loaded on a <see cref="Topic"/>. Used as a parameter on <see cref="ITopicRepository"/>'s
///   <c>Load()</c> overloads to control how much data is fetched in the first place, and on <see cref="ITopicLazyLoader"/>'s
///   <c>Ensure()</c> method to specify which previously deferred data to fill on demand.
/// </summary>
/// <remarks>
///   <see cref="Children"/>, <see cref="ExtendedAttributes"/>, <see cref="Relationships"/>, <see cref="References"/>, and
///   <see cref="VersionHistory"/> all have lazy-loading fill paths via <see cref="ITopicLazyLoader"/>.
///   <para>
///     The default value for all <see cref="ITopicRepository"/> <c>Load</c> overloads is <see cref="None"/>, so lazy loading
///     is the default: Each boundary is fetched on demand via its autoloading accessor or an explicit <c>EnsureLoaded</c> call.
///     Callers that need everything up front may still request <see cref="All"/> explicitly. Indexed attributes are always
///     returned as part of the base graph and are not a separately lazy-loadable boundary.
///   </para>
/// </remarks>
[Flags]
public enum TopicPayload {

  /*----------------------------------------------------------------------------------------------------------------------------
  | NONE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   No additional payload is requested. Indexed attributes are always returned as part of the base graph; this value
  ///   represents the lean baseline, with all available by specifying the additional values.
  /// </summary>
  None                          = 0,

  /*----------------------------------------------------------------------------------------------------------------------------
  | CHILDREN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The topic's immediate children have not been fetched. Accessing <see cref="Topic.Children"/> will trigger an on-demand
  ///   load of exactly one level.
  /// </summary>
  Children                      = 1,

  /*----------------------------------------------------------------------------------------------------------------------------
  | EXTENDED ATTRIBUTES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Extended attributes are loaded alongside the indexed attributes.
  /// </summary>
  ExtendedAttributes            = 1 << 1,

  /*----------------------------------------------------------------------------------------------------------------------------
  | RELATIONSHIPS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Relationship targets are included.
  /// </summary>
  Relationships                 = 1 << 2,

  /*----------------------------------------------------------------------------------------------------------------------------
  | REFERENCES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Topic reference targets are included.
  /// </summary>
  References                    = 1 << 3,

  /*----------------------------------------------------------------------------------------------------------------------------
  | VERSION HISTORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Version history is included.
  /// </summary>
  VersionHistory                = 1 << 4,

  /*----------------------------------------------------------------------------------------------------------------------------
  | ALL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   All payload data. This ensures a comprehensive loading of all available data.
  /// </summary>
  All                           = Children | ExtendedAttributes | Relationships | References | VersionHistory,

} //Enum
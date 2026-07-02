/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Repositories;

/*==============================================================================================================================
| ENUM: LOAD BOUNDARIES
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Identifies one or more deferred boundaries on a <see cref="Topic"/> that have not yet been retrieved from the underlying
///   <see cref="ITopicRepository"/>. Used by <see cref="ITopicLoadResolver"/> to specify which boundaries to ensure are loaded.
/// </summary>
[Flags]
public enum LoadBoundaries {

  /*----------------------------------------------------------------------------------------------------------------------------
  | NONE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   No boundaries are requested. Passing this value to <see cref="ITopicLoadResolver.EnsureLoaded"/> will result in nothing
  ///   being loaded.
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
  ///   The topic's extended attributes have not been fetched. Accessing a deferred attribute on <see cref="Topic.Attributes"/>
  ///   will trigger an on-demand load of the entire extended attributes blob for the topic.
  /// </summary>
  ExtendedAttributes            = 2,

  /*----------------------------------------------------------------------------------------------------------------------------
  | RELATIONSHIPS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The topic's relationship targets have not been fully resolved. Accessing <see cref="Topic.Relationships"/> will trigger
  ///   an on-demand load of all relationships associated with the topic.
  /// </summary>
  Relationships                 = 4,

  /*----------------------------------------------------------------------------------------------------------------------------
  | REFERENCES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The topic's reference targets have not been fully resolved. Accessing <see cref="Topic.References"/> will trigger an
  ///   on-demand load of all references associated with the topic.
  /// </summary>
  References                    = 8,

  /*----------------------------------------------------------------------------------------------------------------------------
  | ALL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   All four deferred boundaries. Passing this flag to <see cref="ITopicLoadResolver.EnsureLoaded"/> ensures that every
  ///   deferred boundary on the topic is populated.
  /// </summary>
  All                           = Children | ExtendedAttributes | Relationships | References,

} //Enum
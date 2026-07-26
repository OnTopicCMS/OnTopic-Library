/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Associations;
using OnTopic.Collections;

namespace OnTopic.Repositories;

/*==============================================================================================================================
| INTERFACE: TOPIC BACKING ACCESSOR
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides direct access to the backing fields of a <see cref="Topic"/>, bypassing the autoloading property getters.
/// </summary>
/// <remarks>
///   <para>
///     <see cref="Topic"/> exposes <see cref="Topic.Children"/>, <see cref="Topic.Relationships"/>, and <see cref=
///     "Topic.References"/> as autoloading getters: Accessing them can trigger a synchronous <see cref=
///     "ITopicLazyLoader.EnsureLoaded(Topic, TopicPayload)"/> call. Repository and resolver infrastructure that reads or
///     writes these collections as part of a load or resolve operation must bypass those getters to avoid infinite loops.
///   </para>
///   <para>
///     <see cref="Topic"/> implements this interface via explicit interface implementations. Callers must cast to <see cref=
///     "ITopicBackingAccessor"/> to access the raw backing fields.
///   </para>
/// </remarks>
public interface ITopicBackingAccessor {

  /*============================================================================================================================
  | PROPERTY: CHILDREN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the raw <see cref="ChildTopicCollection"/> backing field, bypassing the autoloading getter.
  /// </summary>
  /// <remarks>
  ///   This is to be applied as explicit interface implementations; callers must cast to <see cref="ITopicBackingAccessor"/> to
  ///   access this member.
  /// </remarks>
  ChildTopicCollection Children { get; }

  /*============================================================================================================================
  | PROPERTY: RELATIONSHIPS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the raw <see cref="TopicRelationshipMultiMap"/> backing field, bypassing the autoloading getter.
  /// </summary>
  /// <remarks>
  ///   This is to be applied as explicit interface implementations; callers must cast to <see cref="ITopicBackingAccessor"/> to
  ///   access this member.
  /// </remarks>
  TopicRelationshipMultiMap Relationships { get; }

  /*============================================================================================================================
  | PROPERTY: REFERENCES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the raw <see cref="TopicReferenceCollection"/> backing field, bypassing the autoloading getter.
  /// </summary>
  /// <remarks>
  ///   This is to be applied as explicit interface implementations; callers must cast to <see cref="ITopicBackingAccessor"/> to
  ///   access this member.
  /// </remarks>
  TopicReferenceCollection References { get; }

  /*============================================================================================================================
  | PROPERTY: ATTRIBUTES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the raw <see cref="AttributeCollection"/> backing field, bypassing the autoloading getter.
  /// </summary>
  /// <remarks>
  ///   This is to be applied as explicit interface implementations; callers must cast to <see cref="ITopicBackingAccessor"/> to
  ///   access this member.
  /// </remarks>
  AttributeCollection Attributes { get; }

  /*============================================================================================================================
  | PROPERTY: VERSION HISTORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the raw <see cref="VersionHistoryCollection"/> backing field, bypassing the autoloading getter.
  /// </summary>
  /// <remarks>
  ///   This is to be applied as explicit interface implementations; callers must cast to <see cref="ITopicBackingAccessor"/> to
  ///   access this member.
  /// </remarks>
  VersionHistoryCollection VersionHistory { get; }

} //Interface
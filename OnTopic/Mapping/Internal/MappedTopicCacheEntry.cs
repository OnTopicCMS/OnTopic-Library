/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Annotations;

namespace OnTopic.Mapping.Internal {

  /*============================================================================================================================
  | CLASS: MAPPED TOPIC CACHE ENTRY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides an entry to tracking an object mapped using the <see cref="TopicMappingService"/>.
  /// </summary>
  /// <remarks>
  ///   In addition to the actual <see cref="MappedTopic"/>, this also includes a <see cref="Associations"/> property for
  ///   tracking what associations were mapped to the <see cref="MappedTopic"/>. This allows the <see cref="TopicMappingService"
  ///   /> to be update the cached object with any missing associations, which can be identified using the <see cref="
  ///   GetMissingAssociations(AssociationTypes)"/> method. In turn, the cache can then be updated to reflect those new
  ///   associations by using <see cref="AddMissingAssociations(AssociationTypes)"/>. This ensures that even if a topic has
  ///   already been mapped, its scope can be expanded without duplicating effort.
  /// </remarks>
  internal class MappedTopicCacheEntry {

    /*==========================================================================================================================
    | PROPERTY: MAPPED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the mapped object.
    /// </summary>
    internal object MappedTopic { get; set; } = null!;

    /*==========================================================================================================================
    | PROPERTY: IS INITIALIZING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Notes that the target type is currently being initialized.
    /// </summary>
    /// <remarks>
    ///   The <see cref="IsInitializing"/> property allows an entry to be pre-cached prior to the object being completed. This
    ///   allows the <see cref="TopicMappingService"/> to detect circular references within the object initialization sequence.
    ///   This is important because, unlikely property mapping where a cached reference can be returned, a circular reference
    ///   in constructor mapping is expected to throw an exception. By registering that an object is being initialized, the
    ///   <see cref="MappedTopicCache"/> is able to detect circuluar references during constructor mapping.
    /// </remarks>
    internal bool IsInitializing { get; set; }

    /*==========================================================================================================================
    | PROPERTY: ASSOCIATIONS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the associations that the <see cref="MappedTopic"/> was mapped with.
    /// </summary>
    internal AssociationTypes Associations { get; set; } = AssociationTypes.None;

    /*==========================================================================================================================
    | METHOD: GET MISSING ASSOCIATIONS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a target <paramref name="associations"/>, identifies any associations not covered by <see cref="Associations"/>
    ///   and returns them as a new <see cref="AssociationTypes"/> instance.
    /// </summary>
    internal AssociationTypes GetMissingAssociations(AssociationTypes associations) => Associations ^ (associations | Associations);

    /*==========================================================================================================================
    | METHOD: ADD MISSING ASSOCIATIONS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a target <paramref name="associations"/>, adds any missing <see cref="AssociationTypes"/> to the <see cref="
    ///   Associations"/> property.
    /// </summary>
    internal void AddMissingAssociations(AssociationTypes associations) => Associations = associations | Associations;

  } //Class
} //Namespace
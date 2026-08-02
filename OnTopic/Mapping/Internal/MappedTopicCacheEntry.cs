/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Annotations;

namespace OnTopic.Mapping.Internal;

/*==============================================================================================================================
| CLASS: MAPPED TOPIC CACHE ENTRY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides an entry to tracking an object mapped using the <see cref="TopicMappingService"/>.
/// </summary>
/// <remarks>
///   In addition to the actual <see cref="MappedTopic"/>, this also includes a <see cref="Associations"/> property for
///   tracking what associations were mapped to the <see cref="MappedTopic"/>. This allows the <see cref="TopicMappingService"/>
///   to expand the cached object with any missing associations. A caller may peek at the missing associations using the
///   <see cref="GetMissingAssociations(AssociationTypes)"/> method, or record them and receive the newly added subset in a
///   single atomic operation using <see cref="AddMissingAssociations(AssociationTypes)"/>, so that concurrent passes don't both
///   end up mapping the same associations. This ensures that even if a topic has already been mapped, its scope can be expanded
///   without duplicating effort.
/// </remarks>
internal sealed class MappedTopicCacheEntry {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              object                          _lock                           = new();

  /*============================================================================================================================
  | PROPERTY: MAPPED TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a reference to the mapped object.
  /// </summary>
  internal object MappedTopic   { get; set; } = null!;

  /*============================================================================================================================
  | PROPERTY: IS INITIALIZING
  \---------------------------------------------------------------------------------------------------------------------------*/
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
  internal bool IsInitializing  { get; set; }

  /*============================================================================================================================
  | PROPERTY: ASSOCIATIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a reference to the associations that the <see cref="MappedTopic"/> was mapped with.
  /// </summary>
  internal AssociationTypes Associations { get; set; } = AssociationTypes.None;

  /*============================================================================================================================
  | METHOD: GET MISSING ASSOCIATIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a target <paramref name="associations"/>, identifies any associations not covered by <see cref="Associations"/>
  ///   and returns them as a new <see cref="AssociationTypes"/> instance.
  /// </summary>
  /// <remarks>
  ///   This is intended as a quick hint to decide e.g., whether an expansion is needed at all, without any side effects. It
  ///   does not record the result; a caller that intends to map the missing associations should instead use <see cref=
  ///   "AddMissingAssociations(AssociationTypes)"/>, so that concurrent passes cannot both map the same associations.
  /// </remarks>
  internal AssociationTypes GetMissingAssociations(AssociationTypes associations) => Associations ^ (associations | Associations);

  /*============================================================================================================================
  | METHOD: ADD MISSING ASSOCIATIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a target <paramref name="associations"/>, adds any not already covered by <see cref="Associations"/> and returns
  ///   the subset that this call just added.
  /// </summary>
  /// <remarks>
  ///   This is the mutating counterpart to <see cref="GetMissingAssociations(AssociationTypes)"/>: It adds any associations
  ///   that aren't already covered, and reports which ones were added back to the caller so the caller knows which associations
  ///   to process. Because the delta is calculated and saved  under a single lock, two concurrent passes over the same cached
  ///   instance receive disjoint results, ensuring each association is mapped by exactly one caller. A caller that receives
  ///   <see cref="AssociationTypes.None"/> has nothing left to map and should return the cached instance.
  /// </remarks>
  internal AssociationTypes AddMissingAssociations(AssociationTypes associations) {
    lock (_lock) {
      var missing               = GetMissingAssociations(associations);
      Associations              = associations | Associations;
      return missing;
    }
  }

} //Class
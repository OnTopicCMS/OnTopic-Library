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
  private readonly              TaskCompletionSource            _completionSource               = new(TaskCreationOptions.RunContinuationsAsynchronously);

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
  | PROPERTY: COMPLETION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns a <see cref="Task"/> that completes once the <see cref="MappedTopic"/> has been constructed and registered, even
  ///   if not all properties have yet been mapped, thus allowing a second pass to await a first pass that is still initializing
  ///   the same entry, instead of duplicating its work or, worse yet, failing.
  /// </summary>
  /// <remarks>
  ///   The task is settled by <see cref="Complete(Object, AssociationTypes)"/> once the target has been constructed, or by
  ///   <see cref="Fault(Exception)"/> if construction throws, so a second pass awaiting it never hangs. It is settled during
  ///   registration, after the constructor runs but before the first pass maps the target's properties, so a second pass may
  ///   observe an instance whose constructor parameters are set but whose properties are not yet mapped. This early publication
  ///   is what allows a property-level circular reference to resolve to the cached (if partially populated) instance instead of
  ///   recursing indefinitely, while still catching constructor-level circular references.
  /// </remarks>
  internal Task                 Completion                      => _completionSource.Task;

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

  /*============================================================================================================================
  | METHOD: COMPLETE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Publishes the constructed <paramref name="viewModel"/> and its <paramref name="associations"/> as the entry's <see cref=
  ///   "MappedTopic"/> and <see cref="Associations"/>, and settles the <see cref="Completion"/> task, releasing any second pass
  ///   awaiting this entry.
  /// </summary>
  /// <remarks>
  ///   This is the sole writer of <see cref="MappedTopic"/> and the initial writer of <see cref="Associations"/>, so the
  ///   instance, its associations, and the completion signal are always published together under a single lock. Only the first
  ///   completion takes effect; a later duplicate registration is ignored, keeping the cached instance stable, while remaining
  ///   unobservable before the entry is completed.
  /// </remarks>
  /// <param name="viewModel">The constructed view model associated with the entry.</param>
  /// <param name="associations">The associations that the view model was mapped with.</param>
  internal void Complete(object viewModel, AssociationTypes associations) {
    lock (_lock) {
      if (!_completionSource.Task.IsCompleted) {
        MappedTopic             = viewModel;
        Associations            = associations;
        _completionSource.TrySetResult();
      }
    }
  }

  /*============================================================================================================================
  | METHOD: FAULT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Faults the <see cref="Completion"/> task with the supplied <paramref name="exception"/> so that any pass awaiting the
  ///   entry observes the failure instead of hanging when construction of the entry throws.
  /// </summary>
  /// <param name="exception">The exception that occurred while constructing the entry.</param>
  internal void Fault(Exception exception) => _completionSource.TrySetException(exception);

} //Class
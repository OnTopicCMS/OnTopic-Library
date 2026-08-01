/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels;

/*==============================================================================================================================
| VIEW MODEL: EXPANSION SHARED
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a view model that <see cref="ExpansionParentTopicViewModel"/> encounters more than once during a single mapping
///   operation, so that the second encounter triggers an association expansion pass (<c>mapAssociationsOnly</c>), while the
///   first only includes the properties.
/// </summary>
/// <remarks>
///   <para>
///     This has no association-typed members by design. Its content is <see cref="Categories"/>, an ungated (gate <see cref=
///     "AssociationTypes.None"/>) nested-topics collection, and <see cref="Key"/>, a "compatible" property (i.e., mapped
///     directly from a first-class property on <see cref="Topic"/>). Unlike a gated association, which the cache claims once
///     and its flag check then skips on later passes, neither of these is tied to an association, so an expansion pass would
///     redundantly remap both unless the mapper explicitly skips non-association work. <see cref="KeyMapCount"/> records how
///     many times <see cref="Key"/> is assigned, so a test can confirm the compatible property is not reassigned again during
///     the expansion pass.
///   </para>
///   <para>
///     This is only reachable in tandem with <see cref="ExpansionParentTopicViewModel"/>, whose two collections perform the
///     initial and expansion passes against a single, cached instance of this view model.
///   </para>
///   <para>
///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
///   </para>
/// </remarks>
public class ExpansionSharedTopicViewModel {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private                       int                             _keyMapCount;

  /*============================================================================================================================
  | PROPERTY: KEY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   A compatible property, mapped one-to-one from the source <see cref="Topic.Key"/>. Records each assignment via <see cref=
  ///   "KeyMapCount"/>.
  /// </summary>
  public string? Key {
    get;
    set {
      field = value;
      _keyMapCount++;
    }
  }

  /*============================================================================================================================
  | PROPERTY: KEY MAP COUNT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The number of times <see cref="Key"/> has been assigned by the mapping service.
  /// </summary>
  public int KeyMapCount => _keyMapCount;

  /*============================================================================================================================
  | PROPERTY: CATEGORIES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   An ungated nested-topics collection, mapped from the source topic's nested <c>Categories</c> container.
  /// </summary>
  public Collection<KeyOnlyTopicViewModel> Categories { get; } = new();

} //Class
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Collections;

namespace OnTopic.Querying {

  /*============================================================================================================================
  | CLASS: TOPIC COLLECTION (EXTENSIONS)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides extensions for querying <see cref="TopicCollection"/>, <see cref="KeyedTopicCollection"/>, and related
  ///   collections via the generic <see cref="IEnumerable{T}"/> interface.
  /// </summary>
  public static class TopicCollectionExtensions {

    /*==========================================================================================================================
    | METHOD: ANY DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether any of the <see cref="Topic"/> instances in the collection are marked as <see cref="Topic.
    ///   IsDirty()"/>.
    /// </summary>
    /// <remarks>
    ///   This does not determine if the collection itself is dirty—it only determines if any <see cref="Topic"/> instances in
    ///   the collection are <see cref="Topic.IsDirty()"/>. This distinction is important. For example, if a clean <see cref="
    ///   Topic"/> is added to the collection, then the collection will be dirty—but <see cref="AnyDirty(IEnumerable{Topic})"/>
    ///   will be false.
    /// </remarks>
    /// <param name="topics">The collection of <see cref="Topic"/> instances to operate against.</param>
    /// <returns>Returns <c>true</c> if any of the <see cref="Topic"/> instances are <see cref="Topic.IsDirty()"/>.</returns>
    public static bool AnyDirty(this IEnumerable<Topic> topics) => topics.Any(t => t.IsDirty(true));

    /*==========================================================================================================================
    | METHOD: ANY NEW?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether any of the <see cref="Topic"/> instances in the collection are marked as <see cref="Topic.IsNew"/>.
    /// </summary>
    /// <param name="topics">The collection of <see cref="Topic"/> instances to operate against.</param>
    /// <returns>Returns <c>true</c> if any of the <see cref="Topic"/> instances are <see cref="Topic.IsNew"/>.</returns>
    public static bool AnyNew(this IEnumerable<Topic> topics) => topics.Any(t => t.IsNew);

  }
}

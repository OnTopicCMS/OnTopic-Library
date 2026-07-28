/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Runtime.CompilerServices;
using OnTopic.Querying;
using OnTopic.Repositories;

namespace OnTopic.Collections.Specialized;

/*==============================================================================================================================
| CLASS: TOPIC INDEX REGISTRY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Maintains a single, live, incrementally maintained <see cref="TopicIndex"/> per topic graph, keyed by root <see cref=
///   "Topic"/>.
/// </summary>
/// <remarks>
///   <para>
///     The graph itself remains the source of truth; an entry is a disposable derivation of it, kept in sync incrementally as
///     topics are attached, detached, or assigned IDs, but recoverable at any time by discarding it and walking the graph again
///     via <see cref="TopicExtensions.FindAll(Topic)"/>. Keying by root <see cref="Topic"/> instance also provides isolation
///     for tests, as distinct graphs produce disjoint entries, with nothing to reset, and weak keys mean a released graph still
///     releases its index without any explicit lifetime management.
///   </para>
///   <para>
///     Callers never write to this registry directly; the public surface is the read-only <see cref=
///     "TopicExtensions.GetLiveTopicIndex(Topic)"/> accessor. The registry itself is maintained exclusively by <see cref=
///     "ChildTopicCollection"/>'s attach and detach hooks and the <see cref="Topic.Id"/>'s setter, which call the internal
///     members below as topics are attached, detached, or assigned a persisted identifier.
///   </para>
/// </remarks>
internal static class TopicIndexRegistry {

  /*============================================================================================================================
  | PRIVATE FIELDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  private static readonly ConditionalWeakTable<Topic, TopicIndex> _indexes = new();

  /*============================================================================================================================
  | METHOD: GET OR BUILD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the live <see cref="TopicIndex"/> for the graph rooted at <paramref name="rootTopic"/>, building and storing
  ///   it on first access.
  /// </summary>
  /// <remarks>
  ///   Seeded by walking the <paramref name="rootTopic"/>'s full tree via <see cref= "TopicExtensions.FindAll(Topic)"/>, which
  ///   never triggers a lazy load and includes topics under <c>NotLoaded</c> <see cref="ChildTopicCollection.LoadState"/>.
  ///   <paramref name="rootTopic"/> must genuinely be a root (i.e., <c>Parent is null</c>); callers reach this exclusively via
  ///   <see cref="TopicExtensions.GetLiveTopicIndex(Topic)"/>, which derives it from any node.
  /// </remarks>
  /// <param name="rootTopic">The root <see cref="Topic"/> of the graph whose live index should be returned.</param>
  internal static TopicIndex GetOrBuild(Topic rootTopic) => _indexes.GetValue(rootTopic, root => new(root.FindAll()));

  /*============================================================================================================================
  | METHOD: ON ATTACHED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Indexes <paramref name="child"/> and its physical subtree into the live index of <paramref name="parent"/>'s root,
  ///   assuming that index has been materialized.
  /// </summary>
  /// <remarks>
  ///   Called from <see cref="ChildTopicCollection.InsertItem(Int32, Topic)"/>, after the base insertion, with <paramref name=
  ///   "parent"/> being the collection's owner rather than <c><paramref name="child"/>.Parent</c>, as the latter is not yet set
  ///   when the insertion fires. <paramref name="child"/>'s own registry entry is removed first, in case it was itself a root
  ///   with a materialized index before this attach (e.g., loaded standalone, then merged under <paramref name="parent"/>'s
  ///   graph): Attaching it here means it's no longer a root, so that entry is now stale, and <see cref="GetOrBuild(Topic)"/>
  ///   would otherwise hand it back unchanged, rather than rebuilding, if <paramref name="child"/> were later detached and
  ///   became a root again. <see cref="Topic.IsNew"/> topics are skipped, since their placeholder <see cref="Topic.Id"/> is not
  ///   a genuine identity.
  /// </remarks>
  /// <param name="parent">The <see cref="Topic"/> that owns the collection <paramref name="child"/> was inserted into.</param>
  /// <param name="child">The <see cref="Topic"/> that was attached.</param>
  internal static void OnAttached(Topic parent, Topic child) {

    // The child may itself have been a root with a stale materialized index; always drop that entry
    _indexes.Remove(child);

    // Skip unless the parent's root already has a materialized index to maintain
    if (!_indexes.TryGetValue(parent.GetRootTopic(), out var index)) {
      return;
    }

    // Index the child and its physical subtree; tolerate-resident, since a duplicate id here reflects a benign re-attach
    foreach (var topic in child.FindAll()) {
      if (topic.IsNew) {
        continue;
      }
      index.TryAdd(topic.Id, topic);
    }

  }

  /*============================================================================================================================
  | METHOD: ON DETACHED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Removes <paramref name="child"/> and its physical subtree from the live index of <paramref name="parent"/>'s root, if
  ///   that index has been materialized.
  /// </summary>
  /// <remarks>
  ///   Called from <see cref="ChildTopicCollection.RemoveItem(Int32)"/>, before the base removal, with <paramref name="parent"
  ///   /> being the collection's owner. <paramref name="child"/>'s subtree must be computed while it is still reachable, hence
  ///   being place before the base is removed. <see cref="Topic.IsNew"/> topics are skipped, since they'll never had been
  ///   indexed originally, matching <see cref="OnAttached(Topic, Topic)"/>.
  /// </remarks>
  /// <param name="parent">
  ///   The <see cref="Topic"/> that owns the collection <paramref name="child"/> is being removed from.
  /// </param>
  /// <param name="child">The <see cref="Topic"/> being detached.</param>
  internal static void OnDetached(Topic parent, Topic child) {

    // Skip unless the parent's root already has a materialized index to maintain
    if (!_indexes.TryGetValue(parent.GetRootTopic(), out var index)) {
      return;
    }

    // Remove the child and its physical subtree
    foreach (var topic in child.FindAll()) {
      if (topic.IsNew) {
        continue;
      }
      index.TryRemove(topic.Id, out _);
    }

  }

  /*============================================================================================================================
  | METHOD: ON ID ASSIGNED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Indexes <paramref name="topic"/> into the live index of its own root, if that index has been materialized.
  /// </summary>
  /// <remarks>
  ///   Called from <see cref="Topic.Id"/>'s setter after a persisted identifier is assigned (i.e., following <see cref=
  ///   "ITopicRepository.Save(Topic, Boolean)"/>). The setter's guard permits re-setting the same value, so this may fire
  ///   repeatedly for a given topic; <c>TryAdd</c> covers that without an assumption that it fires once.
  /// </remarks>
  /// <param name="topic">The <see cref="Topic"/> that was just assigned a persisted <see cref="Topic.Id"/>.</param>
  internal static void OnIdAssigned(Topic topic) {
    if (_indexes.TryGetValue(topic.GetRootTopic(), out var index)) {
      index.TryAdd(topic.Id, topic);
    }
  }

  /*============================================================================================================================
  | METHOD: INVALIDATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Discards the materialized live index for the graph rooted at <paramref name="rootTopic"/>, if any; the next <see cref=
  ///   "GetOrBuild(Topic)"/> rebuilds it from scratch.
  /// </summary>
  /// <remarks>
  ///   Called from <see cref="ChildTopicCollection.ClearItems"/>: A bulk detach, where per-item bookkeeping via <see cref=
  ///   "OnDetached(Topic, Topic)"/> isn't worth the cost, relative to just rebuilding the index in this rare scenario.
  /// </remarks>
  /// <param name="rootTopic">The root <see cref="Topic"/> of the graph whose live index should be discarded.</param>
  internal static void Invalidate(Topic rootTopic) => _indexes.Remove(rootTopic);

} //Class
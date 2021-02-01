/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using OnTopic.Collections.Specialized;
using OnTopic.Internal.Diagnostics;
using OnTopic.Internal.Reflection;
using OnTopic.Repositories;

namespace OnTopic.References {

  /*============================================================================================================================
  | CLASS: TOPIC REFERENCE DICTIONARY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="Topic"/> objects associated with particular reference keys.
  /// </summary>
  public class TopicReferenceDictionary : IDictionary<string, Topic>, ITrackDirtyKeys {

    /*==========================================================================================================================
    | DISPATCHER
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly TopicPropertyDispatcher<ReferenceSetterAttribute, Topic> _topicPropertyDispatcher;

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    Topic                           _parent;
    readonly                    IDictionary<string, Topic>      _storage;
    readonly                    DirtyKeyCollection              _dirtyKeys;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicReferenceDictionary"/>.
    /// </summary>
    public TopicReferenceDictionary(Topic parent) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(parent, nameof(parent));

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize backing fields
      \-----------------------------------------------------------------------------------------------------------------------*/
      _parent                   = parent;
      _storage                  = new Dictionary<string, Topic>();
      _topicPropertyDispatcher  = new(parent);
      _dirtyKeys                = new();

    }

    /*==========================================================================================================================
    | COUNT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public int Count => _storage.Count;

    /*==========================================================================================================================
    | IS READ ONLY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /*==========================================================================================================================
    | IS FULLY LOADED?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether or not the collection was fully loaded from the persistence store.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     When loading an individual <see cref="Topic"/> or branch from the persistence store, it is possible that topic
    ///     references may not be fully available. In this scenario, updating topic references while e.g. deleting unmatched
    ///     relationships can result in unintended data loss. To account for this, the <see cref="IsFullyLoaded"/> property '
    ///     tracks whether a collection was fully loaded from the persistence store; if it wasn't, the <see cref="
    ///     ITopicRepository"/> should not deleted unmatched topic references.
    ///   </para>
    ///   <para>
    ///     The <see cref="IsFullyLoaded"/> property defaults to <c>true</c>. It should be set to <c>false</c> during the <see cref="
    ///     ITopicRepository.Load(String?, Topic?, Boolean)"/> method if any members of the collection cannot be mapped back to
    ///     a valid <see cref="Topic"/> reference in memory.
    ///   </para>
    /// </remarks>
    public bool IsFullyLoaded { get; set; } = true;

    /*==========================================================================================================================
    | ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public Topic this[string referenceKey] {
      get => _storage[referenceKey];
      set {

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate parameters
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Requires<ArgumentException>(
          value != _parent,
          "A topic reference may not point to itself."
        );

        /*----------------------------------------------------------------------------------------------------------------------
        | Enforce business logic
        >-----------------------------------------------------------------------------------------------------------------------
        | If the reference is eligible for business logic enforcement, but the business logic hasn't yet been enforce, skip
        | further processing and instead route the request through the associated property setter.
        \---------------------------------------------------------------------------------------------------------------------*/
        if (!_topicPropertyDispatcher.Enforce(referenceKey, value)) {
          return;
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Set dirty state
        \---------------------------------------------------------------------------------------------------------------------*/
        if (!_storage.TryGetValue(referenceKey, out var existing) || existing != value) {
          _dirtyKeys.MarkDirty(referenceKey);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Set topic reference
        \---------------------------------------------------------------------------------------------------------------------*/
        _storage[referenceKey] = value;

      }
    }

    /*==========================================================================================================================
    | KEYS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public ICollection<string> Keys => _storage.Keys;

    /*==========================================================================================================================
    | VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public ICollection<Topic> Values => _storage.Values;

    /*==========================================================================================================================
    | ADD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    void ICollection<KeyValuePair<string, Topic>>.Add(KeyValuePair<string, Topic> item) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(item, nameof(item));

      TopicFactory.ValidateKey(item.Key);

      Contract.Requires<ArgumentException>(
        item.Value != _parent,
        "A topic reference may not point to itself."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Enforce business logic
      >-------------------------------------------------------------------------------------------------------------------------
      | If the reference is eligible for business logic enforcement, but the business logic hasn't yet been enforce, skip
      | further processing and instead route the request through the associated property setter.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!_topicPropertyDispatcher.Enforce(item.Key, item.Value)) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Mark dirty
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!_storage.TryGetValue(item.Key, out var existing) || existing != item.Value) {
        _dirtyKeys.MarkDirty(item.Key);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle recipricol references
      \-----------------------------------------------------------------------------------------------------------------------*/
      item.Value.IncomingRelationships.SetTopic(item.Key, _parent, null, true);

      /*------------------------------------------------------------------------------------------------------------------------
      | Add item
      \-----------------------------------------------------------------------------------------------------------------------*/
      _storage.Add(item);

    }

    /// <inheritdoc/>
    public void Add(string key, Topic value) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(key, nameof(key));
      Contract.Requires(value, nameof(value));

      /*------------------------------------------------------------------------------------------------------------------------
      | Add item
      \-----------------------------------------------------------------------------------------------------------------------*/
      var self = this as ICollection<KeyValuePair<string, Topic>>;
      self.Add(new(key, value));

    }

    /*==========================================================================================================================
    | SET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a new topic reference—or updates one, if it already exists. If the value is <c>null</c>, and a value exits, it is
    ///   removed.
    /// </summary>
    public void SetTopic(string key, Topic? value, bool? markDirty = null) => SetTopic(key, value, markDirty, true);

    /// <summary>
    ///   Adds a new topic reference—or updates one, if it already exists. If the value is <c>null</c>, and a value exits, it is
    ///   removed.
    /// </summary>
    internal void SetTopic(string key, Topic? value, bool? markDirty, bool enforceBusinessLogic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish state
      \-----------------------------------------------------------------------------------------------------------------------*/
      var wasDirty = _dirtyKeys.IsDirty(key);

      /*------------------------------------------------------------------------------------------------------------------------
      | Register that business logic has already been enforced
      >-------------------------------------------------------------------------------------------------------------------------
      | We want to ensure that any attempt to set references that have corresponding (writable) properties use those properties,
      | thus enforcing business logic. In order to ensure this is enforced on all entry points exposed by IDictionary, and not
      | just SetTopic, the underlying interceptors (e.g., Add, Item) call the Enforce() method. If it returns false, they assume
      | the property set the value (e.g., by calling the internal SetTopic method with enforceBusinessLogic set to false).
      | Otherwise, the corresponding property will be called. The Register() method thus avoids a redirect loop in this
      | scenario. This, of course, assumes that properties are correctly written to call the enforceBusinessLogic parameter.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!enforceBusinessLogic) {
        _topicPropertyDispatcher.Register(key, value);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set value
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (value is null) {
        if (ContainsKey(key)) {
          Remove(key);
        }
      }
      else {
        this[key] = value;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set dirty state
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (wasDirty is false && markDirty is false) {
        _dirtyKeys.MarkClean(key);
      }

    }

    /*==========================================================================================================================
    | CLEAR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public void Clear() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Mark keys as dirty
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var item in _storage) {
        _dirtyKeys.MarkAs(item.Key, markDirty: !_parent.IsNew);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle recipricol references
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var item in _storage) {
        item.Value.IncomingRelationships.RemoveTopic(item.Key, _parent, true);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Call base method
      \-----------------------------------------------------------------------------------------------------------------------*/
      _storage.Clear();

    }

    /*==========================================================================================================================
    | CONTAINS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, Topic> item) => _storage.Contains(item);

    /*==========================================================================================================================
    | CONTAINS KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public bool ContainsKey(string key) => _storage.ContainsKey(key);

    /*==========================================================================================================================
    | COPY TO
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<string, Topic>[] array, int arrayIndex) => _storage.CopyTo(array, arrayIndex);

    /*==========================================================================================================================
    | GET ENUMERATOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => _storage.GetEnumerator();

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, Topic>> GetEnumerator() => _storage.GetEnumerator();

    /*==========================================================================================================================
    | REMOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    bool ICollection<KeyValuePair<string, Topic>>.Remove(KeyValuePair<string, Topic> item) =>
      Contains(item) && Remove(item.Key);

    /// <inheritdoc/>
    public bool Remove(string key) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(key, nameof(key));

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle existing
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (TryGetValue(key, out var existing)) {
        existing.IncomingRelationships.RemoveTopic(key, _parent, true);
        _dirtyKeys.MarkAs(key, markDirty: !_parent.IsNew);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Call base method
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _storage.Remove(key);

    }

    /*==========================================================================================================================
    | TRY/GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public bool TryGetValue(string key, out Topic value) => _storage.TryGetValue(key, out value!);

    /*==========================================================================================================================
    | GET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to retrieve a topic reference based on its <paramref name="key"/>; if it doesn't exist, returns null.
    /// </summary>
    public Topic? GetTopic(string key, bool inheritFromBase = true) {
      if (TryGetValue(key, out var existing)) {
        return existing;
      }
      else if (inheritFromBase) {
        return _parent.BaseTopic?.References.GetTopic(key);
      }
      return null;
    }

    /*==========================================================================================================================
    | IS DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public bool IsDirty() => _dirtyKeys.IsDirty();

    /// <inheritdoc/>
    public bool IsDirty(string key) => _dirtyKeys.IsDirty(key);

    /*==========================================================================================================================
    | MARK CLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public void MarkClean() => _dirtyKeys.MarkClean();

    /// <inheritdoc/>
    public void MarkClean(string key) => _dirtyKeys.MarkClean(key);

  } //Class
} //Namespace
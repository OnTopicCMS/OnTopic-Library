/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OnTopic.Internal.Diagnostics;
using OnTopic.Internal.Reflection;
using OnTopic.Repositories;

namespace OnTopic.Collections.Specialized {

  /*============================================================================================================================
  | CLASS: TRACKED COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="TrackedItem{T}"/> records, along with methods "updating" those records and working
  ///   with their <see cref="TrackedItem{T}.IsDirty"/> state.
  /// </summary>
  /// <remarks>
  ///   <see cref="TrackedItem{T}"/> records represent individual instances of values associated with a particular <see cref="
  ///   Topic"/>. The <see cref="Topic"/> class tracks these through e.g. its <see cref="Topic.Attributes"/> property. The <see
  ///   cref="TrackedCollection{TItem, TValue, TAttribute}"/> class provides a base class with methods for working with these
  ///   records, such as <see cref="IsDirty(String)"/>, for determining if a given record has been modified, or <see cref=
  ///   "SetValue(String, TValue?, Boolean?, DateTime?)"/> for creating or "updating" a record. (Records are
  ///   immutable, so updates actually involve cloning the record with updated values.)
  /// </remarks>
  public abstract class TrackedCollection<TItem, TValue, TAttribute> :
    KeyedCollection<string, TItem>, ITrackDirtyKeys
    where TItem: TrackedItem<TValue>, new()
    where TAttribute: Attribute
    where TValue : class
  {

    /*==========================================================================================================================
    | DISPATCHER
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly TopicPropertyDispatcher<TItem, TValue, TAttribute> _topicPropertyDispatcher;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TrackedCollection{TItem, TValue, TAttribute}"/> class.
    /// </summary>
    /// <param name="parentTopic">A reference to the topic that the current collection is bound to.</param>
    internal TrackedCollection(Topic parentTopic) : base(StringComparer.OrdinalIgnoreCase) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(parentTopic, nameof(parentTopic));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      AssociatedTopic           = parentTopic;
      _topicPropertyDispatcher  = new(parentTopic);

    }

    /*==========================================================================================================================
    | ASSOCIATED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The <see cref="Topic"/> that the current collection is associated with.
    /// </summary>
    /// <remarks>
    ///   This is used for operations that require inheritance or pass-through of <see cref="Topic"/> properies in order to e.g.
    ///   enforce business logic.
    /// </remarks>
    protected Topic AssociatedTopic { get; init; }

    /*==========================================================================================================================
    | PROPERTY: DELETED ITEMS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   When a <see cref="TrackedItem{T}"/> is deleted, keep track of it so that it can be marked for deletion when the <see
    ///   cref="Topic"/> is saved.
    /// </summary>
    /// <remarks>
    ///   As a performance enhancement, <see cref="ITopicRepository"/> implementations will only save topics that are marked as
    ///   <see cref="IsDirty()"/>. If a <see cref="TrackedItem{T}"/> is deleted, then it won't be marked as <see cref="
    ///   TrackedItem{T}.IsDirty"/>. If no other <see cref="TrackedItem{T}"/> instances were modified, then the <see cref="Topic
    ///   "/> won't get saved, and that <see cref="TrackedItem{T}.Value"/> won't be deleted. Further more, methods like the <see
    ///   cref="TopicRepository.GetUnmatchedAttributes(Topic)"/> method have no way of detecting the deletion of arbitrary
    ///   values—i.e., attributes that were deleted which don't correspond to attributes configured on the <see cref="Metadata.
    ///   ContentTypeDescriptor"/>. By tracking any deleted <see cref="TrackedItem{T}"/> instances, we ensure both scenarios can
    ///   be accounted for.
    /// </remarks>
    internal List<string> DeletedItems { get; } = new();

    /*==========================================================================================================================
    | METHOD: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/

    /// <inheritdoc/>
    public virtual bool IsDirty() => DeletedItems.Count > 0 || Items.Any(a => a.IsDirty);

    /// <summary>
    ///   Determine if a given <see cref="TrackedItem{T}"/> is marked as <see cref="TrackedItem{T}.IsDirty"/>. Will return <c>
    ///   false</c> if the <see cref="TrackedItem{T}.Key"/> cannot be found in the collection.
    /// </summary>
    /// <remarks>
    ///   This method is intended primarily for data storage providers, such as <see cref="ITopicRepository"/>, which may need
    ///   to determine the <see cref="TrackedItem{T}.IsDirty"/> state of a <see cref="TrackedItem{T}"/> prior to saving it to
    ///   the data storage medium. Because <see cref="TrackedItem{T}.IsDirty"/> is a state of the current <see cref="
    ///   TrackedItem{T}"/>, it does not support <c>inheritFromParent</c> or <c>inheritFromBase</c> (which otherwise default to
    ///   <c>true</c>).
    /// </remarks>
    /// <param name="key">The string identifier for the <see cref="TrackedItem{T}"/>.</param>
    /// <returns>
    ///   Returns <c>true</c> if the <see cref="TrackedItem{T}"/> is marked as <see cref="TrackedItem{T}.IsDirty"/>; otherwise
    ///   <c>false</c>.
    /// </returns>
    public bool IsDirty(string key) {
      if (!Contains(key)) {
        return false;
      }
      return this[key].IsDirty;
    }

    /*==========================================================================================================================
    | METHOD: MARK CLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/

    /// <inheritdoc/>
    public void MarkClean() => MarkClean((DateTime?)null);

    /// <summary>
    ///   Marks the collection—including all <see cref="TrackedItem{T}"/> instances—as clean, meaning they have been persisted
    ///   to the underlying <see cref="ITopicRepository"/>.
    /// </summary>
    /// <remarks>
    ///   This method is intended primarily for data storage providers, such as <see cref="ITopicRepository"/>, so that they can
    ///   mark the collection, and all <see cref="TrackedItem{T}"/> instances it contains, as clean. After this, <see cref="
    ///   IsDirty()"/> method will return <c>false</c> until any <see cref="TrackedItem{T}"/> instances are added, modified, or
    ///   removed.
    /// </remarks>
    /// <param name="version">
    ///   The <see cref="DateTime"/> value that the <see cref="TrackedItem{T}"/> was last saved. This corresponds to the <see
    ///   cref="Topic.VersionHistory"/>.
    /// </param>
    public void MarkClean(DateTime? version) {
      if (AssociatedTopic.IsNew) {
        return;
      }
      foreach (var trackedItem in Items.Where(a => a.IsDirty).ToArray()) {
        SetValue(trackedItem.Key, trackedItem.Value, false, false, version?? DateTime.UtcNow);
      }
      DeletedItems.Clear();
    }

    /// <inheritdoc/>
    public void MarkClean(string key) => MarkClean(key, null);

    /// <summary>
    ///   Marks an individual <see cref="TrackedItem{T}"/> as clean.
    /// </summary>
    /// <remarks>
    ///   This method is intended primarily for data storage providers, such as <see cref="ITopicRepository"/>, so that they can
    ///   mark an <see cref="TrackedItem{T}"/> as clean. After this, <see cref="IsDirty(String)"/> will return <c>false</c> for
    ///   that item until it is modified.
    /// </remarks>
    /// <param name="key">The string identifier for the <see cref="TrackedItem{T}"/>.</param>
    /// <param name="version">
    ///   The <see cref="DateTime"/> value that the <see cref="TrackedItem{T}"/> was last saved. This corresponds to the <see
    ///   cref="Topic.VersionHistory"/>.
    /// </param>
    public void MarkClean(string key, DateTime? version) {
      if (AssociatedTopic.IsNew) {
        return;
      }
      else if (Contains(key)) {
        var trackedItem         = this[key];
        if (trackedItem.IsDirty) {
          SetValue(trackedItem.Key, trackedItem.Value, false, false, version?? DateTime.UtcNow);
        }
      }
    }

    /*==========================================================================================================================
    | METHOD: GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    ///   Gets a <see cref="TrackedItem{T}"/> from the collection based on the <see cref="TrackedItem{T}.Key"/>.
    /// </summary>
    /// <param name="key">The string identifier for the <see cref="TrackedItem{T}"/>.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to recusrively search through <see cref="Topic.Parent"/>s in order to get the value.
    /// </param>
    /// <returns>The <typeparamref name="TValue"/> for the <typeparamref name="TItem"/>.</returns>
    public TValue? GetValue(string key, bool inheritFromParent = false) => GetValue(key, null, inheritFromParent);

    /// <summary>
    ///   Gets a <see cref="TrackedItem{T}"/> from the collection based on the <see cref="TrackedItem{T}.Key"/> with a specified
    ///   <paramref name="defaultValue"/>, an optional setting to enable <paramref name="inheritFromParent"/>, and an optional
    ///   setting for <paramref name="inheritFromBase"/>.
    /// </summary>
    /// <param name="key">The string identifier for the <see cref="TrackedItem{T}"/>.</param>
    /// <param name="defaultValue">A string value to which to fall back in the case the value is not found.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to recusrively search through <see cref="Topic.Parent"/>s in order to get the value.
    /// </param>
    /// <param name="inheritFromBase">
    ///   Boolean indicator nothing whether to search through any of the topic's <see cref="Topic.BaseTopic"/> topics in
    ///   order to get the value.
    /// </param>
    /// <returns>The <typeparamref name="TValue"/> for the <see cref="TrackedItem{T}"/>.</returns>
    [return: NotNullIfNotNull("defaultValue")]
    public TValue? GetValue(string key, TValue? defaultValue, bool inheritFromParent = false, bool inheritFromBase = true) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key), nameof(key));
      return GetValue(key, defaultValue, inheritFromParent, inheritFromBase ? 5 : 0);
    }

    /// <summary>
    ///   Gets a <see cref="TrackedItem{T}"/> from the collection based on the <see cref="TrackedItem{T}.Key"/> with a specified
    ///   <paramref name="defaultValue"/> and an optional number of <see cref="Topic.BaseTopic"/>s through whom to crawl to
    ///   retrieve an inherited value.
    /// </summary>
    /// <param name="key">The string identifier for the <see cref="TrackedItem{T}"/>.</param>
    /// <param name="defaultValue">
    ///   A <typeparamref name="TValue"/> to which to fall back in the case the value is not found.
    /// </param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to recusrively search through <see cref="Topic.Parent"/>s in order to get the value.
    /// </param>
    /// <param name="maxHops">The number of recursions to perform when attempting to get the value.</param>
    /// <returns>The <typeparamref name="TValue"/> value for the <typeparamref name="TItem"/>.</returns>
    /// <requires description="The key name must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !key.Contains(" ")
    /// </requires>
    /// <requires
    ///   description="The maximum number of hops should be a positive number." exception="T:System.ArgumentException">
    ///   maxHops &gt;= 0
    /// </requires>
    /// <requires
    ///   description="The maximum number of hops should not exceed 100." exception="T:System.ArgumentException">
    ///   maxHops &lt;= 100
    /// </requires>
    [return: NotNullIfNotNull("defaultValue")]
    internal virtual TValue? GetValue(string key, TValue? defaultValue, bool inheritFromParent, int maxHops) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      TopicFactory.ValidateKey(key, true);
      Contract.Requires<ArgumentOutOfRangeException>(maxHops >= 0, "The maximum number of hops should be a positive number.");
      Contract.Requires<ArgumentOutOfRangeException>(maxHops <= 100, "The maximum number of hops should not exceed 100.");

      TValue? value = null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up value from collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (Contains(key)) {
        value = this[key].Value;
      }

      if (value is not null && value.ToString().Length == 0) {
        value = null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up value from topic pointer
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (
        value is null &&
        maxHops > 0 &&
        BaseCollection is not null
      ) {
        value = BaseCollection.GetValue(key, null, false, maxHops - 1);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up value from parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (
        value is null &&
        inheritFromParent &&
        ParentCollection is not null
      ) {
        value = ParentCollection.GetValue(key, defaultValue, inheritFromParent);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return value, if found
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (value is not null) {
        return value;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Finally, return default
      \-----------------------------------------------------------------------------------------------------------------------*/
      return defaultValue;

    }

    /*==========================================================================================================================
    | METHOD: PARENT COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the corresponding <see cref="TrackedCollection{TItem, TValue, TAttribute}"/> on the <see cref=
    ///   "Topic.Parent"/>, if available.
    /// </summary>
    protected abstract TrackedCollection<TItem, TValue, TAttribute>? ParentCollection { get; }

    /*==========================================================================================================================
    | METHOD: BASE COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the corresponding <see cref="TrackedCollection{TItem, TValue, TAttribute}"/> on the <see cref=
    ///   "Topic.BaseTopic"/>, if available.
    /// </summary>
    protected abstract TrackedCollection<TItem, TValue, TAttribute>? BaseCollection { get; }

    /*==========================================================================================================================
    | METHOD: SET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper method that either adds a new <see cref="TrackedItem{T}"/> object or updates the value of an existing one,
    ///   depending on whether that value already exists.
    /// </summary>
    /// <remarks>
    ///   Working with records can be a bit cumbersome, and especially in determining if a value should be marked as <see cref="
    ///   TrackedItem{T}.IsDirty"/>, since that's based on a comparison with the previous value. The <see cref="SetValue(String,
    ///   TValue?, Boolean?, DateTime?)"/> method handles this logic for implementers, while simultaneously allowing callers to
    ///   explicitly set whether the <see cref="TrackedItem{T}"/> instances should be marked as dirty—via the <paramref name="
    ///   markDirty"/> parameter—and, optionally, what the <paramref name="version"/> should be.
    /// </remarks>
    /// <param name="key">The string identifier for the <see cref="TrackedItem{T}"/>.</param>
    /// <param name="value">The text value for the <see cref="TrackedItem{T}"/>.</param>
    /// <param name="markDirty">
    ///   Specified whether the value should be marked as <see cref="TrackedItem{T}.IsDirty"/>. By default, it will be marked as
    ///   dirty if the value is new or has changed from a previous value. By setting this parameter, that behavior is
    ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being
    ///   persisted to the data store on <see cref="Repositories.ITopicRepository.Save(Topic, Boolean)"/>.
    /// </param>
    /// <param name="version">
    ///   The <see cref="DateTime"/> value that the <see cref="TrackedItem{T}"/> was last modified. This is intended exclusively
    ///   for use when populating the topic graph from a persistent data store as a means of indicating the current version for
    ///   each <see cref="TrackedItem{T}"/>. This is used when e.g. importing values to determine if the existing value is newer
    ///   than the source value.
    /// </param>
    /// <requires
    ///   description="The key must be specified for the TrackedItem{T} key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols"
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    public virtual void SetValue(
      string key,
      TValue? value,
      bool? markDirty = null,
      DateTime? version = null
    )
      => SetValue(key, value, markDirty, true, version);

    /// <summary>
    ///   Internal helper method that either adds a new <see cref="TrackedItem{T}"/> object or updates the value of an existing
    ///   one, depending on whether that value already exists.
    /// </summary>
    /// <remarks>
    ///   When the <paramref name="enforceBusinessLogic"/> parameter is called, no attempt will be made to route the call
    ///   through the corresponding properties, if available. As such, this is intended specifically to be called by internal
    ///   properties as a means of avoiding the property being called again when a caller uses the property's setter directly.
    /// </remarks>
    /// <param name="key">The string identifier for the <see cref="TrackedItem{T}"/>.</param>
    /// <param name="value">The text value for the <see cref="TrackedItem{T}"/>.</param>
    /// <param name="enforceBusinessLogic">
    ///   Instructs the underlying code to call corresponding properties, if available, to ensure business logic is enforced.
    ///   This should be set to <c>false</c> if setting items from internal properties in order to avoid an infinite loop.
    /// </param>
    /// <param name="markDirty">
    ///   Specified whether the value should be marked as <see cref="TrackedItem{T}.IsDirty"/>. By default, it will be marked as
    ///   <c>true</c> if the value is new or has changed from a previous value. By setting this parameter, that behavior is
    ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being
    ///   persisted to the data store on <see cref="Repositories.ITopicRepository.Save(Topic, Boolean)"/>.
    /// </param>
    /// <param name="version">
    ///   The <see cref="DateTime"/> value that the attribute was last modified. This is intended exclusively for use when
    ///   populating the topic graph from a persistent data store as a means of indicating the current version for each
    ///   attribute. This is used when e.g. importing values to determine if the existing value is newer than the source value.
    /// </param>
    /// <requires
    ///   description="The key must be specified for the AttributeValue key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols"
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    internal void SetValue(
      string key,
      TValue? value,
      bool? markDirty,
      bool enforceBusinessLogic,
      DateTime? version = null
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      TopicFactory.ValidateKey(key, true);

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve original item
      \-----------------------------------------------------------------------------------------------------------------------*/
      TItem? originalItem = null;

      if (Contains(key)) {
        originalItem  = this[key];
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Update from business logic
      >-----------------------------------------------------------------------------------------------------------------------—
      | If the original values have already been applied, and SetValue() is being triggered a second time after enforcing
      | business logic, then use the original values, while applying any change in the value triggered by the business logic.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_topicPropertyDispatcher.IsRegistered(key, out var updatedItem)) {
        if (updatedItem.Value != value) {
          updatedItem = updatedItem with {
            Value               = value
          };
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Update existing item
      >-----------------------------------------------------------------------------------------------------------------------—
      | Because TrackedItem<T> is immutable, a new instance must be constructed to replace the previous version.
      \-----------------------------------------------------------------------------------------------------------------------*/
      else if (originalItem is not null) {
        var markAsDirty = originalItem.IsDirty;
        if (AssociatedTopic.IsNew) {
          markAsDirty = true;
        }
        else if (markDirty.HasValue) {
          markAsDirty = markDirty.Value;
        }
        else if (originalItem.Value != value) {
          markAsDirty = true;
        }
        updatedItem             = originalItem with {
          Value                 = value,
          IsDirty               = markAsDirty,
          LastModified          = version?? originalItem.LastModified
        };
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Ignore if null
      >-------------------------------------------------------------------------------------------------------------------------
      | ###NOTE JJC20200501: Null or empty values are treated as deletions, and are not persisted to the data store. With
      | existing values, these are written to ensure that the collection is marked as IsDirty, thus allowing previous values to
      | be overwritten. Non-existent values, however, should simply be ignored.
      \-----------------------------------------------------------------------------------------------------------------------*/
      else if (value is null || String.IsNullOrEmpty(value.ToString())) {
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Create new item
      \-----------------------------------------------------------------------------------------------------------------------*/
      else {
        updatedItem = new TItem() {
          Key                   = key,
          Value                 = value,
          IsDirty               = AssociatedTopic.IsNew || (markDirty ?? true),
          LastModified          = version?? DateTime.UtcNow
        };
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Register that business logic has already been enforced
      >-------------------------------------------------------------------------------------------------------------------------
      | We want to ensure that any attempt to set references that have corresponding (writable) properties use those properties,
      | thus enforcing business logic. In order to ensure this is enforced on all entry points exposed by ICollection, and not
      | just SetValue, the underlying interceptors (e.g., InsertItem, SetItem) call the Enforce() method. If it returns false,
      | they assume the property set the value (e.g., by calling the internal SetValue method with enforceBusinessLogic set to
      | false).  Otherwise, the corresponding property will be called. The Register() method thus avoids a redirect loop in this
      | scenario. This, of course, assumes that properties are correctly written to call the enforceBusinessLogic parameter.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!enforceBusinessLogic) {
        _topicPropertyDispatcher.Register(key, updatedItem);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Persist item to collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (updatedItem is null) {
        return;
      }
      else if (originalItem is not null) {
        this[IndexOf(originalItem)] = updatedItem;
      }
      else {
        Add(updatedItem);
      }

    }

    /*==========================================================================================================================
    | OVERRIDE: INSERT ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Intercepts all attempts to insert a new <see cref="TrackedItem{T}"/> into the collection, to ensure that local
    ///   business logic is enforced.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     If a settable property is available corresponding to the <see cref="TrackedItem{T}.Key"/>, the call should be routed
    ///     through that to ensure local business logic is enforced, if it hasn't already been enforced.
    ///   </para>
    ///   <para>
    ///     Compared to the base implementation, will throw a specific <see cref="ArgumentException"/> error if a duplicate key
    ///     is inserted. This conveniently provides the name of the <see cref="TrackedItem{T}.Key"/> so it's clear what key is
    ///     being duplicated.
    ///   </para>
    /// </remarks>
    /// <param name="index">The location that the <see cref="TrackedItem{T}"/> should be set.</param>
    /// <param name="item">The <see cref="TrackedItem{T}"/> object which is being inserted.</param>
    /// <exception cref="ArgumentException">
    ///   An <see cref="ArgumentException"/> is thrown if an <see cref="TrackedItem{T}"/> with the same <see cref="TrackedItem{T
    ///   }.Key"/> as the <paramref name="item"/> already exists.
    /// </exception>
    protected override void InsertItem(int index, TItem item) {
      Contract.Requires(item, nameof(item));
      if (AssociatedTopic.IsNew && !item.IsDirty) {
        item                    = item with {
          IsDirty               = true
        };
      }
      if (_topicPropertyDispatcher.Enforce(item.Key, item)) {
        if (!Contains(item.Key)) {
          base.InsertItem(index, item);
          if (DeletedItems.Contains(item.Key)) {
            DeletedItems.Remove(item.Key);
          }
        }
        else {
          throw new ArgumentException(
            $"An {nameof(TItem)} with the Key '{item.Key}' already exists. The Value of the existing item is " +
            $"{this[item.Key].Value}; the new item's Value is '{item.Value}'. These {nameof(TItem)}s are associated " +
            $"with the {nameof(Topic)} '{AssociatedTopic.GetUniqueKey()}'.",
            nameof(item)
          );
        }
      }
    }

    /*==========================================================================================================================
    | OVERRIDE: SET ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Intercepts all attempts to update an <see cref="TrackedItem{T}"/> in the collection, to ensure that local business
    ///   logic is enforced.
    /// </summary>
    /// <remarks>
    ///   If a settable property is available corresponding to the <see cref="TrackedItem{T}.Key"/>, the call should be routed
    ///   through that to ensure local business logic is enforced, if it hasn't already been enforced.
    /// </remarks>
    /// <param name="index">The location that the <see cref="TrackedItem{T}"/> should be set.</param>
    /// <param name="item">The <see cref="TrackedItem{T}"/> object which is being inserted.</param>
    protected override void SetItem(int index, TItem item) {
      Contract.Requires(item, nameof(item));
      if (AssociatedTopic.IsNew && !item.IsDirty) {
        item                    = item with {
          IsDirty               = true
        };
      }
      if (_topicPropertyDispatcher.Enforce(item.Key, item)) {
        base.SetItem(index, item);
        if (DeletedItems.Contains(item.Key)) {
          DeletedItems.Remove(item.Key);
        }
      }
    }

    /*==========================================================================================================================
    | OVERRIDE: REMOVE ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Intercepts all attempts to remove an <see cref="TrackedItem{T}"/> from the collection, to ensure that it is
    ///   appropriately marked as <see cref="IsDirty()"/>.
    /// </summary>
    /// <remarks>
    ///   When an <see cref="TrackedItem{T}"/> is removed, <see cref="IsDirty()"/> will return true—even if no remaining <see
    ///   cref="TrackedItem{T}"/>s are marked as <see cref="TrackedItem{T}.IsDirty"/>.
    /// </remarks>
    protected override void RemoveItem(int index) {
      var trackedItem = this[index];
      if (!AssociatedTopic.IsNew) {
        DeletedItems.Add(trackedItem.Key);
      }
      base.RemoveItem(index);
    }

    /*==========================================================================================================================
    | OVERRIDE: CLEAR ITEMS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Intercepts all attempts to clear the <see cref="TrackedCollection{TItem, TValue, TAttribute}"/>, to ensure that it is
    ///   appropriately marked as <see cref="IsDirty()"/>.
    /// </summary>
    /// <remarks>
    ///   When an <see cref="TrackedItem{T}"/> is removed, <see cref="IsDirty()"/> will return true—even if no remaining <see
    ///   cref="TrackedItem{T}"/>s are marked as <see cref="TrackedItem{T}.IsDirty"/>.
    /// </remarks>
    protected override void ClearItems() {
      if (!AssociatedTopic.IsNew) {
        DeletedItems.AddRange(Items.Select(a => a.Key));
      }
      base.ClearItems();
    }

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the EntityCollection to extract the keys from the items.
    /// </summary>
    /// <param name="item">The <see cref="Topic"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override sealed string GetKeyForItem(TItem item) {
      Contract.Requires(item, "The item must be available in order to derive its key.");
      return item.Key;
    }

  } //Class
} //Namespace
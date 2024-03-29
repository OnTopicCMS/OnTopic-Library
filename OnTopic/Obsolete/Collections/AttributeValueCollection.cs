﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using OnTopic.Repositories;

#pragma warning disable IDE0060 // Remove unused parameter

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="AttributeValue"/> objects.
  /// </summary>
  /// <remarks>
  ///   <see cref="AttributeValue"/> objects represent individual instances of attributes associated with particular topics.
  ///   The <see cref="Topic"/> class tracks these through its <see cref="Topic.Attributes"/> property, which is an instance of
  ///   the <see cref="AttributeValueCollection"/> class.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  [Obsolete($"The {nameof(AttributeValueCollection)} class has been renamed to {nameof(AttributeCollection)}.", true)]
  public class AttributeValueCollection : KeyedCollection<string, AttributeValue> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="AttributeValueCollection"/> class.
    /// </summary>
    /// <remarks>
    ///   The <see cref="AttributeValueCollection"/> is intended exclusively for providing access to attributes via the
    ///   <see cref="Topic.Attributes"/> property. For this reason, the constructor is marked as internal.
    /// </remarks>
    /// <param name="parentTopic">A reference to the topic that the current attribute collection is bound to.</param>
    internal AttributeValueCollection(Topic parentTopic) : base(StringComparer.InvariantCultureIgnoreCase) {
       throw new NotImplementedException();
    }

    /*==========================================================================================================================
    | METHOD: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determine if <i>any</i> attributes in the <see cref="AttributeValueCollection"/> are dirty.
    /// </summary>
    /// <remarks>
    ///   This method is intended primarily for data storage providers, such as <see cref="ITopicRepository"/>, which may need
    ///   to determine if any attributes are dirty prior to saving them to the data storage medium. Be aware that this does
    ///   <i>not</i> track whether any <see cref="Topic.Relationships"/> have been modified; as such, it may still be necessary
    ///   to persist changes to the storage medium.
    /// </remarks>
    /// <param name="excludeLastModified">
    ///   Optionally excludes <see cref="AttributeValue"/>s whose keys start with <c>LastModified</c>. This is useful for
    ///   excluding the byline (<c>LastModifiedBy</c>) and dateline (<c>LastModified</c>) since these values are automatically
    ///   generated by e.g. the OnTopic Editor and, thus, may be irrelevant updates if no other attribute values have changed.
    /// </param>
    /// <returns>True if the attribute value is marked as dirty; otherwise false.</returns>
    public bool IsDirty(bool excludeLastModified = false) => throw new NotImplementedException();

    /// <summary>
    ///   Determine if a given attribute is marked as dirty. Will return false if the attribute key cannot be found.
    /// </summary>
    /// <remarks>
    ///   This method is intended primarily for data storage providers, such as <see cref="ITopicRepository"/>, which may need
    ///   to determine if a specific attribute key is dirty prior to saving it to the data storage medium. Because <c>IsDirty
    ///   </c> is a state of the current <see cref="AttributeValue"/>, it does not support <c>inheritFromParent</c> or <c>
    ///   inheritFromDerived</c> (which otherwise default to <c>true</c>).
    /// </remarks>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/>.</param>
    /// <returns>True if the attribute value is marked as dirty; otherwise false.</returns>
    public bool IsDirty(string name) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: MARK CLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Marks the collection—including all <see cref="AttributeValue"/> items—as clean, meaning they have been persisted to
    ///   the underlying <see cref="ITopicRepository"/>.
    /// </summary>
    /// <remarks>
    ///   This method is intended primarily for data storage providers, such as <see cref="ITopicRepository"/>, so that they can
    ///   mark the collection, and all <see cref="AttributeValue"/> items it contains, as clean. After this, <see cref="IsDirty(
    ///   Boolean)"/> will return <c>false</c> until any <see cref="AttributeValue"/> items are modified or removed.
    /// </remarks>
    /// <param name="version">
    ///   The <see cref="DateTime"/> value that the attributes were last saved. This corresponds to the <see cref="Topic.
    ///   VersionHistory"/>.
    /// </param>
    public void MarkClean(DateTime? version = null) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: MARK CLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Marks an individual <see cref="AttributeValue"/> as clean.
    /// </summary>
    /// <remarks>
    ///   This method is intended primarily for data storage providers, such as <see cref="ITopicRepository"/>, so that they can
    ///   mark an <see cref="AttributeValue"/> as clean. After this, <see cref="IsDirty(String)"/> will return <c>false</c> for
    ///   that item until it is modified.
    /// </remarks>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/>.</param>
    /// <param name="version">
    ///   The <see cref="DateTime"/> value that the attribute was last modified. This denotes the <see cref="Topic.
    ///   VersionHistory"/> associated with the specific attribute.
    /// </param>
    public void MarkClean(string name, DateTime? version = null) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a named attribute from the Attributes dictionary.
    /// </summary>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/>.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to search through the topic's parents in order to get the value.
    /// </param>
    /// <returns>The string value for the Attribute.</returns>
    [return: NotNull]
    public string GetValue(string name, bool inheritFromParent = false) => GetValue(name, "", inheritFromParent);

    /// <summary>
    ///   Gets a named attribute from the Attributes dictionary with a specified default value, an optional setting for enabling
    ///   of inheritance, and an optional setting for searching through derived topics for values.
    /// </summary>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/>.</param>
    /// <param name="defaultValue">A string value to which to fall back in the case the value is not found.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to search through the topic's parents in order to get the value.
    /// </param>
    /// <param name="inheritFromDerived">
    ///   Boolean indicator nothing whether to search through any of the topic's <see cref="Topic.DerivedTopic"/> topics in
    ///   order to get the value.
    /// </param>
    /// <returns>The string value for the Attribute.</returns>
    [return: NotNullIfNotNull("defaultValue")]
    public string? GetValue(string name, string? defaultValue, bool inheritFromParent = false, bool inheritFromDerived = true)
       => throw new NotImplementedException();

    /// <summary>
    ///   Gets a named attribute from the Attributes dictionary with a specified default value and an optional number of
    ///   <see cref="Topic.DerivedTopic"/>s through whom to crawl to retrieve an inherited value.
    /// </summary>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/>.</param>
    /// <param name="defaultValue">A string value to which to fall back in the case the value is not found.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to search through the topic's parents in order to get the value.
    /// </param>
    /// <param name="maxHops">The number of recursions to perform when attempting to get the value.</param>
    /// <returns>The string value for the Attribute.</returns>
    /// <requires description="The attribute name must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(name)
    /// </requires>
    /// <requires
    ///   description="The name should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !name.Contains(" ")
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
    internal string? GetValue(string name, string? defaultValue, bool inheritFromParent, int maxHops)
      => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: SET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper method that either adds a new <see cref="AttributeValue"/> object or updates the value of an existing one,
    ///   depending on whether that value already exists.
    /// </summary>
    /// <remarks>
    ///   Minimizes the need for defensive conditions throughout the library.
    /// </remarks>
    /// <param name="key">The string identifier for the AttributeValue.</param>
    /// <param name="value">The text value for the AttributeValue.</param>
    /// <param name="isDirty">
    ///   Specified whether the value should be marked as <see cref="AttributeValue.IsDirty"/>. By default, it will be marked as
    ///   dirty if the value is new or has changed from a previous value. By setting this parameter, that behavior is
    ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being
    ///   persisted to the data store on <see cref="Repositories.ITopicRepository.Save(Topic, Boolean, Boolean)"/>.
    /// </param>
    /// <param name="version">
    ///   The <see cref="DateTime"/> value that the attribute was last modified. This is intended exclusively for use when
    ///   populating the topic graph from a persistent data store as a means of indicating the current version for each
    ///   attribute. This is used when e.g. importing values to determine if the existing value is newer than the source value.
    /// </param>
    /// <param name="isExtendedAttribute">Determines if the attribute originated from an extended attributes data store.</param>
    /// <requires
    ///   description="The key must be specified for the AttributeValue key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   description="The value must be specified for the AttributeValue key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols"
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    public void SetValue(
      string key,
      string? value,
      bool? isDirty = null,
      DateTime? version = null,
      bool? isExtendedAttribute = null
    )
      => SetValue(key, value, isDirty, true, version, isExtendedAttribute);

    /// <summary>
    ///   Protected helper method that either adds a new <see cref="AttributeValue"/> object or updates the value of an existing
    ///   one, depending on whether that value already exists.
    /// </summary>
    /// <remarks>
    ///   When this overload is called, no attempt will be made to route the call through corresponding properties, if
    ///   available. As such, this is intended specifically to be called by internal properties as a means of avoiding a
    ///   feedback loop.
    /// </remarks>
    /// <param name="key">The string identifier for the AttributeValue.</param>
    /// <param name="value">The text value for the AttributeValue.</param>
    /// <param name="isDirty">
    ///   Specified whether the value should be marked as <see cref="AttributeValue.IsDirty"/>. By default, it will be marked as
    ///   dirty if the value is new or has changed from a previous value. By setting this parameter, that behavior is
    ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being
    ///   persisted to the data store on <see cref="Repositories.ITopicRepository.Save(Topic, Boolean, Boolean)"/>.
    /// </param>
    /// <param name="enforceBusinessLogic">
    ///   Instructs the underlying code to call corresponding properties, if available, to ensure business logic is enforced.
    ///   This should be set to false if setting attributes from internal properties in order to avoid an infinite loop.
    /// </param>
    /// <param name="version">
    ///   The <see cref="DateTime"/> value that the attribute was last modified. This is intended exclusively for use when
    ///   populating the topic graph from a persistent data store as a means of indicating the current version for each
    ///   attribute. This is used when e.g. importing values to determine if the existing value is newer than the source value.
    /// </param>
    /// <param name="isExtendedAttribute">Determines if the attribute originated from an extended attributes data store.</param>
    /// <requires
    ///   description="The key must be specified for the AttributeValue key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   description="The value must be specified for the AttributeValue key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols"
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    internal void SetValue(
      string key,
      string? value,
      bool? isDirty,
      bool enforceBusinessLogic,
      DateTime? version = null,
      bool? isExtendedAttribute = null
    ) => throw new NotImplementedException();

    /*==========================================================================================================================
    | OVERRIDE: INSERT ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Intercepts all attempts to insert a new <see cref="AttributeValue"/> into the collection, to ensure that local
    ///   business logic is enforced.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     If a settable property is available corresponding to the <see cref="AttributeValue.Key"/>, the call should be routed
    ///     through that to ensure local business logic is enforced. This is determined by looking for the "__" prefix, which is
    ///     set by the <see cref="SetValue(String, String, Boolean?, Boolean, DateTime?, Boolean?)"/>'s
    ///     <c>enforceBusinessLogic</c> parameter. To avoid an infinite loop, internal setters <i>must</i> call this overload.
    ///   </para>
    ///   <para>
    ///     Compared to the base implementation, will throw a specific <see cref="ArgumentException"/> error if a duplicate key
    ///     is inserted. This conveniently provides the name of the <see cref="AttributeValue.Key"/> so it's clear what key is
    ///     being duplicated.
    ///   </para>
    /// </remarks>
    /// <param name="index">The location that the <see cref="AttributeValue"/> should be set.</param>
    /// <param name="item">The <see cref="AttributeValue"/> object which is being inserted.</param>
    /// <exception cref="ArgumentException">
    ///   An AttributeValue with the Key '{item.Key}' already exists. The Value of the existing item is "{this[item.Key].Value};
    ///   the new item's Value is '{item.Value}'. These AttributeValues are associated with the Topic '{GetUniqueKey()}'."
    /// </exception>
    protected override void InsertItem(int index, AttributeValue item) => throw new NotImplementedException();

    /*==========================================================================================================================
    | OVERRIDE: SET ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Intercepts all attempts to update an <see cref="AttributeValue"/> in the collection, to ensure that local business
    ///   logic is enforced.
    /// </summary>
    /// <remarks>
    ///   If a settable property is available corresponding to the <see cref="AttributeValue.Key"/>, the call should be routed
    ///   through that to ensure local business logic is enforced. This is determined by looking for the "__" prefix, which is
    ///     set by the <see cref="SetValue(String, String, Boolean?, Boolean, DateTime?, Boolean?)"/>'s
    ///     <c>enforceBusinessLogic</c> parameter. To avoid an infinite loop, internal setters <i>must</i> call this overload.
    /// </remarks>
    /// <param name="index">The location that the <see cref="AttributeValue"/> should be set.</param>
    /// <param name="item">The <see cref="AttributeValue"/> object which is being inserted.</param>
    protected override void SetItem(int index, AttributeValue item) => throw new NotImplementedException();

    /*==========================================================================================================================
    | OVERRIDE: REMOVE ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Intercepts all attempts to remove an <see cref="AttributeValue"/> from the collection, to ensure that it is
    ///   appropriately marked as <see cref="IsDirty(Boolean)"/>.
    /// </summary>
    /// <remarks>
    ///   When an <see cref="AttributeValue"/> is removed, <see cref="IsDirty(Boolean)"/> will return true—even if no remaining
    ///   <see cref="AttributeValue"/>s are marked as <see cref="AttributeValue.IsDirty"/>.
    /// </remarks>
    protected override void RemoveItem(int index) => throw new NotImplementedException();

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the EntityCollection to extract the keys from the items.
    /// </summary>
    /// <param name="item">The <see cref="Topic"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(AttributeValue item) {
      Contract.Requires(item, "The item must be available in order to derive its key.");
      return item.Key;
    }

  } //Class
} //Namespace

#pragma warning restore IDE0060 // Remove unused parameter
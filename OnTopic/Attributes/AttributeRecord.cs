/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Collections.Specialized;
using OnTopic.Metadata;
using OnTopic.Repositories;

namespace OnTopic.Attributes {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE RECORD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents the immutable value of a particular attribute on a <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Provides values and metadata specific to individual attribute values, such as state (e.g., the <see cref="
  ///     TrackedRecord{T}.IsDirty"/> property signifies whether the attribute value has changed) and its <see cref="
  ///     TrackedRecord{T}.LastModified"/> date.
  ///   </para>
  ///   <para>
  ///     Typically, the <see cref="AttributeRecord"/> will be exposed as part of a <see cref="AttributeCollection"/> via the
  ///     <see cref="Topic.Attributes"/> collection.
  ///   </para>
  ///   <para>
  ///     Be aware that while <see cref="AttributeRecord"/> represents the value of a specific attribute, the metadata for
  ///     describing the purpose, constraints, and usage of that particular attribute is described by the <see cref="
  ///     AttributeDescriptor"/> class.
  ///   </para>
  ///   <para>
  ///     This class is immutable: once it is constructed, the values cannot be changed. To change a value, callers must either
  ///     create a new instance of the <see cref="AttributeRecord"/> class or, preferably, call the <see cref="Topic.Attributes
  ///     "/>'s <see cref="AttributeCollection.SetValue(String, String, Boolean?, DateTime?, Boolean?)"/> method.
  ///   </para>
  /// </remarks>
  public record AttributeRecord: TrackedRecord<string> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public AttributeRecord(): base() { }

    /// <summary>
    ///   Initializes a new instance of the <see cref="AttributeRecord"/> class, using the specified key/value pair.
    /// </summary>
    /// <param name="key">
    ///   The string identifier for the <see cref="AttributeRecord"/> collection item key/value pair.
    /// </param>
    /// <param name="value">
    ///   The string value text for the <see cref="AttributeRecord"/> collection item key/value pair.
    /// </param>
    /// <param name="isDirty">
    ///   An optional boolean indicator noting whether the <see cref="AttributeRecord"/> collection item is a new value, and
    ///   should thus be saved to the database when <see cref="ITopicRepository.Save(Topic, Boolean)"/> is next called.
    /// </param>
    /// <param name="lastModified">
    ///   The <see cref="DateTime"/> value that the attribute was last modified. This is intended primarily for use when
    ///   populating the topic graph from a persistent data store as a means of indicating the current version for each
    ///   attribute. This is used when e.g. importing values to determine if the existing value is newer than the source value.
    /// </param>
    /// <param name="isExtendedAttribute">Determines if the attribute originated from an extended attributes data store.</param>
    /// <requires
    ///   description="The key must be specified for the key/value pair." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    public AttributeRecord(
      string key,
      string? value,
      bool isDirty              = true,
      DateTime? lastModified    = null,
      bool? isExtendedAttribute = null
    ): base(key, value, isDirty, lastModified) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Set local values
      \-----------------------------------------------------------------------------------------------------------------------*/
      IsExtendedAttribute       = isExtendedAttribute;

    }

    /*==========================================================================================================================
    | PROPERTY: IS EXTENDED ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if this attribute originated from a data store as an extended attribute.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     How an attribute is stored in the underlying repository doesn't impact how the attribute is treated as part of the
    ///     object model. By tracking this, however, OnTopic is able to evaluate configuration mismatches during <see
    ///     cref="ITopicRepository.Save(Topic, Boolean)"/>. This allows the <see cref="ITopicRepository"/> to effective handle
    ///     scenarios where the configuration for an <see cref="AttributeDescriptor"/> has changed prior to the last time a <see
    ///     cref="Topic"/> was saved, and thus change the location where it is stored.
    ///   </para>
    ///   <para>
    ///     This is important because, otherwise, <see cref="ITopicRepository"/> implementations rely primarily on <see
    ///     cref="TrackedRecord{T}.IsDirty"/> to determine if a value should be saved. If an attribute's value hasn't changed,
    ///     but the location it should be stored has, that could potentially result in the attribute being deleted, as the
    ///     attribute won't show up for when <see cref="TopicRepository.GetAttributes"/> is called with <c>isDirty</c> set to
    ///     <c>true</c> and <c>isExtendedAttribute</c> is set to either <c>true</c> or <c>false</c>. By introducing <see cref="
    ///     IsExtendedAttribute"/>, the <see cref="TopicRepository"/> is able to detect conflicts between the configuration and
    ///     the underlying data store, and ensure data is stored appropriately.
    ///   </para>
    ///   <para>
    ///     The <see cref="IsExtendedAttribute"/> property maps to the <see cref="AttributeDescriptor.IsExtendedAttribute"/>
    ///     property. The former describes where the data was <i>actually</i> stored, whereas the latter describes where the
    ///     data <i>should</i> be stored.
    ///   </para>
    /// </remarks>
    public bool? IsExtendedAttribute { get; init; }

  } //Class
} //Namespace
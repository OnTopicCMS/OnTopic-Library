/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Collections.Specialized;
using OnTopic.Metadata;
using OnTopic.Repositories;

namespace OnTopic.References {

  /*============================================================================================================================
  | CLASS: TOPIC REFERENCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents the immutable value of a particular topic reference on a <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Provides values and metadata specific to individual attribute values, such as state (e.g., the <see cref="TrackedItem{
  ///     T}.IsDirty"/> property signifies whether the attribute value has changed) and its <see cref="TrackedItem{T}.
  ///     LastModified"/> date.
  ///   </para>
  ///   <para>
  ///     Typically, the <see cref="TopicReference"/> will be exposed as part of a <see cref="TopicReferenceCollection"/> via
  ///     the <see cref="Topic.References"/> collection.
  ///   </para>
  ///   <para>
  ///     Be aware that while <see cref="TopicReference"/> represents the value of a specific topic reference, the metadata for
  ///     describing the purpose, constraints, and usage of that particular attribute is described by the <see
  ///     cref="AttributeDescriptor"/> class.
  ///   </para>
  ///   <para>
  ///     This class is immutable: once it is constructed, the values cannot be changed. To change a value, callers must either
  ///     create a new instance of the <see cref="TopicReference"/> class or, preferably, call the <see cref="Topic.References"
  ///     />'s <see cref="TrackedCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, DateTime?)"/> method.
  ///   </para>
  /// </remarks>
  public record TopicReference: TrackedItem<Topic> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public TopicReference(): base() { }

    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicReference"/> class, using the specified key/value pair.
    /// </summary>
    /// <param name="key">
    ///   The string identifier for the <see cref="TopicReference"/> collection item key/value pair.
    /// </param>
    /// <param name="value">
    ///   The string value text for the <see cref="TopicReference"/> collection item key/value pair.
    /// </param>
    /// <param name="isDirty">
    ///   An optional boolean indicator noting whether the <see cref="TopicReference"/> collection item is a new value, and
    ///   should thus be saved to the database when <see cref="ITopicRepository.Save(Topic, Boolean)"/> is next called.
    /// </param>
    /// <param name="lastModified">
    ///   The <see cref="DateTime"/> value that the attribute was last modified. This is intended primarily for use when
    ///   populating the topic graph from a persistent data store as a means of indicating the current version for each
    ///   attribute. This is used when e.g. importing values to determine if the existing value is newer than the source value.
    /// </param>
    /// <requires
    ///   description="The key must be specified for the key/value pair." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    public TopicReference(
      string key,
      Topic value,
      bool isDirty              = true,
      DateTime? lastModified    = null
    ): base(key, value, isDirty, lastModified) {

    }

  } //Class
} //Namespace
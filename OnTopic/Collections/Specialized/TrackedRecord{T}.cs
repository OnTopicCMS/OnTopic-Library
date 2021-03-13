/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OnTopic.Attributes;
using OnTopic.Internal.Diagnostics;
using OnTopic.Repositories;

namespace OnTopic.Collections.Specialized {

  /*============================================================================================================================
  | CLASS: TRACKED RECORD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a base class for tracking versioned records, such as <see cref="AttributeRecord"/>.
  /// </summary>
  /// <remarks>
  ///   The <see cref="TrackedRecord{T}"/> class is comparable to the <see cref="KeyValuePair"/>, in that it tracks the <see
  ///   cref="Key"/> and <see cref="Value"/> for an item, but it additionally provides metadata related to the record, including
  ///   the <see cref="LastModified"/> and whether or not it <see cref="IsDirty"/>. This makes it easier for e.g. <see cref="
  ///   ITopicRepository"/> implementations to make more informed decisions about whether a record needs to be saved or
  ///   overwritten during a <see cref="ITopicRepository.Refresh"/> or <see cref="ITopicRepository.Rollback"/>.
  /// </remarks>
  public abstract record TrackedRecord<T> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    ///   Constructs a new instance of a <see cref="TrackedRecord{T}"/> class.
    /// </summary>
    protected TrackedRecord() {
      LastModified              = DateTime.UtcNow;
    }

    /// <summary>
    ///   Constructs a new instance of a <see cref="TrackedRecord{T}"/> class.
    /// </summary>
    /// <param name="key">The <see cref="Key"/> for the <see cref="TrackedRecord{T}"/> instance.</param>
    /// <param name="value">The <see cref="Value"/> for the <see cref="TrackedRecord{T}"/> instance.</param>
    /// <param name="isDirty">The optional <see cref="IsDirty"/> state for the <see cref="TrackedRecord{T}"/> instance.</param>
    /// <param name="lastModified">
    ///   The optional <see cref="LastModified"/> for the <see cref="TrackedRecord{T}"/> instance.
    /// </param>
    protected TrackedRecord(string key, T? value, bool isDirty = true, DateTime? lastModified = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      TopicFactory.ValidateKey(key, false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Set properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      Key                       = key;
      Value                     = value;
      IsDirty                   = isDirty;
      LastModified              = lastModified?? DateTime.UtcNow;

    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the <see cref="Key"/> for a given item.
    /// </summary>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    [NotNull]
    public string? Key { get; init; }

    /*==========================================================================================================================
    | PROPERTY: VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the <see cref="Value"/> for the given item.
    /// </summary>
    public T? Value { get; init; }

    /*==========================================================================================================================
    | PROPERTY: IS DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if the item has been modified relative to its persisted state.
    /// </summary>
    /// <remarks>
    ///   The <see cref="IsDirty"/> property is used by the <see cref="Repositories.ITopicRepository"/> to determine whether or
    ///   not the value has been persisted to the data store. If it is set to <c>true</c>, the item's value is sent to the
    ///   data store when <see cref="Repositories.ITopicRepository.Save(Topic, Boolean)"/> is called. Otherwise, it is ignored,
    ///   thus preventing the need to update records (or create new versions of records) whose values haven't changed.
    /// </remarks>
    public bool IsDirty { get; init; }

    /*==========================================================================================================================
    | PROPERTY: LAST MODIFIED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the <see cref="LastModified"/> for the given item.
    /// </summary>
    /// <remarks>
    ///   If loaded from a data store from e.g. <see cref="ITopicRepository.Load(Int32, Topic?, Boolean)"/>, the <see cref="
    ///   LastModified"/> should be set to the <c>Version</c>. If the <see cref="Value"/> is novel, however, then it should be
    ///   set to the current date. That won't be the same date established by <see cref="ITopicRepository.Save(Topic, Boolean)"
    ///   /> for the <c>Version</c>, however, which is why this property is labeled <see cref="LastModified"/>.
    /// </remarks>
    public DateTime LastModified { get; init; }

  } //Class
} //Namespace
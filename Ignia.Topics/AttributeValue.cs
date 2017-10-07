/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.Contracts;
using Ignia.Topics.Collections;
using Ignia.Topics.Repositories;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents the immutable value of a particular attribute on a <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Provides values and metadata specific to individual attribute values, such as state (e.g., the <see cref="IsDirty"/>
  ///     property signifies whether the attribute value has changed) and its <see cref="LastModified"/> date.
  ///   </para>
  ///   <para>
  ///     Typically, the <see cref="AttributeValue"/> will be exposed as part of a <see cref="AttributeValueCollection"/> via
  ///     the <see cref="Topic.Attributes"/> collection.
  ///   </para>
  ///   <para>
  ///     Be aware that while <see cref="AttributeValue"/> represents the value of a specific attribute, the metadata for
  ///     describing the purpose, constraints, and usage of that particular attribute is described by the <see
  ///     cref="Attribute"/> class.
  ///   </para>
  ///   <para>
  ///     This class is immutable: once it is constructed, the values cannot be changed. To change a value, callers must either
  ///     create a new instance of the <see cref="AttributeValue"/> class or, preferably, call the
  ///     <see cref="Topic.Attributes"/>'s <see cref="AttributeValueCollection.SetValue(String, String, Boolean?)"/> method.
  ///   </para>
  /// </remarks>
  public class AttributeValue {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     string                          _key                            = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="AttributeValue"/> class, using the specified key/value pair.
    /// </summary>
    /// <param name="key">
    ///   The string identifier for the <see cref="AttributeValue"/> collection item key/value pair.
    /// </param>
    /// <param name="value">
    ///   The string value text for the <see cref="AttributeValue"/> collection item key/value pair.
    /// </param>
    /// <param name="isDirty">
    ///   An optional boolean indicator noting whether the <see cref="AttributeValue"/> collection item is a new value, and
    ///   should thus be saved to the database when <see cref="ITopicRepository.Save(Topic, Boolean, Boolean)"/> is next called.
    /// </param>
    /// <requires
    ///   description="The key must be specified for the key/value pair." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    public AttributeValue(string key, string value, bool isDirty = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key));
      Topic.ValidateKey(key);

      /*------------------------------------------------------------------------------------------------------------------------
      | Set local values
      \-----------------------------------------------------------------------------------------------------------------------*/
      Key = key;
      Value = value;
      IsDirty = isDirty;
      EnforceBusinessLogic = true;

    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="AttributeValue"/> class, using the specified key/value pair.
    /// </summary>
    /// <param name="key">
    ///   The string identifier for the <see cref="AttributeValue"/> collection item key/value pair.
    /// </param>
    /// <param name="value">
    ///   The string value text for the <see cref="AttributeValue"/> collection item key/value pair.
    /// </param>
    /// <param name="isDirty">
    ///   An optional boolean indicator noting whether the <see cref="AttributeValue"/> collection item is a new value, and
    ///   should thus be saved to the database when <see cref="ITopicRepository.Save(Topic, Boolean, Boolean)"/> is next called.
    /// </param>
    /// <param name="enforceBusinessLogic">
    ///   If disabled, <see cref="AttributeValueCollection"/> will not call local properties on <see cref="Topic"/> that
    ///   correspond to the <paramref name="key"/> as a means of enforcing the business logic.
    /// </param>
    /// <requires
    ///   description="The key must be specified for the key/value pair." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    internal AttributeValue(string key, string value, bool isDirty, bool enforceBusinessLogic) : this(key, value, isDirty) {
      EnforceBusinessLogic = enforceBusinessLogic;
    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the key of the attribute.
    /// </summary>
    /// <requires description="The value from the getter must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    public string Key {
      get => _key;
      private set {
        Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value));
        Topic.ValidateKey(value);
        _key = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the current value of the attribute.
    /// </summary>
    public string Value {
      get;
      private set;
    }

    /*==========================================================================================================================
    | PROPERTY: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Boolean setting which is set automatically when an attribute's <see cref="Value"/> is set to a new value.
    /// </summary>
    /// <remarks>
    ///   The IsDirty property is used by the <see cref="Topics.Repositories.ITopicRepository"/> to determine whether or not
    ///   the value has been persisted to the database. If it is set to true, the attribute's value is sent to the database
    ///   when <see cref="Topics.Repositories.ITopicRepository.Save(Topic, Boolean, Boolean)"/> is called. Otherwise, it is ignored,
    ///   thus preventing the need to update attributes (or create new versions of attributes) whose values haven't changed.
    /// </remarks>
    public bool IsDirty {
      get;
      set;
    }

    /*==========================================================================================================================
    | PROPERTY: ENFORCE BUSINESS LOGIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether business logic should be enforced when adding an <see cref="AttributeValue"/> to an
    ///   <see cref="AttributeValueCollection"/>.
    /// </summary>
    /// <remarks>
    ///   By default, when a user attempts to update an attribute's value by calling
    ///   <see cref="AttributeValueCollection.SetValue(String, String, Boolean?)"/>, or when an <see cref="AttributeValue"/> is
    ///   added to the <see cref="AttributeValueCollection"/>, the <see cref="AttributeValueCollection"/> will automatically
    ///   attempt to call any corresponding setters on <see cref="Topic"/> (or a derived instance) to ensure that the business
    ///   logic is enforced. To avoid an infinite loop, however, this is disabled when properties on <see cref="Topic"/> call
    ///   <see cref="Topic.SetAttributeValue(String, String, Boolean?)"/>. When that happens, the
    ///   <see cref="EnforceBusinessLogic"/> value is set to false to communicate to the <see cref="AttributeValueCollection"/>
    ///   that it should not call the local property. This value is only intended for internal use.
    /// </remarks>
    /// <requires description="The value from the getter must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    internal bool EnforceBusinessLogic {
      get;
      set;
    }

    /*=========================================================================================================================
    | PROPERTY: LAST MODIFIED
    \------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Read-only reference to the last DateTime the <see cref="AttributeValue"/> instance was updated.
    /// </summary>
    public readonly DateTime LastModified = DateTime.Now;

  } // Class

} // Namespace

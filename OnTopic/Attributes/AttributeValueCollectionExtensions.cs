/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Globalization;
using OnTopic.Collections.Specialized;
using OnTopic.Internal.Diagnostics;
using OnTopic.Repositories;

namespace OnTopic.Attributes {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE COLLECTION (EXTENSIONS)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides extensions for setting and retrieving values from the <see cref="AttributeValueCollection"/> using strongly
  ///   typed values.
  /// </summary>
  public static class AttributeValueCollectionExtensions {

    /*==========================================================================================================================
    | METHOD: GET BOOLEAN VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a named attribute from the Attributes dictionary with a specified default value, an optional setting for enabling
    ///   of inheritance, and an optional setting for searching through base topics for values. Return as a boolean.
    /// </summary>
    /// <param name="attributes">The instance of the <see cref="AttributeValueCollection"/> this extension is bound to.</param>
    /// <param name="name">The string identifier for the <see cref="AttributeRecord"/>.</param>
    /// <param name="defaultValue">A string value to which to fall back in the case the value is not found.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to search through the topic's parents in order to get the value.
    /// </param>
    /// <param name="inheritFromBase">
    ///   Boolean indicator nothing whether to search through any of the topic's <see cref="Topic.BaseTopic"/>s in order to get
    ///   the value.
    /// </param>
    /// <returns>The value for the attribute as a boolean.</returns>
    public static bool GetBoolean(
      this                      AttributeValueCollection attributes,
      string                    name,
      bool                      defaultValue                    = default,
      bool                      inheritFromParent               = false,
      bool                      inheritFromBase                 = true
    ) {
      Contract.Requires(attributes);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(name), nameof(name));
      return Int32.TryParse(
        attributes.GetValue(
          name,
          defaultValue ? "1" : "0",
          inheritFromParent,
          inheritFromBase ? 5 : 0
        ),
        out var result
      ) ? result is 1 : defaultValue;
    }

    /*==========================================================================================================================
    | METHOD: GET INTEGER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a named attribute from the Attributes dictionary with a specified default value, an optional setting for enabling
    ///   of inheritance, and an optional setting for searching through base topics for values. Return as a integer.
    /// </summary>
    /// <param name="attributes">The instance of the <see cref="AttributeValueCollection"/> this extension is bound to.</param>
    /// <param name="name">The string identifier for the <see cref="AttributeRecord"/>.</param>
    /// <param name="defaultValue">A string value to which to fall back in the case the value is not found.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to search through the topic's parents in order to get the value.
    /// </param>
    /// <param name="inheritFromBase">
    ///   Boolean indicator nothing whether to search through any of the topic's <see cref="Topic.BaseTopic"/>s in order to get
    ///   the value.
    /// </param>
    /// <returns>The value for the attribute as an integer.</returns>
    public static int GetInteger(
      this                      AttributeValueCollection attributes,
      string                    name,
      int                       defaultValue                    = default,
      bool                      inheritFromParent               = false,
      bool                      inheritFromBase                 = true
    ) {
      Contract.Requires(attributes);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(name), nameof(name));
      return Int32.TryParse(
        attributes.GetValue(
          name,
          defaultValue.ToString(CultureInfo.InvariantCulture),
          inheritFromParent,
          inheritFromBase? 5 : 0
        ),
        out var result
      ) ? result : defaultValue;
    }

    /*==========================================================================================================================
    | METHOD: GET DOUBLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a named attribute from the Attributes dictionary with a specified default value, an optional setting for enabling
    ///   of inheritance, and an optional setting for searching through base topics for values. Return as a double.
    /// </summary>
    /// <param name="attributes">The instance of the <see cref="AttributeValueCollection"/> this extension is bound to.</param>
    /// <param name="name">The string identifier for the <see cref="AttributeRecord"/>.</param>
    /// <param name="defaultValue">A string value to which to fall back in the case the value is not found.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to search through the topic's parents in order to get the value.
    /// </param>
    /// <param name="inheritFromBase">
    ///   Boolean indicator nothing whether to search through any of the topic's <see cref="Topic.BaseTopic"/>s  in order to get
    ///   the value.
    /// </param>
    /// <returns>The value for the attribute as a double.</returns>
    public static double GetDouble(
      this                      AttributeValueCollection attributes,
      string                    name,
      double                    defaultValue                    = default,
      bool                      inheritFromParent               = false,
      bool                      inheritFromBase                 = true
    ) {
      Contract.Requires(attributes);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(name), nameof(name));
      return Double.TryParse(
        attributes.GetValue(
          name,
          defaultValue.ToString(CultureInfo.InvariantCulture),
          inheritFromParent,
          inheritFromBase? 5 : 0
        ),
        out var result
      ) ? result : defaultValue;
    }

    /*==========================================================================================================================
    | METHOD: GET DATETIME
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a named attribute from the Attributes dictionary with a specified default value, an optional setting for enabling
    ///   of inheritance, and an optional setting for searching through base topics for values. Return as a DateTime.
    /// </summary>
    /// <param name="attributes">The instance of the <see cref="AttributeValueCollection"/> this extension is bound to.</param>
    /// <param name="name">The string identifier for the <see cref="AttributeRecord"/>.</param>
    /// <param name="defaultValue">A string value to which to fall back in the case the value is not found.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to search through the topic's parents in order to get the value.
    /// </param>
    /// <param name="inheritFromBase">
    ///   Boolean indicator nothing whether to search through any of the topic's <see cref="Topic.BaseTopic"/>s in order to get
    ///   the value.
    /// </param>
    /// <returns>The value for the attribute as a DateTime object.</returns>
    public static DateTime GetDateTime(
      this                      AttributeValueCollection        attributes,
      string                    name,
      DateTime                  defaultValue                    = default,
      bool                      inheritFromParent               = false,
      bool                      inheritFromBase                 = true
    ) {
      Contract.Requires(attributes);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(name), nameof(name));
      return DateTime.TryParse(
        attributes.GetValue(
          name,
          defaultValue.ToString(CultureInfo.InvariantCulture),
          inheritFromParent,
          inheritFromBase ? 5 : 0
        ),
        out var result
      ) ? result : defaultValue;
    }

    /*==========================================================================================================================
    | METHOD: SET BOOLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper method that either adds a new <see cref="AttributeRecord"/> object or updates the value of an existing one,
    ///   depending on whether that value already exists.
    /// </summary>
    /// <param name="attributes">The instance of the <see cref="AttributeValueCollection"/> this extension is bound to.</param>
    /// <param name="key">The string identifier for the <see cref="AttributeRecord"/>.</param>
    /// <param name="value">The boolean value for the <see cref="AttributeRecord"/>.</param>
    /// <param name="isDirty">
    ///   Specified whether the value should be marked as <see cref="TrackedRecord{T}.IsDirty"/>. By default, it will be marked
    ///   as dirty if the value is new or has changed from a previous value. By setting this parameter, that behavior is
    ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being
    ///   persisted to the data store on <see cref="ITopicRepository.Save(Topic, Boolean)"/>.
    /// </param>
    /// <requires
    ///   description="The key must be specified for the AttributeRecord key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   description="The value must be specified for the AttributeRecord key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols"
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    public static void SetBoolean(
      this                      AttributeValueCollection        attributes,
      string                    key,
      bool                      value,
      bool?                     isDirty                         = null
    ) => attributes?.SetValue(key, value ? "1" : "0", isDirty);

    /*==========================================================================================================================
    | METHOD: SET INTEGER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper method that either adds a new <see cref="AttributeRecord"/> object or updates the value of an existing one,
    ///   depending on whether that value already exists.
    /// </summary>
    /// <param name="attributes">The instance of the <see cref="AttributeValueCollection"/> this extension is bound to.</param>
    /// <param name="key">The string identifier for the <see cref="AttributeRecord"/>.</param>
    /// <param name="value">The integer value for the <see cref="AttributeRecord"/>.</param>
    /// <param name="isDirty">
    ///   Specified whether the value should be marked as <see cref="TrackedRecord{T}.IsDirty"/>. By default, it will be marked
    ///   as dirty if the value is new or has changed from a previous value. By setting this parameter, that behavior is
    ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being
    ///   persisted to the data store on <see cref="ITopicRepository.Save(Topic, Boolean)"/>.
    /// </param>
    /// <requires
    ///   description="The key must be specified for the AttributeRecord key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   description="The value must be specified for the AttributeRecord key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols"
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    public static void SetInteger(
      this                      AttributeValueCollection        attributes,
      string                    key,
      int                       value,
      bool?                     isDirty                         = null
    ) => attributes?.SetValue(
      key,
      value.ToString(CultureInfo.InvariantCulture),
      isDirty
    );

    /*==========================================================================================================================
    | METHOD: SET DOUBLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper method that either adds a new <see cref="AttributeRecord"/> object or updates the value of an existing one,
    ///   depending on whether that value already exists.
    /// </summary>
    /// <param name="attributes">The instance of the <see cref="AttributeValueCollection"/> this extension is bound to.</param>
    /// <param name="key">The string identifier for the <see cref="AttributeRecord"/>.</param>
    /// <param name="value">The double value for the <see cref="AttributeRecord"/>.</param>
    /// <param name="isDirty">
    ///   Specified whether the value should be marked as <see cref="TrackedRecord{T}.IsDirty"/>. By default, it will be marked
    ///   as dirty if the value is new or has changed from a previous value. By setting this parameter, that behavior is
    ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being
    ///   persisted to the data store on <see cref="ITopicRepository.Save(Topic, Boolean)"/>.
    /// </param>
    /// <requires
    ///   description="The key must be specified for the AttributeRecord key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   description="The value must be specified for the AttributeRecord key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols"
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    public static void SetDouble(
      this                      AttributeValueCollection        attributes,
      string                    key,
      double                    value,
      bool?                     isDirty                         = null
    ) => attributes?.SetValue(
      key,
      value.ToString(CultureInfo.InvariantCulture),
      isDirty
    );

    /*==========================================================================================================================
    | METHOD: SET DATETIME
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper method that either adds a new <see cref="AttributeRecord"/> object or updates the value of an existing one,
    ///   depending on whether that value already exists.
    /// </summary>
    /// <param name="attributes">The instance of the <see cref="AttributeValueCollection"/> this extension is bound to.</param>
    /// <param name="key">The string identifier for the <see cref="AttributeRecord"/>.</param>
    /// <param name="value">The <see cref="DateTime"/> value for the <see cref="AttributeRecord"/>.</param>
    /// <param name="isDirty">
    ///   Specified whether the value should be marked as <see cref="TrackedRecord{T}.IsDirty"/>. By default, it will be marked
    ///   as dirty if the value is new or has changed from a previous value. By setting this parameter, that behavior is
    ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being
    ///   persisted to the data store on <see cref="ITopicRepository.Save(Topic, Boolean)"/>.
    /// </param>
    /// <requires
    ///   description="The key must be specified for the AttributeRecord key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   description="The value must be specified for the AttributeRecord key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols"
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    public static void SetDateTime(
      this                      AttributeValueCollection        attributes,
      string                    key,
      DateTime                  value,
      bool?                     isDirty                         = null
    ) => attributes?.SetValue(
      key,
      value.ToString(CultureInfo.InvariantCulture),
      isDirty
    );

  } //Class
} //Namespace
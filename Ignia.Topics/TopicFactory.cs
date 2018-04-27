/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: TOPIC FACTORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   A static class for centralizing logic for creating strongly-typed <see cref="Topic"/> instances based on their content
  ///   type.
  /// </summary>
  public static class TopicFactory {

    /*==========================================================================================================================
    | STATIC VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    static                      Dictionary<string, Type>        _typeLookup                     = new Dictionary<string, Type>();

    /*==========================================================================================================================
    | METHOD: GET TOPIC TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Static helper method for looking up a class type based on a string name.
    /// </summary>
    /// <remarks>
    ///   Currently, this method uses <see cref="Type.GetType()"/>, which can be non-performant. As such, this helper method
    ///   caches its results in a static lookup table keyed by the string value.
    /// </remarks>
    /// <param name="contentType">A string representing the key of the target content type.</param>
    /// <returns>A class type corresponding to a derived class of <see cref="Topic"/>.</returns>
    /// <requires description="The contentType key must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(contentType)
    /// </requires>
    /// <requires
    ///   decription="The contentType should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !contentType.Contains(" ")
    /// </requires>
    private static Type GetTopicType(string contentType) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(contentType));
      Contract.Ensures(Contract.Result<Type>() != null);
      TopicFactory.ValidateKey(contentType);

      /*----------------------------------------------------------------------------------------------------------------------
      | Return cached entry
      \---------------------------------------------------------------------------------------------------------------------*/
      if (_typeLookup.Keys.Contains(contentType)) {
        return _typeLookup[contentType];
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Determine if there is a matched type
      \---------------------------------------------------------------------------------------------------------------------*/
      var baseType = typeof(Topic);
      var targetType = Type.GetType("Ignia.Topics." + contentType);

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate type
      \---------------------------------------------------------------------------------------------------------------------*/
      if (targetType == null) {
        targetType = baseType;
      }
      else if (!targetType.IsSubclassOf(baseType)) {
        targetType = baseType;
        throw new ArgumentException("The topic \"Ignia.Topics." + contentType + "\" does not derive from \"Ignia.Topics.Topic\".");
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Cache findings
      \---------------------------------------------------------------------------------------------------------------------*/
      lock (_typeLookup) {
        if (_typeLookup.Keys.Contains(contentType)) {
          _typeLookup.Add(contentType, targetType);
        }
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return result
      \---------------------------------------------------------------------------------------------------------------------*/
      return targetType;

    }

    /*==========================================================================================================================
    | METHOD: CREATE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Factory method for creating new strongly-typed instances of the topics class, assuming a strongly-typed subclass is
    ///   available.
    /// </summary>
    /// <remarks>
    ///   The create method will look in the Ignia.Topics namespace for a class with the same name as the content type. For
    ///   instance, if the content type is "Page", it will look for an "Ignia.Topics.Page" class. If found, it will confirm that
    ///   the class derives from the <see cref="Topic"/> class and, if so, return a new instance of that class. If the class
    ///   exists but does not derive from <see cref="Topic"/>, then an exception will be thrown. And otherwise, a new instance
    ///   of the generic <see cref="Topic"/> class will be created.
    /// </remarks>
    /// <param name="key">A string representing the key for the new topic instance.</param>
    /// <param name="contentType">A string representing the key of the target content type.</param>
    /// <param name="parent">Optional topic to set as the new topic's parent.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when the class representing the content type is found, but doesn't derive from <see cref="Topic"/>.
    /// </exception>
    /// <returns>A strongly-typed instance of the <see cref="Topic"/> class based on the target content type.</returns>
    /// <requires description="The topic key must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   decription="The key should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !key.Contains(" ")
    /// </requires>
    /// <requires description="The content type key must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(contentType)
    /// </requires>
    /// <requires
    ///   decription="The contentType should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !contentType.Contains(" ")
    /// </requires>
    public static Topic Create(string key, string contentType, Topic parent = null) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key));
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(contentType));
      Contract.Ensures(Contract.Result<Topic>() != null);
      TopicFactory.ValidateKey(key);
      TopicFactory.ValidateKey(contentType);

      /*----------------------------------------------------------------------------------------------------------------------
      | Determine target type
      \---------------------------------------------------------------------------------------------------------------------*/
      var targetType = TopicFactory.GetTopicType(contentType);

      /*----------------------------------------------------------------------------------------------------------------------
      | Identify the appropriate topic
      \---------------------------------------------------------------------------------------------------------------------*/
      var topic = (Topic)Activator.CreateInstance(targetType);

      /*----------------------------------------------------------------------------------------------------------------------
      | Set the topic's Key and Content Type
      \---------------------------------------------------------------------------------------------------------------------*/
      topic.Key = key;
      topic.ContentType = contentType;

      /*----------------------------------------------------------------------------------------------------------------------
      | Set the topic's parent, if supplied
      \---------------------------------------------------------------------------------------------------------------------*/
      if (parent != null) {
        topic.Parent = parent;
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return the topic
      \---------------------------------------------------------------------------------------------------------------------*/
      return topic;

    }

    /// <summary>
    ///   Factory method for creating new strongly-typed instances of the topics class, assuming a strongly-typed subclass is
    ///   available. Used for cases where a <see cref="Topic"/> is being deserialized from an existing instance, as indicated
    ///   by the <paramref name="id"/> parameter.
    /// </summary>
    /// <remarks>
    ///   By default, when creating new attributes, the <see cref="AttributeValue"/>s for both <see cref="Topic.Key"/> and <see
    ///   cref="ContentType"/> will be set to true, which is required in order to correctly save new topics to the database.
    ///   When the <paramref name="id"/> parameter is set, however, the <see cref="Topic.Key"/> and <see
    ///   cref="Topic.ContentType"/> on the new <see cref="Topic"/> are set to false, as it is assumed these are being set to
    ///   the same values currently used in the persistance store.
    /// </remarks>
    /// <param name="key">A string representing the key for the new topic instance.</param>
    /// <param name="contentType">A string representing the key of the target content type.</param>
    /// <param name="id">The unique identifier assigned by the data store for an existing topic.</param>
    /// <param name="parent">Optional topic to set as the new topic's parent.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when the class representing the content type is found, but doesn't derive from <see cref="Topic"/>.
    /// </exception>
    /// <returns>A strongly-typed instance of the <see cref="Topic"/> class based on the target content type.</returns>
    public static Topic Create(string key, string contentType, int id, Topic parent = null) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate input
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(id > 0);
      Contract.Ensures(Contract.Result<Topic>() != null);

      /*----------------------------------------------------------------------------------------------------------------------
      | Create object
      \---------------------------------------------------------------------------------------------------------------------*/
      var topic = Create(key, contentType, parent);

      /*----------------------------------------------------------------------------------------------------------------------
      | Assign identifier
      \---------------------------------------------------------------------------------------------------------------------*/
      topic.Id = id;

      /*----------------------------------------------------------------------------------------------------------------------
      | Set dirty state to false
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(topic.Key != null);
      Contract.Assume(topic.ContentType != null);

      topic.Attributes.SetValue("Key", key, false, false);
      topic.Attributes.SetValue("ContentType", contentType, false, false);

      if (parent != null) {
        topic.Attributes.SetValue("ParentId", parent.Id.ToString(), false, false);
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return object
      \---------------------------------------------------------------------------------------------------------------------*/
      return topic;

    }

    /*==========================================================================================================================
    | METHOD: VALIDATE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Validates the format of a key used for an individual topic.
    /// </summary>
    /// <remarks>
    ///   Topic keys may be exposed as, for example, virtual routes and, thus, should not contain spaces, slashes, question
    ///   marks or other symbols reserved for URLs. This method is marked static so that it can also be used by the static Code
    ///   Contract Checker.
    /// </remarks>
    /// <param name="topicKey">The topic key that should be validated.</param>
    /// <param name="isOptional">Allows the topicKey to be optional (i.e., a null reference).</param>
    [Pure]
    public static void ValidateKey(string topicKey, bool isOptional = false) {
      Contract.Requires<InvalidKeyException>(isOptional || !String.IsNullOrEmpty(topicKey));
      Contract.Requires<InvalidKeyException>(
        String.IsNullOrEmpty(topicKey) || Regex.IsMatch(topicKey?? "", @"^[a-zA-Z0-9\.\-_]+$"),
        "Key names should only contain letters, numbers, hyphens, and/or underscores."
      );
    }

  } //class
} //Namespace

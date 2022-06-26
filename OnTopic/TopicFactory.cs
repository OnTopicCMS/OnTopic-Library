/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Text.RegularExpressions;
using OnTopic.Collections.Specialized;
using OnTopic.Lookup;
using OnTopic.Metadata;

namespace OnTopic {

  /*============================================================================================================================
  | CLASS: TOPIC FACTORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   A static class for centralizing logic for creating strongly-typed <see cref="Topic"/> instances based on their content
  ///   type.
  /// </summary>
  public static class TopicFactory {

    /*==========================================================================================================================
    | PROPERTY: TYPE LOOKUP SERVICE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes static variables for the <see cref="TopicFactory"/>.
    /// </summary>
    public static ITypeLookupService TypeLookupService { get; set; } = new DynamicTopicLookupService();

    /*==========================================================================================================================
    | METHOD: CREATE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Factory method for creating new strongly-typed instances of the topics class, assuming a strongly-typed subclass is
    ///   available. Used for cases where a <see cref="Topic"/> is being deserialized from an existing instance, as indicated
    ///   by the <paramref name="id"/> parameter.
    /// </summary>
    /// <remarks>
    ///   When the <paramref name="id"/> parameter is set the <see cref="TrackedRecord{T}.IsDirty"/> property is set to
    ///   <c>false</c> on <see cref="Topic.Key"/> as well as on <see cref="Topic.ContentType"/>, since it is assumed these are
    ///   being set to the same values currently used in the persistence store.
    /// </remarks>
    /// <param name="key">A string representing the key for the new topic instance.</param>
    /// <param name="contentType">A string representing the key of the target content type.</param>
    /// <param name="parent">Optional topic to set as the new topic's parent.</param>
    /// <param name="id">The unique identifier assigned by the data store for an existing topic.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when the class representing the content type is found, but doesn't derive from <see cref="Topic"/>.
    /// </exception>
    /// <returns>A strongly-typed instance of the <see cref="Topic"/> class based on the target content type.</returns>
    public static Topic Create(string key, string contentType, Topic? parent = null, int id = -1) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key), nameof(key));
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(contentType), nameof(contentType));

      ValidateKey(key);
      ValidateKey(contentType);

      /*------------------------------------------------------------------------------------------------------------------------
      | Determine target type
      \-----------------------------------------------------------------------------------------------------------------------*/
      var targetType            = TypeLookupService.Lookup(contentType);

      //Fallback to generic AttributeDescriptor if the specific attribute descriptor cannot be found
      if (targetType is null && contentType.EndsWith("AttributeDescriptor", StringComparison.OrdinalIgnoreCase)) {
        targetType              = typeof(AttributeDescriptor);
      }

      if (targetType is null) {
        targetType              = typeof(Topic);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify the appropriate topic
      \---------------------------------------------------------------------------------------------------------------------*/
      return (Topic)Activator.CreateInstance(targetType, key, contentType, parent, id)!;

    }

    /// <inheritdoc cref="Create(String, String, Topic?, Int32)"/>
    [ExcludeFromCodeCoverage]
    [Obsolete("The 'id' parameter has been moved to the end of the parameter list.", true)]
    public static Topic Create(string key, string contentType, int id, Topic? parent) =>
      throw new NotImplementedException();

    /// <inheritdoc cref="Create(String, String, Topic?, Int32)"/>
    public static Topic Create(string key, string contentType, int id) =>
      Create(key, contentType, null, id);

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
    public static void ValidateKey(string? topicKey, bool isOptional = false) {
      Contract.Requires<InvalidKeyException>(isOptional || !String.IsNullOrEmpty(topicKey));
      Contract.Requires<InvalidKeyException>(
        String.IsNullOrEmpty(topicKey) || Regex.IsMatch(topicKey, @"^[a-zA-Z0-9\.\-_]+$"),
        "Key names should only contain letters, numbers, hyphens, periods, and/or underscores."
      );
    }

  } //Class
} //Namespace
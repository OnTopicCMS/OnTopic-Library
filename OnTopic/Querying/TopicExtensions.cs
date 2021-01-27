/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using OnTopic.Attributes;
using OnTopic.Collections;
using OnTopic.Internal.Diagnostics;
using OnTopic.Metadata;

namespace OnTopic.Querying {

  /*============================================================================================================================
  | CLASS: TOPIC (EXTENSIONS)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides extensions for querying <see cref="OnTopic.Topic"/>.
  /// </summary>
  public static class TopicExtensions {

    /*==========================================================================================================================
    | METHOD: FIND FIRST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Finds the first instance of a <see cref="Topic"/> in the topic tree that satisfies the delegate.
    /// </summary>
    /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
    /// <param name="predicate">The function to validate whether a <see cref="Topic"/> should be included in the output.</param>
    /// <returns>The first instance of the topic to be satisfied.</returns>
    public static Topic? FindFirst(this Topic topic, Func<Topic, bool> predicate) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));
      Contract.Requires(predicate, nameof(predicate));

      /*------------------------------------------------------------------------------------------------------------------------
      | Search attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (predicate(topic)) {
        return topic;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse over children
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var child in topic.Children) {
        var nestedResult = child.FindFirst(predicate);
        if (nestedResult is not null) {
          return nestedResult;
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Indicate no results found
      \-----------------------------------------------------------------------------------------------------------------------*/
      return null;

    }

    /*==========================================================================================================================
    | METHOD: FIND FIRST PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Finds the first instance of <see cref="Topic"/> among the parents that satisfies the delegate.
    /// </summary>
    /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
    /// <param name="predicate">The function to validate whether a <see cref="Topic"/> should be included in the output.</param>
    /// <returns>The first instance of a parent topic to be satisfied.</returns>
    public static Topic? FindFirstParent(this Topic topic, Func<Topic, bool> predicate) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));
      Contract.Requires(predicate, nameof(predicate));

      /*------------------------------------------------------------------------------------------------------------------------
      | Search attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (predicate(topic)) {
        return topic;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Find lowest common root
      \-----------------------------------------------------------------------------------------------------------------------*/
      while (topic.Parent is not null) {
        if (predicate(topic.Parent)) {
          return topic.Parent;
        }
        topic = topic.Parent;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Indicate no results found
      \-----------------------------------------------------------------------------------------------------------------------*/
      return null;

    }

    /*==========================================================================================================================
    | METHOD: FIND ALL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a collection of all topics descending from—and including—the current topic.
    /// </summary>
    /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
    /// <returns>A collection of topics descending from the current topic.</returns>
    public static ReadOnlyTopicCollection FindAll(this Topic topic) => topic.FindAll(t => true);

    /// <summary>
    ///   Retrieves a collection of topics based on a supplied function.
    /// </summary>
    /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
    /// <param name="predicate">The function to validate whether a <see cref="Topic"/> should be included in the output.</param>
    /// <returns>A collection of topics matching the input parameters.</returns>
    public static ReadOnlyTopicCollection FindAll(this Topic topic, Func<Topic, bool> predicate) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));
      Contract.Requires(predicate, nameof(predicate));

      /*------------------------------------------------------------------------------------------------------------------------
      | Search attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var results = new TopicCollection();

      if (predicate(topic)) {
        results.Add(topic);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse over children
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var child in topic.Children) {
        var nestedResults = child.FindAll(predicate);
        foreach (var matchedTopic in nestedResults) {
          if (!results.Contains(matchedTopic)) {
            results.Add(matchedTopic);
          }
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return results
      \-----------------------------------------------------------------------------------------------------------------------*/
      return new(results);

    }

    /*==========================================================================================================================
    | METHOD: FIND ALL BY ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a collection of topics based on an attribute name and value.
    /// </summary>
    /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
    /// <param name="attributeKey">The string identifier for the <see cref="AttributeValue"/> against which to be searched.</param>
    /// <param name="attributeValue">The text value for the <see cref="AttributeValue"/> against which to be searched.</param>
    /// <returns>A collection of topics matching the input parameters.</returns>
    /// <requires description="The attribute name must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(name)
    /// </requires>
    /// <requires
    ///   decription="The name should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !name.Contains(" ")
    /// </requires>
    public static ReadOnlyTopicCollection FindAllByAttribute(this Topic topic, string attributeKey, string attributeValue) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(attributeKey), nameof(attributeKey));
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(attributeValue), nameof(attributeValue));
      TopicFactory.ValidateKey(attributeKey);

      /*------------------------------------------------------------------------------------------------------------------------
      | Return results
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topic.FindAll(t =>
        !String.IsNullOrEmpty(t.Attributes.GetValue(attributeKey)) &&
        t.Attributes.GetValue(attributeKey).Contains(attributeValue, StringComparison.OrdinalIgnoreCase)
      );

    }

    /*==========================================================================================================================
    | METHOD: GET TOPIC INDEX
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves all topics from the topic cache, and places them in an dictionary indexed by <see cref="Topic.Id"/>.
    /// </summary>
    /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
    /// <returns>A dictionary of topics indexed by <see cref="Topic.Id"/>.</returns>
    public static TopicIndex GetTopicIndex(this Topic topic) => new TopicIndex(topic.FindAll());

    /*==========================================================================================================================
    | METHOD: GET ROOT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves the root <see cref="Topic"/> in the current topic graph.
    /// </summary>
    /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
    /// <returns>The <see cref="Topic"/> at the root o the current topic graph.</returns>
    public static Topic GetRootTopic(this Topic topic) => topic.FindFirstParent(t => t.Parent is null)?? topic;

    /*==========================================================================================================================
    | METHOD: GET BY UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <see cref="Topic"/> with the specified <paramref name="uniqueKey"/>, if available.
    /// </summary>
    /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
    /// <param name="uniqueKey">The <see cref="Topic.GetUniqueKey()"/> of the <see cref="Topic"/> to return.</param>
    /// <returns>A <see cref="Topic"/> with the specified <paramref name="uniqueKey"/>, if found.</returns>
    public static Topic? GetByUniqueKey(this Topic topic, string uniqueKey) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, "The topic parameter must be specified.");
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(uniqueKey), nameof(uniqueKey));

      /*------------------------------------------------------------------------------------------------------------------------
      | Find lowest common root
      \-----------------------------------------------------------------------------------------------------------------------*/
      var currentTopic          = (Topic?)topic.GetRootTopic();

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle request for root
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (currentTopic!.Key.Equals(uniqueKey, StringComparison.OrdinalIgnoreCase)) {
        return currentTopic;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Process keys
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (uniqueKey.StartsWith(currentTopic!.Key + ":", StringComparison.OrdinalIgnoreCase)) {
        uniqueKey = uniqueKey[(currentTopic!.Key.Length + 1)..];
      }
      var keys                  = uniqueKey.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);

      /*------------------------------------------------------------------------------------------------------------------------
      | Navigate to the specific path
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var key in keys) {
        currentTopic = currentTopic?.Children?.GetTopic(key);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      return currentTopic;

    }

    /*==========================================================================================================================
    | METHOD: GET CONTENT TYPE DESCRIPTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves the <see cref="ContentTypeDescriptor"/> for the current <see cref="Topic"/>.
    /// </summary>
    /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
    /// <returns>The <see cref="ContentTypeDescriptor"/> associated with the <see cref="Topic.ContentType"/>.</returns>
    public static ContentTypeDescriptor? GetContentTypeDescriptor(this Topic topic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, "The topic parameter must be specified.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Find the root
      \-----------------------------------------------------------------------------------------------------------------------*/
      var rootContentType       = topic.GetByUniqueKey("Root:Configuration:ContentTypes");

      /*------------------------------------------------------------------------------------------------------------------------
      | Find content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      var contentTypeDescriptor = rootContentType?.FindFirst(t =>
        t.Key.Equals(topic.ContentType, StringComparison.OrdinalIgnoreCase) &&
        t.GetType().IsAssignableFrom(typeof(ContentTypeDescriptor))
      ) as ContentTypeDescriptor;

      /*------------------------------------------------------------------------------------------------------------------------
      | Return content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      return contentTypeDescriptor;

    }

  } //Class
} //Namespace
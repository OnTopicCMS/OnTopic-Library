/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/using System;
using Ignia.Topics.Diagnostics;
using Ignia.Topics.Collections;
using Target = Ignia.Topics;

namespace Ignia.Topics.Querying {

  /*============================================================================================================================
  | CLASS: TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides extensions for querying <see cref="Ignia.Topics.Topic"/>.
  /// </summary>
  public static class Topic {

    /*==========================================================================================================================
    | METHOD: FIND FIRST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Finds the first instance of a <see cref="Target.Topic"/> in the topic tree that satisfies the delegate.
    /// </summary>
    /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
    /// <param name="predicate">The function to validate whether a <see cref="Topic"/> should be included in the output.</param>
    /// <returns>The first instance of the topic to be satisfied.</returns>
    public static Target.Topic FindFirst(this Target.Topic topic, Func<Target.Topic, bool> predicate) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "The topic parameter must be specified.");

      /*----------------------------------------------------------------------------------------------------------------------
      | Search attributes
      \---------------------------------------------------------------------------------------------------------------------*/
      if (predicate(topic)) {
        return topic;
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Recurse over children
      \---------------------------------------------------------------------------------------------------------------------*/
      foreach (var child in topic.Children) {
        var nestedResult = child.FindFirst(predicate);
        if (nestedResult != null) {
          return nestedResult;
        }
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Indicate no results found
      \---------------------------------------------------------------------------------------------------------------------*/
      return null;

    }

    /*==========================================================================================================================
    | METHOD: FIND ALL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a collection of topics based on a supplied function.
    /// </summary>
    /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
    /// <param name="predicate">The function to validate whether a <see cref="Topic"/> should be included in the output.</param>
    /// <returns>A collection of topics matching the input parameters.</returns>
    public static ReadOnlyTopicCollection<Target.Topic> FindAll(this Target.Topic topic, Func<Target.Topic, bool> predicate) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "The topic parameter must be specified.");
      Contract.Ensures(Contract.Result<ReadOnlyTopicCollection<Target.Topic>>() != null);

      /*----------------------------------------------------------------------------------------------------------------------
      | Search attributes
      \---------------------------------------------------------------------------------------------------------------------*/
      var results = new TopicCollection();

      if (predicate(topic)) {
        results.Add(topic);
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Recurse over children
      \---------------------------------------------------------------------------------------------------------------------*/
      foreach (var child in topic.Children) {
        var nestedResults = child.FindAll(predicate);
        foreach (var matchedTopic in nestedResults) {
          if (!results.Contains(matchedTopic.Key)) {
            results.Add(matchedTopic);
          }
        }
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return results
      \---------------------------------------------------------------------------------------------------------------------*/
      return results.AsReadOnly();

    }

    /*==========================================================================================================================
    | METHOD: FIND ALL BY ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a collection of topics based on an attribute name and value.
    /// </summary>
    /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/> against which to be searched.</param>
    /// <param name="value">The text value for the <see cref="AttributeValue"/> against which to be searched.</param>
    /// <returns>A collection of topics matching the input parameters.</returns>
    /// <requires description="The attribute name must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(name)
    /// </requires>
    /// <requires
    ///   decription="The name should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !name.Contains(" ")
    /// </requires>
    public static ReadOnlyTopicCollection<Target.Topic> FindAllByAttribute(this Target.Topic topic, string name, string value) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "The topic parameter must be specified.");
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(name), "The attribute name must be specified.");
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value), "The attribute value must be specified.");
      Contract.Ensures(Contract.Result<ReadOnlyTopicCollection<Target.Topic>>() != null);
      TopicFactory.ValidateKey(name);

      /*----------------------------------------------------------------------------------------------------------------------
      | Return results
      \---------------------------------------------------------------------------------------------------------------------*/
      return topic.FindAll(t =>
        !String.IsNullOrEmpty(t.Attributes.GetValue(name)) &&
        t.Attributes.GetValue(name).IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0
      );

    }

  }
}

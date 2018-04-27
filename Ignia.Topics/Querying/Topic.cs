/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/using System;
using System.Diagnostics.Contracts;
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
    | METHOD: FIND ALL BY ATTRIBUTE
    >===========================================================================================================================
    | ###TODO JJC080313: Consider adding an overload of the out-of-the-box FindAll() method that supports recursion, thus
    | allowing a search by any criteria - including attributes.
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
      | Search attributes
      \---------------------------------------------------------------------------------------------------------------------*/
      var results = new TopicCollection();

      if (
        !String.IsNullOrEmpty(topic.Attributes.GetValue(name)) &&
        topic.Attributes.GetValue(name).IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0
        ) {
        results.Add(topic);
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Search children, if recursive
      \---------------------------------------------------------------------------------------------------------------------*/
      foreach (var child in topic.Children) {
        var nestedResults = child.FindAllByAttribute(name, value);
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

  }
}

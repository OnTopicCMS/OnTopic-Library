/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ignia.Topics.Mapping;
using Ignia.Topics.Metadata;

namespace Ignia.Topics.Models {

  /*============================================================================================================================
  | CLASS: TOPIC DATA MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple class suitable for modeling all attributes and relationships of a <see cref="Topic"/> without any of
  ///   the business logic.
  /// </summary>
  /// <remarks>
  ///   A <see cref="Topic"/> class introduces a degree of overhead in terms of complexity, enforcing business logic, and
  ///   maintaining relationship integrity as part of the overall topic graph. During data transfers tasks, such as used by the
  ///   API, it may be desirable to work with a light-weight, simplified version that is optimized exclusively for modeling the
  ///   data without any attempt to maintain broader integrity of the data or relationships.
  /// </remarks>
  public class TopicDataModel {

    /*==========================================================================================================================
    | PROPERTY:  KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's <see cref="Key"/> attribute, the locally unique text identifier for the topic.
    /// </summary>
    /// <requires description="The value from the getter must not be null." exception="T:System.ArgumentNullException">
    ///   value != null
    /// </requires>
    [Required]
    public string Key {
      get => Attributes["Key"];
      set => Attributes["Key"] = value;
    }

    /*==========================================================================================================================
    | PROPERTY: UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's <see cref="UniqueKey"/> attribute, the unique text identifier for the topic.
    /// </summary>
    /// <requires description="The value from the getter must not be null." exception="T:System.ArgumentNullException">
    ///   value != null
    /// </requires>
    [Required]
    public string UniqueKey {
      get => Attributes["UniqueKey"];
      set => Attributes["UniqueKey"] = value;
    }

    /*==========================================================================================================================
    | PROPERTY: CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the key name of the content type that the current topic represents.
    /// </summary>
    /// <remarks>
    ///   Each topic is associated with a content type. The content type determines which attributes are displayed in the Topics
    ///   Editor (via the <see cref="ContentTypeDescriptor.AttributeDescriptors"/> property). The content type also determines,
    ///   by default, which view is rendered by the <see cref="Topics.ITopicRoutingService"/> (assuming the value isn't
    ///   overwritten down the pipe).
    /// </remarks>
    [Required]
    public string ContentType {
      get => Attributes["ContentType"];
      set => Attributes["ContentType"] = value;
    }

    /*==========================================================================================================================
    | PROPERTY: RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a dictionary of dictionaries representing relationships.
    /// </summary>
    /// <remarks>
    ///   The outer dictionary represents the relationship scopes—i.e., sets of relationships. The inner dictionary represents
    ///   the individual relationships, with the key representing the <see cref="Topic.GetUniqueKey()"/> and the value
    ///   representing the <see cref="Topic.Title"/> (or, <see cref="Topic.Key"/> if blank).
    /// </remarks>
    [Required]
    public Dictionary<string, Dictionary<string, string>> Relationships { get; } = new Dictionary<string, Dictionary<string, string>>();

    /*==========================================================================================================================
    | PROPERTY: ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a dictionary of dictionaries representing attributes.
    /// </summary>
    /// <remarks>
    ///   In the dictionary, the key maps to the <see cref="AttributeValue.Key"/>, while the value maps to the <see
    ///   cref="AttributeValue.Value"/>.
    /// </remarks>
    [Required]
    public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();

    /*==========================================================================================================================
    | PROPERTY: CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a list of child <see cref="TopicDataModel"/> instances, representing the topic hierarchy.
    /// </summary>
    [Required]
    public List<TopicDataModel> Children { get; } = new List<TopicDataModel>();

  } //Class
} //Namespace

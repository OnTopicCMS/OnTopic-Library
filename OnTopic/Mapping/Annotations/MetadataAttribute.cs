﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ATTRIBUTE: METADATA
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Flags that a property should be mapped to a list of metadata available in the <c>Configuration</c> namespace.
  /// </summary>
  /// <remarks>
  ///   In the Topic Editor, the <c>TopicLookup</c> allows editors to select values from drop-down lists representing topics.
  ///   Those topics, by default, are stored in the <c>Configuration:Metadata</c> namespace. The metadata attribute allows a
  ///   strongly-typed reference to be created, thus pulling either a reference to a specific topic (in the case of a single
  ///   value property) or a collection of the metadata (in the case of a collection).
  /// </remarks>
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
  public sealed class MetadataAttribute : Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="MetadataAttribute"/> class by providing a (required) key.
    /// </summary>
    /// <param name="key">The key represents the name of the Metadata topic that should be mapped to.</param>
    public MetadataAttribute(string key) {
      TopicFactory.ValidateKey(key, false);
      Key = key;
    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the value of the key.
    /// </summary>
    public string Key { get; }

  } //Class
} //Namespace
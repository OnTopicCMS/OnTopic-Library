/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ATTRIBUTE: RELATIONSHIP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc cref="CollectionAttribute"/>
  [AttributeUsage(System.AttributeTargets.Property)]
  [Obsolete("The [Relationship] attribute has been renamed to [Collection].", true)]
  public sealed class RelationshipAttribute : Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="RelationshipAttribute"/> by providing an <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key value of the collection associated with the current property.</param>
    public RelationshipAttribute(string key) {
      TopicFactory.ValidateKey(key, false);
      Key = key;
    }

    /// <summary>
    ///   Annotates a property with the <see cref="RelationshipAttribute"/> by providing the <see cref="RelationshipType"/>.
    /// </summary>
    /// <param name="type">Optional. The type of collection the collection is associated with.</param>
    public RelationshipAttribute(RelationshipType type = RelationshipType.Any) {
      Type = type;
    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the value of the collection key.
    /// </summary>
    public string? Key { get; }

    /*==========================================================================================================================
    | PROPERTY: TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the value of the <see cref="CollectionType"/>.
    /// </summary>
    #pragma warning disable CA1019 // Define accessors for attribute arguments
    public RelationshipType Type { get; set; }
    #pragma warning restore CA1019 // Define accessors for attribute arguments

  } //Class
} //Namespace
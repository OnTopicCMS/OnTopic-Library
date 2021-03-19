/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ATTRIBUTE: COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides the <see cref="ITopicMappingService"/> with instructions as to which collection to map to.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     By default, <see cref="ITopicMappingService"/> implementations will attempt to map a collection property to the first
  ///     collection it finds with a key that matches the property name. For example, if a property is named <c>Categories</c>
  ///     it will first look in <see cref="Topic.Relationships"/> for a relationship named <c>Categories</c>, then it will
  ///     search <see cref="Topic.References"/>, then <see cref="Topic.Children"/> for a set of nested topics, and finally <see
  ///     cref="Topic.IncomingRelationships"/>.
  ///   </para>
  ///   <para>
  ///     This attribute instructs the <see cref="ITopicMappingService"/> to instead look for a specified key. This allows the
  ///     target property name to be decoupled from the source's collection key. In addition, this attribute can be used to
  ///     specify the type of collection expected, which is useful if there might be ambiguity between collection names (for
  ///     example, if there is a <see cref="Topic.Relationships"/> with the same key as an <see cref="Topic.
  ///     IncomingRelationships"/>), which is not an uncommon scenario.
  ///   </para>
  /// </remarks>
  [AttributeUsage(AttributeTargets.Property)]
  public sealed class CollectionAttribute : Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="CollectionAttribute"/> by providing an <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key value of the collection associated with the current property.</param>
    public CollectionAttribute(string key) {
      TopicFactory.ValidateKey(key, false);
      Key = key;
    }

    /// <summary>
    ///   Annotates a property with the <see cref="CollectionAttribute"/> by providing the <see cref="CollectionType"/>.
    /// </summary>
    /// <param name="type">Optional. The type of collection the collection is associated with.</param>
    public CollectionAttribute(CollectionType type = CollectionType.Any) {
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
    public CollectionType Type { get; set; }
    #pragma warning restore CA1019 // Define accessors for attribute arguments

  } //Class
} //Namespace
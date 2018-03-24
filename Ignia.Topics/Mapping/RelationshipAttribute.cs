/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ignia.Topics;
using Ignia.Topics.Collections;

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | ATTRIBUTE: RELATIONSHIP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides the <see cref="ITopicMappingService"/> with instructions as to which key to follow for the relationship.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     By default, <see cref="ITopicMappingService"/> implementations will attempt to map a collection property to the first
  ///     relationship it finds with a key that matches the property name. For example, if a property is named "Categories" it
  ///     will first look in <see cref="Topic.Relationships"/> for a relationship named "Categories", then it will search <see
  ///     cref="Topic.Children"/> for a set of nested topics, and finally <see cref="Topic.IncomingRelationships"/>.
  ///   </para>
  ///     This attribute instructs the <see cref="ITopicMappingService"/> to instead look for a specified key. This allows the
  ///     target property name to be decoupled from the source's relationship key. In addition, this attribute can be used to
  ///     specify the type of relationship expected, which is useful if there might be ambiguity between relationship names (for
  ///     example, if there is a <see cref="Topic.Relationships"/> with the same key as an <see
  ///     cref="Topic.IncomingRelationships"/>).
  ///   </para>
  /// </remarks>
  [System.AttributeUsage(System.AttributeTargets.Property)]
  sealed class RelationshipAttribute : System.Attribute {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     string                          _key                            = null;
    private                     RelationshipType                _type                           = RelationshipType.Any;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="RelationshipAttribute"/> by providing an <paramref name="key"/>. Optionally
    ///   specifies the <see cref="RelationshipType"/> as well.
    /// </summary>
    /// <param name="key">The key value of the relationships associated with the current property.</param>
    /// <param name="type">Optional. The type of collection the relationship is associated with.</param>
    public RelationshipAttribute(string key, RelationshipType type = RelationshipType.Any) {
      TopicFactory.ValidateKey(key, false);
      _key = key;
      _type = type;
    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the value of the relationship key.
    /// </summary>
    public string Key {
      get {
        return _key;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the value of the relationship type.
    /// </summary>
    public RelationshipType Type {
      get {
        return _type;
      }
    }

  } //Class

} //Namespace

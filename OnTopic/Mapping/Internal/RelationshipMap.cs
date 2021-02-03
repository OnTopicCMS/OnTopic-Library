/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Mapping.Internal {

  /*============================================================================================================================
  | CLASS: RELATIONSHIP MAP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a mapping of the relationship between <see cref="CollectionType"/> and <see cref="Relationships"/>.
  /// </summary>
  /// <remarks>
  ///   While the <see cref="CollectionType"/> and <see cref="Relationships"/> enumerations are distinct, there are times when
  ///   a single <see cref="CollectionType"/> needs to be related to an item in the collection of <see cref="Relationships"/>.
  ///   This mapping makes that feasible.
  /// </remarks>
  static internal class RelationshipMap {

    /*==========================================================================================================================
    | CONSTRUCTOR (STATIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    static RelationshipMap() {

      var mappings = new Dictionary<CollectionType, Relationships> {
        { CollectionType.Any, Relationships.None },
        { CollectionType.Children, Relationships.Children },
        { CollectionType.Relationship, Relationships.Relationships },
        { CollectionType.NestedTopics, Relationships.None },
        { CollectionType.MappedCollection, Relationships.MappedCollections },
        { CollectionType.IncomingRelationship, Relationships.IncomingRelationships }
      };

      Mappings = mappings;

    }

    /*==========================================================================================================================
    | PROPERTY: MAPPINGS
    \-------------------------------------------------------------------------------------------------------------------------*/
    static internal Dictionary<CollectionType, Relationships> Mappings { get; }

  } //Class
} //Namespace
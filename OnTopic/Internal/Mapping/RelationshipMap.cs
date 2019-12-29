/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Internal.Mapping {

  /*============================================================================================================================
  | CLASS: RELATIONSHIP MAP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a mapping of the relationship between <see cref="RelationshipType"/> and <see cref="Relationships"/>.
  /// </summary>
  /// <remarks>
  ///   While the <see cref="RelationshipType"/> and <see cref="Relationships"/> enumerations are distinct, there are times when
  ///   a single <see cref="RelationshipType"/> needs to be related to an item in the collection of <see cref="Relationships"/>.
  ///   This mapping makes that feasible.
  /// </remarks>
  static internal class RelationshipMap {

    /*==========================================================================================================================
    | CONSTRUCTOR (STATIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    static RelationshipMap() {

      var mappings = new Dictionary<RelationshipType, Relationships> {
        { RelationshipType.Any, Relationships.None },
        { RelationshipType.Children, Relationships.Children },
        { RelationshipType.Relationship, Relationships.Relationships },
        { RelationshipType.NestedTopics, Relationships.None },
        { RelationshipType.MappedCollection, Relationships.MappedCollections },
        { RelationshipType.IncomingRelationship, Relationships.IncomingRelationships }
      };

      Mappings = mappings;

    }

    /*==========================================================================================================================
    | PROPERTY: MAPPINGS
    \-------------------------------------------------------------------------------------------------------------------------*/
    static internal Dictionary<RelationshipType, Relationships> Mappings { get; }

  } //Class
} //Namespace
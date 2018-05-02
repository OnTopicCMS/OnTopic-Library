/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ignia.Topics.Mapping {

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
    | STATIC VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    static Dictionary<RelationshipType, Relationships> _mappings = null;

    /*==========================================================================================================================
    | PROPERTY: MAPPINGS
    \-------------------------------------------------------------------------------------------------------------------------*/
    static internal Dictionary<RelationshipType, Relationships> Mappings {
      get {
        if (_mappings == null) {
          var mappings = new Dictionary<RelationshipType, Relationships>();
          mappings.Add(RelationshipType.Children, Relationships.Children);
          mappings.Add(RelationshipType.Relationship, Relationships.Relationships);
          mappings.Add(RelationshipType.NestedTopics, Relationships.None);
          mappings.Add(RelationshipType.IncomingRelationship, Relationships.IncomingRelationships);
          _mappings = mappings;
        }
        return _mappings;
      }
    }

  }
}

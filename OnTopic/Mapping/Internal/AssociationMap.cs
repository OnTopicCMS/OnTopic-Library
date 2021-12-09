/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Annotations;

namespace OnTopic.Mapping.Internal {

  /*============================================================================================================================
  | CLASS: ASSOCIATION MAP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a mapping of the relationship between <see cref="CollectionType"/> and <see cref="AssociationTypes"/>.
  /// </summary>
  /// <remarks>
  ///   While the <see cref="CollectionType"/> and <see cref="AssociationTypes"/> enumerations are distinct, there are times
  ///   when a single <see cref="CollectionType"/> needs to be related to an item in the collection of <see cref="
  ///   AssociationTypes"/>. This mapping makes that feasible.
  /// </remarks>
  static internal class AssociationMap {

    /*==========================================================================================================================
    | CONSTRUCTOR (STATIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    static AssociationMap() {

      var mappings = new Dictionary<CollectionType, AssociationTypes> {
        { CollectionType.Any, AssociationTypes.None },
        { CollectionType.Children, AssociationTypes.Children },
        { CollectionType.Relationship, AssociationTypes.Relationships },
        { CollectionType.NestedTopics, AssociationTypes.None },
        { CollectionType.MappedCollection, AssociationTypes.MappedCollections },
        { CollectionType.IncomingRelationship, AssociationTypes.IncomingRelationships }
      };

      Mappings = mappings;

    }

    /*==========================================================================================================================
    | PROPERTY: MAPPINGS
    \-------------------------------------------------------------------------------------------------------------------------*/
    static internal Dictionary<CollectionType, AssociationTypes> Mappings { get; }

  } //Class
} //Namespace
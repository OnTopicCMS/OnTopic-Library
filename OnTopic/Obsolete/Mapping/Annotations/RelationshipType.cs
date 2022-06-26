/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ENUM: RELATIONSHIP TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc cref="CollectionType"/>
  [Obsolete($"{nameof(RelationshipType)} has been renamed to {nameof(CollectionType)}", true)]
  public enum RelationshipType {

    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    Any                         = 0,
    Children                    = 1,
    Relationship                = 2,
    IncomingRelationship        = 3,
    NestedTopics                = 4,
    MappedCollection            = 5

    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

  } //Enum
} //Namespace
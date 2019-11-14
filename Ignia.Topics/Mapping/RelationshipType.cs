/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | ENUM: RELATIONSHIP TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Enum that allows a relationship to be specified.
  /// </summary>
  /// <remarks>
  ///   This differs from <see cref="Relationships"/>, which allows <i>multiple</i> relationships to be specified.
  /// </remarks>
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

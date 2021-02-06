/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ENUM: COLLECTION TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Enum that allows a collection to be specified.
  /// </summary>
  /// <remarks>
  ///   This differs from <see cref="AssociationTypes"/>, which allows <i>multiple</i> collections to be specified, and also
  ///   includes the <see cref="Topic.Parent"/> as a source.
  /// </remarks>
  public enum CollectionType {

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
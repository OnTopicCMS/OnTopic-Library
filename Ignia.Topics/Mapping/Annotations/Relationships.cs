/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace Ignia.Topics.Mapping.Annotations {

  /*============================================================================================================================
  | ENUM: RELATIONSHIPS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Enum that allows one or more relationship types to be specified.
  /// </summary>
  /// <remarks>
  ///   This differs from <see cref="RelationshipType"/>, which only allows <i>one</i> relationship to be specified.
  /// </remarks>
  [Flags]
  public enum Relationships {

    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    None                        = 0,
    Parents                     = 1,
    Children                    = 1 << 1,
    Relationships               = 1 << 2,
    IncomingRelationships       = 1 << 3,
    MappedCollections           = 1 << 4,
    References                  = 1 << 5,
    All = Parents | Children | Relationships | IncomingRelationships | MappedCollections | References

    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

  } //Enum

} //Namespace

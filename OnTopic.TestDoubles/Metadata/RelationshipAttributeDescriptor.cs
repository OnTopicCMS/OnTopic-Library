/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Diagnostics.CodeAnalysis;
using OnTopic.Metadata;

namespace OnTopic.TestDoubles.Metadata {

  /*============================================================================================================================
  | CLASS: RELATIONSHIP (ATTRIBUTE DESCRIPTOR)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents metadata for describing a relationship attribute type, including information on how it will be presented and
  ///   validated in the editor.
  /// </summary>
  /// <remarks>
  ///   This class is primarily used by the Topic Editor interface to determine how attributes are displayed as part of the
  ///   CMS; except in very specific scenarios, it is not typically used elsewhere in the Topic Library itself.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  public class RelationshipAttributeDescriptor : AttributeDescriptor {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public RelationshipAttributeDescriptor(
      string key,
      string contentType,
      Topic parent,
      int id = -1
    ) : base(
      key,
      contentType,
      parent,
      id
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize values
      \-----------------------------------------------------------------------------------------------------------------------*/
      ModelType = ModelType.Relationship;

    }

  } //Class
} //Namespace
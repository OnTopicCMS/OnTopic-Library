/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Metadata.AttributeTypes {

  /*============================================================================================================================
  | CLASS: NESTED TOPIC LIST ATTRIBUTE (DESCRIPTOR)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents metadata for describing a nested topic list attribute type, including information on how it will be presented
  ///   and validated in the editor.
  /// </summary>
  /// <remarks>
  ///   This class is primarily used by the Topic Editor interface to determine how attributes are displayed as part of the
  ///   CMS; except in very specific scenarios, it is not typically used elsewhere in the Topic Library itself.
  /// </remarks>
  public class NestedTopicListAttribute : AttributeDescriptor {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public NestedTopicListAttribute(
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
    }

    /*==========================================================================================================================
    | PROPERTY: MODEL TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override ModelType ModelType => ModelType.NestedTopic;

  } //Class
} //Namespace
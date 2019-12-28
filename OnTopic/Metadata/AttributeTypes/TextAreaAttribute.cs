/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Metadata.AttributeTypes {

  /*============================================================================================================================
  | CLASS: TEXT AREA ATTRIBUTE (DESCRIPTOR)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents metadata for describing a text area attribute type, including information on how it will be presented and
  ///   validated in the editor.
  /// </summary>
  /// <remarks>
  ///   This class is primarily used by the Topic Editor interface to determine how attributes are displayed as part of the
  ///   CMS; except in very specific scenarios, it is not typically used elsewhere in the Topic Library itself.
  /// </remarks>
  public class TextAreaAttribute : AttributeTypeDescriptor {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public TextAreaAttribute(
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
    | PROPERTY: IS EXTENDED ATTRIBUTE?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override bool IsExtendedAttribute => true;

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.TestDoubles.Metadata {

  /*============================================================================================================================
  | CLASS: TEXT (ATTRIBUTE DESCRIPTOR)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents metadata for describing a text attribute type, including information on how it will be presented and
  ///   validated in the editor.
  /// </summary>
  /// <remarks>
  ///   This class is primarily used by the Topic Editor interface to determine how attributes are displayed as part of the
  ///   CMS; except in very specific scenarios, it is not typically used elsewhere in the Topic Library itself.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  public class TextAttributeDescriptor : AttributeDescriptor {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public TextAttributeDescriptor(
      string key,
      string contentType,
      Topic? parent = null,
      int id = -1
    ) : base(
      key,
      contentType,
      parent,
      id
    ) {
    }

  } //Class
} //Namespace
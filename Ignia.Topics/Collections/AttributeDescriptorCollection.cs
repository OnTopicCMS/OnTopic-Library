/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace Ignia.Topics.Collections {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE DESCRIPTOR COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="AttributeDescriptor"/> objects.
  /// </summary>
  public class AttributeDescriptorCollection : TopicCollection<AttributeDescriptor> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="AttributeDescriptorCollection"/> class.
    /// </summary>
    public AttributeDescriptorCollection() : base(null, null) {
    }

  }
}

/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Collections;

namespace OnTopic.Metadata {

  /*============================================================================================================================
  | CLASS: CONTENT TYPE DESCRIPTOR COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="ContentTypeDescriptor"/> objects.
  /// </summary>
  public class ContentTypeDescriptorCollection : TopicCollection<ContentTypeDescriptor> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ContentTypeDescriptorCollection"/> class.
    /// </summary>
    public ContentTypeDescriptorCollection() : base(null) {
    }

  } //Class
} //Namespace
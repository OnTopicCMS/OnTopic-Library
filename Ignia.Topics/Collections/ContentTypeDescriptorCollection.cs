/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ignia.Topics.Collections {

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
    public ContentTypeDescriptorCollection() : base(null, null) {
    }

  }
}

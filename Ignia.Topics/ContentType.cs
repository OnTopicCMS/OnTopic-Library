/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: CONTENT TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents metadata for describing a content type, including a collection of <see cref="AttributeDescriptor"/>s
  ///   associated with it.
  /// </summary>
  /// <remarks>
  ///   The <see cref="ContentType"/> class is provided exclusively for backward compatibility with topics of the content type
  ///   "ContentType" so that the <see cref="TopicFactory.Create(String, String, Topic)"/> factory method can find a match. Databases
  ///   should be updated to instead use "ContentTypeDescriptor", which better differentiates the metadata from e.g.
  ///   <see cref="Topic.ContentType"/>. The <see cref="ContentType"/> class derives from <see cref="ContentTypeDescriptor"/> to
  ///   ensure they support the same functionality.
  /// </remarks>
  [Obsolete("The ContentType class is deprecated. Topic databases and code should be updated to use ContentTypeDescriptor instead")]
  public class ContentType : ContentTypeDescriptor {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ContentType"/> class.
    /// </summary>
    public ContentType() : base() { }

  } //Class

} //Namespace
﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Metadata.AttributeTypes {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE TYPE (DESCRIPTOR)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a base class for attribute type classes.
  /// </summary>
  /// <remarks>
  ///   In the OnTopic Editor, every attribute is assigned a type, which represents how the data should be presented in the
  ///   editor, what constraints those data have, and how those data should be stored in the data source. In Version 4.0.0 and
  ///   above, these attribute types are described by their own <see cref="ContentTypeDescriptor"/>s, which offer a strongly
  ///   typed representation of those properties. This class provides a base for those representations.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  [Obsolete(
    $"The {nameof(AttributeTypeDescriptor)} class is obsolete. Derive from {nameof(AttributeDescriptor)} instead.",
    true
  )]
  public abstract class AttributeTypeDescriptor : AttributeDescriptor {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    protected AttributeTypeDescriptor(
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

  } //Class
} //Namespace
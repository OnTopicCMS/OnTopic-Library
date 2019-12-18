/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace Ignia.Topics.Metadata.AttributeTypes {

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

    /*==========================================================================================================================
    | PROPERTY: MODEL TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override ModelType ModelType => ModelType.ScalarValue;

    /*==========================================================================================================================
    | PROPERTY: EDITOR TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    #if NETSTANDARD2_0
      public override string EditorType => GetType().Name.Replace("Attribute", "");
    #else
      public override string EditorType => GetType().Name.Replace("Attribute", "", StringComparison.InvariantCultureIgnoreCase);
    #endif

  } //Class
} //Namespace
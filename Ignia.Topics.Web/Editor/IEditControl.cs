/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace Ignia.Topics.Web.Editor {

  /*============================================================================================================================
  | INTERFACE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides an interface that all topic editor controls must implement in order to participate in the editor's life
  ///   cycle.
  /// </summary>
  /// <remarks>
  ///   These controls represent attribute types, and are instantiated as Attributes in a Content Type.
  /// </remarks>
  public interface IEditControl {

    /*==========================================================================================================================
    | PROPERTY: INHERITED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the value of a control, as inherited from any Topic pointers. 
    /// </summary>
    /// <remarks>
    ///   If this value is set, then the control should not be marked as required, as it is inheriting its value from a
    ///   different source.
    /// </remarks>
    string InheritedValue { get; set; }

    /*==========================================================================================================================
    | PROPERTY: VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the value of a control, ignoring inheritance.
    /// </summary>
    /// <remarks>
    ///   The value should not be set to an inherited value, as otherwise, that value will end up being duplicated in the local
    ///   Topic.
    /// </remarks>
    string Value { get; set; }

    /*==========================================================================================================================
    | PROPERTY: ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets a reference to the specific attribute associated with this attribute type control.
    /// </summary>
    /// <remarks>
    ///   This allows the type to retrieve configuration or extended attributes specifically from an attribute, as opposed to
    ///   having them set via the DefaultValues attribute. This allows the Attribute Content Type to be subclassed, thus
    ///   permitting more user-friendly interfaces to be developed for particular attributes when using the Oroboros
    ///   Configuration. For instance, if an attribute-related control includes a property named Color, then an attribute could
    ///   be added to that Attribute's Content Type allowing the color to be defined, as opposed to the publisher needing to
    ///   know how to set the Color attribute using the DefaultValue attribute.
    /// </remarks>
    Topic Attribute { get; set; }

    /*==========================================================================================================================
    | PROPERTY: REQUIRED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="IEditControl"/> is required.
    /// </summary>
    bool Required { get; set; }

  } // Class

} // Namespace

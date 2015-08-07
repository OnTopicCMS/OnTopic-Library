namespace Ignia.Topics {

/*===========================================================================================================================
| INTERFACE: EDIT CONTROL
|
| Author        Jeremy Caney, Ignia LLC (Jeremy.Caney@Ignia.com)
| Client        Ignia
| Project       Topics Editor
|
| Purpose       Provides an interface that all topic editor controls must implement in order to participate in the editor's
|               life cycle.  These controls represent attribute types, and are instantiated as Attributes in a Content Type.
|
>============================================================================================================================
| Revisions     Date      Author        Comments
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
|               06.07.10  Jeremy Caney  Initial version template
|               08.14.13  Jeremy Caney  Added the Attribute property, so controls can lookup metadata associated with them.
\--------------------------------------------------------------------------------------------------------------------------*/

/*===========================================================================================================================
| INTERFACE
\--------------------------------------------------------------------------------------------------------------------------*/
  public interface IEditControl {

  /*=========================================================================================================================
  | PROPERTY: INHERITED VALUE
  >--------------------------------------------------------------------------------------------------------------------------
  | Gets or sets the value of a control, as inherited from any Topic pointers.  If this value is set, then the control should
  | not be marked as required, as it is inheriting its value from a different source.
  \------------------------------------------------------------------------------------------------------------------------*/
    string InheritedValue { get; set; }

  /*=========================================================================================================================
  | PROPERTY: VALUE
  >--------------------------------------------------------------------------------------------------------------------------
  | Gets or sets the value of a control, ignoring inheritance.  The value should not be set to an inherited value, as other-
  | wise, that value will end up being duplicatd in the local Topic.
  \------------------------------------------------------------------------------------------------------------------------*/
    string Value { get; set; }

  /*=========================================================================================================================
  | PROPERTY: ATTRIBUTE
  >--------------------------------------------------------------------------------------------------------------------------
  | Gets or sets a reference to the specific attribute associated with this attribute type control.  This allows the type
  | to retrieve configuration or extended attributes specifically from an attribute, as opposed to having them set via the
  | DefaultValues attribute.  This allows the Attribute Content Type to be subclassed, thus permitting more user-friendly
  | interfaces to be developed for particular attributes when using the Oroborus Configuration.  For instance, if an
  | attribute-related control includes a property named Color, then an attribute could be added to that Attribute's Content
  | Type allowing the color to be defined, as opposed to the publisher needing to know how to set the Color attribute using
  | the DefaultValue attribute.
  \------------------------------------------------------------------------------------------------------------------------*/
    Topic Attribute { get; set; }

  /*=========================================================================================================================
  | PROPERTY: REQUIRED
  >--------------------------------------------------------------------------------------------------------------------------
  | Gets or sets whether or not the control is required or not.
  \------------------------------------------------------------------------------------------------------------------------*/
    bool Required { get; set; }

    } //Class

  } //Namespace

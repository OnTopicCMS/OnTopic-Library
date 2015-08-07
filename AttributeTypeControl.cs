namespace Ignia.Topics {

/*===========================================================================================================================
| ATTRIBUTE TYPE CONTROL
|
| Author        Jeremy Caney, Ignia LLC (Jeremy.Caney@Ignia.com)
| Client        Ignia, LLC
| Project       Topics Editor
|
| Purpose       Provides a base class for controls used in the Topics Editor.  Each control maps to an Attribute Type, and
|               provides a common interface for the Topics Editor to interact with, while leaving the rendering and business
|               logic of the attribute up to derived controls.
|
>============================================================================================================================
| Revisions     Date       Author               Comments
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
|               10.16.13   Jeremy Caney         Initial version created.
|               MM.DD.YY   FName LName          Description
\--------------------------------------------------------------------------------------------------------------------------*/

/*===========================================================================================================================
| DEFINE ASSEMBLY ATTRIBUTES
>============================================================================================================================
| Declare and define attributes used to compile the finished assembly.
\--------------------------------------------------------------------------------------------------------------------------*/
  using System;
  using System.Web;
  using System.Web.UI;
  using System.Web.Security;
  using System.Web.Routing;
  using System.Web.Compilation;
  using Ignia.Topics;

/*===========================================================================================================================
| CLASS
\--------------------------------------------------------------------------------------------------------------------------*/
  public class AttributeTypeControl : UserControl, IEditControl {

  /*=========================================================================================================================
  | DECLARE PRIVATE VARIABLES
  >==========================================================================================================================
  | Declare variables for property use
  \------------------------------------------------------------------------------------------------------------------------*/
  //private     bool                    _enableValidation                       = true;
  //private     Topic                   _topic                                  = null;

  /*=========================================================================================================================
  | CONSTRUCTOR
  \------------------------------------------------------------------------------------------------------------------------*/
    public AttributeTypeControl() : base() {
      }

  /*=========================================================================================================================
  | PROPERTY: INHERITED VALUE
  >--------------------------------------------------------------------------------------------------------------------------
  | Gets or sets the value of a control, as inherited from any Topic pointers.  If this value is set, then the control should
  | not be marked as required, as it is inheriting its value from a different source.
  \------------------------------------------------------------------------------------------------------------------------*/
    public virtual string InheritedValue { get; set; }

  /*=========================================================================================================================
  | PROPERTY: VALUE
  >--------------------------------------------------------------------------------------------------------------------------
  | Gets or sets the value of a control, ignoring inheritance.  The value should not be set to an inherited value, as other-
  | wise, that value will end up being duplicatd in the local Topic.
  \------------------------------------------------------------------------------------------------------------------------*/
    public virtual string Value { get; set; }

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
    public virtual Topic Attribute { get; set; }

  /*=========================================================================================================================
  | PROPERTY: REQUIRED
  >--------------------------------------------------------------------------------------------------------------------------
  | Gets or sets whether or not the control is required or not.
  \------------------------------------------------------------------------------------------------------------------------*/
    public virtual bool Required { get; set; }



    } //Class

  } //Namespace
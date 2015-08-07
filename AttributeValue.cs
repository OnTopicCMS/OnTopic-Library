namespace Ignia.Topics {

/*==============================================================================================================================
| ATTRIBUTE VALUE
|
| Author        Katherine Trunkey, Ignia LLC (katherine.trunkey@ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       Represents an instance of an Attribute value; provides values and metadata specific to individual attribute
|               value instances, such as state (IsDirty = has changed) and last modified date. State (IsDirty) is evaluated as
|               part of the setter for Value.
|
>===============================================================================================================================
| Revisions     Date            Author                  Comments
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
|               08.06.14        Katherine Trunkey       Created initial version
\-----------------------------------------------------------------------------------------------------------------------------*/

/*==============================================================================================================================
| DEFINE ASSEMBLY ATTRIBUTES
>===============================================================================================================================
| Declare and define attributes used in the compiling of the finished assembly.
\-----------------------------------------------------------------------------------------------------------------------------*/
  using System;
  using System.Text;

/*==============================================================================================================================
| CLASS
\-----------------------------------------------------------------------------------------------------------------------------*/
  public class AttributeValue {

  /*============================================================================================================================
  | DECLARE PRIVATE VARIABLES
  >=============================================================================================================================
  | Declare variables for property use
  \---------------------------------------------------------------------------------------------------------------------------*/
    private     string          _key                    = null;
    private     string          _value                  = null;
    private     bool            _isDirty                = false;

  /*============================================================================================================================
  | CONSTRUCTOR
  >=============================================================================================================================
  | Allows a new object to be instantiated. Optional overloads allow object to be constructed based on specified key/value
  | pairs or for the IsDirty (has been changed) property to be set.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public AttributeValue() { }

    public AttributeValue(string key, string value) {
      this.Key                  = key;
      this.Value                = value;
      }

    public AttributeValue(string key, string value, bool isDirty) {
      this.Key                  = key;
      this.Value                = value;
      this.IsDirty              = isDirty;
      }

  /*============================================================================================================================
  | KEY
  >=============================================================================================================================
  | String containing the key of the attribute.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public string Key {
      get {
        return _key;
        }
      set {
        _key = value;
        }
      }

  /*============================================================================================================================
  | VALUE
  >=============================================================================================================================
  | String containing the current value of the attribute. Sets IsDirty based on whether the value has changed.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public string Value {
      get {
        return _value;
        }
      set {
        _isDirty = !value.Equals(_value) || _isDirty;
        _value = value;
        }
      }

  /*============================================================================================================================
  | IS DIRTY
  >=============================================================================================================================
  | Boolean setting which is set automatically when an attribute's Value is set to a new value.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public bool IsDirty {
      get {
        return _isDirty;
        }
      set {
        _isDirty = value;
        }
      }

  /*=========================================================================================================================
  | PROPERTY: LAST MODIFIED
  >==========================================================================================================================
  | Provides reference to last DateTime the AttributeValue instance was updated.
  \------------------------------------------------------------------------------------------------------------------------*/
    public readonly DateTime LastModified       = DateTime.Now;

    } //Class

  } //Namepace
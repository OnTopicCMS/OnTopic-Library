/*==============================================================================================================================
| Author        Jeremy Caney, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
|
| Purpose       The ContentType object provides a specific implementation of Topic that is optimized for working with
|               ContentType Topics.
|
\=============================================================================================================================*/
using System;
using System.Collections.Generic;

namespace Ignia.Topics {

  /*==============================================================================================================================
  | CLASS
  \-----------------------------------------------------------------------------------------------------------------------------*/
  public class ContentType : Topic {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
    private   Dictionary<string, Attribute>             _supportedAttributes            = null;

  /*============================================================================================================================
  | CONSTRUCTOR
  >=============================================================================================================================
  | Constructors for the topic object.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public ContentType() : base() { }

    public ContentType(string key) : base(key) { }

  /*============================================================================================================================
  | PROPERTY: SUPPORTED ATTRIBUTE
  >=============================================================================================================================
  | Provides a list of Attribute objects that are supported for objects implementing this ContentType.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public Dictionary<string, Attribute> SupportedAttributes {
      get {

        if (_supportedAttributes == null) {

        /*----------------------------------------------------------------------------------------------------------------------
        | CREATE NEW INSTANCE
        \---------------------------------------------------------------------------------------------------------------------*/
          _supportedAttributes = new Dictionary<string, Attribute>();

        /*----------------------------------------------------------------------------------------------------------------------
        | VALIDATE ATTRIBUTES COLLECTION
        \---------------------------------------------------------------------------------------------------------------------*/
          if (!this.Contains("Attributes")) {
            throw new Exception("The ContentType '" + this.Title + "' does not contain a nested topic named 'Attributes' as expected.");
          }

        /*----------------------------------------------------------------------------------------------------------------------
        | GET VALUES FROM SELF
        >-----------------------------------------------------------------------------------------------------------------------
        | ### NOTE KLT052015: The (ContentType)Topic.Attributes property is an AttributeValue collection, not an Attribute
        | collection.
        >-----------------------------------------------------------------------------------------------------------------------
        | ### NOTE KLT052015: The only place this is really used (and where the strongly-typed Attribute is needed) is in
        | SqlTopicDataProvider.cs (lines 408 - 422), where it is used to add Attributes to the null Attributes collection; the
        | Type property is used for determining whether the Attribute Topic is a Relationships definition or Nested Topic.
        \---------------------------------------------------------------------------------------------------------------------*/
          foreach (Attribute attribute in this["Attributes"]) {
            _supportedAttributes.Add(attribute.Key, attribute);
          }

        /*----------------------------------------------------------------------------------------------------------------------
        | GET VALUES FROM PARENT
        \---------------------------------------------------------------------------------------------------------------------*/
          ContentType parent = this.Parent as ContentType;
          if (parent != null) {
            foreach (Attribute attribute in parent.SupportedAttributes.Values) {
              if (!_supportedAttributes.ContainsKey(attribute.Key)) {
                _supportedAttributes.Add(attribute.Key, attribute);
              }
            }
          }

        }

        return _supportedAttributes;
      }
    }

  } //Class

} //Namespace

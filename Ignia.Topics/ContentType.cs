/*==============================================================================================================================
| Author        Jeremy Caney, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a specific implementation of Topic that is optimized for working with ContentType Topics.
  /// </summary>
  public class ContentType : Topic {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
    private   Dictionary<string, Attribute>             _supportedAttributes            = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///  Initializes a new instance of the <see cref="ContentType"/> class.
    /// </summary>
    /// <remarks>
    ///   Optional overload allows the object to be constructed based on the Attribute's <see cref="Topic.Key"/> property.
    /// </remarks>
    /// <param name="key">
    ///   The string identifier for the <see cref="ContentType"/> Topic.
    /// </param>
    public ContentType() : base() { }

    public ContentType(string key) : base(key) { }

    /*==========================================================================================================================
    | PROPERTY: SUPPORTED ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///  Provides a list of Attribute objects that are supported for objects implementing this ContentType.
    /// </summary>
    public Dictionary<string, Attribute> SupportedAttributes {
      get {

        if (_supportedAttributes == null) {

          /*--------------------------------------------------------------------------------------------------------------------
          | Create new instance
          \-------------------------------------------------------------------------------------------------------------------*/
          _supportedAttributes = new Dictionary<string, Attribute>();

          /*--------------------------------------------------------------------------------------------------------------------
          | Validate Attributes collection
          \-------------------------------------------------------------------------------------------------------------------*/
          if (!this.Contains("Attributes")) {
            throw new Exception("The ContentType '" + this.Title + "' does not contain a nested topic named 'Attributes' as expected.");
          }

          /*--------------------------------------------------------------------------------------------------------------------
          | Get values from self
          >---------------------------------------------------------------------------------------------------------------------
          | ### NOTE KLT052015: The (ContentType)Topic.Attributes property is an AttributeValue collection, not an Attribute
          | collection.
          >---------------------------------------------------------------------------------------------------------------------
          | ### NOTE KLT052015: The only place this is really used (and where the strongly-typed Attribute is needed) is in
          | SqlTopicDataProvider.cs (lines 408 - 422), where it is used to add Attributes to the null Attributes collection; the
          | Type property is used for determining whether the Attribute Topic is a Relationships definition or Nested Topic.
          \-------------------------------------------------------------------------------------------------------------------*/
          foreach (Attribute attribute in this["Attributes"]) {
            _supportedAttributes.Add(attribute.Key, attribute);
          }

          /*--------------------------------------------------------------------------------------------------------------------
          | Get values from parent
          \-------------------------------------------------------------------------------------------------------------------*/
          ContentType parent = this.Parent as ContentType;
          if (parent != null) {
            foreach (Attribute attribute in parent.SupportedAttributes.Values) {
              if (!_supportedAttributes.ContainsKey(attribute.Key)) {
                _supportedAttributes.Add(attribute.Key, attribute);
              }
            }
          }

        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return the dictionary object
        \---------------------------------------------------------------------------------------------------------------------*/
        return _supportedAttributes;

      }
    }

  } //Class

} //Namespace

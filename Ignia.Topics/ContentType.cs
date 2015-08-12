/*==============================================================================================================================
| Author        Jeremy Caney, Ignia LLC
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
  ///   Provides a strongly-typed implementation of <see cref="Topic"/> that exposes properties specific to ContentType.
  /// </summary>
  /// <remarks>
  ///   Each topic is associated with a content type. The content type determines which attributes are displayed in the Topics 
  ///   Editor (via the <see cref="SupportedAttributes"/> property). The content type also determines, by default, which view
  ///   is rendered by the <see cref="Topics.Web.TopicsRouteHandler"/> (assuming the value isn't overwritten down the pipe). 
  /// </remarks>
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
    public ContentType() : base() { }

    /// <param name="key">
    ///   The string identifier for the <see cref="ContentType"/> Topic.
    /// </param>
    public ContentType(string key) : base(key) { }

    /*==========================================================================================================================
    | PROPERTY: SUPPORTED ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a list of <see cref="Attribute"/> objects that are supported for objects implementing this ContentType.
    /// </summary>
    /// <remarks>
    ///   Attributes are not just derived from the specific Content Type topic in the database. They are also inherited from 
    ///   any parent content types. For instance, if a Content Type "Page" has an attribute "Body", then all Content Types 
    ///   created underneath "Page" will also have an attribute "Body". As such, the <see cref="SupportedAttributes"/> property
    ///   must crawl through each parent Content Type to collate the list of supported attributes.
    /// </remarks>
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
            throw new Exception(
              "The ContentType '" + this.Title + "' does not contain a nested topic named 'Attributes' as expected."
            );
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

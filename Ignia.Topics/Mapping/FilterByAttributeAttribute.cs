/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ignia.Topics.Collections;

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | ATTRIBUTE: FILTER BY ATTRIBUTE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Flags that a collection property should be filtered by a specified <c>attributeKey</c> and <c>attributeValue</c>.
  /// </summary>
  /// <remarks>
  ///   By default, <see cref="ITopicMappingService"/> will add any corresponding relationships to a collection, assuming they
  ///   are assignable to the collection's base type. With the <c>[FilterByAttribute(attributeKey, attributeValue)]</c>
  ///   attribute, the collection will instead be filtered to only those topics that have the specified attribute value
  ///   assigned.
  /// </remarks>
  [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=true, Inherited=true)]
  public sealed class FilterByAttributeAttribute : System.Attribute {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     string                          _attributeKey                   = null;
    private                     string                          _attributeValue                 = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="FilterByAttributeAttribute"/> class by providing a (required) attribute key
    ///   and value.
    /// </summary>
    /// <param name="attributeKey">The key of the attribute to filter by.</param>
    /// <param name="attributeValue">The value of the attribute to filter by.</param>
    public FilterByAttributeAttribute(string attributeKey, string attributeValue) {
      TopicFactory.ValidateKey(attributeKey, false);
      _attributeKey = attributeKey;
      _attributeValue = attributeValue;
    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the attribute key.
    /// </summary>
    public string Key {
      get {
        return _attributeKey;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the value of the attribute.
    /// </summary>
    public string Value {
      get {
        return _attributeValue;
      }
    }

  } //Class

} //Namespace

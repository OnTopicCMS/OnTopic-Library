/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ATTRIBUTE: FILTER BY ATTRIBUTE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Flags that a collection property should be filtered by a specified <c>attributeKey</c> and <c>attributeValue</c>.
  /// </summary>
  /// <remarks>
  ///   By default, <see cref="ITopicMappingService"/> will add any corresponding topics to a collection, assuming they are
  ///   assignable to the collection's base type. With the <c>[FilterByAttribute(attributeKey, attributeValue)]</c> attribute,
  ///   the collection will instead be filtered to only those topics that have the specified attribute value assigned.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple=true, Inherited=true)]
  public sealed class FilterByAttributeAttribute : Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="FilterByAttributeAttribute"/> class by providing a (required) attribute key
    ///   and value.
    /// </summary>
    /// <param name="key">The key of the attribute to filter by.</param>
    /// <param name="value">The value of the attribute to filter by.</param>
    public FilterByAttributeAttribute(string key, string value) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      TopicFactory.ValidateKey(key, false);
      Contract.Requires<ArgumentException>(
        !key.Equals("ContentType", StringComparison.OrdinalIgnoreCase),
        "The ContentType is not stored as an attribute. To filter by ContentType, use [FilterByContentType] instead."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Set properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      Key = key;
      Value = value;

    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the attribute key.
    /// </summary>
    public string Key { get; }

    /*==========================================================================================================================
    | PROPERTY: VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the value of the attribute.
    /// </summary>
    public string Value { get; }

  } //Class
} //Namespace
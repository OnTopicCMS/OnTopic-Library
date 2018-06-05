/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Collections;

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | ATTRIBUTE: ATTRIBUTE KEY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Flags that a property should be mapped to a specific <c>attributeKey</c> in when calling <see
  ///   cref="AttributeValueCollection.GetValue(String, Boolean)"/>.
  /// </summary>
  /// <remarks>
  ///   By default, <see cref="ITopicMappingService"/> implementations will attempt to map the property of the target data
  ///   transfer object to a corresponding attribute of the same name on the source topic. This attribute instructs the
  ///   <see cref="ITopicMappingService"/> to instead look for a specified key. This allows the target property name to be
  ///   decoupled from the source attribute key.
  /// </remarks>
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public sealed class AttributeKeyAttribute : System.Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="AttributeKeyAttribute"/> class by providing a (required) attribute key.
    /// </summary>
    /// <param name="attributeKey">The key value of the attribute associated with the current property.</param>
    public AttributeKeyAttribute(string attributeKey) {
      TopicFactory.ValidateKey(attributeKey, false);
      Value = attributeKey;
    }

    /*==========================================================================================================================
    | PROPERTY: VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the value of the attribute key.
    /// </summary>
    public string Value { get; }

  } //Class

} //Namespace

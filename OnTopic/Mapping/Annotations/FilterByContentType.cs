/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ATTRIBUTE: FILTER BY CONTENT TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Flags that a collection property should be filtered by a specified <c>ContentType</c>.
  /// </summary>
  /// <remarks>
  ///   By default, <see cref="ITopicMappingService"/> will add any corresponding topic to a collection, assuming they are
  ///   assignable to the collection's base type. With the <c>[FilterByContentType(contentType)]</c> attribute, the collection
  ///   will instead be filtered to only those topics that have the specified content type.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple=true, Inherited=true)]
  public sealed class FilterByContentTypeAttribute : Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="FilterByContentTypeAttribute"/> class by providing a (required) content type.
    /// </summary>
    /// <param name="contentType">The content type to filter by.</param>
    public FilterByContentTypeAttribute(string contentType) {
      TopicFactory.ValidateKey(contentType, false);
      ContentType = contentType;
    }

    /*==========================================================================================================================
    | PROPERTY: CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the content type.
    /// </summary>
    public string ContentType { get; }

  } //Class
} //Namespace
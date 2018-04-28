/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: CONTENT TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents metadata for describing a content type, including a collection of <see cref="AttributeDescriptor"/>s
  ///   associated with it.
  /// </summary>
  /// <remarks>
  ///   The <see cref="ContentType"/> class is provided exclusively for backward compatibility with topics of the content type
  ///   "ContentType" so that the <see cref="TopicFactory.Create(String, String, Topic)"/> factory method can find a match. Databases
  ///   should be updated to instead use "ContentTypeDescriptor", which better differentiates the metadata from e.g.
  ///   <see cref="Topic.ContentType"/>. The <see cref="ContentType"/> class derives from <see cref="ContentTypeDescriptor"/> to
  ///   ensure they support the same functionality.
  /// </remarks>
  [Obsolete("The ContentType class is deprecated. Topic databases and code should be updated to use ContentTypeDescriptor instead")]
  public class ContentType : ContentTypeDescriptor {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="ContentType"/> class with a <see cref="Topic.Key"/>, <see
    ///   cref="Topic.ContentType"/>, and, optionally, <see cref="Topic.Parent"/>, <see cref="Topic.Id"/>.
    /// </summary>
    /// <remarks>
    ///   By default, when creating new attributes, the <see cref="AttributeValue"/>s for both <see cref="Topic.Key"/> and <see
    ///   cref="Topic.ContentType"/> will be set to <see cref="AttributeValue.IsDirty"/>, which is required in order to
    ///   correctly save new topics to the database. When the <paramref name="id"/> parameter is set, however, the <see
    ///   cref="AttributeValue.IsDirty"/> property is set to <c>false</c> on <see cref="Topic.Key"/> as well as on <see
    ///   cref="Topic.ContentType"/>, since it is assumed these are being set to the same values currently used in the
    ///   persistance store.
    /// </remarks>
    /// <param name="key">A string representing the key for the new topic instance.</param>
    /// <param name="contentType">A string representing the key of the target content type.</param>
    /// <param name="id">The unique identifier assigned by the data store for an existing topic.</param>
    /// <param name="parent">Optional topic to set as the new topic's parent.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when the class representing the content type is found, but doesn't derive from <see cref="Topic"/>.
    /// </exception>
    /// <returns>A strongly-typed instance of the <see cref="Topic"/> class based on the target content type.</returns>
    public ContentType(string key, string contentType, Topic parent, int id = -1) : base(key, contentType, parent, id) { }

  } //Class

} //Namespace
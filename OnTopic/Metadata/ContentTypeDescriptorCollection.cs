/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using OnTopic.Collections;
using OnTopic.Internal.Diagnostics;
using OnTopic.Querying;
using OnTopic.Repositories;

namespace OnTopic.Metadata {

  /*============================================================================================================================
  | CLASS: CONTENT TYPE DESCRIPTOR COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="ContentTypeDescriptor"/> objects.
  /// </summary>
  /// <remarks>
  ///   While the <see cref="ContentTypeDescriptorCollection"/> can be used to store any collection of <see cref=
  ///   "ContentTypeDescriptor"/> objects, it has additional tooling in the form of the <see cref="Refresh(
  ///   ContentTypeDescriptor?)"/> as well as the <see cref="ContentTypeDescriptorCollection(ContentTypeDescriptor?)"/>
  ///   constructor for handling a flattened list of <see cref="ContentTypeDescriptor"/>s used for the <see cref=
  ///   "ITopicRepository.GetContentTypeDescriptors()"/> method.
  /// </remarks>
  public class ContentTypeDescriptorCollection : KeyedTopicCollection<ContentTypeDescriptor> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ContentTypeDescriptorCollection"/> class.
    /// </summary>
    public ContentTypeDescriptorCollection() : base(null) {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="ContentTypeDescriptorCollection"/> class based on a root <see cref=
    ///   "ContentTypeDescriptor"/>.
    /// </summary>
    /// <param name="rootContentType">The <see cref="ContentTypeDescriptor"/> from which to initialize the collection.</param>
    public ContentTypeDescriptorCollection(ContentTypeDescriptor? rootContentType) : base(null) {
      Refresh(rootContentType);
    }

    /*==========================================================================================================================
    | REFRESH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Updates the current instance of the <see cref="ContentTypeDescriptorCollection"/> based on a new root <see cref=
    ///   "ContentTypeDescriptor"/>.
    /// </summary>
    /// <param name="rootContentType">The <see cref="ContentTypeDescriptor"/> from which to initialize the collection.</param>
    public void Refresh(ContentTypeDescriptor? rootContentType) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      #pragma warning disable IDE0078 // Use pattern matching
      if (rootContentType is null || rootContentType is { Children: { Count: 0 } }) {
        return;
      }
      #pragma warning restore IDE0078 // Use pattern matching

      Contract.Requires(
        rootContentType.Key.Equals("ContentTypes", StringComparison.OrdinalIgnoreCase),
        $"The {nameof(rootContentType)} is expected to represent the root of the content type topic graph, with a " +
        $"key of 'ContentTypes'. Instead, the supplied ContentTypeDescriptor has a key of '{rootContentType.Key}'."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Clear any existing values
      \-----------------------------------------------------------------------------------------------------------------------*/
      Clear();

      /*------------------------------------------------------------------------------------------------------------------------
      | Add all ContentTypeDescriptors to collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var contentTypeDescriptors = rootContentType
        .FindAll(t => typeof(ContentTypeDescriptor).IsAssignableFrom(t.GetType()))
        .Cast<ContentTypeDescriptor>();
      foreach (var contentType in contentTypeDescriptors) {
        Add((ContentTypeDescriptor)contentType);
      }

    }

  } //Class
} //Namespace
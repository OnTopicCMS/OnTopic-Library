/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using OnTopic.Collections;
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
  public class ContentTypeDescriptorCollection : TopicCollection<ContentTypeDescriptor> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ContentTypeDescriptorCollection"/> class.
    /// </summary>
    public ContentTypeDescriptorCollection() : base(null) {
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
      if (rootContentType == null || rootContentType.Children.Count == 0) {
        return;
      }

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
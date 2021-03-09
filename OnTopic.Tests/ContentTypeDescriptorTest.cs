/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Metadata;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: CONTENT TYPE DESCRIPTOR TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="ContentTypeDescriptor"/> class and other types associated with it, such as <see
  ///   cref="AttributeDescriptor"/>, <see cref="ContentTypeDescriptorCollection"/>, and <see cref="
  ///   AttributeDescriptorCollection"/>.
  /// </summary>
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class ContentTypeDescriptorTest {

    /*==========================================================================================================================
    | TEST: CONTENT TYPE DESCRIPTOR: PERMITTED CONTENT TYPES: RETURNS COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a hierarchy of <see cref="ContentTypeDescriptor"/>s, and ensures that <see cref="ContentTypeDescriptor.
    ///   PermittedContentTypes"/> returns all <see cref="ContentTypeDescriptor"/> instances from the associated <c>ContentTypes
    ///   </c> collection in <see cref="Topic.Relationships"/>.
    /// </summary>
    [TestMethod]
    public void ContentTypeDescriptor_PermittedContentTypes_ReturnsCollection() {

      var page                  = new ContentTypeDescriptor("Page", "ContentTypeDescriptor");
      var video                 = new ContentTypeDescriptor("Video", "ContentTypeDescriptor", page);

      page.Relationships.SetValue("ContentTypes", page);
      page.Relationships.SetValue("ContentTypes", video);

      Assert.AreEqual<int>(2, page.PermittedContentTypes.Count);
      Assert.AreEqual<int>(0, video.PermittedContentTypes.Count);
      Assert.IsTrue(page.PermittedContentTypes.Contains(page));
      Assert.IsTrue(page.PermittedContentTypes.Contains(video));

    }

    /*==========================================================================================================================
    | TEST: CONTENT TYPE DESCRIPTOR: RESET PERMITTED CONTENT TYPES: RETURNS UPDATED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a hierarchy of <see cref="ContentTypeDescriptor"/>s, and ensures that <see cref="ContentTypeDescriptor.
    ///   PermittedContentTypes"/> returns all updated <see cref="ContentTypeDescriptor"/> instances from the associated
    ///   <c>ContentTypes</c> collection in <see cref="Topic.Relationships"/> after calling <see cref="ContentTypeDescriptor.
    ///   ResetPermittedContentTypes"/>.
    /// </summary>
    [TestMethod]
    public void ContentTypeDescriptor_ResetPermittedContentTypes_ReturnsUpdated() {

      var page                  = new ContentTypeDescriptor("Page", "ContentTypeDescriptor");
      var video                 = new ContentTypeDescriptor("Video", "ContentTypeDescriptor", page);
      var slideshow             = new ContentTypeDescriptor("Slideshow", "ContentTypeDescriptor", page);

      page.Relationships.SetValue("ContentTypes", page);
      page.Relationships.SetValue("ContentTypes", video);

      var initialCollection     = page.PermittedContentTypes.ToList();

      page.ResetPermittedContentTypes();
      page.Relationships.Remove("ContentTypes", page);
      page.Relationships.SetValue("ContentTypes", slideshow);

      Assert.AreEqual<int>(2, page.PermittedContentTypes.Count);
      Assert.IsTrue(page.PermittedContentTypes.Contains(video));
      Assert.IsTrue(page.PermittedContentTypes.Contains(slideshow));
      Assert.IsFalse(page.PermittedContentTypes.Contains(page));
      Assert.IsTrue(initialCollection.Contains(page));
      Assert.IsFalse(initialCollection.Contains(slideshow));

    }

    /*==========================================================================================================================
    | TEST: CONTENT TYPE DESCRIPTOR: ATTRIBUTE DESCRIPTORS: RETURNS INHERITED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a hierarchy of <see cref="ContentTypeDescriptor"/>s, and ensures that <see cref="ContentTypeDescriptor.
    ///   AttributeDescriptors"/> returns all <see cref="AttributeDescriptor"/> instances from the associated <c>Attributes</c>
    ///   collection in <see cref="Topic.Relationships"/>, as well as those from each <see cref="Topic.Parent"/>.
    /// </summary>
    [TestMethod]
    public void ContentTypeDescriptor_AttributeDescriptors_ReturnsInherited() {

      var page                  = new ContentTypeDescriptor("Page", "ContentTypeDescriptor");
      var pageAttributes        = new ContentTypeDescriptor("Attributes", "List", page);
      var titleAttribute        = new AttributeDescriptor("Title", "AttributeDescriptor", pageAttributes);

      var video                 = new ContentTypeDescriptor("Video", "ContentTypeDescriptor", page);
      var videoAttributes       = new ContentTypeDescriptor("Attributes", "List", video);
      var urlAttribute          = new AttributeDescriptor("Url", "AttributeDescriptor", videoAttributes);

      Assert.AreEqual<int>(1, page.AttributeDescriptors.Count);
      Assert.AreEqual<int>(2, video.AttributeDescriptors.Count);
      Assert.IsTrue(video.AttributeDescriptors.Contains(titleAttribute));
      Assert.IsTrue(video.AttributeDescriptors.Contains(urlAttribute));

    }

  } //Class
} //Namespace
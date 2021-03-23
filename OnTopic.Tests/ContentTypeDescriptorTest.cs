/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OnTopic.Metadata;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: CONTENT TYPE DESCRIPTOR TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="ContentTypeDescriptor"/> class and other types associated with it, such as <see
  ///   cref="AttributeDescriptor"/>, <see cref="ContentTypeDescriptorCollection"/>, and <see cref="
  ///   AttributeDescriptorCollection"/>.
  /// </summary>
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
    [Fact]
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
    [Fact]
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
    [Fact]
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

    /*==========================================================================================================================
    | TEST: CONTENT TYPE DESCRIPTOR: RESET ATTRIBUTE DESCRIPTORS: RETURNS UPDATED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a hierarchy of <see cref="ContentTypeDescriptor"/>s, and ensures that <see cref="ContentTypeDescriptor.
    ///   AttributeDescriptors"/> returns all new <see cref="AttributeDescriptor"/> instances from the associated <c>Attributes
    ///   </c>collection in <see cref="Topic.Relationships"/>, as well as those from each <see cref="Topic.Parent"/>, after
    ///   calling <see cref="ContentTypeDescriptor.ResetAttributeDescriptors()"/>.
    /// </summary>
    [Fact]
    public void ContentTypeDescriptor_ResetAttributeDescriptors_ReturnsUpdated() {

      var page                  = new ContentTypeDescriptor("Page", "ContentTypeDescriptor");
      var pageAttributes        = new ContentTypeDescriptor("Attributes", "List", page);
      var titleAttribute        = new AttributeDescriptor("Title", "AttributeDescriptor", pageAttributes);

      var video                 = new ContentTypeDescriptor("Video", "ContentTypeDescriptor", page);
      var videoAttributes       = new ContentTypeDescriptor("Attributes", "List", video);
      var urlAttribute          = new AttributeDescriptor("Url", "AttributeDescriptor", videoAttributes);

      var initialCollection     = video.AttributeDescriptors.ToList();

      var descriptionAttribute  = new AttributeDescriptor("Description", "AttributeDescriptor", pageAttributes);

      pageAttributes.Children.Remove(titleAttribute);
      page.ResetAttributeDescriptors();

      Assert.AreEqual<int>(1, page.AttributeDescriptors.Count);
      Assert.AreEqual<int>(2, video.AttributeDescriptors.Count);
      Assert.IsTrue(video.AttributeDescriptors.Contains(descriptionAttribute));
      Assert.IsTrue(video.AttributeDescriptors.Contains(urlAttribute));
      Assert.IsFalse(video.AttributeDescriptors.Contains(titleAttribute));
      Assert.IsTrue(initialCollection.Contains(titleAttribute));
      Assert.IsFalse(initialCollection.Contains(descriptionAttribute));

    }

    /*==========================================================================================================================
    | TEST: IS TYPE OF: DERIVED CONTENT TYPE: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Associates a new topic with several content types, and confirms that the topic is reported as a type of those content
    ///   types.
    /// </summary>
    [Fact]
    public void IsTypeOf_DerivedContentType_ReturnsTrue() {

      var contentType = new ContentTypeDescriptor("Root", "ContentTypeDescriptor");
      for (var i = 0; i < 5; i++) {
        var childContentType = new ContentTypeDescriptor("ContentType" + i, "ContentTypeDescriptor", contentType);
        contentType             = childContentType;
      }

      Assert.IsTrue(contentType.IsTypeOf("Root"));

    }

    /*==========================================================================================================================
    | TEST: IS TYPE OF: INVALID CONTENT TYPE: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Associates a new topic with several content types, and confirms that the topic is not reported as a type of a content
    ///   type that is not in that chain.
    /// </summary>
    [Fact]
    public void IsTypeOf_InvalidContentType_ReturnsFalse() {

      var contentType = new ContentTypeDescriptor("Root", "ContentTypeDescriptor");
      for (var i = 0; i < 5; i++) {
        var childContentType = new ContentTypeDescriptor("ContentType" + i, "ContentTypeDescriptor", contentType);
        contentType             = childContentType;
      }

      Assert.IsFalse(contentType.IsTypeOf("DifferentRoot"));

    }

    /*==========================================================================================================================
    | TEST: CONTENT TYPE DESCRIPTOR COLLECTION: CONSTRUCT WITH VALUES: RETURNS VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a new <see cref="ContentTypeDescriptorCollection"/> with a <see cref="ContentTypeDescriptor"/> and confirms
    ///   that all child <see cref="ContentTypeDescriptor"/>s within the topic graph are added.
    /// </summary>
    [Fact]
    public void ContentTypeDescriptorCollection_ConstructWithValues_ReturnsValues() {

      var rootContentType       = new ContentTypeDescriptor("ContentTypes", "ContentTypeDescriptor");
      var pageContentType       = new ContentTypeDescriptor("Page", "ContentTypeDescriptor", rootContentType);
      _                         = new ContentTypeDescriptor("Video", "ContentTypeDescriptor", pageContentType);

      var contentTypeCollection = new ContentTypeDescriptorCollection(rootContentType);

      Assert.AreEqual<int>(3, contentTypeCollection.Count);

    }

    /*==========================================================================================================================
    | TEST: CONTENT TYPE DESCRIPTOR COLLECTION: REFRESH: RETURNS UPDATED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a new <see cref="ContentTypeDescriptorCollection"/> with a <see cref="ContentTypeDescriptor"/> and confirms
    ///   that all child <see cref="ContentTypeDescriptor"/>s within the topic graph are added.
    /// </summary>
    [Fact]
    public void ContentTypeDescriptorCollection_Refresh_ReturnsUpdated() {

      var rootContentType       = new ContentTypeDescriptor("ContentTypes", "ContentTypeDescriptor");
      var pageContentType       = new ContentTypeDescriptor("Page", "ContentTypeDescriptor", rootContentType);
      var videoContentType      = new ContentTypeDescriptor("Video", "ContentTypeDescriptor", pageContentType);
      var slideshowContentType  = new ContentTypeDescriptor("Slideshow", "ContentTypeDescriptor");

      var contentTypeCollection = new ContentTypeDescriptorCollection(rootContentType);

      pageContentType.Children.Remove(videoContentType);
      pageContentType.Children.Add(slideshowContentType);

      contentTypeCollection.Refresh(rootContentType);

      Assert.AreEqual<int>(3, contentTypeCollection.Count);
      Assert.IsTrue(contentTypeCollection.Contains(slideshowContentType));
      Assert.IsFalse(contentTypeCollection.Contains(videoContentType));

    }

    /*==========================================================================================================================
    | TEST: CONTENT TYPE DESCRIPTOR COLLECTION: CONSTRUCT WITH VALUES: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a new <see cref="ContentTypeDescriptorCollection"/> with a <see cref="ContentTypeDescriptor"/> and confirms
    ///   that an exception is thrown if the <see cref="Topic.Key"/> is not set to the expected value of <c>ContentTypes</c>,
    ///   which represents the root content type.
    /// </summary>
    [Fact]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ContentTypeDescriptorCollection_ConstructWithValues_ThrowsException() {

      var rootContentType       = new ContentTypeDescriptor("ContentType", "ContentTypeDescriptor");
      var pageContentType       = new ContentTypeDescriptor("Page", "ContentTypeDescriptor", rootContentType);
      _                         = new ContentTypeDescriptor("Video", "ContentTypeDescriptor", pageContentType);
      _                         = new ContentTypeDescriptorCollection(pageContentType);

    }

  } //Class
} //Namespace
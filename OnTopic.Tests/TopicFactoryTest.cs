/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Metadata;
using OnTopic.Repositories;
using Xunit;

namespace OnTopic.Tests;

/*==============================================================================================================================
| CLASS: TOPIC FACTORY TESTS
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides unit tests for the <see cref="TopicFactory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TopicFactoryTest {

  /*============================================================================================================================
  | TEST: CREATE: RETURNS TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a topic using the factory method, and ensures it's correctly returned.
  /// </summary>
  [Fact]
  public void Create_ReturnsTopic() {
    var topic                   = TopicFactory.Create("Test", "Page");
    Assert.NotNull(topic);
    Assert.Equal("Test", topic.Key);
    Assert.Equal("Page", topic.ContentType);
  }

  /*============================================================================================================================
  | TEST: CREATE: CONTENT TYPE: RETURNS DERIVED TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a topic of a content type which maps to a class derived from <see cref="Topic"/>, and ensures the derived
  ///   version of the <see cref="Topic"/> class is returned.
  /// </summary>
  [Fact]
  public void Create_ContentType_ReturnsDerivedTopic() {
    var topic                   = TopicFactory.Create("Test", "ContentTypeDescriptor");
    Assert.NotNull(topic);
    Assert.IsType<ContentTypeDescriptor>(topic);
  }

  /*============================================================================================================================
  | TEST: CREATE: ATTRIBUTE DESCRIPTOR: RETURNS FALLBACK
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a topic with a <see cref="Topic.ContentType"/> ending with <c>AttributeDescriptor</c> and ensures that, by
  ///   convention, a <see cref="AttributeDescriptor"/> is returned.
  /// </summary>
  /// <remarks>
  ///   This is a special use case to address the fact that we expect concrete types of <see cref="AttributeDescriptor"/> to
  ///   be in external plugin libraries, but the <see cref="ITopicRepository"/> only needs to know that they're an <see cref=
  ///   "AttributeDescriptor"/>. This is similar to how other types will fallback to <see cref="Topic"/> if no matching type
  ///   can be found in the <see cref="TopicFactory.TypeLookupService"/>.
  /// </remarks>
  [Fact]
  public void Create_AttributeDescriptor_ReturnsFallback() {
    var topic                   = TopicFactory.Create("Test", "ArbitraryAttributeDescriptor");
    Assert.NotNull(topic);
    Assert.IsType<AttributeDescriptor>(topic);
  }

  /*============================================================================================================================
  | TEST: NORMALIZE UNIQUE KEY: BARE KEY: RETURNS QUALIFIED KEY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Establishes a bare key and confirms that <see cref="TopicFactory.NormalizeUniqueKey(String, String)"/> qualifies it
  ///   against the root key.
  /// </summary>
  [Fact]
  public void NormalizeUniqueKey_BareKey_ReturnsQualifiedKey() =>
    Assert.Equal("Root:Web", TopicFactory.NormalizeUniqueKey("Web", "Root"));

  /*============================================================================================================================
  | TEST: NORMALIZE UNIQUE KEY: QUALIFIED KEY: RETURNS UNCHANGED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Establishes an already-qualified key and confirms that <see cref="TopicFactory.NormalizeUniqueKey(String, String)"/>
  ///   leaves it unchanged.
  /// </summary>
  [Fact]
  public void NormalizeUniqueKey_QualifiedKey_ReturnsUnchanged() =>
    Assert.Equal("Root:Web", TopicFactory.NormalizeUniqueKey("Root:Web", "Root"));

  /*============================================================================================================================
  | TEST: NORMALIZE UNIQUE KEY: ROOT KEY: RETURNS UNCHANGED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Establishes the root key itself and confirms that <see cref="TopicFactory.NormalizeUniqueKey(String, String)"/>
  ///   leaves it unchanged.
  /// </summary>
  [Fact]
  public void NormalizeUniqueKey_RootKey_ReturnsUnchanged() =>
    Assert.Equal("Root", TopicFactory.NormalizeUniqueKey("Root", "Root"));

  /*============================================================================================================================
  | TEST: NORMALIZE UNIQUE KEY: FALSE POSITIVE ROOT: RETURNS QUALIFIED KEY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Establishes a key that merely starts with the same characters as the root key, but is not actually a child of it, and
  ///   confirms that <see cref="TopicFactory.NormalizeUniqueKey(String, String)"/> still qualifies it, rather than mistaking
  ///   it for an already-qualified key.
  /// </summary>
  [Fact]
  public void NormalizeUniqueKey_FalsePositiveRoot_ReturnsQualifiedKey() =>
    Assert.Equal("Root:Rootbeer", TopicFactory.NormalizeUniqueKey("Rootbeer", "Root"));

  /*============================================================================================================================
  | TEST: NORMALIZE UNIQUE KEY: LEADING COLON: RETURNS QUALIFIED KEY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Establishes a key with a leading colon and confirms that <see cref="TopicFactory.NormalizeUniqueKey(String, String)"/>
  ///   qualifies it against the root key without introducing a doubled colon.
  /// </summary>
  [Fact]
  public void NormalizeUniqueKey_LeadingColon_ReturnsQualifiedKey() =>
    Assert.Equal("Root:Web", TopicFactory.NormalizeUniqueKey(":Web", "Root"));

  /*============================================================================================================================
  | TEST: NORMALIZE UNIQUE KEY: CUSTOM ROOT KEY: RETURNS QUALIFIED KEY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Establishes a bare key with a non-<c>Root</c> root key and confirms that <see cref=
  ///   "TopicFactory.NormalizeUniqueKey(String, String)"/> honors the supplied root key.
  /// </summary>
  [Fact]
  public void NormalizeUniqueKey_CustomRootKey_ReturnsQualifiedKey() =>
    Assert.Equal("Configuration:Metadata", TopicFactory.NormalizeUniqueKey("Metadata", "Configuration"));

} //Class
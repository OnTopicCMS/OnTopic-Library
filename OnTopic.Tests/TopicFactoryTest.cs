/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
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
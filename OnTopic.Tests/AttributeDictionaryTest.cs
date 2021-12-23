/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Attributes;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE DICTIONARY TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="AttributeDictionary"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class AttributeDictionaryTest {

    /*==========================================================================================================================
    | TEST: GET VALUE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="AttributeDictionary"/> based on the <paramref name="input"/> data and confirms that <see cref=
    ///   "AttributeDictionary.GetValue(String)"/> returns the <paramref name="expected"/> value.
    /// </summary>
    /// <param name="input">The value to add to the dictionary.</param>
    /// <param name="expected">The value expected to be returned.</param>
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(" ", null)]
    [InlineData("0", "0")]
    [InlineData("True", "True")]
    [InlineData("Hello", "Hello")]
    public void GetValue_ReturnsExpectedValue(string? input, string? expected) {

      var attributes            = new AttributeDictionary() {{"Key", input}};

      Assert.Equal(expected, attributes.GetValue("Key"));

    }

    /*==========================================================================================================================
    | TEST: GET BOOLEAN: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="AttributeDictionary"/> based on the <paramref name="input"/> data and confirms that <see cref=
    ///   "AttributeDictionary.GetBoolean(String)"/> returns the <paramref name="expected"/> value.
    /// </summary>
    /// <param name="input">The value to add to the dictionary.</param>
    /// <param name="expected">The value expected to be returned.</param>
    [Theory]
    [InlineData("0", false)]
    [InlineData("1", true)]
    [InlineData("False", false)]
    [InlineData("True", true)]
    [InlineData("", null)]
    [InlineData("Hello", null)]
    public void GetBoolean_ReturnsExpectedValue(string input, bool? expected) {

      var attributes            = new AttributeDictionary() {{"Key", input}};

      Assert.Equal(expected, attributes.GetBoolean("Key"));

    }

    /*==========================================================================================================================
    | TEST: GET INTEGER: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="AttributeDictionary"/> based on the <paramref name="input"/> data and confirms that <see cref=
    ///   "AttributeDictionary.GetInteger(String)"/> returns the <paramref name="expected"/> value.
    /// </summary>
    /// <param name="input">The value to add to the dictionary.</param>
    /// <param name="expected">The value expected to be returned.</param>
    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("2.4", null)]
    [InlineData("", null)]
    [InlineData("Hello", null)]
    public void GetInteger_ReturnsExpectedValue(string input, int? expected) {

      var attributes            = new AttributeDictionary() {{"Key", input}};

      Assert.Equal(expected, attributes.GetInteger("Key"));

    }

    /*==========================================================================================================================
    | TEST: GET DOUBLE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="AttributeDictionary"/> based on the <paramref name="input"/> data and confirms that <see cref=
    ///   "AttributeDictionary.GetDouble(String)"/> returns the <paramref name="expected"/> value.
    /// </summary>
    /// <param name="input">The value to add to the dictionary.</param>
    /// <param name="expected">The value expected to be returned.</param>
    [Theory]
    [InlineData("0.0", 0.0)]
    [InlineData("1.0", 1.0)]
    [InlineData("1", 1.0)]
    [InlineData("1.4", 1.4)]
    [InlineData("", null)]
    [InlineData("Hello", null)]
    public void GetDouble_ReturnsExpectedValue(string input, double? expected) {

      var attributes            = new AttributeDictionary() {{"Key", input}};

      Assert.Equal(expected, attributes.GetDouble("Key"));

    }

    /*==========================================================================================================================
    | TEST: GET DATE/TIME: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="AttributeDictionary"/> based on the <paramref name="input"/> data and confirms that <see cref=
    ///   "AttributeDictionary.GetDateTime(String)"/> returns the expected value.
    /// </summary>
    /// <param name="input">The value to add to the dictionary.</param>
    /// <param name="isSet">Determines whether a valid <see cref="DateTime"/> is expected in response.</param>
    [Theory]
    [InlineData("1976-10-15 01:02:03", true)]
    [InlineData("October 15, 1976 01:02:03 AM", true)]
    [InlineData("15 Oct 1976 01:02:03", true)]
    [InlineData("10/15/1976 01:02:03 AM", true)]
    [InlineData("", false)]
    [InlineData("Hello", false)]
    public void GetDate_ReturnsExpectedValue(string input, bool isSet) {

      var attributes            = new AttributeDictionary() {{"Key", input}};

      Assert.Equal(isSet? new DateTime(1976, 10, 15, 1, 2, 3) : null, attributes.GetDateTime("Key"));

    }

    /*==========================================================================================================================
    | TEST: GET URI: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="AttributeDictionary"/> based on the <paramref name="input"/> data and confirms that <see cref=
    ///   "AttributeDictionary.GetUri(String)"/> returns the expected value.
    /// </summary>
    /// <param name="input">The value to add to the dictionary.</param>
    [Theory]
    [InlineData("https://www.github.com/OnTopicCMS")]
    [InlineData("Some:\\\\URL")]
    public void GetUri_ReturnsExpectedValue(string input) {

      var attributes            = new AttributeDictionary() {{"Key", input}};

      Assert.Equal(new Uri(input), attributes.GetUri("Key"));

    }

    /*==========================================================================================================================
    | TEST: GET {TYPE}: INVALID KEY: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="AttributeDictionary"/> and confirms that each of the <see cref="AttributeDictionary.GetValue(
    ///   String)"/> methods return <c>null</c> if an invalid key is passed.
    /// </summary>
    [Fact]
    public void GetType_InvalidKey_ReturnsNull() {

      var attributes            = new AttributeDictionary();

      Assert.Null(attributes.GetValue("MissingKey"));
      Assert.Null(attributes.GetBoolean("MissingKey"));
      Assert.Null(attributes.GetInteger("MissingKey"));
      Assert.Null(attributes.GetDouble("MissingKey"));
      Assert.Null(attributes.GetDateTime("MissingKey"));
      Assert.Null(attributes.GetUri("MissingKey"));

    }

    /*==========================================================================================================================
    | TEST: AS ATTRIBUTE DICTIONARY: EXCLUDED KEYS: EXCLUDED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="AttributeDictionary"/> using <see cref="AttributeCollection.AsAttributeDictionary(Boolean)"/>
    ///   and confirms that <see cref="AttributeDictionary.GetValue(String)"/> doesn't include the excluded values.
    /// </summary>
    [Fact]
    public void AsAttributeDictionary_ExcludedKeys_Excluded() {

      var topic                 = new Topic("Test", "Page");

      topic.Attributes.SetValue("Title", "Page Title");
      topic.Attributes.SetValue("LastModified", "October 15, 1976");
      topic.Attributes.SetValue("Subtitle", "Subtitle");

      var attributes            = topic.Attributes.AsAttributeDictionary();

      Assert.Single(attributes.Keys);
      Assert.Null(attributes.GetValue("Title"));
      Assert.Null(attributes.GetValue("LastModified"));
      Assert.Equal("Subtitle", attributes.GetValue("Subtitle"));

    }

    /*==========================================================================================================================
    | TEST: AS ATTRIBUTE DICTIONARY: INHERIT FROM BASE: INHERITS VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="AttributeDictionary"/> using <see cref="AttributeCollection.AsAttributeDictionary(Boolean)"/>
    ///   and confirms that <see cref="AttributeDictionary.GetValue(String)"/> correctly inherits values.
    /// </summary>
    [Fact]
    public void AsAttributeDictionary_InheritFromBase_InheritsValues() {

      var baseTopic             = new Topic("BaseTopic", "Page");
      var topic                 = new Topic("Test", "Page") {
        BaseTopic               = baseTopic
      };

      baseTopic.Attributes.SetValue("Subtitle", "Subtitle");

      var attributes            = topic.Attributes.AsAttributeDictionary(true);

      Assert.Single(attributes.Keys);
      Assert.Equal("Subtitle", attributes.GetValue("Subtitle"));

    }

  } //Class
} //Namespace
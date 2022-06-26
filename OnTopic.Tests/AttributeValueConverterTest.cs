/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE CONVERTER TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="AttributeValueConverter"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class AttributeValueConverterTest {

    /*==========================================================================================================================
    | TEST: IS CONVERTIBLE? TYPE: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Passes a <paramref name="type"/> into the <see cref="AttributeValueConverter.IsConvertible(Type)"/> method and
    ///   confirms that result matches the <paramref name="result"/>.
    ///   <c>true</c>
    /// </summary>
    [Theory]
    [InlineData(                typeof(string),                 true)]
    [InlineData(                typeof(int),                    true)]
    [InlineData(                typeof(bool),                   true)]
    [InlineData(                typeof(DateTime),               true)]
    [InlineData(                typeof(Uri),                    true)]
    [InlineData(                typeof(int?),                   true)]
    [InlineData(                typeof(bool?),                  true)]
    [InlineData(                typeof(DateTime?),              true)]
    [InlineData(                typeof(object),                 false)]
    public void IsConvertible_Type_ReturnsExpected(Type type, bool result) =>
      Assert.True(AttributeValueConverter.IsConvertible(type) == result);

    /*==========================================================================================================================
    | TEST: CONVERT: FROM STRING VALUE: SUCCEEDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Passes a valid <paramref name="input"/> into <see cref="AttributeValueConverter.Convert(String?, Type)"/> based on a
    ///   <paramref name="type"/> and confirms that the result matches the <paramref name="expected"/>.
    /// </summary>
    [Theory]
    [InlineData(                typeof(string),                 "String",                       "String")]
    [InlineData(                typeof(int),                    "1",                            1)]
    [InlineData(                typeof(bool),                   "1",                            true)]
    [InlineData(                typeof(bool),                   "0",                            false)]
    [InlineData(                typeof(bool),                   "true",                         true)]
    [InlineData(                typeof(bool),                   "false",                        false)]
    [InlineData(                typeof(string),                 "",                             "")]
    [InlineData(                typeof(string),                 null,                           null)]
    [InlineData(                typeof(int?),                   "",                             null)]
    [InlineData(                typeof(bool?),                  "",                             null)]
    [InlineData(                typeof(DateTime?),              "",                             null)]
    [InlineData(                typeof(object),                 "Value",                        null)]
    public void Convert_FromStringValue_Succeeds(Type type, string? input, object? expected) =>
      Assert.Equal(expected, AttributeValueConverter.Convert(input, type));

    /*==========================================================================================================================
    | TEST: CONVERT: TO DATE/TIME: SUCCEEDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Passes a valid valid <paramref name="input"/> string into <see cref="AttributeValueConverter.Convert(String?, Type)"/>
    ///   and confirms that the result matches the expected <see cref="DateTime"/> value.
    /// </summary>
    [Theory]
    [InlineData(                "1976-10-15 01:02:03")]
    [InlineData(                "October 15, 1976 01:02:03 AM")]
    [InlineData(                "15 Oct 1976 01:02:03")]
    [InlineData(                "10/15/1976 01:02:03 AM")]
    public void Convert_ToDateTime_Succeeds(string? input) =>
      Assert.Equal(new DateTime(1976, 10, 15, 1, 2, 3), AttributeValueConverter.Convert(input, typeof(DateTime)));

    /*==========================================================================================================================
    | TEST: CONVERT: TO URI: SUCCEEDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Passes a valid input string into <see cref="AttributeValueConverter.Convert(String?, Type)"/> and confirms that the
    ///   result matches the expected <see cref="Uri"/> value.
    /// </summary>
    [Fact]
    public void Convert_ToUri_Succeeds() {
      Assert.Equal("/OnTopicCMS/", ((Uri?)AttributeValueConverter.Convert("https://www.github.com/OnTopicCMS/", typeof(Uri)))?.LocalPath);
      Assert.False(((Uri?)AttributeValueConverter.Convert("/OnTopicCMS/", typeof(Uri)))?.IsAbsoluteUri);
    }

    /*==========================================================================================================================
    | TEST: CONVERT: FROM STRING VALUE: FAILS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Passes an invalid value into <see cref="AttributeValueConverter.Convert(String?, Type)"/> based on a <paramref name="
    ///   type"/> and confirms that the result is <c>null</c>.
    /// </summary>
    [Theory]
    [InlineData(                typeof(int))]
    [InlineData(                typeof(bool))]
    [InlineData(                typeof(DateTime))]
    [InlineData(                typeof(int?))]
    [InlineData(                typeof(bool?))]
    [InlineData(                typeof(DateTime?))]
    [InlineData(                typeof(object))]
    public void Convert_FromString_Fails(Type type) =>
      Assert.Null(AttributeValueConverter.Convert("ABC", type));


    /*==========================================================================================================================
    | TEST: CONVERT: FROM VALUE TYPE: SUCCEEDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Passes a valid <paramref name="input"/> into <see cref="AttributeValueConverter.Convert(Object?)"/> and confirms that
    ///   the result matches the <paramref name="expected"/> string.
    /// </summary>
    [Theory]
    [InlineData(                "String",                       "String")]
    [InlineData(                true,                           "1")]
    [InlineData(                false,                          "0")]
    [InlineData(                1,                              "1")]
    [InlineData(                null,                           null)]
    [InlineData(                typeof(string),                 "System.String")]
    public void Convert_FromValueType_Succeeds(object input, string? expected) =>
      Assert.Equal(expected, AttributeValueConverter.Convert(input));


    /*==========================================================================================================================
    | TEST: CONVERT: FROM DATE/TIME: SUCCEEDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Passes a valid valid <see cref="DateTime"/> to <see cref="AttributeValueConverter.Convert(Object?)"/> and confirms the
    ///   result matches the expected <see cref="String"/> value.
    /// </summary>
    [Fact]
    public void Convert_FromDateTime_Succeeds() =>
      Assert.Equal("10/15/1976 01:02:03", AttributeValueConverter.Convert(new DateTime(1976, 10, 15, 1, 2, 3)));

    /*==========================================================================================================================
    | TEST: CONVERT: FROM URI: SUCCEEDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="Uri"/> based on the <paramref name="input"/> and confirms <see cref="AttributeValueConverter
    ///   .Convert(object?)"/> returns the expected <see cref="String"/> value.
    /// </summary>
    [Theory]
    [InlineData(                "https://www.github.com/OnTopicCMS")]
    [InlineData(                "https://github.com/OnTopicCMS/OnTopic-Library/issues?q=is%3Aopen+is%3Aissue+no%3Amilestone")]
    [InlineData(                "/OnTopicCMS/")]
    public void Convert_FromUri_Succeeds(string input) =>
      Assert.Equal(input, AttributeValueConverter.Convert(new Uri(input, UriKind.RelativeOrAbsolute)));

  } //Class
} //Namespace
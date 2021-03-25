/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using OnTopic.Attributes;
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
    | TEST: CONVERT: VALUE: SUCCEEDS
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
    public void Convert_Value_Succeeds(Type type, string? input, object? expected) =>
      Assert.Equal(expected, AttributeValueConverter.Convert(input, type));

  } //Class
} //Namespace
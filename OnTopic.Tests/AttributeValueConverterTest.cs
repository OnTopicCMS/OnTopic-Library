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

  } //Class
} //Namespace
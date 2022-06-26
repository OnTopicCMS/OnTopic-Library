/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using System.Globalization;
using OnTopic.Mapping;

namespace OnTopic.Attributes {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE CONVERTER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Attribute values are stored as strings, but may be deserialized to other value types using e.g. the <see cref="
  ///   TopicMappingService"/> or the <see cref="AttributeCollectionExtensions"/>. This class provides basic methods for
  ///   converting from the string representation to supported value types.
  /// </summary>
  internal static class AttributeValueConverter {

    /*==========================================================================================================================
    | PROPERTY: CONVERTIBLE TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A list of types that are allowed to be converted using <see cref="Convert(String, Type)"/>.
    /// </summary>
     private static Collection<Type> ConvertibleTypes { get; } =
      new() {
        typeof(bool),
        typeof(bool?),
        typeof(int),
        typeof(int?),
        typeof(double),
        typeof(double?),
        typeof(string),
        typeof(DateTime),
        typeof(DateTime?),
        typeof(Uri)
      };

    /*==========================================================================================================================
    | METHOD: IS CONVERTIBLE?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether the <see cref="AttributeValueConverter"/> supports converting a <see cref="String"/> to a <paramref
    ///   name="type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to be converted to.</param>
    /// <returns></returns>

    internal static bool IsConvertible(Type type) => ConvertibleTypes.Contains(type);

    /*==========================================================================================================================
    | METHOD: CONVERT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Converts the supplied <paramref name="value"/> to an object of type <paramref name="type"/>.
    /// </summary>
    /// <param name="value">The <see cref="String"/> value to convert to the <paramref name="type"/>.</param>
    /// <param name="type">The <see cref="Type"/> to be converted to.</param>
    /// <returns>An instance of the <paramref name="value"/> as a <paramref name="type"/>.</returns>
    internal static object? Convert(string? value, Type type) {

      var valueObject = (object?)null;

      //Treat empty as null for non-strings, regardless of whether they’re nullable
      if (!type.Equals(typeof(string)) && String.IsNullOrWhiteSpace(value)) {
        return null;
      }

      if (value is null) return null;

      if (type.Equals(typeof(string))) {
        valueObject = value;
      }
      else if (type.Equals(typeof(bool)) || type.Equals(typeof(bool?))) {
        if (value is "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase)) {
          valueObject = true;
        }
        else if (value is "0" || value.Equals("false", StringComparison.OrdinalIgnoreCase)) {
          valueObject = false;
        }
      }
      else if (type.Equals(typeof(int)) || type.Equals(typeof(int?))) {
        if (Int32.TryParse(value, out var intValue)) {
          valueObject = intValue;
        }
      }
      else if (type.Equals(typeof(double)) || type.Equals(typeof(double?))) {
        if (Double.TryParse(value, out var doubleValue)) {
          valueObject = doubleValue;
        }
      }
      else if (type.Equals(typeof(DateTime)) || type.Equals(typeof(DateTime?))) {
        if (DateTime.TryParse(value, out var date)) {
          valueObject = date;
        }
      }
      else if (type.Equals(typeof(Uri))) {
        if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri)) {
          valueObject = uri;
        }
      }

      return valueObject;

    }

    /// <inheritdoc/>
    internal static T? Convert<T>(string? value) => (T?)Convert(value, typeof(T))?? default;

    /// <summary>
    ///   Converts the supplied <paramref name="value"/> to an object of type <see cref="String"/>.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="String"/>.</param>
    /// <returns>An instance of the <paramref name="value"/> as a <see cref="String"/>.</returns>
    internal static string? Convert(object? value) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle null values
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (value is null) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle type-specific rules
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (value is string stringValue) {
        return stringValue;
      }
      else if (value is bool boolValue) {
        return boolValue ? "1" : "0";
      }
      else if (value is int intValue) {
        return intValue.ToString(CultureInfo.InvariantCulture);
      }
      else if (value is double doubleValue) {
        return doubleValue.ToString(CultureInfo.InvariantCulture);
      }
      else if (value is DateTime dateTimeValue) {
        return dateTimeValue.ToString(CultureInfo.InvariantCulture);
      }
      else if (value is Uri uriValue) {
        return uriValue.ToString();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Fallback to default
      \-----------------------------------------------------------------------------------------------------------------------*/
      return value.ToString();

    }

  }
}
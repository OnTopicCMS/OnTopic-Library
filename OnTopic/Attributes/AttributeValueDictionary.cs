/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping;

namespace OnTopic.Attributes {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE DICTIONARY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a light-weight dictionary of attribute values.
  /// </summary>
  /// <remarks>
  ///   The <see cref="AttributeValueDictionary"/> is used by the <see cref="TopicMappingService"/> to support self-constructed
  ///   models that don't require the use of the <see cref="TopicMappingService"/> to set the values of scalar properties. Any
  ///   model that has a primary constructor accepting a single <see cref="AttributeValueDictionary"/> will be initialized using
  ///   a <see cref="AttributeValueDictionary"/> containing all attributes from not only the current <see cref="Topic"/>, but
  ///   also any <see cref="Topic.BaseTopic"/>s it references.
  /// </remarks>
  public class AttributeValueDictionary: Dictionary<string, string?> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public AttributeValueDictionary(): base(StringComparer.OrdinalIgnoreCase) { }

    /*==========================================================================================================================
    | METHOD: GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets an attribute value as a string from the <see cref="AttributeValueDictionary"/> based on the <paramref name="
    ///   attributeKey"/>.
    /// </summary>
    /// <param name="attributeKey">The key of the attribute to retrieve.</param>
    /// <returns></returns>
    public string? GetValue(string attributeKey) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(attributeKey), nameof(attributeKey));
      TryGetValue(attributeKey, out var value);
      return String.IsNullOrWhiteSpace(value)? null : value;
    }

    /*==========================================================================================================================
    | METHOD: GET BOOLEAN VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets an attribute value as a Boolean from the <see cref="AttributeValueDictionary"/> based on the <paramref name="
    ///   attributeKey"/>.
    /// </summary>
    /// <param name="attributeKey">The string identifier for the <see cref="AttributeRecord"/>.</param>
    /// <returns>The value for the attribute as a boolean.</returns>
    public bool? GetBoolean(string attributeKey) => AttributeValueConverter.Convert<bool?>(GetValue(attributeKey));

    /*==========================================================================================================================
    | METHOD: GET INTEGER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets an attribute value as an integer from the <see cref="AttributeValueDictionary"/> based on the <paramref name="
    ///   attributeKey"/>.
    /// </summary>
    /// <param name="attributeKey">The string identifier for the <see cref="AttributeRecord"/>.</param>
    /// <returns>The value for the attribute as an integer.</returns>
    public int? GetInteger(string attributeKey) => AttributeValueConverter.Convert<int?>(GetValue(attributeKey));

    /*==========================================================================================================================
    | METHOD: GET DOUBLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets an attribute value as a double from the <see cref="AttributeValueDictionary"/> based on the <paramref name="
    ///   attributeKey"/>.
    /// </summary>
    /// <param name="attributeKey">The string identifier for the <see cref="AttributeRecord"/>.</param>
    /// <returns>The value for the attribute as a double.</returns>
    public double? GetDouble(string attributeKey) => AttributeValueConverter.Convert<double?>(GetValue(attributeKey));

    /*==========================================================================================================================
    | METHOD: GET DATETIME
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets an attribute value as a date/time from the <see cref="AttributeValueDictionary"/> based on the <paramref name="
    ///   attributeKey"/>.
    /// </summary>
    /// <param name="attributeKey">The string identifier for the <see cref="AttributeRecord"/>.</param>
    /// <returns>The value for the attribute as a DateTime object.</returns>
    public DateTime? GetDateTime(string attributeKey) => AttributeValueConverter.Convert<DateTime?>(GetValue(attributeKey));

    /*==========================================================================================================================
    | METHOD: GET URI
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets an attribute value as a URI from the <see cref="AttributeValueDictionary"/> based on the <paramref name="
    ///   attributeKey"/>.
    /// </summary>
    /// <param name="attributeKey">The string identifier for the <see cref="AttributeRecord"/>.</param>
    /// <returns>The value for the attribute as a Uri object.</returns>
    public Uri? GetUri(string attributeKey) => AttributeValueConverter.Convert<Uri?>(GetValue(attributeKey));

  } //Class
} //Namespace
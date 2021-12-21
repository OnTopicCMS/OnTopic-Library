/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping;

namespace OnTopic.Attributes {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE DICTIONARY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a light-weight dictionary of attribute values.
  /// </summary>
  /// <remarks>
  ///   The <see cref="AttributeDictionary"/> is used by the <see cref="TopicMappingService"/> to support self-constructed
  ///   models that don't require the use of the <see cref="TopicMappingService"/> to set the values of scalar properties. Any
  ///   model that has a primary constructor accepting a single <see cref="AttributeDictionary"/> will be initialized using a
  ///   <see cref="AttributeDictionary"/> containing all attributes from not only the current <see cref="Topic"/>, but also any
  ///   <see cref="Topic.BaseTopic"/>s it references.
  /// </remarks>
  public class AttributeDictionary: Dictionary<string, string?> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public AttributeDictionary(): base(StringComparer.OrdinalIgnoreCase) { }

    /*==========================================================================================================================
    | METHOD: GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets an attribute value as a string from the <see cref="AttributeDictionary"/> based on the <paramref name="
    ///   attributeKey"/>.
    /// </summary>
    /// <param name="attributeKey">The key of the attribute to retrieve.</param>
    /// <returns></returns>
    public string? GetValue(string attributeKey) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(attributeKey), nameof(attributeKey));
      TryGetValue(attributeKey, out var value);
      return String.IsNullOrWhiteSpace(value)? null : value;
    }


  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ignia.Topics.Serialization {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE COLLECTION (JSON CONVERTER)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   A converter that JSON.NET can use to determine how to efficiently serialize <see cref="AttributeValueCollection"/>
  ///   instances.
  /// </summary>
  /// <remarks>
  ///   Out of the box, the <see cref="AttributeValueCollection"/> contains <see cref="AttributeValue"/> instances with
  ///   properties such as <see cref="AttributeValue.LastModified"/> and <see cref="AttributeValue.IsDirty"/>, which aren't
  ///   needed for serialization. Instead, the <see cref="AttributeValueCollectionJsonConverter"/> provides a more efficient
  ///   format that exclusively includes the key/value pairs.
  /// </remarks>
  public class AttributeValueCollectionJsonConverter : JsonConverter {

    /*==========================================================================================================================
    | PROPERTY: CAN CONVERT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a type, allows the <see cref="AttributeValueCollectionJsonConverter"/> to determine whether it's capable of
    ///   converting that type.
    /// </summary>
    /// <param name="objectType">An instance of the object being converted.</param>
    /// <returns></returns>
    public override bool CanConvert(Type objectType) => typeof(AttributeValueCollection).IsAssignableFrom(objectType);

    /*==========================================================================================================================
    | METHOD: WRITE JSON
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Reads the supplied <paramref name="value"/>, and converts it to a JSON object.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
      writer.WriteStartObject();
      foreach (var attribute in (AttributeValueCollection)value) {
        writer.WritePropertyName(attribute.Key);
        serializer.Serialize(writer, attribute.Value);
      }
      writer.WriteEndObject();
    }
  } //Class
} //Namespace

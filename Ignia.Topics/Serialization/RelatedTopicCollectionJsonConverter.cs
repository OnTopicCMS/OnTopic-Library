﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Collections;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ignia.Topics.Serialization {

  /*============================================================================================================================
  | CLASS: RELATED TOPIC COLLECTION (JSON CONVERTER)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   A converter that JSON.NET can use to determine how to efficiently serialize <see cref="RelatedTopicCollection"/>
  ///   instances.
  /// </summary>
  /// <remarks>
  ///   Out of the box, the <see cref="RelatedTopicCollection"/> contains <see cref="NamedTopicCollection"/> instances, which in
  ///   turn reference instances of <see cref="Topic"/>. This introduces a lot of overhead, complexity, and unnecessary data
  ///   during the serialization process. Instead, the <see cref="RelatedTopicCollectionJsonConverter"/> provides a more
  ///   efficient format that compresses the outer collection into an object, and the inner collection into basic objects
  ///   containing simply the target <see cref="Topic"/>'s <see cref="Topic.GetUniqueKey()"/> and <see cref="Topic.Title"/>.
  /// </remarks>
  public class RelatedTopicCollectionJsonConverter : JsonConverter {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository                = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="RelatedTopicCollectionJsonConverter"/>.
    /// </summary>
    public RelatedTopicCollectionJsonConverter() {}

    /// <summary>
    ///   Establishes a new instance of a <see cref="RelatedTopicCollectionJsonConverter"/> with optional dependencies.
    /// </summary>
    public RelatedTopicCollectionJsonConverter(ITopicRepository topicRepository) {
      _topicRepository = topicRepository;
    }

    /*==========================================================================================================================
    | PROPERTY: CAN CONVERT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a type, allows the <see cref="RelatedTopicCollectionJsonConverter"/> to determine whether it's capable of
    ///   converting that type.
    /// </summary>
    /// <param name="objectType">An instance of the object being converted.</param>
    /// <returns></returns>
    public override bool CanConvert(Type objectType) => typeof(RelatedTopicCollection).IsAssignableFrom(objectType);

    /*==========================================================================================================================
    | METHOD: WRITE JSON
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Reads the supplied <paramref name="value"/>, and converts it to a JSON object.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
      writer.WriteStartObject();
      foreach (var scope in (RelatedTopicCollection)value) {
        writer.WritePropertyName(scope.Name);
        writer.WriteStartArray();
        foreach (var topic in scope) {
          writer.WriteStartObject();
          writer.WritePropertyName("uniqueKey");
          writer.WriteValue(topic.GetUniqueKey());
          writer.WritePropertyName("title");
          writer.WriteValue(topic.Title);
          writer.WriteEndObject();
        }
        writer.WriteEndArray();
      }
      writer.WriteEndObject();
    }

    /*==========================================================================================================================
    | PROPERTY: CAN READ
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Informs the serialization library whether or not this the <see cref="AttributeValueCollectionJsonConverter"/> is
    ///   capable of reading JSON data.
    /// </summary>
    public override bool CanRead => _topicRepository != null;

    /*==========================================================================================================================
    | METHOD: READ JSON
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Reads the JSON input, and populates the supplied <paramref name="existingValue"/>.
    /// </summary>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate suitability
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!CanConvert(objectType)) {
        throw new NotImplementedException(
          $"The {nameof(RelatedTopicCollectionJsonConverter)} cannot read objects of type {objectType.Name}"
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate request type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (reader.TokenType == JsonToken.Null) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure object is created
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (existingValue == null) {
        existingValue = new RelatedTopicCollection(null);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Populate existing value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var scopes                = (RelatedTopicCollection)existingValue;
      var jObject               = JObject.Load(reader);

      foreach (var scope in jObject.Properties()) {
        foreach (var topic in scope.Value) {
          scopes.SetTopic(scope.Name, _topicRepository.Load(topic.Value<string>("uniqueKey")));
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return results
      \-----------------------------------------------------------------------------------------------------------------------*/
      return scopes;

    }

  } //Class
} //Namespace

/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Globalization;
using OnTopic.Attributes;
using OnTopic.Internal.Diagnostics;
using OnTopic.Associations;
using System.Diagnostics.CodeAnalysis;

namespace OnTopic.Tests.Entities {

  /*============================================================================================================================
  | TOPIC ENTITY: CUSTOM
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a derived version of <see cref="Topic"/> with additional properties for evaluating the enforcement of business
  ///   logic.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class CustomTopic: Topic {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public CustomTopic(string key, string contentType, Topic? parent = null, int id = -1): base(key, contentType, parent, id) {
    }

    /*==========================================================================================================================
    | TEXT ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a text property which is intended to be mapped to a text attribute.
    /// </summary>
    [AttributeSetter]
    public string? TextAttribute {
      get => Attributes.GetValue("TextAttribute");
      set => SetAttributeValue("TextAttribute", value);
    }

    /*==========================================================================================================================
    | BOOLEAN ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a Boolean property which is intended to be mapped to a Boolean attribute.
    /// </summary>
    [AttributeSetter]
    public bool BooleanAttribute {
      get => Attributes.GetBoolean("BooleanAttribute");
      set => SetAttributeValue("BooleanAttribute", value? "1" : "0");
    }

    /*==========================================================================================================================
    | BOOLEAN AS STRING ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a string property which is intended to be mapped to a Boolean attribute.
    /// </summary>
    [AttributeSetter]
    public string BooleanAsStringAttribute {
      get => Attributes.GetValue("BooleanAttribute", "0");
      set => SetAttributeValue("BooleanAttribute", value);
    }

    /*==========================================================================================================================
    | NUMERIC ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a numeric property which is intended to be mapped to a numeric attribute.
    /// </summary>
    [AttributeSetter]
    public int NumericAttribute {
      get => Attributes.GetInteger("NumericAttribute");
      set {
        Contract.Requires<ArgumentOutOfRangeException>(
          value >= 0,
          $"{nameof(NumericAttribute)} expects a positive value."
        );
        SetAttributeValue("NumericAttribute", value.ToString(CultureInfo.InvariantCulture));
      }
    }

    /*==========================================================================================================================
    | DATE/TIME ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a date/time property which is intended to be mapped to a date/time attribute.
    /// </summary>
    [AttributeSetter]
    public DateTime DateTimeAttribute {
      get => Attributes.GetDateTime("DateTimeAttribute");
      set {
        Contract.Requires<ArgumentOutOfRangeException>(
          value.Year > 2000,
          $"{nameof(DateTimeAttribute)} expects a date after 2000."
        );
        SetAttributeValue("DateTimeAttribute", value.ToString(CultureInfo.InvariantCulture));
      }
    }

    /*==========================================================================================================================
    | TOPIC REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a topic reference property which is intended to be mapped to a topic reference.
    /// </summary>
    [ReferenceSetter]
    public Topic? TopicReference {
      get => References.GetValue("TopicReference");
      set {
        Contract.Requires<ArgumentOutOfRangeException>(
          value?.ContentType == ContentType,
          $"{nameof(TopicReference)} expects a topic with the same content type as the parent: {ContentType}."
        );
        References.SetValue("TopicReference", value);
      }
    }

  } //Class
} //Namespace
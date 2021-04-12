/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Metadata {

  /*============================================================================================================================
  | ENUM: MODEL TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides options describing how a specific attribute is exposed in terms of the Topic Library's object model.
  /// </summary>
  public enum ModelType {

    /*--------------------------------------------------------------------------------------------------------------------------
    | NONE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   No value is configured.
    /// </summary>
    None                        = 0,

    /*--------------------------------------------------------------------------------------------------------------------------
    | SCALAR VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A standard attribute value, exposed primarily through the <see cref="Topic.Attributes"/> collection, or a
    ///   corresponding property.
    /// </summary>
    ScalarValue                 = 1,

    /*--------------------------------------------------------------------------------------------------------------------------
    | RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A relationship to a collection of separate <see cref="Topic"/>s. Relationships are exposed as strongly-typed
    ///   collections through the <see cref="Topic.Relationships"/> property.
    /// </summary>
    Relationship                = 2,

    /*--------------------------------------------------------------------------------------------------------------------------
    | TOPIC REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A reference to a separate <see cref="Topic"/>. References are ideally exposed and populated as strongly-typed
    ///   properties on <see cref="Topic"/>—or a derived class—as done with e.g. <see cref="Topic.BaseTopic"/>.
    /// </summary>
    Reference                   = 3,

    /*--------------------------------------------------------------------------------------------------------------------------
    | NESTED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A set of child <see cref="Topic"/>s packaged as part of the current <see cref="Topic"/>.
    /// </summary>
    NestedTopic                 = 4,

    /*--------------------------------------------------------------------------------------------------------------------------
    | REFLEXIVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A special value for <see cref="AttributeDescriptor"/>s placed on <see cref="AttributeDescriptor"/>s, which specifies
    ///   that the <see cref="ModelType"/> should be inherited from the <see cref="AttributeDescriptor.ModelType"/>.
    /// </summary>
    /// <remarks>
    ///   This is useful for implementing an <see cref="AttributeDescriptor"/> which reflects the <see cref="AttributeDescriptor
    ///   "/> it is placed on. For example, the <see cref="AttributeDescriptor.DefaultValue"/> attribute should store the value
    ///   in the same location as the <see cref="AttributeDescriptor"/> for which it's setting.
    /// </remarks>
    Reflexive                   = 5

  } //Enum
} //Namespace
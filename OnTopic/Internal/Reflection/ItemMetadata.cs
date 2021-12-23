/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections;
using System.Reflection;
using OnTopic.Attributes;

namespace OnTopic.Internal.Reflection {

  /*============================================================================================================================
  | CLASS: ITEM METADATA
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides metadata associated with a given parameter, method, or property.
  /// </summary>
  internal abstract class ItemMetadata {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly static     List<Type>                      _listTypes                      = new();
    private readonly            ICustomAttributeProvider        _attributeProvider;
    private readonly            Type                            _type                           = default!;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ItemMetadata"/> class associated with a <see cref="MemberInfo"/> or <see
    ///   cref="ParameterInfo"/> instance.
    /// </summary>
    /// <param name="name">The <see cref="Name"/> of the <see cref="MemberInfo"/> or <see cref="ParameterInfo"/>.</param>
    /// <param name="attributeProvider">
    ///   The <see cref="MemberInfo"/> or <see cref="ParameterInfo"/> associated with the <see cref="ItemMetadata"/>.
    /// </param>
    internal ItemMetadata(string name, ICustomAttributeProvider attributeProvider) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Set Fields
      \-----------------------------------------------------------------------------------------------------------------------*/
      _attributeProvider        = attributeProvider;

      /*------------------------------------------------------------------------------------------------------------------------
      | Set Properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      Name                      = name;

    }

    /*==========================================================================================================================
    | NAME
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="MemberInfo.Name"/>
    internal string Name { get; }

    /*==========================================================================================================================
    | TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the <see cref="Type"/> associated with this member. For properties and get methods, this is the return type. For
    ///   set methods, this is the type of the parameter.
    /// </summary>
    /// <remarks>
    ///   Ideally, the <see cref="Type"/> would be provided as part of the <see cref="ItemMetadata"/> constructor.
    ///   Unfortunately, however, the logic for setting this type varies based on whether it is a parameter, a methor, or a
    ///   property. As such, it makes more sense for this logic to be implemented in derived classes. To facilitate this, the
    ///   <see cref="Type"/> property is provided with an initter, which will automatically set <see cref="IsNullable"/>, <see
    ///   cref="IsList"/>, and <see cref="IsConvertible"/> when it is set. If this is not done properly, dependency classes will
    ///   not work properly, and will likely fail. Since there are only two expected derived classes—<see cref="MemberAccessor"
    ///   /> and <see cref="ParameterMetadata"/>—this shouldn't be a problem. To help avoid this scenario, a <see cref="
    ///   ArgumentNullException"/> is thrown with instructions in the unexpected case that <see cref="Type"/> is not set.
    /// </remarks>
    public Type Type {
      get {
        return _type?? throw new ArgumentNullException(
          nameof(Type),
          $"This {nameof(Type)} property must be initialized by classes derived by {nameof(ItemMetadata)}"
        );
      }
      init {
        _type                   = value;
        IsNullable              = !Type.IsValueType || Nullable.GetUnderlyingType(Type) != null;
        IsList                  = isList();
        IsConvertible           = AttributeValueConverter.IsConvertible(Type);
        bool isList()
          => typeof(IList).IsAssignableFrom(Type) || Type.IsGenericType && _listTypes.Contains(Type.GetGenericTypeDefinition());
      }
    }

    /*==========================================================================================================================
    | IS NULLABLE?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determine if the member accepts null values.
    /// </summary>
    /// <remarks>
    ///   If the <see cref="Type"/> is a reference type, then it will always accept null values; this doesn't detect C# 9.0
    ///   nullable reference types.
    /// </remarks>
    internal bool IsNullable { get; init; }

    /*==========================================================================================================================
    | CUSTOM ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a cached list of custom attributes associated with member.
    /// </summary>
    internal List<Attribute> CustomAttributes {
      get {
        _customAttributes ??= _attributeProvider.GetCustomAttributes(true).OfType<Attribute>().ToList();
        return _customAttributes;
      }
    }

  } //Class
} //Namespace
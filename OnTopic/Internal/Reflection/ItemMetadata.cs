/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections;
using System.Reflection;
using OnTopic.Attributes;
using OnTopic.Mapping;
using OnTopic.Mapping.Internal;
using OnTopic.Mapping.Reverse;

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
    private                     List<Attribute>                 _customAttributes               = default!;
    private                     ItemConfiguration?              _itemConfiguration;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="ItemMetadata"/> with required dependencies.
    /// </summary>
    static ItemMetadata() {
      _listTypes.Add(typeof(IEnumerable<>));
      _listTypes.Add(typeof(ICollection<>));
      _listTypes.Add(typeof(IList<>));
    }

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
    | CONFIGURATION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a reference to the configuration settings for the current item, based on the annotations configured in the code.
    /// </summary>
    /// <remarks>
    ///   The <see cref="ItemConfiguration"/> class identifies annotations used by the <see cref="TopicMappingService"/> and the
    ///   <see cref="ReverseTopicMappingService"/>. While it can be called on any <see cref="Type"/>, it is only expected to
    ///   offer a benefit for model classes intended for mapping.
    /// </remarks>
    internal ItemConfiguration Configuration {
      get {
        if (_itemConfiguration is null) {
          _itemConfiguration = new(this);
        }
        return _itemConfiguration;
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
    | IS LIST?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determine if the member is a <see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IEnumerable{T}"/>, or <see cref="
    ///   ICollection{T}"/>.
    /// </summary>
    internal bool IsList { get; init; }

    /*==========================================================================================================================
    | IS CONVERTIBLE?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determine if the member is of a type that can be converted using the <see cref="AttributeValueConverter"/> class.
    /// </summary>
    internal bool IsConvertible { get; init; }

    /*==========================================================================================================================
    | MAYBE COMPATIBLE?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determine if the item corresponds to a member on the <see cref="Topic"/> class (or derivative) associated with the
    ///   parent <see cref="TypeAccessor"/>.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     The <see cref="MaybeCompatible"/> does not <i>guarantee</i> that a corresponding compatible property will be found.
    ///     First, it cannot know what the source <see cref="Topic"/> will be, so it relies only evaluates topics who match the
    ///     naming convention (e.g., <c>{ContentType}TopicViewModel</c> maps to <c>{ContentType}</c>), and otherwise falls back
    ///     to <see cref="Topic"/>; if a model is mapped to a different derivative of <see cref="Topic"/>, additional matches
    ///     may be available. Second, it evaluates whether the <see cref="Topic"/> member is of a type that can be assigned to
    ///     the model member, or otherwise <i>converted</i>—i.e., that the <see cref="Topic"/> member is a string, and the model
    ///     member is one of the <see cref="AttributeValueConverter.ConvertibleTypes"/>. This provides a hint for avoiding type
    ///     checks in cases we don't expect to be compatible, while deferring to the caller to provide more sophisticated checks
    ///     based on the actual <see cref="Topic"/>.
    ///   </para>
    ///   <para>
    ///     The <see cref="MaybeCompatible"/> property must be set from the <see cref="TypeAccessor"/>, which is best positioned
    ///     to efficiently determine the corresponding attributes. To do this, it must rely on attributes accessed via the <see
    ///     cref="Configuration"/> property. As a result, the <see cref="MaybeCompatible"/> must be set <i>after</i> the <see
    ///     cref="ItemMetadata"/> instance has been constructed, instead of during initialization like the other properties.
    ///     That isn't ideal, but isn't easy to avoid without making each <see cref="ItemMetadata"/> aware of the parent <see
    ///     cref="TypeAccessor"/>.
    ///   </para>
    /// </remarks>
    internal bool MaybeCompatible { get; set; }

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
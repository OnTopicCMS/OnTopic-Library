/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

using System;

namespace Ignia.Topics.Mapping.Annotations {

  /*============================================================================================================================
  | ATTRIBUTE: MAP TO PARENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Denotes that the properties on a view model should be mapped to the parent <see cref="Topic"/>, optionally prefixed by
  ///   the provided key.
  /// </summary>
  /// <remarks>
  ///   By default, complex property objects that don't map to known attributes (e.g., a compatible property) or relationships
  ///   are bypassed. The <see cref="MapToParentAttribute"/> informs the <see cref="IReverseTopicMappingService"/> to treat the
  ///   properties of such complex objects as members of the parent <see cref="Topic"/>. By default, these will be prefixed by
  ///   the name of the property that the complex object is assigned to. Optionally, however, this may be overwritten—including
  ///   by an empty string, if no prefix is desired.
  /// </remarks>
  /// <example>
  ///   As an example, imagine that a view model has a property <c>BillingContact</c> which is of type <c>Contact</c>. By
  ///   default, this will be ignored, since there's no obvious way to map it back to a <see cref="Topic"/>. If it is annotated
  ///   with the <see cref="MapToParentAttribute"/>, however, then its properties will be mapped to the parent topic. So, for
  ///   example, if the <c>Contact</c> class has a property named <c>FirstName</c>, then that will be saved to an attribute
  ///   named <c>BillingContactFirstName</c>. Alternatively, if a prefix of <see cref="String.Empty"/> were provided, then that
  ///   same property would instead be mapped to <c>FirstName</c>—which could potentially introduce conflicts if there is also a
  ///   <c>FirstName</c> property on the parent object.
  /// </example>
  [System.AttributeUsage(System.AttributeTargets.Property, Inherited=true)]
  public sealed class MapToParentAttribute : System.Attribute {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     string?                         _attributePrefix                 = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="MapToParentAttribute"/> class.
    /// </summary>
    public MapToParentAttribute() { }

    /*==========================================================================================================================
    | PROPERTY: ATTRIBUTE PREFIX
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The string that will be prepended to each property name when mapping to topic attributes. Defaults to the name of the
    ///   property being annotated.
    /// </summary>
    public string? AttributePrefix {
      get => _attributePrefix;
      set {
        TopicFactory.ValidateKey(value, true);
        _attributePrefix = value;
      }
    }

  } //Class
} //Namespace
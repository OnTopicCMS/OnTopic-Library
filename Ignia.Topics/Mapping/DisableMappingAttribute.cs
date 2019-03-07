/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | ATTRIBUTE: DISABLE MAPPING ATTRIBUTE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Instructs the <see cref="IReverseTopicMappingService"/> to ignore the decorated property.
  /// </summary>
  /// <remarks>
  ///   The <see cref="DisableMappingAttribute"/> is useful when a property is not supported by the <see
  ///   cref="IReverseTopicMappingService"/> (as in the case of e.g. <see cref="Topic.Parent"/> or <see cref="Topic.Children"/>)
  ///   or is designated for special handling by the caller, and not intended to utilize the default mapping rules.
  /// </remarks>
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public sealed class DisableMappingAttribute: System.Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="DisableMappingAttribute"/>.
    /// </summary>
    public DisableMappingAttribute() {
    }

  } //Class

} //Namespace

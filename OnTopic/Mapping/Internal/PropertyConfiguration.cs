/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using OnTopic.Internal.Reflection;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Mapping.Internal {

  /*============================================================================================================================
  | CLASS: PROPERTY ATTRIBUTES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Evaluates a <see cref="PropertyInfo"/> instance for known <see cref="Attribute"/>, and exposes them through a set of
  ///   property values.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The <see cref="PropertyConfiguration"/> class is utilized by implementations of <see cref="ITopicMappingService"/> to
  ///     facilitate the mapping of source <see cref="Topic"/> instances to Data Transfer Objects (DTOs), such as View Models.
  ///     The attribute values provide hints to the mapping service that help manage how the mapping occurs.
  ///   </para>
  ///   <para>
  ///     For example, by default a property on a DTO class will be mapped to a property or attribute of the same name on the
  ///     source <see cref="Topic"/>. If the <see cref="AttributeKeyAttribute"/> is attached to a property on the DTO, however,
  ///     then the <see cref="ITopicMappingService"/> will instead use the value defined by that attribute, thus allowing a
  ///     property on the DTO to be aliased to a different property or attribute name on the source <see cref="Topic"/>.
  ///   </para>
  /// </remarks>
  internal class PropertyConfiguration: ItemConfiguration {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="MemberAccessor"/> instance, exposes a set of properties associated with known <see cref="Attribute"
    ///   /> instances.
    /// </summary>
    /// <param name="memberAccessor">
    ///   The <see cref="MemberAccessor"/> instance to check for <see cref="Attribute"/> values.
    /// </param>
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    internal PropertyConfiguration(MemberAccessor memberAccessor, string? attributePrefix = ""):
      base(memberAccessor.CustomAttributes, memberAccessor.Name, attributePrefix)
    {
      Property                  = (PropertyInfo)memberAccessor.MemberInfo;
      MemberAccessor            = memberAccessor;
    }

    /*==========================================================================================================================
    | PROPERTY: PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The <see cref="PropertyInfo"/> that the current <see cref="PropertyConfiguration"/> is associated with.
    /// </summary>
    internal PropertyInfo Property { get; }

    /*==========================================================================================================================
    | PROPERTY: MEMBER ACCESSOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The <see cref="MemberAccessor"/> that the current <see cref="PropertyConfiguration"/> is associated with.
    /// </summary>
    internal MemberAccessor MemberAccessor { get; }

    /*==========================================================================================================================
    | METHOD: VALIDATE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a target DTO, will automatically identify any attributes that derive from <see cref="ValidationAttribute"/> and
    ///   ensure that their conditions are satisfied.
    /// </summary>
    /// <param name="target">The target DTO to validate the current property on.</param>
    internal void Validate(object target) {
      foreach (ValidationAttribute validator in CustomAttributes.OfType<ValidationAttribute>()) {
        validator.Validate(MemberAccessor.GetValue(target), MemberAccessor.Name);
      }
    }

  }
}
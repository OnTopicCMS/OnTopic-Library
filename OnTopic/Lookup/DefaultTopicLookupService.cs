/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Reflection;
using OnTopic.Metadata;

namespace OnTopic.Lookup {

  /*============================================================================================================================
  | CLASS: TYPE INDEX
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="DefaultTopicLookupService"/> can be configured to provide a lookup of .
  /// </summary>
  public class DefaultTopicLookupService: StaticTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="DefaultTopicLookupService"/>. Optionally accepts a list of <see
    ///   cref="Type"/> instances and a default <see cref="Type"/> value.
    /// </summary>
    /// <remarks>
    ///   Any <see cref="Type"/> instances submitted via <paramref name="types"/> should be unique by <see
    ///   cref="MemberInfo.Name"/>; if they are not, they will be removed.
    /// </remarks>
    /// <param name="types">The list of <see cref="Type"/> instances to expose as part of this service.</param>
    public DefaultTopicLookupService(IEnumerable<Type>? types = null) : base(types) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure editor types are accounted for
      \-----------------------------------------------------------------------------------------------------------------------*/
      TryAdd(typeof(ContentTypeDescriptor));
      TryAdd(typeof(AttributeDescriptor));

    }

    /*==========================================================================================================================
    | METHOD: LOOKUP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    /// <remarks>
    ///   The <see cref="DefaultTopicLookupService"/> version of <see cref="Lookup(String)"/> will automatically fall back to
    ///   <see cref="AttributeDescriptor"/> if the <paramref name="typeName"/> ends with <c>AttributeDescriptor</c>, but a
    ///   <see cref="AttributeDescriptor"/> with the specified name cannot be found. This accounts for the fact that strongly
    ///   typed <see cref="AttributeDescriptor"/> classes are expected to be in external plugins which are not statically
    ///   registered with the <see cref="DefaultTopicLookupService"/>. In that case, the base <see cref="AttributeDescriptor"/>
    ///   class will provide access to the attributes needed by most applications, including the core OnTopic library.
    /// </remarks>
    public override Type? Lookup(string typeName) {
      if (typeName is null) {
        return DefaultType;
      }
      else if (Contains(typeName)) {
        return base.Lookup(typeName);
      }
      else if (typeName.EndsWith("AttributeDescriptor", StringComparison.OrdinalIgnoreCase)) {
        return typeof(AttributeDescriptor);
      }
      return DefaultType;
    }

  } //Class
} //Namespace
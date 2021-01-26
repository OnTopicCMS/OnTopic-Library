/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Metadata;

namespace OnTopic.Lookup {

  /*============================================================================================================================
  | CLASS: DYNAMIC TOPIC LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="DynamicTopicLookupService"/> will search all assemblies for <see cref="Type"/>s that derive from <see
  ///   cref="Topic"/>.
  /// </summary>
  public class DynamicTopicLookupService : DynamicTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="DynamicTopicLookupService"/>.
    /// </summary>
    public DynamicTopicLookupService() : base(
      t => typeof(Topic).IsAssignableFrom(t),
      typeof(Topic)
    ) { }

    /*==========================================================================================================================
    | METHOD: LOOKUP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    /// <remarks>
    ///   The <see cref="DynamicTopicLookupService"/> version of <see cref="Lookup(String)"/> will automatically fall back to
    ///   <see cref="AttributeDescriptor"/> if the <paramref name="typeName"/> ends with <c>AttributeDescriptor</c>, but a
    ///   <see cref="AttributeDescriptor"/> with the specified name cannot be found. This accounts for the fact that strongly
    ///   typed <see cref="AttributeDescriptor"/> classes are expected to be in external plugins which may not be available
    ///   unless the current application is configured to use the OnTopic Editor. In that case, the base <see cref="
    ///   AttributeDescriptor"/> class will provide access to the attributes needed by most applications, including the core
    ///   OnTopic library.
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
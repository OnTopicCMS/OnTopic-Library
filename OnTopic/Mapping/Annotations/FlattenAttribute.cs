/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ATTRIBUTE: FLATTEN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Instructs the <see cref="ITopicMappingService"/> to include all children of topics in a collection into a single, flat
  ///   list.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     By default, <see cref="ITopicMappingService"/> will populate all items in a collection—and, if the <see
  ///     cref="IncludeAttribute"/> is defined, then also include their specified associations. The <see cref="FlattenAttribute"
  ///     /> allows all subsequent children to not only be included, but to be elevated to a single list. This can be especially
  ///     useful when combined with e.g. <see cref="FilterByAttributeAttribute"/> as well as strongly-typed collections (e.g.,
  ///     of a specific view model type), as it allows a list to provide, effectively, search results.
  ///   </para>
  /// </remarks>
  [AttributeUsage(AttributeTargets.Property)]
  public sealed class FlattenAttribute : Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="FlattenAttribute"/>.
    /// </summary>
    public FlattenAttribute() {
    }

  } //Class
} //Namespace
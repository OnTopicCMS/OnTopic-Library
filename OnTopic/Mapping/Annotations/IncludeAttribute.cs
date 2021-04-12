/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ATTRIBUTE: INCLUDE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Instructs the <see cref="ITopicMappingService"/> to include the specified <see cref="Associations"/> on that property.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     By default, <see cref="ITopicMappingService"/> will populate all associations on the initial data transfer object,
  ///     but won't continue to do so on associated objects. So, for instance, a <c>Children</c> collection will cause all
  ///     children to be loaded, but the mapper won't populate <i>their</i> <c>Children</c> (assuming that property is
  ///     available).
  ///   </para>
  ///   <para>
  ///     The <see cref="IncludeAttribute"/> overrides this behavior. If set, the <see cref="ITopicMappingService"/> will
  ///     populate the <see cref="Associations"/> specified on the associated topics.
  ///   </para>
  /// </remarks>
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
  public sealed class IncludeAttribute : Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="IncludeAttribute"/> by providing an <paramref name="associations"/>.
    /// </summary>
    /// <param name="associations">The specific associations that should be crawled.</param>
    public IncludeAttribute(AssociationTypes associations) {
      Associations = associations;
    }

    /*==========================================================================================================================
    | PROPERTY: ASSOCIATIONS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the type(s) of associations that should be recursed over.
    /// </summary>
    public AssociationTypes Associations { get; }

  } //Class
} //Namespace
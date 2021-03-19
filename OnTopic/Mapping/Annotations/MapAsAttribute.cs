/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ATTRIBUTE: MAP AS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides the <see cref="ITopicMappingService"/> with instructions as to which type to map an association to.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     By default, <see cref="ITopicMappingService"/> implementations will attempt to map an association—such as children, a
  ///     set of relationships, or a topic reference—to the view model associated with the <see cref="Topic.ContentType"/> of
  ///     the target topic. So, for example, if a <c>Parent</c> property points to a <see cref="Topic"/> with a <see cref="Topic
  ///     .ContentType"/> of <c>Page</c>, then it will look for a <c>PageTopicViewModel</c> or <c>PageViewModel</c>. If that
  ///     type cannot be found, or is incompatible with the property type, it will be ignored.
  ///   </para>
  ///   <para>
  ///     This attribute instead instructs the <see cref="ITopicMappingService"/> to map the association to a specific type,
  ///     independent of its <see cref="Topic.ContentType"/>. This can be useful if a model only needs a limited set of
  ///     attributes, and doesn't need to map the full list of properties on the default view model. It can also be useful if
  ///     e.g. a collection should have multiple unrelated types bound by a common interface. Both of these scenarios are common
  ///     when modeling a list of related topics, which may only require e.g. the <c>Title</c> and <c>WebPath</c> for each one.
  ///     It can also have a notable impact on performance by limiting the number of unnecessary properties that get mapped as
  ///     part of the request.
  ///   </para>
  /// </remarks>
  [AttributeUsage(AttributeTargets.Property)]
  public sealed class MapAsAttribute : Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="MapAsAttribute"/> by providing an <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The view model to map the association to.</param>
    public MapAsAttribute(Type type) {
      Type = type;
    }

    /*==========================================================================================================================
    | PROPERTY: TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the value of the <see cref="Type"/> that the association should be mapped to.
    /// </summary>
    public Type Type { get; }

  } //Class
} //Namespace
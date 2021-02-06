/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Threading.Tasks;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Mapping {

  /*============================================================================================================================
  | INTERFACE: TOPIC MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="ITopicMappingService"/> interface provides an abstraction for mapping <see cref="Topic"/> instances to
  ///   Data Transfer Objects, such as View Models.
  /// </summary>
  public interface ITopicMappingService {

    /*==========================================================================================================================
    | METHOD: MAP (DYNAMIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a topic, will identify any View Models named, by convention, "{ContentType}ViewModel" and populate them
    ///   according to the rules of the mapping implementation.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Because the class is using reflection to determine the target View Models, the return type is <see cref="Object"/>.
    ///     These results may need to be cast to a specific type, depending on the context. That said, strongly-typed views
    ///     should be able to cast the object to the appropriate View Model type. If the type of the View Model is known
    ///     upfront, and it's imperative that it be strongly typed, then prefer <see cref="MapAsync{T}(Topic, AssociationTypes)
    ///     "/>.
    ///   </para>
    ///   <para>
    ///     Because the target object is being dynamically constructed, it must implement a default constructor.
    ///   </para>
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="associations">Determines what associations the mapping should follow, if any.</param>
    /// <returns>An instance of the dynamically determined View Model with properties appropriately mapped.</returns>
    Task<object?> MapAsync(Topic? topic, AssociationTypes associations = AssociationTypes.All);

    /*==========================================================================================================================
    | METHOD: MAP (GENERIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a topic and a generic type, will instantiate a new instance of the generic type and populate it according to the
    ///   rules of the mapping implementation.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Because the target object is being dynamically constructed, it must implement a default constructor.
    ///   </para>
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="associations">Determines what associations the mapping should follow, if any.</param>
    /// <returns>
    ///   An instance of the requested View Model <typeparamref name="T"/> with properties appropriately mapped.
    /// </returns>
    Task<T?> MapAsync<T>(Topic? topic, AssociationTypes associations = AssociationTypes.All) where T : class, new();

    /*==========================================================================================================================
    | METHOD: MAP (INSTANCES)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a topic and an instance of a data transfer object, will populate the latter according to the rules of the
    ///   mapping implementation.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="target">The data transfer object to populate.</param>
    /// <param name="associations">Determines what associations the mapping should follow, if any.</param>
    /// <returns>
    ///   An instance of the requested View Model instance with properties appropriately mapped.
    /// </returns>
    Task<object?> MapAsync(Topic? topic, object target, AssociationTypes associations = AssociationTypes.All);

  } //Interface
} //Namespace
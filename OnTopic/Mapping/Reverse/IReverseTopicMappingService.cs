/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Threading.Tasks;
using OnTopic.Mapping.Annotations;
using OnTopic.Metadata;
using OnTopic.Models;
using OnTopic.Repositories;

namespace OnTopic.Mapping.Reverse {

  /*============================================================================================================================
  | INTERFACE: REVERSE TOPIC MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="IReverseTopicMappingService"/> interface provides an abstraction for mapping Data Transfer Objects
  ///   (DTOs), such as View Models or, more likely, Binding Models, back to <see cref="Topic"/> instances.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Unlike something like e.g. AutoMapper, the reverse mapping scenario isn't simply a matter of "flipping the script" on
  ///     the <see cref="ITopicMappingService"/>. A binding model should not be relied upon to be an accurate description of the
  ///     <see cref="ContentTypeDescriptor"/> (even though, ideally, it should be). Therefore, the <see
  ///     cref="IReverseTopicMappingService"/> will instead rely upon the <see cref="ContentTypeDescriptor"/> to define the
  ///     model and use that to selectively pull in property values accordingly. At the same time, however, it must honor hints
  ///     from the binding model regarding what properties are intended for—especially in terms of e.g aliased relationships.
  ///   </para>
  ///   <para>
  ///     The current design of <see cref="IReverseTopicMappingService"/> is not intended to support mapping associations, as
  ///     this overlaps with a broader concern of merging topics with an existing <see cref="Topic"/> graph (e.g., as part of an
  ///     <see cref="ITopicRepository"/>. It will map the associations to those topics, but not those topics themselves.
  ///   </para>
  ///   <para>
  ///     Despite the differences between the <see cref="ITopicMappingService"/> and <see cref="IReverseTopicMappingService"/>s,
  ///     many attributes are able to be reused between them. For instance, the <see cref="AttributeKeyAttribute"/> can still
  ///     map a property on a binding model to an attribute of a different name on a <see cref="Topic"/>, just as the <see
  ///     cref="CollectionAttribute"/> can with collections. Other attributes, however, provde no benefit in the reverse
  ///     scenario, such as <see cref="FlattenAttribute"/> or <see cref="FilterByAttributeAttribute"/>, which really only make
  ///     sense in creating a "produced view" that is a subset of the original model. That is valuable when creating a view
  ///     model, but isn't a useful use case when working with binding models.
  ///   </para>
  /// </remarks>
  public interface IReverseTopicMappingService {

    /*==========================================================================================================================
    | METHOD: MAP (DYNAMIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a binding model, will create a new instance of <see cref="Topic"/> (or the appropriate subclass), and will map
    ///   the properties of the binding model to attributes on the <see cref="Topic"/>.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Because the class is using reflection to determine the target View Models, the return type is <see cref="Topic"/>.
    ///     These results may need to be cast to a specific type, depending on the context. That said, strongly-typed views
    ///     should be able to cast the object to the appropriate View Model type. If the type of the Topic is known
    ///     upfront, and it is imperative that it be strongly-typed, then prefer <see cref="MapAsync{T}(ITopicBindingModel)"/>.
    ///   </para>
    /// </remarks>
    /// <param name="source">
    ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
    /// </param>
    /// <returns>An instance of the dynamically determined <see cref="Topic"/> with attributes appropriately mapped.</returns>
    Task<Topic?> MapAsync(ITopicBindingModel source);

    /*==========================================================================================================================
    | METHOD: MAP (GENERIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a binding model, will instantiate a new instance of the generic type and map the properties of the binding model
    ///   to attributes on the <see cref="Topic"/>.
    /// </summary>
    /// <param name="source">
    ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
    /// </param>
    /// <returns>
    ///   An instance of the requested Topc <typeparamref name="T"/> with attributes appropriately mapped.
    /// </returns>
    Task<T?> MapAsync<T>(ITopicBindingModel source) where T : Topic;

    /*==========================================================================================================================
    | METHOD: MAP (INSTANCES)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a binding model and an existing <see cref="Topic"/>, will map the properties of the binding model to attributes
    ///   on the <see cref="Topic"/>.
    /// </summary>
    /// <param name="source">
    ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
    /// </param>
    /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
    /// <returns>
    ///   An instance of provided <see cref="Topic"/> with attributes appropriately mapped.
    /// </returns>
    Task<Topic?> MapAsync(ITopicBindingModel source, Topic target);

  } //Interface
} //Namespace
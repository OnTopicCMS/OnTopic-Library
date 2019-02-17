/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | ATTRIBUTE: MAP TO (TYPE)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Instructs the <see cref="ITopicMappingService"/> to map to the specified <see cref="Type"/> instead inferring the type
  ///   based on the <see cref="Topic.ContentType"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     By default, <see cref="ITopicMappingService"/> implementations will attempt to automatically infer the target view
  ///     model based on the <c>{<see cref="Topic.ContentType"/>}TopicViewModel</c> convention. The <see
  ///     cref="MapToTypeAttribute"/> allows an alternate <see cref="Type"/> to be specified, thus forcing the source <see
  ///     cref="Topic"/> to be mapped to a specific topic view model.
  ///   </para>
  ///   <para>
  ///     If no <see cref="MapToTypeAttribute.Type"/> is specified, then the <see cref="Type"/> associated with the decorated
  ///     property is instead used. If that property implements <see cref="IList"/>, then the <see cref="Type"/> stored within
  ///     that collection will be used. For instance, if a property is of type <c>TopicviewModelCollection&lt;TopicViewModel&gt;
  ///     </c>, then the <see cref="MapToTypeAttribute.Type"/> will default to <c>TopicViewModel</c>.
  ///   </para>
  ///   <para>
  ///     If the <see cref="MapToTypeAttribute.Type"/> is specified, but is not assignable from the target type of the property,
  ///     then it will not be added; an exception will not be thrown. If the <see cref="Type"/> cannot be constructed—e.g. it is
  ///     an interface, an abstract class, doesn't have a default constructor, etc.—then an <see cref="ArgumentException"/> will
  ///     be thrown.
  ///   </para>
  /// </remarks>
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public sealed class MaximumDepth : System.Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="MapToTypeAttribute"/>.
    /// </summary>
    public MaximumDepth(int depth) {
      Depth = depth;
    }

    /*==========================================================================================================================
    | PROPERTY: DEPTH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the maximum depth.
    /// </summary>
    public int Depth { get; }

  } //Class

} //Namespace

/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Reverse;

namespace OnTopic.ViewModels.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: ASSOCIATED TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a model for binding an association of a <see cref="ITopicBindingModel"/> to another <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   While implementors may choose to create a custom <see cref="IAssociatedTopicBindingModel"/> implementation, the out-of-
  ///   the-box <see cref="AssociatedTopicBindingModel"/> implementation satisfies all of the requirements of the <see cref="
  ///   ReverseTopicMappingService"/>. The only reason to implement a custom definition is if the caller needs additional
  ///   metadata for separate validation or processing.
  /// </remarks>
  public record AssociatedTopicBindingModel : IAssociatedTopicBindingModel {

    /*==========================================================================================================================
    | PROPERTY: UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the topic's <see cref="UniqueKey"/> attribute, the unique text identifier for the topic.
    /// </summary>
    /// <requires description="The value from the getter must not be null." exception="T:System.ArgumentNullException">
    ///   value is not null
    /// </requires>
    [Required]
    public string UniqueKey { get; init; } = default!;

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;
using OnTopic.Mapping.Reverse;
using OnTopic.Models;

namespace OnTopic.ViewModels.BindingModels {

  /*============================================================================================================================
  | CLASS: ASSOCIATED TOPIC BINDING MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for binding an association of a binding model to an existing <see cref="Topic"/>.
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
    public string? UniqueKey { get; init; }

  } //Class
} //Namespaces
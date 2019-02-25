/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;
using Ignia.Topics.Mapping;

namespace Ignia.Topics.ViewModels {

  /*============================================================================================================================
  | INTERFACE: RELATED TOPIC BINDING MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for binding a relationship of a binding model to an existing <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   It is strictly required that any binding models used as relationships implement the <see
  ///   cref="IRelatedTopicBindingModel"/> interface for the default <see cref="ReverseTopicMappingService"/> to correctly
  ///   identify and map a relationship back to a <see cref="Topic"/>.
  /// </remarks>
  public interface IRelatedTopicBindingModel {

    /*==========================================================================================================================
    | PROPERTY: UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's <see cref="UniqueKey"/> attribute, the unique text identifier for the topic.
    /// </summary>
    /// <requires description="The value from the getter must not be null." exception="T:System.ArgumentNullException">
    ///   value != null
    /// </requires>
    [Required]
    string UniqueKey { get; set; }

  } //Class
} //Namespace

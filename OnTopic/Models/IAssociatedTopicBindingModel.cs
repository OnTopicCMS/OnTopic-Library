/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;
using OnTopic.Mapping.Reverse;

namespace OnTopic.Models {

  /*============================================================================================================================
  | INTERFACE: ASSOCIATED TOPIC BINDING MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for binding an association of a binding model to an existing <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   It is strictly required that any binding models used as associations implement the <see cref="
  ///   IAssociatedTopicBindingModel"/> interface for the default <see cref="ReverseTopicMappingService"/> to correctly identify
  ///   and map an association back to a <see cref="Topic"/>.
  /// </remarks>
  public interface IAssociatedTopicBindingModel {

    /*==========================================================================================================================
    | PROPERTY: UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's <see cref="UniqueKey"/> attribute, the unique text identifier for the topic.
    /// </summary>
    /// <requires description="The value from the getter must not be null." exception="T:System.ArgumentNullException">
    ///   value is not null
    /// </requires>
    [Required, NotNull, DisallowNull]
    string? UniqueKey { get; init; }

  } //Class
} //Namespace
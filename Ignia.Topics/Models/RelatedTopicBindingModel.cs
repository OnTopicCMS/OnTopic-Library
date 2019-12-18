/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;

namespace Ignia.Topics.Models {

  /*============================================================================================================================
  | CLASS: RELATED TOPIC BINDING MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for binding a relationship of a binding model to an existing <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   While implementors may choose to create a custom <see cref="IRelatedTopicBindingModel"/> implementation, the out-of-the-
  ///   box <see cref="RelatedTopicBindingModel"/> implementation satisfies all of the requirements of the <see
  ///   cref="ReverseTopicMappingService"/>. The only reason to implement a custom definition is if the caller needs additional
  ///   metadata for separate validation or processing.
  /// </remarks>
  public class RelatedTopicBindingModel : IRelatedTopicBindingModel {

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
    public string? UniqueKey { get; set; }

  } //Class
} //Namespaces
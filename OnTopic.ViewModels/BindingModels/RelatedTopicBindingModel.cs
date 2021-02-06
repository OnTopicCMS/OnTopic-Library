/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Mapping.Reverse;
using OnTopic.Models;

namespace OnTopic.ViewModels.BindingModels {

  /*============================================================================================================================
  | CLASS: RELATED TOPIC BINDING MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for binding a relationship of a binding model to an existing <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   While implementors may choose to create a custom <see cref="IAssociatedTopicBindingModel"/> implementation, the out-of-
  ///   the-box <see cref="AssociatedTopicBindingModel"/> implementation satisfies all of the requirements of the <see cref="
  ///   ReverseTopicMappingService"/>. The only reason to implement a custom definition is if the caller needs additional
  ///   metadata for separate validation or processing.
  /// </remarks>
  [Obsolete(
    "The RelatedTopicBindingModel has been replaced by the AssociatedTopicBindingModel. Please update references.",
    true
  )]
  public record RelatedTopicBindingModel: AssociatedTopicBindingModel {

  } //Class
} //Namespaces
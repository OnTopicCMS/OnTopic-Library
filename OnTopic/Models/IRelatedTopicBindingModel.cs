/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Reverse;

namespace OnTopic.Models {

  /*============================================================================================================================
  | INTERFACE: RELATED TOPIC BINDING MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for binding an association of a binding model to an existing <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   It is strictly required that any binding models used as associations implement the <see cref="IRelatedTopicBindingModel"
  ///   /> interface for the default <see cref="ReverseTopicMappingService"/> to correctly identify and map an association back
  ///   to a <see cref="Topic"/>.
  /// </remarks>
  [Obsolete(
    "The IRelatedTopicBindingModel has been replaced by the IAssociatedTopicBindingModel. Please update references.",
    true
  )]
  public interface IRelatedTopicBindingModel: IAssociatedTopicBindingModel {

  } //Class
} //Namespace
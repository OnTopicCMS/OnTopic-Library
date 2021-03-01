/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Reverse;

namespace OnTopic.Models {

  /*============================================================================================================================
  | INTERFACE: TOPIC BINDING MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for binding form values to controller actions.
  /// </summary>
  /// <remarks>
  ///   It is strictly required that topic binding models implement the <see cref="ITopicBindingModel"/> interface for the
  ///   default <see cref="ReverseTopicMappingService"/> to correctly identify and map a binding model to a <see cref="Topic"/>.
  /// </remarks>
  public interface ITopicBindingModel: ICoreTopicViewModel {


  } //Class
} //Namespace
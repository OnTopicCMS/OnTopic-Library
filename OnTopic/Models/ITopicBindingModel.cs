/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;
using OnTopic.Mapping.Reverse;
using OnTopic.Metadata;

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
  public interface ITopicBindingModel: IKeyedTopicViewModel {

    /*==========================================================================================================================
    | PROPERTY: CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the key name of the content type that the current topic represents.
    /// </summary>
    /// <remarks>
    ///   Each topic is associated with a content type. The content type determines which attributes are displayed in the Topics
    ///   Editor (via the <see cref="ContentTypeDescriptor.AttributeDescriptors"/> property).
    /// </remarks>
    [Required]
    string? ContentType { get; init; }

  } //Class
} //Namespace
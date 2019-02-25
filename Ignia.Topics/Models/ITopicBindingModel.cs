/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;
using Ignia.Topics.Mapping;
using Ignia.Topics.Metadata;

namespace Ignia.Topics.Models {

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
  public interface ITopicBindingModel {

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's Key attribute, the primary text identifier for the topic.
    /// </summary>
    /// <requires description="The value from the getter must not be null." exception="T:System.ArgumentNullException">
    ///   value != null
    /// </requires>
    /// <requires
    ///   description="The Key should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    [Required]
    string Key { get; set; }

    /*==========================================================================================================================
    | PROPERTY: CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the key name of the content type that the current topic represents.
    /// </summary>
    /// <remarks>
    ///   Each topic is associated with a content type. The content type determines which attributes are displayed in the Topics
    ///   Editor (via the <see cref="ContentTypeDescriptor.AttributeDescriptors"/> property). The content type also determines,
    ///   by default, which view is rendered by the <see cref="Topics.ITopicRoutingService"/> (assuming the value isn't
    ///   overwritten down the pipe).
    /// </remarks>
    [Required]
    string ContentType { get; set; }

  } //Class
} //Namespace

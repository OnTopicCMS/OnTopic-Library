/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using OnTopic.Mapping;
using OnTopic.Metadata;

namespace OnTopic.Models {

  /*============================================================================================================================
  | INTERFACE: KEYED TOPIC VIEW MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Ensures that a model maintains, at minimum, a <see cref="Key"/> property, necessary to support e.g. a <see cref="
  ///   KeyedCollection{TKey, TValue}"/>.
  /// </summary>
  /// <remarks>
  ///   It is not required that topic view models implement the <see cref="IKeyedTopicViewModel"/> interface for the <see cref="
  ///   TopicMappingService"/> to correctly identify and map <see cref="Topic"/>s to topic view models. That said, the interface
  ///   does ensure that those view models can be keyed, which is useful for, especially, child collections.
  /// </remarks>
  public interface IKeyedTopicViewModel {

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the topic's <see cref="Key"/> attribute, the primary text identifier for the <see cref="Topic"/>.
    /// </summary>
    /// <requires description="The value from the getter must not be null." exception="T:System.ArgumentNullException">
    ///   value is not null
    /// </requires>
    [Required, NotNull, DisallowNull]
    string? Key { get; init; }

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
    [Required, NotNull, DisallowNull]
    string? ContentType { get; init; }

  } //Class
} //Namespace
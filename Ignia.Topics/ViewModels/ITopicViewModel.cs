/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Ignia.Topics.Mapping;
using Ignia.Topics.Metadata;

namespace Ignia.Topics.ViewModels {

  /*============================================================================================================================
  | INTERFACE: TOPIC VIEW MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for feeding views.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     It is not strictly required that topic view models implement the <see cref="ITopicViewModel"/> interface for the
  ///     default <see cref="TopicMappingService"/> to correctly identify and map <see cref="Topic"/>s to topic view models.
  ///     That said, the interface does define properties that presentation infrastructure may expect. As a result, if it isn't
  ///     provided via the public interface then it will instead need to be defined in some other way.
  ///   </para>
  ///   <para>
  ///     For instance, in the default MVC library, the <c>TopicViewResult</c> class requires that the <see
  ///     cref="Topic.ContentType"/> and <see cref="Topic.View"/> be supplied separately if they're not provided as part of a
  ///     <see cref="ITopicViewModel"/>. The exact details of this will obviously vary based on the implementation of the
  ///     presentation layer and any supporting libraries.
  ///   </para>
  /// </remarks>
  public interface ITopicViewModel {

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
    string Key { get; set; }

    /*==========================================================================================================================
    | PROPERTY: UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's <see cref="UniqueKey"/> attribute, the unique text identifier for the topic.
    /// </summary>
    /// <requires description="The value from the getter must not be null." exception="T:System.ArgumentNullException">
    ///   value != null
    /// </requires>
    string UniqueKey { get; set; }

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
    string ContentType { get; set; }

    /*==========================================================================================================================
    | PROPERTY: VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the View attribute, representing the default view to be used for the topic.
    /// </summary>
    /// <remarks>
    ///   This value can be set via the query string (via the <see cref="ITopicRoutingService"/> class), via the Accepts header
    ///   (also via the <see cref="ITopicRoutingService"/> class), on the topic itself (via this property), or via the
    ///   <see cref="ContentType"/>. By default, it will be set to the name of the <see cref="ContentType"/>; e.g., if the
    ///   Content Type is "Page", then the view will be "Page". This will cause the <see cref="ITopicRoutingService"/> to look
    ///   for a view at, for instance, /Common/Templates/Page/Page.aspx.
    /// </remarks>
    /// <requires description="The value from the getter must be provided." exception="T:System.ArgumentNullException">
    ///   !string.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The View should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !value?.Contains(" ")?? true
    /// </requires>
    string View { get; set; }

    /*==========================================================================================================================
    | PROPERTY: IS HIDDEN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether the current topic is hidden.
    /// </summary>
    bool IsHidden { get; set; }

    /*==========================================================================================================================
    | PROPERTY: TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Title attribute, which represents the friendly name of the topic.
    /// </summary>
    /// <remarks>
    ///   While the <see cref="ITopicViewModel.Key"/> may not contain, for instance, spaces or symbols, there are no
    ///   restrictions on what characters can be used in the title. For this reason, it provides the default public value for
    ///   referencing topics.
    /// </remarks>
    /// <requires description="The value from the getter must be provided." exception="T:System.ArgumentNullException">
    ///   !string.IsNullOrWhiteSpace(value)
    /// </requires>
    string Title { get; set; }

  } //Class
} //Namespace

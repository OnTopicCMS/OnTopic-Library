/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Ignia.Topics.Repositories;

namespace Ignia.Topics.AspNetCore.Mvc.Models {

  /*============================================================================================================================
  | CLASS: TOPIC VIEW MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a reference to an <see cref="ITopicRepository"/> implementation as well as a current <see cref="Topic"/>
  ///   associated with the current request.
  /// </summary>
  /// <remarks>
  ///   Typically, views should be bound to view models which are, in effect, data transfer objects. They may hold references to
  ///   other view models, but all other properties of them and their related view models will be scalar values. On occasion,
  ///   however, some views may, for a variety of reasons, need full access to a <see cref="Topic"/> and, in fact, the entire
  ///   topic graph via an implementation of <see cref="ITopicRepository"/>. This is typically because the view requires full
  ///   access to the tree, or arbitrary access to the <see cref="AttributeValueCollection"/>. In those cases, the <see
  ///   cref="TopicEntityViewModel"/> provides a convenient wrapper for delivering both to the current page.
  /// </remarks>
  public class TopicEntityViewModel {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic View Model with appropriate dependencies.
    /// </summary>
    /// <returns>A Topic view model.</returns>
    public TopicEntityViewModel(ITopicRepository topicRepository, Topic topic) {

      TopicRepository           = topicRepository;
      Topic                     = topic;
      RootTopic                 = topic;

      while (RootTopic.Parent != null) {
        RootTopic = RootTopic.Parent;
      }

    }

    /*==========================================================================================================================
    | PROPERTY: TOPIC REPOSITORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the topic repository associated with the current request.
    /// </summary>
    /// <returns>The <see cref="ITopicRepository"/> associated with the current request.</returns>
    public ITopicRepository TopicRepository { get; }

    /*==========================================================================================================================
    | PROPERTY: ROOT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the root topic associated with the object graph. This can be used to easily find other topics in the tree.
    /// </summary>
    /// <returns>The <see cref="GetTopic()"/> at the root of the object graph.</returns>
    public Topic RootTopic { get; }

    /*==========================================================================================================================
    | PROPERTY: TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the topic associated with the current page based on the URL or route.
    /// </summary>
    /// <returns>The <see cref="GetTopic()"/> associated with the current page.</returns>
    public Topic Topic { get; }

  } // Class

} // Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Ignia.Topics;
using Ignia.Topics.Repositories;

namespace Ignia.Topics.Web.Mvc {

  /*============================================================================================================================
  | CLASS: TOPIC VIEW MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a default generic view model for views related to the Topic Library. 
  /// </summary>
  /// <remarks>
  ///   Will be the primary model used by the <see cref="TopicController{T}"/>, and any specialized derivatives (although those 
  ///   derivatives may well need to be created to extend the view model with data specific to that content type and its views. 
  /// </remarks>
  public class TopicViewModel {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private             ITopicRepository    _topicRepository        = null;
    private             Topic               _currentTopic           = null;
    private             Topic               _rootTopic              = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic View Model with appropriate dependencies.
    /// </summary>
    /// <returns>A Topic view model.</returns>
    public TopicViewModel(ITopicRepository topicRepository, Topic currentTopic) {
      _topicRepository = topicRepository;
      _currentTopic = currentTopic;
    }

    /*==========================================================================================================================
    | PROPERTY: TOPIC REPOSITORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the topic repository associated with the current request. 
    /// </summary>
    /// <returns>The <see cref="ITopicRepository"/> associated with the current request.</returns>
    public ITopicRepository TopicRepository {
      get {
        return _topicRepository;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: ROOT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the root topic associated with the object graph. This can be used to easily find other topics in the tree.
    /// </summary>
    /// <returns>The <see cref="Topic"/> at the root of the object graph.</returns>
    public Topic RootTopic {
      get {
        if (_rootTopic == null) {
          _rootTopic = _currentTopic;
          while (_rootTopic.Parent != null) {
            _rootTopic = _rootTopic.Parent;
          }
        }
        return _rootTopic;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: CURRENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the topic associated with the current page based on the URL or route. 
    /// </summary>
    /// <returns>The <see cref="Topic"/> associated with the current page.</returns>
    public Topic CurrentTopic {
      get {
        return _currentTopic;
      }
    }



  } //Class

} //Namespace
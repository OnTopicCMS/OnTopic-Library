/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Ignia.Topics;
using Ignia.Topics.Repositories;

namespace Ignia.Topics.Web.Mvc {

  /*============================================================================================================================
  | CLASS: TOPIC TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a default ASP.NET MVC Controller for any paths associated with the Ignia Topic Library. Responsible for 
  ///   identifying the topic associated with the given path, determining its content type, and returning a view associated with
  ///   that content type (with potential overrides for multiple views). 
  /// </summary>
  class TopicController<T> : AsyncController where T : Topic {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private     ITopicRepository                _topicRepository        = null;
    private     T                               _currentTopic           = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic Controller with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    public TopicController(ITopicRepository topicRepository, T currentTopic) {
      _topicRepository = topicRepository;
      _currentTopic = currentTopic;
    }

    /*==========================================================================================================================
    | GET: /PATH/PATH/PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides access to a view associated with the current topic's Content Type, if appropriate, view (as defined by the query 
    ///   string or topic's view. 
    /// </summary>
    /// <returns>A view associated with the requested topic's Content Type and view.</returns>
    public async Task<ActionResult> Index(string path) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate Topic Key
      var topic = GetTopic(path);

      if (topic == null) {
        return HttpNotFound("A topic with the UniqueKey '" + path + "' does not exist.");
      }
      \-----------------------------------------------------------------------------------------------------------------------*/

      /*------------------------------------------------------------------------------------------------------------------------
      | Lookup and return appropriate model, view
      return await GetView(path);
      \-----------------------------------------------------------------------------------------------------------------------*/
      return HttpNotFound("This controller is not yet implemented.");

    }

  } //Class

} //Namespace



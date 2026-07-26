/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Repositories;

namespace OnTopic.Collections;

/*==============================================================================================================================
| CLASS: CHILD TOPIC COLLECTION
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a collection of <see cref="Topic"/> objects representing the immediate children of a <see cref="Topic"/>.
/// </summary>
/// <remarks>
///   The <see cref="ChildTopicCollection"/> is intended exclusively for providing access to children via the <see cref=
///   "Topic.Children"/> property. For this reason, the constructor is marked as internal.
/// </remarks>
public class ChildTopicCollection : KeyedTopicCollection {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              Topic                           _parent;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="ChildTopicCollection"/> class.
  /// </summary>
  /// <param name="parent">A reference to the topic that the current child collection is bound to.</param>
  internal ChildTopicCollection(Topic parent) {
    _parent                     = parent;
  }

} //Class
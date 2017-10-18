/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Ignia.Topics.Collections {

  /*============================================================================================================================
  | CLASS: TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a named version of the <see cref="TopicCollection{T}"/>, suitable for use in
  ///   <see cref="RelatedTopicCollection"/>, or other derivatives of <see cref="KeyedCollection{TKey, TItem}"/>.
  /// </summary>
  public class NamedTopicCollection: TopicCollection<Topic> {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    string                      _name                           = "";

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="NamedTopicCollection"/> class.
    /// </summary>
    /// <param name="name">Provides a name for the collection, used to identify different collections.</param>
    /// <param name="topics">Optionally seeds the collection with an optional list of topic references.</param>
    public NamedTopicCollection(string name = "", IEnumerable<Topic> topics = null) : base(new Topic()) {
      _name = name;
      if (topics != null) {
        CopyTo(topics.ToArray(), 0);
      }
    }

    /*==========================================================================================================================
    | PROPERTY: NAME
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides an optional name for the collection.
    /// </summary>
    /// <remarks>
    ///   The Name property is optional, and primary intended to differentiate multiple <see cref="TopicCollection{T}"/>
    ///   instances being referenced in a single collection, such as the <see cref="RelatedTopicCollection"/>.
    /// </remarks>
    public string Name {
      get => _name;
    }

  } //Class

} //Namespace
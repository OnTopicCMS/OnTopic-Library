/*==============================================================================================================================
| Author        Jeremy Caney, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace Ignia.Topics.Web {

  /*============================================================================================================================
  | CLASS: TYPED TOPIC PAGE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic page class implementation which is aware of the strongly-typed Topic.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class TypedTopicPage<T> : TopicPage where T : Topic {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    /// Initializes a new instance of the <see cref="TypedTopicPage{T}"/> class.
    /// </summary>
    public TypedTopicPage() : base() { }

  } //Class

} //Namespace

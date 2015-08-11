/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Configuration.Provider;
using Ignia.Topics.Providers;

namespace Ignia.Topics.Configuration {

  /*============================================================================================================================
  | CLASS: TOPIC DATA PROVIDER COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom implementation of a <see cref="ProviderCollection"/> responsible for encapsulating a set of
  ///   <see cref="TopicDataProviderBase"/> elements.
  /// </summary>
  public class TopicDataProviderCollection : ProviderCollection {

    /*==========================================================================================================================
    | INDEXER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the <see cref="TopicDataProviderBase"/> with the specified name.
    /// </summary>
    /// <param name="name">
    ///   The string name value used as the indexer for the <see cref="TopicDataProviderBase"/> item in the collection.
    /// </param>
    new public TopicDataProviderBase this[string name] {
      get {
        return (TopicDataProviderBase)base[name];
      }
    }

  } //Class

} //Namespace

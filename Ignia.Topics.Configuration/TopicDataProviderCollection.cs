/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Configuration.Provider;
using Ignia.Topics.Repositories;

namespace Ignia.Topics.Configuration {

  /*============================================================================================================================
  | CLASS: TOPIC DATA PROVIDER COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom implementation of a <see cref="ProviderCollection"/> responsible for encapsulating a set of
  ///   <see cref="TopicRepositoryBase"/> elements.
  /// </summary>
  public class TopicDataProviderCollection : ProviderCollection {

    /*==========================================================================================================================
    | INDEXER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the <see cref="TopicRepositoryBase"/> with the specified name.
    /// </summary>
    /// <param name="name">
    ///   The string name value used as the indexer for the <see cref="TopicRepositoryBase"/> item in the collection.
    /// </param>
    new public ITopicRepository this[string name] {
      get {
        return (ITopicRepository)base[name];
      }
    }

  } // Class

} // Namespace

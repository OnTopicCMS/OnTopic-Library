/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Configuration.Provider;
using Ignia.Topics.Providers;

namespace Ignia.Topics.Configuration {

  /*============================================================================================================================
  | CLASS: TOPIC MAPPING PROVIDER COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom implementation of a <see cref="ProviderCollection"/> responsible for encapsulating a set of 
  ///   <see cref="TopicMappingProviderBase"/> elements.
  /// </summary>
  public class TopicMappingProviderCollection : ProviderCollection {

    /*==========================================================================================================================
    | INDEXER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the <see cref="TopicMappingProviderBase"/> with the specified name.
    /// </summary>
    /// <param name="name">
    ///   The string name value used as the indexer for the <see cref="TopicMappingProviderBase"/> item in the collection.
    /// </param>
    new public TopicMappingProviderBase this[string name] {
      get {
        return (TopicMappingProviderBase)base[name];
      }
    }

  } // Class

} // Namespace


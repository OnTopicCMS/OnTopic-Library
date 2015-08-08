/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a custom implementation of a ProviderCollection responsible for encapsulating a set of
|               TopicMappingProviderBase elements.
|
\=============================================================================================================================*/
using System.Configuration.Provider;

namespace Ignia.Topics.Configuration {

  /*==============================================================================================================================
  | CLASS
  \-----------------------------------------------------------------------------------------------------------------------------*/
  public class TopicMappingProviderCollection : ProviderCollection {

  /*============================================================================================================================
  | INDEXER
  \---------------------------------------------------------------------------------------------------------------------------*/
    new public TopicMappingProviderBase this[string name] {
      get {
        return (TopicMappingProviderBase)base[name];
      }
    }

  } //Class

} //Namespace


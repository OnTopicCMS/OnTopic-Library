/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a custom implementation of a ProviderCollection responsible for encapsulating a set of
|               TopicDataProviderBase elements.
|
\=============================================================================================================================*/
using System.Configuration.Provider;
using Ignia.Topics.Providers;

namespace Ignia.Topics.Configuration {

  /*==============================================================================================================================
  | CLASS
  \-----------------------------------------------------------------------------------------------------------------------------*/
  public class TopicDataProviderCollection : ProviderCollection {

  /*============================================================================================================================
  | INDEXER
  \---------------------------------------------------------------------------------------------------------------------------*/
    new public TopicDataProviderBase this[string name] {
      get {
        return (TopicDataProviderBase)base[name];
      }
    }

  } //Class

} //Namespace

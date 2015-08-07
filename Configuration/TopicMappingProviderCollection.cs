namespace Ignia.Topics.Configuration {

/*==============================================================================================================================
| TOPIC MAPPING PROVIDER COLLECTION
|
| Author        Katherine Trunkey, Ignia LLC (katherine.trunkey@ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a custom implementation of a ProviderCollection responsible for encapsulating a set of
|               TopicMappingProviderBase elements.
|
>===============================================================================================================================
| Revisions     Date            Author                  Comments
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
|               08.14.14        Katherine Trunkey       Created initial version.
\-----------------------------------------------------------------------------------------------------------------------------*/

/*==============================================================================================================================
| DEFINE ASSEMBLY ATTRIBUTES
>===============================================================================================================================
| Declare and define attributes used in the compiling of the finished assembly.
\-----------------------------------------------------------------------------------------------------------------------------*/
  using System;
  using System.Configuration;
  using System.Configuration.Provider;
  using System.Text;

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

    }
  }

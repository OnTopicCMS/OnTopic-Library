/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
|
| Purpose       Provides a customized version of the ConfigurationSection class in order to permit configuration of the Topics
|               data management classes via the application's web.config.
|
\=============================================================================================================================*/
using System.Configuration;

namespace Ignia.Topics.Configuration {

  /*==============================================================================================================================
  | CLASS
  \-----------------------------------------------------------------------------------------------------------------------------*/
  public class TopicsSection : ConfigurationSection {

  /*============================================================================================================================
  | FACTORY METHOD: GET CONFIG
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static TopicsSection GetConfig() {
      return ConfigurationManager.GetSection("topics") as TopicsSection;
    }

  /*============================================================================================================================
  | ATTRIBUTE: ROOT TOPIC NAMESPACE
  >=============================================================================================================================
  | Sets the root (parent) Topic expected to be defined for Topics data.
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("rootTopicNamespace", DefaultValue = "Root") ]
    public string RootTopicNamespace {
      get {
        return (string)this["rootTopicNamespace"];
      }
      set {
        this["rootTopicNamespace"] = value;
      }
    }

  /*============================================================================================================================
  | ATTRIBUTE: TOPIC DELIMITER
  >=============================================================================================================================
  | Sets the delimiter character expected to be used for separating Topics (Topic keys).
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("topicDelimiter", DefaultValue = "/") ]
    public string TopicDelimiter {
      get {
        return (string)this["topicDelimiter"];
      }
      set {
        this["topicDelimiter"] = value;
      }
    }

  /*============================================================================================================================
  | ATTRIBUTE: DEFAULT DATA PROVIDER
  >=============================================================================================================================
  | Sets the default Topics data provider that should be used.
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("defaultDataProvider", DefaultValue = "SqlTopicDataProvider") ]
    public string DefaultDataProvider {
      get {
        return (string)this["defaultDataProvider"];
      }
      set {
        this["defaultDataProvider"] = value;
      }
    }

  /*============================================================================================================================
  | ATTRIBUTE: DEFAULT MAPPING PROVIDER
  >=============================================================================================================================
  | Sets the default Topics mapping provider that should be used.
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("defaultMappingProvider", DefaultValue = "NullTopicMappingProvider") ]
    public string DefaultMappingProvider {
      get {
        return (string)this["defaultMappingProvider"];
      }
      set {
        this["defaultMappingProvider"] = value;
      }
    }

  /*============================================================================================================================
  | ELEMENT: VERSIONING
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("versioning") ]
    public VersioningElement Versioning {
      get {
        return this["versioning"] as VersioningElement;
      }
    }

  /*============================================================================================================================
  | ELEMENT: EDITOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("editor") ]
    public EditorElement Editor {
      get {
        return this["editor"] as EditorElement;
      }
    }

  /*============================================================================================================================
  | ELEMENT: VIEWS
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("views") ]
    public ViewsElement Views {
      get {
        return this["views"] as ViewsElement;
      }
    }

  /*============================================================================================================================
  | COLLECTION: PAGE TYPES
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("pageTypes") ]
    public PageTypeElementCollection PageTypes {
      get {
        return this["pageTypes"] as PageTypeElementCollection;
      }
    }

  /*============================================================================================================================
  | COLLECTION: PROVIDERS
  [ ConfigurationProperty("providers") ]
    public ProviderSettingsCollection Providers {
      get {
        return (ProviderSettingsCollection)base["providers"];
      }
    }
  \---------------------------------------------------------------------------------------------------------------------------*/

  /*============================================================================================================================
  | COLLECTION: DATA PROVIDERS
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("dataProviders") ]
    public ProviderSettingsCollection DataProviders {
      get {
        return (ProviderSettingsCollection)base["dataProviders"];
      }
    }

  /*============================================================================================================================
  | COLLECTION: MAPPING PROVIDERS
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("mappingProviders") ]
    public ProviderSettingsCollection MappingProviders {
      get {
        return (ProviderSettingsCollection)base["mappingProviders"];
      }
    }

  } //Class

} //Namespace


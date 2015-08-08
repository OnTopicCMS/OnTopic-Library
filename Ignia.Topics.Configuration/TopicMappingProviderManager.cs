/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
|
| Purpose       Provides initialization of the specified Topic MAPPING provider based on retrieving the appropriate data from
|               the custom configuration.
|
\=============================================================================================================================*/
using System;
using System.Configuration;
using System.Web.Configuration;
using Ignia.Topics.Providers;

namespace Ignia.Topics.Configuration {

  /*==============================================================================================================================
  | CLASS
  \-----------------------------------------------------------------------------------------------------------------------------*/
  public class TopicMappingProviderManager {

  /*============================================================================================================================
  | PRIVATE MEMBERS
  \---------------------------------------------------------------------------------------------------------------------------*/
    private     static  TopicMappingProviderBase        _defaultMappingProvider = null;
    private     static  TopicMappingProviderCollection  _mappingProviders       = null;

  /*============================================================================================================================
  | STATIC CONSTRUCTOR
  >=============================================================================================================================
  | Constructor that also fires the data provider initialization method.
  \---------------------------------------------------------------------------------------------------------------------------*/
    static TopicMappingProviderManager() {
      Initialize();
    }

  /*============================================================================================================================
  | METHOD: INITIALIZE
  >=============================================================================================================================
  | Initializes the TopicMappingProviderBase and TopicMappingProviderCollection classes as configured in the web.config. As part
  | of this, each data provider is initialized using the values defined in the web.config.
  \---------------------------------------------------------------------------------------------------------------------------*/
    private static void Initialize() {

    /*--------------------------------------------------------------------------------------------------------------------------
    | RETRIEVE <TOPICS /> CONFIGURATION SECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
      TopicsSection     topicsConfiguration             = (TopicsSection)ConfigurationManager.GetSection("topics");

      if (topicsConfiguration == null) {
        throw new ConfigurationErrorsException("The Topics configuration section (<topics />) is not set correctly.");
      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | INSTANTIATE MAPPING PROVIDERS
    \-------------------------------------------------------------------------------------------------------------------------*/
      _mappingProviders                                 = new TopicMappingProviderCollection();

      ProvidersHelper.InstantiateProviders(topicsConfiguration.MappingProviders, _mappingProviders, typeof(TopicMappingProviderBase));

    /*--------------------------------------------------------------------------------------------------------------------------
    | SET MAPPING PROVIDERS TO READ-ONLY
    \-------------------------------------------------------------------------------------------------------------------------*/
      _mappingProviders.SetReadOnly();

    /*--------------------------------------------------------------------------------------------------------------------------
    | RETRIEVE DEFAULT DATA PROVIDER SETTING
    \-------------------------------------------------------------------------------------------------------------------------*/
      _defaultMappingProvider                           = _mappingProviders[topicsConfiguration.DefaultMappingProvider];

      if (_defaultMappingProvider == null) {
        throw new Exception("The defaultDataProvider value is not available from the Topics configuration section (<topics />)");
      }

    }

  /*============================================================================================================================
  | PROPERTY: MAPPING PROVIDER
  >=============================================================================================================================
  | Returns the Topics mapping provider as set to the defaultMappingProvider attribute on the <topics /> configuration section.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static TopicMappingProviderBase MappingProvider {
      get {
        return _defaultMappingProvider;
      }
    }

  /*============================================================================================================================
  | PROPERTY: MAPPING PROVIDERS
  >=============================================================================================================================
  | Returns the collection of Topics mapping providers as configured in the mappingProviders element in the <topics />
  | configuration section.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static TopicMappingProviderCollection MappingProviders {
      get {
        return _mappingProviders;
      }
    }

  } //Class

} //Namespace

/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Configuration;
using System.Web.Configuration;
using Ignia.Topics.Providers;

namespace Ignia.Topics.Configuration {

  /*============================================================================================================================
  | CLASS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides initialization of the specified Topic MAPPING provider based on retrieving the appropriate data from the
  ///   custom configuration.
  /// </summary>
  public class TopicMappingProviderManager {

  /*============================================================================================================================
  | PRIVATE MEMBERS
  \---------------------------------------------------------------------------------------------------------------------------*/
    private     static  TopicMappingProviderBase        _defaultMappingProvider = null;
    private     static  TopicMappingProviderCollection  _mappingProviders       = null;

    /*==========================================================================================================================
    | STATIC CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    /// Initializes the <see cref="TopicMappingProviderManager"/> class and fires the data provider initialization method.
    /// </summary>
    static TopicMappingProviderManager() {
      Initialize();
    }

    /*==========================================================================================================================
    | METHOD: INITIALIZE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes the TopicMappingProviderBase and <see cref="TopicMappingProviderCollection"/> classes as configured in
    ///   the web.config. As part of this, each data provider is initialized using the values defined in the web.config.
    /// </summary>
    /// <exception cref="ConfigurationErrorsException">
    ///   The Topics configuration section (<topics />) is not set correctly.
    /// </exception>
    /// <exception cref="Exception">
    ///   The defaultDataProvider value is not available from the Topics configuration section (<topics />)
    /// </exception>
    private static void Initialize() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve <topics /> configuration section
      \-----------------------------------------------------------------------------------------------------------------------*/
      TopicsSection     topicsConfiguration             = (TopicsSection)ConfigurationManager.GetSection("topics");

      if (topicsConfiguration == null) {
        throw new ConfigurationErrorsException("The Topics configuration section (<topics />) is not set correctly.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Instantiate mapping providers
      \-----------------------------------------------------------------------------------------------------------------------*/
      _mappingProviders                                 = new TopicMappingProviderCollection();

      ProvidersHelper.InstantiateProviders(topicsConfiguration.MappingProviders, _mappingProviders, typeof(TopicMappingProviderBase));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set mapping providers to read-only
      \-----------------------------------------------------------------------------------------------------------------------*/
      _mappingProviders.SetReadOnly();

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve default data provider setting
      \-----------------------------------------------------------------------------------------------------------------------*/
      _defaultMappingProvider                           = _mappingProviders[topicsConfiguration.DefaultMappingProvider];

      if (_defaultMappingProvider == null) {
        throw new Exception("The defaultDataProvider value is not available from the Topics configuration section (<topics />).");
      }

    }

    /*==========================================================================================================================
    | PROPERTY: MAPPING PROVIDER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the Topics mapping provider as set to the defaultMappingProvider attribute on the <topics /> configuration
    ///   section.
    /// </summary>
    public static TopicMappingProviderBase MappingProvider {
      get {
        return _defaultMappingProvider;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: MAPPING PROVIDERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the collection of Topics mapping providers as configured in the mappingProviders element in the <topics />
    ///   configuration section.
    /// </summary>
    public static TopicMappingProviderCollection MappingProviders {
      get {
        return _mappingProviders;
      }
    }

  } //Class

} //Namespace

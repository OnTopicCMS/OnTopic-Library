/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Web.Configuration;
using Ignia.Topics.Repositories;

namespace Ignia.Topics.Configuration {

  /*============================================================================================================================
  | CLASS: TOPIC DATA PROVIDER MANAGER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides initialization of the specified Topic data provider based on retrieving the appropriate data from the custom
  ///   configuration.
  /// </summary>
  public class TopicDataProviderManager {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
    private     static  TopicRepositoryBase             _defaultDataProvider    = null;
    private     static  TopicDataProviderCollection     _dataProviders          = null;

    /*==========================================================================================================================
    | STATIC CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes the <see cref="TopicDataProviderManager"/> class and fires the data provider initialization method.
    /// </summary>
    static TopicDataProviderManager() {
      Initialize();
    }

    /*==========================================================================================================================
    | METHOD: INITIALIZE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes the TopicDataProviderBase and TopicDataProviderCollection classes as configured in the web.config. As
    ///   part of this, each data provider is initialized using the values defined in the web.config.
    /// </summary>
    /// <exception cref="ConfigurationErrorsException">
    ///   The Topics configuration section (<topics />) is not set correctly.
    /// </exception>
    /// <exception cref="Exception">
    ///   The defaultDataProvider value is not available from the Topics configuration section (<topics />).
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
      | Instantiate data providers
      \-----------------------------------------------------------------------------------------------------------------------*/
      _dataProviders                                    = new TopicDataProviderCollection();

      ProvidersHelper.InstantiateProviders(topicsConfiguration.DataProviders, _dataProviders, typeof(TopicRepositoryBase));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set data providers to read-only
      \-----------------------------------------------------------------------------------------------------------------------*/
      _dataProviders.SetReadOnly();

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve default data provider setting
      \-----------------------------------------------------------------------------------------------------------------------*/
      _defaultDataProvider                              = _dataProviders[topicsConfiguration.DefaultDataProvider];

      if (_defaultDataProvider == null) {
        throw new Exception("The defaultDataProvider value is not available from the Topics configuration section (<topics />).");
      }

    }

    /*==========================================================================================================================
    | PROPERTY: DATA PROVIDER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the Topics data provider as set to the defaultDataProvider attribute on the <topics /> configuration section.
    /// </summary>
    public static TopicRepositoryBase DataProvider {
      get {
        Contract.Ensures(Contract.Result<TopicRepositoryBase>() != null);
        return _defaultDataProvider;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: DATA PROVIDERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the collection of Topics data providers as configured in the dataProviders element in the <topics />
    ///   configuration section.
    /// </summary>
    public static TopicDataProviderCollection DataProviders {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate return value
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Ensures(Contract.Result<TopicDataProviderCollection>() != null);

        return _dataProviders;
      }
    }

  } // Class

} // Namespace

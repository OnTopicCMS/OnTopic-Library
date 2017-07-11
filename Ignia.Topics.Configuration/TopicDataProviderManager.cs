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
    private     static  TopicsSection                   _configuration          = null;
    private     static  TopicRepositoryBase             _defaultDataProvider    = null;
    private     static  TopicDataProviderCollection     _dataProviders          = null;

    /*==========================================================================================================================
    | STATIC CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes the <see cref="TopicDataProviderManager"/> class and fires the data provider initialization method.
    /// </summary>
    static TopicDataProviderManager() {
    }

    /*==========================================================================================================================
    | PROPERTY: CONFIGURATION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves the Topic Section from the configuration.
    /// </summary>
    /// <exception cref="ConfigurationErrorsException">
    ///   The Topics configuration section (<topics />) is not set correctly.
    /// </exception>
    public static TopicsSection Configuration {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Establish requirements
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Ensures(Contract.Result<TopicsSection>() != null);

        /*----------------------------------------------------------------------------------------------------------------------
        | Retrieve <topics /> configuration section
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_configuration == null) {
          _configuration = (TopicsSection)ConfigurationManager.GetSection("topics");
        }

        if (_configuration == null) {
          throw new ConfigurationErrorsException("The Topics configuration section (<topics />) is not set correctly.");
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return results
        \---------------------------------------------------------------------------------------------------------------------*/
        return _configuration;
      }

    }

    /*==========================================================================================================================
    | PROPERTY: DATA PROVIDER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the Topics data provider as set to the defaultDataProvider attribute on the <topics /> configuration section.
    /// </summary>
    /// <exception cref="Exception">
    ///   The defaultDataProvider value is not available from the Topics configuration section (<topics />).
    /// </exception>
    public static TopicRepositoryBase DataProvider {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Establish requirements
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Ensures(Contract.Result<TopicRepositoryBase>() != null);

        /*----------------------------------------------------------------------------------------------------------------------
        | Retrieve default data provider setting
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_defaultDataProvider == null) {
          _defaultDataProvider = DataProviders[Configuration.DefaultDataProvider];
        }

        if (_defaultDataProvider == null) {
          throw new Exception("The defaultDataProvider value is not available from the Topics configuration section (<topics />).");
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return data
        \---------------------------------------------------------------------------------------------------------------------*/
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
        | Establish requirements
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Ensures(Contract.Result<TopicDataProviderCollection>() != null);

        /*----------------------------------------------------------------------------------------------------------------------
        | Instantiate data providers
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_dataProviders == null) {
          _dataProviders = new TopicDataProviderCollection();
          ProvidersHelper.InstantiateProviders(Configuration.DataProviders, _dataProviders, typeof(TopicRepositoryBase));
          _dataProviders.SetReadOnly();
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return data
        \---------------------------------------------------------------------------------------------------------------------*/
        return _dataProviders;

      }
    }

  } // Class

} // Namespace

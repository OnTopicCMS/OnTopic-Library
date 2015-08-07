namespace Ignia.Topics.Configuration {

/*==============================================================================================================================
| TOPIC DATA PROVIDER MANAGER
|
| Author        Katherine Trunkey, Ignia LLC (katherine.trunkey@ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides initialization of the specified Topic data provider based on retrieving the appropriate data from the
|               custom configuration.
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
  using System.Collections.Generic;
  using System.Configuration;
  using System.Linq;
  using System.Text;
  using System.Web.Configuration;

/*==============================================================================================================================
| CLASS
\-----------------------------------------------------------------------------------------------------------------------------*/
  public class TopicDataProviderManager {

  /*============================================================================================================================
  | PRIVATE MEMBERS
  \---------------------------------------------------------------------------------------------------------------------------*/
    private     static  TopicDataProviderBase           _defaultDataProvider    = null;
    private     static  TopicDataProviderCollection     _dataProviders          = null;

  /*============================================================================================================================
  | STATIC CONSTRUCTOR
  >=============================================================================================================================
  | Constructor that also fires the data provider initialization method.
  \---------------------------------------------------------------------------------------------------------------------------*/
    static TopicDataProviderManager() {
      Initialize();
      }

  /*============================================================================================================================
  | METHOD: INITIALIZE
  >=============================================================================================================================
  | Initializes the TopicDataProviderBase and TopicDataProviderCollection classes as configured in the web.config. As part of
  | this, each data provider is initialized using the values defined in the web.config.
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
    | INSTANTIATE DATA PROVIDERS
    \-------------------------------------------------------------------------------------------------------------------------*/
      _dataProviders                                    = new TopicDataProviderCollection();

      ProvidersHelper.InstantiateProviders(topicsConfiguration.DataProviders, _dataProviders, typeof(TopicDataProviderBase));

    /*--------------------------------------------------------------------------------------------------------------------------
    | SET DATA PROVIDERS TO READ-ONLY
    \-------------------------------------------------------------------------------------------------------------------------*/
      _dataProviders.SetReadOnly();

    /*--------------------------------------------------------------------------------------------------------------------------
    | RETRIEVE DEFAULT DATA PROVIDER SETTING
    \-------------------------------------------------------------------------------------------------------------------------*/
      _defaultDataProvider                              = _dataProviders[topicsConfiguration.DefaultDataProvider];

      if (_defaultDataProvider == null) {
        throw new Exception("The defaultDataProvider value is not available from the Topics configuration section (<topics />)");
        }

      }

  /*============================================================================================================================
  | PROPERTY: DATA PROVIDER
  >=============================================================================================================================
  | Returns the Topics data provider as set to the defaultDataProvider attribute on the <topics /> configuration section.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static TopicDataProviderBase DataProvider {
      get {
        return _defaultDataProvider;
        }
      }

  /*============================================================================================================================
  | PROPERTY: DATA PROVIDERS
  >=============================================================================================================================
  | Returns the collection of Topics data providers as configured in the dataProviders element in the <topics /> configuration
  | section.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static TopicDataProviderCollection DataProviders {
      get {
        return _dataProviders;
        }
      }

    }
  }
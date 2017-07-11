/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Configuration;
using Ignia.Topics.Repositories;
using System.Diagnostics.Contracts;
using System.Configuration;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: TOPIC REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The Topic Repository class provides access to a cached collection of Topic trees to support systems where we may want
  ///   to implement multiple taxonomies for different purposes.
  /// </summary>
  public static class TopicRepository {

    /*============================================================================================================================
    | PRIVATE VARIABLES
    \---------------------------------------------------------------------------------------------------------------------------*/
    private static      TopicsSection                   _configuration          = null;
    private static      ITopicRepository                _topicRepository           = null;
    private static      Topic                           _rootTopic              = null;
    private static      ContentTypeCollection           _contentTypes           = null;

    /*==========================================================================================================================
    | CONTENT TYPES
    >===========================================================================================================================
    | ###TODO JJC092813: Need to identify a way of handling cache dependencies and/or recycling of ContentTypes based on
    | changes.
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a list of available <see cref="ContentType"/> objects from the Configuration.
    /// </summary>
    public static ContentTypeCollection ContentTypes {
      get {

        /*------------------------------------------------------------------------------------------------------------------------
        | Validate return value
        \-----------------------------------------------------------------------------------------------------------------------*/
        Contract.Ensures(Contract.Result<ContentTypeCollection>() != null);

        if (_contentTypes == null) {
          _contentTypes                 = new ContentTypeCollection();

          /*--------------------------------------------------------------------------------------------------------------------
          | Ensure root topic is available for use with supporting methods
          \-------------------------------------------------------------------------------------------------------------------*/
          if (RootTopic == null) throw new Exception("Root topic is null");

          /*--------------------------------------------------------------------------------------------------------------------
          | Ensure the parent ContentTypes topic is available to iterate over
          \-------------------------------------------------------------------------------------------------------------------*/
          if (RootTopic.GetTopic("Configuration:ContentTypes") == null) throw new Exception ("Configuration:ContentTypes");

          /*--------------------------------------------------------------------------------------------------------------------
          | Add available Content Types to the collection
          \-------------------------------------------------------------------------------------------------------------------*/
          foreach (Topic topic in RootTopic.GetTopic("Configuration:ContentTypes").FindAllByAttribute("ContentType", "ContentType")) {
            // Ensure the Topic is used as the strongly-typed ContentType
            ContentType contentType     = topic as ContentType;
            // Add ContentType Topic to collection if not already added
            if (contentType != null && !_contentTypes.Contains(contentType.Key)) {
              _contentTypes.Add(contentType);
            }
          }

        }
        return _contentTypes;
      }
      set {
        _contentTypes = value;
      }
    }

    /*==========================================================================================================================
    | ROOT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Static reference to the root <see cref="Topic"/>.
    /// </summary>
    public static Topic RootTopic {
      get {
        Contract.Ensures(Contract.Result<Topic>() != null);
        if (_rootTopic == null) {
          _rootTopic = DataProvider.Load("", -1);
        }
        return _rootTopic;
      }
      set {
        _rootTopic = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: CONFIGURATION SECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the current configuration for the application from the web.config file. 
    /// </summary>
    /// <remarks>
    ///   The web.config stores information such as data providers, views directory, etc. 
    /// </remarks>
    public static TopicsSection Configuration {
      get {
        if (_configuration == null) {
          _configuration = (TopicsSection)ConfigurationManager.GetSection("archive");
          if (_configuration == null) {
            throw new ConfigurationErrorsException("The Topics configuration section (<topics/>) is not set correctly.");
          }
        }
        return _configuration;
      }
    }

    /*==========================================================================================================================
    | TOPIC PROVIDER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the currently configured data provider for topics data.
    /// </summary>
    /// <remarks>
    ///   Pulled from the implementing website's configuration via the <see
    ///   cref="Ignia.Topics.Configuration.TopicDataProviderManager"/>.
    /// </remarks>
    public static ITopicRepository DataProvider {
      get {
        Contract.Ensures(Contract.Result<ITopicRepository>() != null);
        if (_topicRepository == null) {
          _topicRepository = TopicDataProviderManager.DataProvider as ITopicRepository;
        }
        return _topicRepository;
      }
      set {
        _topicRepository = value;
      }
    }

  } // Class

} // Namespace


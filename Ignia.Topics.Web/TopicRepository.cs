/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Configuration;
using Ignia.Topics.Diagnostics;
using Ignia.Topics.Collections;
using Ignia.Topics.Repositories;
using Ignia.Topics.Web.Configuration;

namespace Ignia.Topics.Web {

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
    private static              TopicsSection                   _configuration                  = null;
    private static              ITopicRepository                _topicRepository                = null;
    private static              Topic                           _rootTopic                      = null;
    private static              ContentTypeDescriptorCollection _contentTypes                   = null;

    /*==========================================================================================================================
    | CONTENT TYPES
    >===========================================================================================================================
    | ###TODO JJC092813: Need to identify a way of handling cache dependencies and/or recycling of ContentTypes based on
    | changes.
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a list of available <see cref="ContentTypeDescriptor"/> objects from the Configuration.
    /// </summary>
    [Obsolete("The TopicRepository class is obsolete, as is the ContentTypes property. Instead, clients should use Dependency Injection with the ITopicRepository interface.", false)]
    public static ContentTypeDescriptorCollection ContentTypes {
      get {
        if (_contentTypes != null) {
          return _contentTypes;
        }
        return DataProvider.GetContentTypeDescriptors();
      }
    }

    /*==========================================================================================================================
    | ROOT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Static reference to the root <see cref="Topic"/>.
    /// </summary>
    [Obsolete("The TopicRepository class is obsolete, as is the RootTopic property. Instead, clients should use Dependency Injection with the ITopicRepository interface.", false)]
    public static Topic RootTopic {
      get {
        if (_rootTopic == null) {
          _rootTopic = DataProvider.Load();
        }
        return _rootTopic;
      }
      set => _rootTopic = value?? throw new NullReferenceException("The root element could not be successfully loaded.");
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
    [Obsolete("The TopicRepository class is obsolete, as is the Configuration property. Instead, clients should use Dependency Injection with the ITopicRepository interface.", false)]
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
    [Obsolete("The TopicRepository class is obsolete, as is the TopicProviders property. Instead, clients should use Dependency Injection with the ITopicRepository interface.", false)]
    public static ITopicRepository DataProvider {
      get {
        if (_topicRepository == null) {
          throw new Exception("The TopicRepository has not been configured with an instance of ITopicProvider. Configure the application by setting the TopicRepository.DataProvider in e.g. the global.asax file.");
        }
        return _topicRepository;
      }
      set => _topicRepository = value;
    }

  } // Class

} // Namespace


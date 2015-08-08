/*==============================================================================================================================
| Author        Casey Margell, Ignia LLC (casey.margell@ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       The Topic Repository object provides access to a cached collection of Topic trees to support systems where we
|               may want to implement multiple taxonomies for different purposes.
|
\=============================================================================================================================*/
using System;
using Ignia.Topics.Configuration;
using Ignia.Topics.Providers;

namespace Ignia.Topics {

  /*==============================================================================================================================
  | CLASS: TOPIC REPOSITORY
  \-----------------------------------------------------------------------------------------------------------------------------*/
  public static class TopicRepository {

  /*============================================================================================================================
  | DECLARE PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
    private static      TopicDataProviderBase           _dataProvider           = null;
    private static      TopicMappingProviderBase        _mappingProvider        = null;
    private static      Topic                           _rootTopic              = null;
    private static      ContentTypeCollection           _contentTypes           = null;

  /*============================================================================================================================
  | CONTENT TYPES
  >=============================================================================================================================
  | Get a list of available Content Types from the Configuration.
  >-----------------------------------------------------------------------------------------------------------------------------
  | ###TODO JJC092813: Need to identify a way of handling cache dependencies and/or recycling of ContentTypes based on changes.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static ContentTypeCollection ContentTypes {
      get {
        if (_contentTypes == null) {
          _contentTypes                 = new ContentTypeCollection();

        /*----------------------------------------------------------------------------------------------------------------------
        | MAKE SURE ROOT TOPIC IS AVAILABLE FOR USE WITH SUPPORTING METHODS
        \---------------------------------------------------------------------------------------------------------------------*/
          if (RootTopic == null) throw new Exception("Root topic is null");

        /*----------------------------------------------------------------------------------------------------------------------
        | MAKE SURE THE PARENT CONTENTTYPES TOPIC IS AVAILABLE TO ITERATE OVER
        \---------------------------------------------------------------------------------------------------------------------*/
          if (RootTopic.GetTopic("Configuration:ContentTypes") == null) throw new Exception ("Configuration:ContentTypes");

        /*----------------------------------------------------------------------------------------------------------------------
        | ADD AVAILABLE CONTENT TYPES TO THE COLLECTION
        \---------------------------------------------------------------------------------------------------------------------*/
          foreach (Topic topic in RootTopic.GetTopic("Configuration:ContentTypes").FindAllByAttribute("ContentType", "ContentType", true)) {
          //Make sure the Topic is used as the strongly-typed ContentType
            ContentType contentType     = topic as ContentType;
          //Add ContentType Topic to collection if not already added
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

  /*============================================================================================================================
  | ROOT TOPIC
  >=============================================================================================================================
  | Static reference to the root Topic
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static Topic RootTopic {
      get {
        if (_rootTopic == null) {
          _rootTopic = Topic.Load();
        }
        return _rootTopic;
      }
      set {
        _rootTopic = value;
      }
    }

  /*============================================================================================================================
  | TOPIC PROVIDER
  >=============================================================================================================================
  | Getter for the data provider that we're using. Pulled from the Ignia.Topics.Configuration TopicDataProviderManager.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static TopicDataProviderBase DataProvider {
      get {
        if (_dataProvider == null) {
          _dataProvider = TopicDataProviderManager.DataProvider;
          MappingProvider.DataProvider = _dataProvider;
        }
        return _dataProvider;
      }
      set {
        _dataProvider = value;
      }
    }

  /*============================================================================================================================
  | MAPPING PROVIDER
  >=============================================================================================================================
  | Getter for the data provider that we're using. Pulled from the Ignia.Topics.Configuration TopicMappingProviderManager.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static TopicMappingProviderBase MappingProvider {
      get {
        if (_mappingProvider == null) {
          _mappingProvider = TopicMappingProviderManager.MappingProvider;
        }
        return _mappingProvider;
      }
    }

  /*============================================================================================================================
  | METHOD: LOAD
  >=============================================================================================================================
  | Static method that passes Load requests along to the current topic data provider.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static Topic Load(string topic, int depth = 0, DateTime? version = null) {
      return DataProvider.Load(topic, depth, version);
    }

    public static Topic Load(int topicId, int depth = 0, DateTime? version = null) {
      return DataProvider.Load(topicId, depth, version);
    }

  /*============================================================================================================================
  | METHOD: SAVE
  >=============================================================================================================================
  | Static method that passes Save requests along to the current topic data provider.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static int Save(Topic topic, bool isRecursive, bool isDraft = false) {
      return DataProvider.Save(topic, isRecursive, isDraft);
    }

  /*============================================================================================================================
  | METHOD: MOVE
  >=============================================================================================================================
  | Static method that passes Move requests along to the current topic data provider.
  | For ordering: Optionally accepts the sibling topic to place topic behind.  Defaults
  | to placing topic in front of existing siblings.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static bool Move(Topic topic, Topic target) {
      ReorderSiblings(topic);
      return DataProvider.Move(topic, target);
    }

    public static bool Move(Topic topic, Topic target, Topic sibling) {
      ReorderSiblings(topic, sibling);
      return DataProvider.Move(topic, target, sibling);
    }

    private static void ReorderSiblings(Topic source) {
      ReorderSiblings(source, null);
    }

    private static void ReorderSiblings(Topic source, Topic sibling) {

      Topic   parent          = source.Parent;
      int     sortOrder       = -1;

    //If there is no sibling, inject the source at the beginning of the collection
      if (sibling == null) {
        source.SortOrder = sortOrder++;
      }

    //Loop through each topic to assign a new priority order
      foreach (Topic topic in parent.SortedChildren) {
      //Assuming the topic isn't the source, increment the sortOrder
        if (topic != source) {
          topic.SortOrder = sortOrder++;
        }
      //If the topic is the sibling, then assign the next sortOrder to the source
        if (topic == sibling) {
          source.SortOrder = sortOrder++;
        }
      }

    }

  /*============================================================================================================================
  | METHOD: DELETE
  >=============================================================================================================================
  | Static method that passes Delete requests along to the current topic data provider.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static void Delete(Topic topic) {
      Delete(topic, true);
    }

    public static void Delete(Topic topic, bool isRecursive) {
      DataProvider.Delete(topic, isRecursive);
    }

  } // Class

} //Namespace


namespace Ignia.Topics {

/*=============================================================================================================================
| FILE PATH
|
| Author        Katherine Trunkey, Ignia LLC (Katherine.Trunkey@Ignia.com)
| Client        Ignia, LLC
| Project       OnTopic
|
| Purpose       Provides a strongly-typed class associated with the FilePath.ascx Attribute Type control and locic associated
|               with building a configured file path from values passed to the constructor.
|
>==============================================================================================================================
| Revisions     Date            Author                  Comments
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
|               10.05.14        Katherine Trunkey       Initial version created.
\----------------------------------------------------------------------------------------------------------------------------*/

/*=============================================================================================================================
| DEFINE ASSEMBLY ATTRIBUTES
>==============================================================================================================================
| Declare and define attributes used to compile the finished assembly.
\----------------------------------------------------------------------------------------------------------------------------*/
  using System;
  using System.Linq;
  using System.Web;

/*=============================================================================================================================
| CLASS
\----------------------------------------------------------------------------------------------------------------------------*/
  public class FilePath {

  /*===========================================================================================================================
  | DECLARE PRIVATE VARIABLES
  \--------------------------------------------------------------------------------------------------------------------------*/

  /*===========================================================================================================================
  | CONSTRUCTOR
  \--------------------------------------------------------------------------------------------------------------------------*/
    public FilePath() { }

  /*===========================================================================================================================
  | GET PATH
  >============================================================================================================================
  | Static helper method that returns a constructed file path based on evaluation and processing of the parameter
  | values/settings passed to the method.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public static string GetPath(
      Topic     topic,
      string    attributeKey,
      bool      includeLeafTopic        = true,
      string[]  truncatePathAtTopic     = null
      ) {

    /*-------------------------------------------------------------------------------------------------------------------------
    | ONLY PROCESS THE PATH IF BOTH TOPIC AND ATTRIBUTEKEY ARE PROVIDED
    \------------------------------------------------------------------------------------------------------------------------*/
      if (topic == null || String.IsNullOrEmpty(attributeKey)) return "";

    /*-------------------------------------------------------------------------------------------------------------------------
    | BUILD CONFIGURED FILE PATH STRING BASED ON VALUES AND SETTINGS PARAMETERS PASSED TO THE METHOD
    \------------------------------------------------------------------------------------------------------------------------*/
      string    filePath                = "";
      string    relativePath            = null;
      Topic     startTopic              = topic;
      Topic     endTopic                = includeLeafTopic? topic : topic.Parent;

    /*-------------------------------------------------------------------------------------------------------------------------
    | CRAWL UP THE TOPICS TREE TO FIND FILE PATH VALUES SET AT A HIGHER LEVEL
    \------------------------------------------------------------------------------------------------------------------------*/
      while (String.IsNullOrEmpty(filePath) && startTopic != null) {
        startTopic                      = startTopic.Parent;
        if (startTopic != null && !String.IsNullOrEmpty(attributeKey)) {
          filePath                      = startTopic.GetAttribute(attributeKey);
          }
        }

    /*-------------------------------------------------------------------------------------------------------------------------
    | ADD TOPIC KEYS (DIRECTORY NAMES) BETWEEN THE START TOPIC AND END TOPIC BASED ON THE TOPICS' WEBPATH PROPERTIES
    \------------------------------------------------------------------------------------------------------------------------*/
      relativePath                      = endTopic.WebPath.Substring(startTopic.WebPath.Length);

    /*-------------------------------------------------------------------------------------------------------------------------
    | PERFORM PATH TRUNCATION BASED ON TOPICS INCLUDED IN TRUNCATEPATHATTOPIC
    \------------------------------------------------------------------------------------------------------------------------*/
      if (truncatePathAtTopic != null) {
        foreach (string truncationTopic in truncatePathAtTopic) {
          int truncateTopicLocation     = relativePath.IndexOf(truncationTopic, StringComparison.InvariantCultureIgnoreCase);
          if (truncateTopicLocation >= 0) {
            relativePath                = relativePath.Substring(0, truncateTopicLocation + truncationTopic.Length + 1);
            }
          }
        }

    /*-------------------------------------------------------------------------------------------------------------------------
    | ADD RESULTING RELATIVE PATH TO THE ORIGINAL FILE PATH (BASED ON STARTING TOPIC)
    \------------------------------------------------------------------------------------------------------------------------*/
      filePath                         += relativePath;

    /*-------------------------------------------------------------------------------------------------------------------------
    | REPLACE PATH SLASHES WITH BACKSLASHES IF THE RESULTING FILE PATH VALUE USES A UNC OR BASIC FILE PATH FORMAT
    \------------------------------------------------------------------------------------------------------------------------*/
      if (filePath.IndexOf("\\") >= 0) {
        filePath                        = filePath.Replace("/", "\\");
        }

    /*-------------------------------------------------------------------------------------------------------------------------
    | RETURN RESULTING FILE PATH
    \------------------------------------------------------------------------------------------------------------------------*/
      return filePath;
      }

    } //Class

  } //Namespace
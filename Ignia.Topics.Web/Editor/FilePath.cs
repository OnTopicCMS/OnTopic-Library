/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Diagnostics;

namespace Ignia.Topics.Web.Editor {

  /*============================================================================================================================
  | CLASS: FILE PATH
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed class associated with the FilePath.ascx Attribute Type control and logic associated with
  ///   building a configured file path from values passed to the constructor.
  /// </summary>
  public static class FilePath {

    /*==========================================================================================================================
    | METHOD: GET PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Static helper method that returns a constructed file path based on evaluation and processing of the parameter
    ///   values/settings passed to the method.
    /// </summary>
    /// <param name="topic">The topic object.</param>
    /// <param name="attributeKey">The attribute key.</param>
    /// <param name="includeLeafTopic">Boolean indicator as to whether to include the endpoint/leaf topic in the path.</param>
    /// <param name="truncatePathAtTopic">The assembled topic keys at which to end the path string.</param>
    /// <returns>A constructed file path.</returns>
    public static string GetPath(
      Topic     topic,
      string    attributeKey,
      bool      includeLeafTopic        = true,
      string[]  truncatePathAtTopic     = null
      ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Only process the path if both topic and attribtueKey are provided
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null || String.IsNullOrEmpty(attributeKey)) return "";

      /*------------------------------------------------------------------------------------------------------------------------
      | Build configured file path string base on values and settings parameters passed to the method
      \-----------------------------------------------------------------------------------------------------------------------*/
      var       filePath                = "";
      var       relativePath            = (string)null;
      var       startTopic              = topic;
      var       endTopic                = includeLeafTopic? topic : topic.Parent;

      /*------------------------------------------------------------------------------------------------------------------------
      | Crawl up the topics tree to find file path values set at a higher level
      \-----------------------------------------------------------------------------------------------------------------------*/
      while (String.IsNullOrEmpty(filePath) && startTopic != null) {
        startTopic                      = startTopic.Parent;
        if (startTopic != null && !String.IsNullOrEmpty(attributeKey)) {
          filePath                      = startTopic.Attributes.GetValue(attributeKey);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Add topic keys (directory names) between the start topic and the end topic based on the topic's WebPath property
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (startTopic != null) {
        Contract.Assume(
          startTopic.GetWebPath().Length <= endTopic.GetWebPath().Length,
          "Assumes the startTopic path length is shorter than the endTopic path length."
          );
        relativePath                    = endTopic.GetWebPath().Substring(startTopic.GetWebPath().Length);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Perform path truncation based on topics included in TruncatePathAtTopic
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (truncatePathAtTopic != null) {
        foreach (var truncationTopic in truncatePathAtTopic) {
          var truncateTopicLocation     = relativePath.IndexOf(truncationTopic, StringComparison.InvariantCultureIgnoreCase);
          if (truncateTopicLocation >= 0) {
            relativePath                = relativePath.Substring(0, truncateTopicLocation + truncationTopic.Length + 1);
          }
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Add resulting relative path to the original file path (based on starting topic)
      \-----------------------------------------------------------------------------------------------------------------------*/
      filePath                         += relativePath;

      /*------------------------------------------------------------------------------------------------------------------------
      | Replace path slashes with backslashes if the resulting file path value uses a UNC or basic file path format
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (filePath.IndexOf("\\") >= 0) {
        filePath                        = filePath.Replace("/", "\\");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return resulting file path
      \-----------------------------------------------------------------------------------------------------------------------*/
      return filePath;

    }

  } // Class

} // Namespace
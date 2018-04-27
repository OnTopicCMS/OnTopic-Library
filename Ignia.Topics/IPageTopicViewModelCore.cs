/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ignia.Topics.Mapping;

namespace Ignia.Topics {

  /*============================================================================================================================
  | INTERFACE: PAGE TOPIC VIEW MODEL CORE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for feeding views based on the page content type.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     It is not strictly required that topic view models implement the <see cref="IPageTopicViewModelCore"/> interface for
  ///     the default <see cref="TopicMappingService"/> to correctly identify and map <see cref="Topic"/>s to topic view models.
  ///     That said, the interface does define properties that presentation infrastructure may expect. As a result, if it isn't
  ///     provided via the public interface then it will instead need to be defined in some other way.
  ///   </para>
  /// </remarks>
  public interface IPageTopicViewModelCore : ITopicViewModelCore {

    /*==========================================================================================================================
    | PROPERTY: WEB PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Web Path attribute, representing the URL representation of the path to the associated topic.
    /// </summary>
    string WebPath { get; set; }

    /*==========================================================================================================================
    | PROPERTY: TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Title attribute, which represents the friendly name of the topic.
    /// </summary>
    /// <remarks>
    ///   While the <see cref="ITopicViewModelCore.Key"/> may not contain, for instance, spaces or symbols, there are no
    ///   restrictions on what characters can be used in the title. For this reason, it provides the default public value for
    ///   referencing topics.
    /// </remarks>
    /// <requires description="The value from the getter must be provided." exception="T:System.ArgumentNullException">
    ///   !string.IsNullOrWhiteSpace(value)
    /// </requires>
    string Title { get; set; }

    /*==========================================================================================================================
    | PROPERTY: META KEYWORDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Meta Keywords attribute, which represents the HTML metadata that will be presented alongside the
    ///   page.
    /// </summary>
    string MetaKeywords { get; set; }

    /*==========================================================================================================================
    | PROPERTY: META DESCRIPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Meta Description attribute, which represents the HTML metadata that will be presented alongside the
    ///   page.
    /// </summary>
    string MetaDescription { get; set; }

  } //Class
} //Namespace

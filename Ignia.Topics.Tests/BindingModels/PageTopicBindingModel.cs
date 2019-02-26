/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ignia.Topics.Mapping;
using Ignia.Topics.Models;

namespace Ignia.Topics.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: PAGE TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a minimal implementation of a custom topic binding model with a couple of scalar values mapping to properties
  ///   on the <c>Page</c> content type.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class PageTopicBindingModel : BasicTopicBindingModel {

    public PageTopicBindingModel(string key = null) : base(key, "Page") { }

    public string Title { get; set; }

    [AttributeKey("MetaTitle")]
    public string BrowserTitle { get; set; }

    public bool IsHidden { get; set; }

  } //Class

} //Namespace

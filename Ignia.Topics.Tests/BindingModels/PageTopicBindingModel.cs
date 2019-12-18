/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ignia.Topics.Mapping.Annotations;

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

    public PageTopicBindingModel(string? key = null) : base(key, "Page") { }

    [Required]
    public string? Title { get; set; }

    [AttributeKey("MetaTitle")]
    public string? BrowserTitle { get; set; }

    [DefaultValue("Default page description")]
    public string? MetaDescription { get; set; }

    public bool IsHidden { get; set; }

  } //Class
} //Namespace
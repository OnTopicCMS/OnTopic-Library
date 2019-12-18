/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;
using Ignia.Topics.Models;

namespace Ignia.Topics.Tests.BindingModels {

  /*============================================================================================================================
  | VIEW MODEL: DEFAULT VALUE TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing views properties annotated with default value attributes.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class BasicTopicBindingModel : ITopicBindingModel {

    public BasicTopicBindingModel() { }

    public BasicTopicBindingModel(string? key, string? contentType) {
      Key = key;
      ContentType = contentType;
    }

    public string? Key { get; set; }

    [Required]
    public string? ContentType { get; set; }

  } //Class
} //Namespace
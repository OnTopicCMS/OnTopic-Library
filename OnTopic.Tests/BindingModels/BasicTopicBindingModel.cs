/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;
using OnTopic.Models;

namespace OnTopic.Tests.BindingModels {

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

    public BasicTopicBindingModel(string key, string contentType) {
      Key = key;
      ContentType = contentType;
    }

    [Required]
    public string Key { get; init; } = default!;

    [Required]
    public string ContentType { get; init; } = default!;

  } //Class
} //Namespace
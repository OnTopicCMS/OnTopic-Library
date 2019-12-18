/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;

namespace Ignia.Topics.Tests.BindingModels {

  /*============================================================================================================================
  | VIEW MODEL: MINIMUM VALUE PROPERTY TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing view properties annotated with minimum value attributes.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class MinimumLengthPropertyTopicBindingModel : BasicTopicBindingModel {

    public MinimumLengthPropertyTopicBindingModel(string? key = null) : base(key, "Page") { }

    [MinLength(13)]
    public string? Title { get; set; }

  } //Class
} //Namespace
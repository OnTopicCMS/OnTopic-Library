/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using OnTopic.Models;

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: RECORD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed binding model based on a C# 9.0 <c>record</c> data type to ensure that it can be properly
  ///   mapped from.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class RecordTopicBindingModel : ITopicBindingModel {

    public RecordTopicBindingModel() { }

    [Required]
    public string Key { get; init; } = default!;

    [Required]
    public string ContentType { get; init; } = default!;

  } //Class
} //Namespace
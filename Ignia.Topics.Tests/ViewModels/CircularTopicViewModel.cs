/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ignia.Topics.Mapping;

namespace Ignia.Topics.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: CIRCULAR TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing views with a circular reference.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class CircularTopicViewModel {

    [Recurse(Relationships.Parents)]
    public CircularTopicViewModel Parent { get; set; }

    [Recurse(Relationships.Children | Relationships.Parents)]
    public List<CircularTopicViewModel> Children { get; set; }

  } //Class
} //Namespace
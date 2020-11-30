/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: CHILDREN TOPIC (INVALID)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom binding model with an invalid <see cref="Children"/> property. An <see
  ///   cref="InvalidOperationException"/> should be thrown when it is mapped.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class InvalidChildrenTopicBindingModel : BasicTopicBindingModel {

    public InvalidChildrenTopicBindingModel(string? key = null) : base(key, "Page") { }

    public List<BasicTopicBindingModel> Children { get; } = new();

  } //Class
} //Namespace
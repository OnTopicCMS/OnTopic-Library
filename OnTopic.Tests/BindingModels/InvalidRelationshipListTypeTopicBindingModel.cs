/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using OnTopic.ViewModels.BindingModels;

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: RELATIONSHIP LIST TYPE TOPIC (INVALID)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom binding model with an invalid collection type—i.e., it implements a <see
  ///   cref="Dictionary{TKey, TValue}"/>, even though relationships are expected to return a type implementing <see
  ///   cref="IList"/>. An <see cref="InvalidOperationException"/> should be thrown when it is mapped.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class InvalidRelationshipListTypeTopicBindingModel : BasicTopicBindingModel {

    public InvalidRelationshipListTypeTopicBindingModel(string key) : base(key, "ContentTypeDescriptor") { }

    public Dictionary<string, AssociatedTopicBindingModel> ContentTypes { get; } = new();

  } //Class
} //Namespace
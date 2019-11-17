/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ignia.Topics.Mapping.Annotations;
using Ignia.Topics.Models;

namespace Ignia.Topics.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: MAP TO PARENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a minimal implementation of a custom topic binding model with mapped complex properties.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class MapToParentTopicBindingModel : BasicTopicBindingModel {

    [MapToParent(AttributePrefix="")]
    public ContactTopicBindingModel PrimaryContact { get; set; } = new ContactTopicBindingModel();

    [MapToParent(AttributePrefix="Alternate")]
    public ContactTopicBindingModel AlternateContact { get; set; } = new ContactTopicBindingModel();

    [MapToParent]
    public ContactTopicBindingModel BillingContact { get; set; } = new ContactTopicBindingModel();

  } //Class
} //Namespace

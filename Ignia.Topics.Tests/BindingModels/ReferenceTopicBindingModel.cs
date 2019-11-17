/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ignia.Topics.Mapping.Annotations;
using Ignia.Topics.Models;

namespace Ignia.Topics.Tests.BindingModels {

  /*============================================================================================================================
  | VIEW MODEL: REFERENCE TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing a property associated with a topic pointer—i.e. a reference
  ///   to another topic.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class ReferenceTopicBindingModel : BasicTopicBindingModel {

    public ReferenceTopicBindingModel(string key) : base(key, "AttributeDescriptor") {}

    [AttributeKey("TopicId")]
    public RelatedTopicBindingModel? DerivedTopic { get; set; }

  } //Class
} //Namespace

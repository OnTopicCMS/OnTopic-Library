/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: CONSTRUCTED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a topic view model with a constructor containing a variety of parameters—including parameters with attributes,
  ///   and representing each of the main types of attribute (e.g., scalar values, relationships, references, etc.).
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class ConstructedTopicViewModel {

    public ConstructedTopicViewModel(
      [AttributeKey("Value")]string scalarValue,
      [Include(AssociationTypes.All)]
      ConstructedTopicViewModel? topicReference,
      [Collection("Related")]Collection<ConstructedTopicViewModel>? relationships,
      [DisableMapping]int optionalValue = 5
    ) {
      ScalarValue               = scalarValue;
      TopicReference            = topicReference;
      Relationships             = relationships;
      OptionalValue             = optionalValue;
    }

    public string ScalarValue { get; }
    public ConstructedTopicViewModel? TopicReference { get; }
    public Collection<ConstructedTopicViewModel>? Relationships { get; }
    public int OptionalValue { get; } = 1;

  } //Class
} //Namespace
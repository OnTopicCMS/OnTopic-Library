/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: LOAD TESTING
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple view model with a series of properties that can be used for load testing the <see cref="
  ///   TopicMappingService"/>.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class LoadTestingViewModel: KeyOnlyTopicViewModel {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="LoadTestingViewModel"/> with an <paramref name="attributes"/> dictionary.
    /// </summary>
    /// <param name="attributes">An <see cref="AttributeValueDictionary"/> of attribute values.</param>
    public LoadTestingViewModel(AttributeValueDictionary attributes) {
      Contract.Requires(attributes);
      Property0 = attributes.GetInteger("Property0");
      Property1 = attributes.GetInteger("Property1");
      Property2 = attributes.GetInteger("Property2");
      Property3 = attributes.GetInteger("Property3");
      Property4 = attributes.GetInteger("Property4");
      Property5 = attributes.GetInteger("Property5");
      Property6 = attributes.GetInteger("Property6");
      Property7 = attributes.GetInteger("Property7");
      Property8 = attributes.GetInteger("Property8");
      Property9 = attributes.GetInteger("Property9");
      Property10 = attributes.GetInteger("Property10");
      Property11 = attributes.GetInteger("Property11");
      Property12 = attributes.GetInteger("Property12");
      Property13 = attributes.GetInteger("Property13");
      Property14 = attributes.GetInteger("Property14");
      Property15 = attributes.GetInteger("Property15");
      Property16 = attributes.GetInteger("Property16");
      Property17 = attributes.GetInteger("Property17");
      Property18 = attributes.GetInteger("Property18");
      Property19 = attributes.GetInteger("Property19");
      Property20 = attributes.GetInteger("Property20");
    }

    /// <summary>
    ///   Initializes a new <see cref="PageTopicViewModel"/> with no parameters.
    /// </summary>
    public LoadTestingViewModel() { }

    /*==========================================================================================================================
    | PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    public int? Property0 { get; init; }
    public int? Property1 { get; init; }
    public int? Property2 { get; init; }
    public int? Property3 { get; init; }
    public int? Property4 { get; init; }
    public int? Property5 { get; init; }
    public int? Property6 { get; init; }
    public int? Property7 { get; init; }
    public int? Property8 { get; init; }
    public int? Property9 { get; init; }
    public int? Property10 { get; init; }
    public int? Property11 { get; init; }
    public int? Property12 { get; init; }
    public int? Property13 { get; init; }
    public int? Property14 { get; init; }
    public int? Property15 { get; init; }
    public int? Property16 { get; init; }
    public int? Property17 { get; init; }
    public int? Property18 { get; init; }
    public int? Property19 { get; init; }
    public int? Property20 { get; init; }

  } //Class
} //Namespace
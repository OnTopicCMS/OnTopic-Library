/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: ATTRIBUTE DICTIONARY CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing a constructor with a <see cref="AttributeDictionary"/>.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public record AttributeValueDictionaryConstructorTopicViewModel: PageTopicViewModel {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="AttributeValueDictionaryConstructorTopicViewModel"/> with an <paramref name="attributes"
    ///   /> dictionary.
    /// </summary>
    /// <param name="attributes">An <see cref="AttributeValueDictionaryConstructorTopicViewModel"/> of attribute values.</param>
    public AttributeValueDictionaryConstructorTopicViewModel(AttributeDictionary attributes) : base(attributes) {
      Contract.Requires(attributes, nameof(attributes));
      MappedProperty = attributes.GetValue(nameof(MappedProperty));
    }

    /// <summary>
    ///   Initializes a new <see cref="AttributeValueDictionaryConstructorTopicViewModel"/> with no parameters.
    /// </summary>
    public AttributeValueDictionaryConstructorTopicViewModel() { }

    /*==========================================================================================================================
    | PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    public string? MappedProperty { get; init; }
    public string? UnmappedProperty { get; init; }


  } //Class
} //Namespace
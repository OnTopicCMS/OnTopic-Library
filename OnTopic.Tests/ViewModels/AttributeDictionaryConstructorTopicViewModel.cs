/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels;

/*==============================================================================================================================
| VIEW MODEL: ATTRIBUTE DICTIONARY CONSTRUCTOR
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a strongly-typed data transfer object for testing a constructor with a <see cref="AttributeDictionary"/>.
/// </summary>
/// <remarks>
///   <see cref="MappedProperty"/> and <see cref="UnmappedProperty"/> are decorated with <see cref="DisableMappingAttribute"/>
///   so they can only be populated via the <see cref="AttributeDictionary"/> constructor, not the reflection-based property
///   mapper's fallback pass; this isolates tests to the constructor-dictionary path they're meant to exercise. This is a sample
///   class intended for test purposes only; it is not designed for use in a production environment.
/// </remarks>
public record AttributeDictionaryConstructorTopicViewModel: PageTopicViewModel {

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new <see cref="AttributeDictionaryConstructorTopicViewModel"/> with an <paramref name="attributes"
  ///   /> dictionary.
  /// </summary>
  /// <param name="attributes">An <see cref="AttributeDictionaryConstructorTopicViewModel"/> of attribute values.</param>
  public AttributeDictionaryConstructorTopicViewModel(AttributeDictionary attributes) : base(attributes) {
    Contract.Requires(attributes, nameof(attributes));
    MappedProperty              = attributes.GetValue(nameof(MappedProperty));
  }

  /// <summary>
  ///   Initializes a new <see cref="AttributeDictionaryConstructorTopicViewModel"/> with no parameters.
  /// </summary>
  public AttributeDictionaryConstructorTopicViewModel() { }

  /*============================================================================================================================
  | PROPERTIES
  \---------------------------------------------------------------------------------------------------------------------------*/
  [DisableMapping]
  public string? MappedProperty { get; init; }

  [DisableMapping]
  public string? UnmappedProperty { get; init; }


} //Class
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping;

namespace OnTopic.Tests.ViewModels;

/*==============================================================================================================================
| VIEW MODEL: CIRCULAR CONSTRUCTOR TOPIC
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a strongly typed data transfer object, implemented as a positional <c>record</c>, for testing constructor mapping
///   of a topic reference that may form a circular reference.
/// </summary>
/// <remarks>
///   <para>
///     Unlike <see cref="CircularTopicViewModel"/>, which expresses its circular reference through settable properties, this
///     model maps its <see cref="Self"/> reference through a positional constructor parameter on a record. This allows the <see
///     cref="TopicMappingService"/> to be exercised for two distinct behaviors: A non-cyclic reference should map successfully,
///     while a true self-reference should be detected as a constructor cycle and throw a <see cref="TopicMappingException"/>,
///     since a partially constructed instance cannot be returned from a constructor.
///   </para>
///   <para>
///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
///   </para>
/// </remarks>
/// <param name="Key">The key of the mapped topic.</param>
/// <param name="Self">An optional reference to another <see cref="CircularConstructorTopicViewModel"/>.</param>
public record CircularConstructorTopicViewModel(
  string Key,
  [Include(AssociationTypes.References)] CircularConstructorTopicViewModel? Self
);
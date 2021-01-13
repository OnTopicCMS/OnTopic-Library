/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Collections;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: RELATED ENTITY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple view model with a relationship mapping to actual <see cref="Topic"/> entities.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     View models should only reference collections and basic value objects. That said, for edge cases, the <see
  ///     cref="Mapping.TopicMappingService"/> provides support for mapping actual <see cref="Topic"/> entities. This is not a
  ///     best practice, and should be discouraged when creating view models.
  ///   </para>
  ///   <para>
  ///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  ///   </para>
  /// </remarks>
  public class RelatedEntityTopicViewModel: KeyOnlyTopicViewModel {

    public TopicCollection RelatedTopics { get; } = new();

  } //Class
} //Namespace
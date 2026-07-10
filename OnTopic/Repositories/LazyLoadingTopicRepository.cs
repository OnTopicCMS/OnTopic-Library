/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Associations;

namespace OnTopic.Repositories;

/*==============================================================================================================================
| CLASS: LAZY LOADING TOPIC REPOSITORY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides an abstract base class for centralizing infrastructure for implementations of <see cref="ITopicRepository"/> that
///   support lazy-loading, independent of the underlying persistence store.
/// </summary>
/// <remarks>
///   This sits between <see cref="ObservableTopicRepository"/>, which offers only event handling, and the two families of
///   concrete <see cref="ITopicRepository"/> base classes: <see cref="TopicRepository"/>, for implementations that persist
///   directly to a data store, and <see cref="TopicRepositoryDecorator"/>, for implementations that wrap another <see cref=
///   "ITopicRepository"/>. Both need to stamp topics with an <see cref="ITopicLoadResolver"/> and resolve deferred
///   associations, but neither should be coupled to the other's specific concerns (e.g., <see cref="TopicRepository"/>'s
///   sealed <c>Save()</c>, <c>Move()</c>, and <c>Delete()</c> template methods, which <see cref="TopicRepositoryDecorator"/>
///   must remain free to override for delegation).
/// </remarks>
public abstract class LazyLoadingTopicRepository : ObservableTopicRepository {

} //Class
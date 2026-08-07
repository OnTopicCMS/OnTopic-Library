/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
namespace OnTopic.Repositories;

/*==============================================================================================================================
| INTERFACE: SITEMAP TOPIC REPOSITORY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a narrow, read-only seam for retrieving the minimal <see cref="Topic"/> graph required to render the sitemap,
///   without exposing the full read/write surface of <see cref="ITopicRepository"/>.
/// </summary>
public interface ISitemapTopicRepository {

  /*============================================================================================================================
  | METHOD: LOAD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a detached, lightweight <see cref="Topic"/> graph containing only the fields the sitemap renders. The graph is
  ///   independent of any shared cache and is expected to be discarded once the response is rendered.
  /// </summary>
  Task<Topic> Load();

} //Interface
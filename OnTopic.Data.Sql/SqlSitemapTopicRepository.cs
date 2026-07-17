/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Repositories;

namespace OnTopic.Data.Sql;

/*==============================================================================================================================
| CLASS: SQL SITEMAP TOPIC REPOSITORY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides data access to the minimal <see cref="Topic"/> graph required to render the sitemap, sourced from Microsoft SQL
///   Server.
/// </summary>
/// <remarks>
///   Concrete implementation of the <see cref="ISitemapTopicRepository"/> interface. Unlike <see cref="SqlTopicRepository"/>,
///   <see cref="Load"/> accepts no <c>referenceTopic</c> to merge into, raises no <see cref="ITopicRepository.TopicLoaded"/>
///   event, and stamps no <see cref="ITopicLazyLoader"/>: Each call returns an entirely fresh, detached graph, with no
///   relationship to any other topic graph in memory, intended to be discarded once the response is rendered. Caching can be
///   done at the controller level of the rendered XML.
/// </remarks>
public class SqlSitemapTopicRepository : ISitemapTopicRepository {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              string                          _connectionString;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Instantiates a new instance of the <see cref="SqlSitemapTopicRepository"/> with a dependency on a connection string to
  ///   provide necessary access to a SQL database.
  /// </summary>
  /// <param name="connectionString">A connection string to a SQL server that contains the Topics database.</param>
  /// <returns>A new instance of the <see cref="SqlSitemapTopicRepository"/>.</returns>
  public SqlSitemapTopicRepository(string connectionString) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(connectionString), nameof(connectionString));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Set private fields
    \-------------------------------------------------------------------------------------------------------------------------*/
    _connectionString           = connectionString;

  }

  /*============================================================================================================================
  | METHOD: LOAD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  public async Task<Topic> Load() {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish database connection
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topic                   = (Topic?)null;

    using var connection        = new SqlConnection(_connectionString);
    using var command           = new SqlCommand("GetSitemap", connection) {
      CommandType               = CommandType.StoredProcedure,
      CommandTimeout            = 120
    };

    /*--------------------------------------------------------------------------------------------------------------------------
    | Process database query
    \-------------------------------------------------------------------------------------------------------------------------*/
    try {
      await connection.OpenAsync().ConfigureAwait(false);
      using var reader          = (SqlDataReader)await command.ExecuteReaderAsync().ConfigureAwait(false);
      topic                     = await reader.LoadTopicGraph(referenceTopic: null, markDirty: false).ConfigureAwait(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Catch exception
    \-------------------------------------------------------------------------------------------------------------------------*/
    catch (SqlException exception) {
      throw new TopicRepositoryException($"Topics failed to load: '{exception.Message}'", exception);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate results
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Assume(
      topic,
      "The 'GetSitemap' stored procedure did not return a topic graph."
    );

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return objects
    \-------------------------------------------------------------------------------------------------------------------------*/
    return topic;

  }

} //Class
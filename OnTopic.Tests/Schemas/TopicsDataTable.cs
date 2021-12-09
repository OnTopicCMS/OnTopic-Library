/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Data;
using OnTopic.Data.Sql;

namespace OnTopic.Tests.Schemas {

  /*============================================================================================================================
  | CLASS: TOPICS DATA TABLE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a <see cref="DataTable"/> which maps to the expected schema of the <c>Topics</c> table.
  /// </summary>
  /// <remarks>
  ///   This allows testing of the <see cref="SqlTopicRepository"/> via its <see cref="SqlDataReaderExtensions"/> methods.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  public class TopicsDataTable: DataTable {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the <see cref="TopicsDataTable"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="TopicsDataTable"/>.</returns>
    public TopicsDataTable() : base("Topics") {

      /*------------------------------------------------------------------------------------------------------------------------
      | Add TopicId column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(int),
        ColumnName              = "TopicId",
        Unique                  = true
      });

      /*------------------------------------------------------------------------------------------------------------------------
      | Add TopicKey column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(string),
        ColumnName              = "TopicKey"
      });

      /*------------------------------------------------------------------------------------------------------------------------
      | Add ContentType column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(string),
        ColumnName              = "ContentType"
      });

      /*------------------------------------------------------------------------------------------------------------------------
      | Add ParentId column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(int),
        ColumnName              = "ParentId",
        AllowDBNull             = true
      });

    }

    /*==========================================================================================================================
    | ADD ROW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a new <see cref="DataRow"/> to the <see cref="TopicsDataTable"/>.
    /// </summary>
    public void AddRow(int topicId, string topicKey, string contentType, int? parentId = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Verify parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicId, nameof(topicId));
      Contract.Requires(topicKey, nameof(topicKey));
      Contract.Requires(contentType, nameof(contentType));

      /*------------------------------------------------------------------------------------------------------------------------
      | Create new row
      \-----------------------------------------------------------------------------------------------------------------------*/
      var row = NewRow();

      row["TopicId"]            = topicId;
      row["TopicKey"]           = topicKey;
      row["ContentType"]        = contentType;
      row["ParentId"]           = parentId.HasValue? (object)parentId : DBNull.Value;

      /*------------------------------------------------------------------------------------------------------------------------
      | Add row to table
      \-----------------------------------------------------------------------------------------------------------------------*/
      Rows.Add(row);

    }

  } //Class
} //Namespace
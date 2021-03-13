/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using OnTopic.Data.Sql;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Tests.Schemas {

  /*============================================================================================================================
  | CLASS: VERSION HISTORY DATA TABLE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a <see cref="DataTable"/> which maps to the expected schema of the <c>VersionHistory</c> records returns from
  ///   the <c>GetTopics</c> stored procedure.
  /// </summary>
  /// <remarks>
  ///   This allows testing of the <see cref="SqlTopicRepository"/> via its <see cref="SqlDataReaderExtensions"/> methods.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  public class VersionHistoryDataTable: DataTable {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the <see cref="VersionHistoryDataTable"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="VersionHistoryDataTable"/>.</returns>
    public VersionHistoryDataTable() : base("VersionHistory") {

      /*------------------------------------------------------------------------------------------------------------------------
      | Add TopicId column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(int),
        ColumnName              = "TopicId",
        Unique                  = true
      });

      /*------------------------------------------------------------------------------------------------------------------------
      | Add Version column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(DateTime),
        ColumnName              = "Version"
      });

    }

    /*==========================================================================================================================
    | ADD ROW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a new <see cref="DataRow"/> to the <see cref="VersionHistoryDataTable"/>.
    /// </summary>
    public void AddRow(int topicId, DateTime version) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Verify parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicId, nameof(topicId));
      Contract.Requires(version, nameof(version));

      /*------------------------------------------------------------------------------------------------------------------------
      | Create new row
      \-----------------------------------------------------------------------------------------------------------------------*/
      var row = NewRow();

      row["TopicId"]            = topicId;
      row["Version"]            = version;

      /*------------------------------------------------------------------------------------------------------------------------
      | Add row to table
      \-----------------------------------------------------------------------------------------------------------------------*/
      Rows.Add(row);

    }

  } //Class
} //Namespace
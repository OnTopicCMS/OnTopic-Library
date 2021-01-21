/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Data;
using OnTopic.Data.Sql;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Tests.Schemas {

  /*============================================================================================================================
  | CLASS: TOPIC REFERENCES DATA TABLE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a <see cref="DataTable"/> which maps to the expected schema of the <c>TopicReferences</c> table.
  /// </summary>
  /// <remarks>
  ///   This allows testing of the <see cref="SqlTopicRepository"/> via its <see cref="SqlDataReaderExtensions"/> methods.
  /// </remarks>
  public class TopicReferencesDataTable: DataTable {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the <see cref="TopicReferencesDataTable"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="TopicReferencesDataTable"/>.</returns>
    public TopicReferencesDataTable() : base("TopicReferences") {

      /*------------------------------------------------------------------------------------------------------------------------
      | Add Source_TopicId column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(int),
        ColumnName              = "Source_TopicId",
        Unique                  = true
      });

      /*------------------------------------------------------------------------------------------------------------------------
      | Add RelationshipKey column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(string),
        ColumnName              = "ReferenceKey"
      });

      /*------------------------------------------------------------------------------------------------------------------------
      | Add Target_TopicId column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(int),
        ColumnName              = "Target_TopicId",
        AllowDBNull             = true
      });

      /*------------------------------------------------------------------------------------------------------------------------
      | Add ParentId column
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
    ///   Adds a new <see cref="DataRow"/> to the <see cref="TopicReferencesDataTable"/>.
    /// </summary>
    public void AddRow(int sourceTopicId, string referenceKey, int? targetTopicId, DateTime? version = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Verify parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(sourceTopicId, nameof(sourceTopicId));
      Contract.Requires(referenceKey, nameof(referenceKey));
      Contract.Requires(targetTopicId, nameof(targetTopicId));

      /*------------------------------------------------------------------------------------------------------------------------
      | Create new row
      \-----------------------------------------------------------------------------------------------------------------------*/
      var row = NewRow();

      row["Source_TopicId"]     = sourceTopicId;
      row["ReferenceKey"]       = referenceKey;
      row["Target_TopicId"]     = targetTopicId.HasValue ? (object)targetTopicId : DBNull.Value;
      row["Version"]            = version?? DateTime.UtcNow;

      /*------------------------------------------------------------------------------------------------------------------------
      | Add row to table
      \-----------------------------------------------------------------------------------------------------------------------*/
      Rows.Add(row);

    }

  } //Class
} //Namespace
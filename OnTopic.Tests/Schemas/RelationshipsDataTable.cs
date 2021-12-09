/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Data;
using OnTopic.Data.Sql;

namespace OnTopic.Tests.Schemas {

  /*============================================================================================================================
  | CLASS: RELATIONSHIPS DATA TABLE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a <see cref="DataTable"/> which maps to the expected schema of the <c>Relationships</c> table.
  /// </summary>
  /// <remarks>
  ///   This allows testing of the <see cref="SqlTopicRepository"/> via its <see cref="SqlDataReaderExtensions"/> methods.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  public class RelationshipsDataTable: DataTable {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the <see cref="AttributesDataTable"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="AttributesDataTable"/>.</returns>
    public RelationshipsDataTable() : base("Relationships") {

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
        ColumnName              = "RelationshipKey"
      });

      /*------------------------------------------------------------------------------------------------------------------------
      | Add Target_TopicId column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(int),
        ColumnName              = "Target_TopicId"
      });

      /*------------------------------------------------------------------------------------------------------------------------
      | Add IsDeleted column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(bool),
        ColumnName              = "IsDeleted"
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
    ///   Adds a new <see cref="DataRow"/> to the <see cref="RelationshipsDataTable"/>.
    /// </summary>
    public void AddRow(int sourceTopicId, string relationshipKey, int targetTopicId, bool isDeleted, DateTime? version = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Verify parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(sourceTopicId, nameof(sourceTopicId));
      Contract.Requires(relationshipKey, nameof(relationshipKey));
      Contract.Requires(targetTopicId, nameof(targetTopicId));

      /*------------------------------------------------------------------------------------------------------------------------
      | Create new row
      \-----------------------------------------------------------------------------------------------------------------------*/
      var row = NewRow();

      row["Source_TopicId"]     = sourceTopicId;
      row["RelationshipKey"]    = relationshipKey;
      row["Target_TopicId"]     = targetTopicId;
      row["IsDeleted"]          = isDeleted;
      row["Version"]            = version?? DateTime.UtcNow;

      /*------------------------------------------------------------------------------------------------------------------------
      | Add row to table
      \-----------------------------------------------------------------------------------------------------------------------*/
      Rows.Add(row);

    }

  } //Class
} //Namespace
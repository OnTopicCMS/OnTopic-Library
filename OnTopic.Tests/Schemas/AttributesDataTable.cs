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
  | CLASS: ATTRIBUTES DATA TABLE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a <see cref="DataTable"/> which maps to the expected schema of the <c>Attributes</c> table.
  /// </summary>
  /// <remarks>
  ///   This allows testing of the <see cref="SqlTopicRepository"/> via its <see cref="SqlDataReaderExtensions"/> methods.
  /// </remarks>
  public class AttributesDataTable: DataTable {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the <see cref="AttributesDataTable"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="AttributesDataTable"/>.</returns>
    public AttributesDataTable() : base("Attributes") {

      /*------------------------------------------------------------------------------------------------------------------------
      | Add TopicId column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(int),
        ColumnName              = "TopicId",
        Unique                  = true
      });

      /*------------------------------------------------------------------------------------------------------------------------
      | Add AttributeKey column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(string),
        ColumnName              = "AttributeKey"
      });

      /*------------------------------------------------------------------------------------------------------------------------
      | Add AttributeValue column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(string),
        ColumnName              = "AttributeRecord",
        AllowDBNull             = true
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
    ///   Adds a new <see cref="DataRow"/> to the <see cref="AttributesDataTable"/>.
    /// </summary>
    public void AddRow(int topicId, string attributeKey, string? attributeValue, DateTime? version = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Verify parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicId, nameof(topicId));
      Contract.Requires(attributeKey, nameof(attributeKey));

      /*------------------------------------------------------------------------------------------------------------------------
      | Create new row
      \-----------------------------------------------------------------------------------------------------------------------*/
      var row = NewRow();

      row["TopicId"]            = topicId;
      row["AttributeKey"]       = attributeKey;
      row["AttributeRecord"]     = attributeValue is null? DBNull.Value : attributeValue;
      row["Version"]            = version?? DateTime.UtcNow;

      /*------------------------------------------------------------------------------------------------------------------------
      | Add row to table
      \-----------------------------------------------------------------------------------------------------------------------*/
      Rows.Add(row);

    }

  } //Class
} //Namespace
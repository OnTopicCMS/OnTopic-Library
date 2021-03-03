/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Data;
using System.Xml;
using OnTopic.Data.Sql;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Tests.Schemas {

  /*============================================================================================================================
  | CLASS: EXTENDED ATTRIBUTES DATA TABLE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a <see cref="DataTable"/> which maps to the expected schema of the <c>ExtendedAttributes</c> table.
  /// </summary>
  /// <remarks>
  ///   This allows testing of the <see cref="SqlTopicRepository"/> via its <see cref="SqlDataReaderExtensions"/> methods.
  /// </remarks>
  public class ExtendedAttributesDataTable: DataTable {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the <see cref="ExtendedAttributesDataTable"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="ExtendedAttributesDataTable"/>.</returns>
    public ExtendedAttributesDataTable() : base("ExtendedAttributes") {

      /*------------------------------------------------------------------------------------------------------------------------
      | Add TopicId column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(int),
        ColumnName              = "TopicId",
        Unique                  = true
      });

      /*------------------------------------------------------------------------------------------------------------------------
      | Add AttributesXml column
      \-----------------------------------------------------------------------------------------------------------------------*/
      Columns.Add(new DataColumn() {
        DataType                = typeof(XmlDocument),
        ColumnName              = "AttributesXml"
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
    ///   Adds a new <see cref="DataRow"/> to the <see cref="ExtendedAttributesDataTable"/>.
    /// </summary>
    public void AddRow(int topicId, XmlDocument xml, DateTime? version = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Verify parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicId, nameof(topicId));
      Contract.Requires(xml, nameof(xml));

      /*------------------------------------------------------------------------------------------------------------------------
      | Create new row
      \-----------------------------------------------------------------------------------------------------------------------*/
      var row = NewRow();

      row["TopicId"]            = topicId;
      row["AttributesXml"]      = xml;
      row["Version"]            = version?? DateTime.UtcNow;

      /*------------------------------------------------------------------------------------------------------------------------
      | Add row to table
      \-----------------------------------------------------------------------------------------------------------------------*/
      Rows.Add(row);

    }

  } //Class
} //Namespace
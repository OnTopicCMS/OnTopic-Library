/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Data.Sql {

  /*============================================================================================================================
  | CLASS: SQL DATA READER EXTENSIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Extension methods for the <see cref="SqlDataReader"/> class.
  /// </summary>
  internal static class SqlDataReaderExtensions {

    /*==========================================================================================================================
    | DELEGATE: TRY PARSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    public delegate bool TryParse<in String, U, out Boolean>(String input, out U output);

    /*==========================================================================================================================
    | METHOD: GET INTEGER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves an integer value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    internal static int GetInteger(this SqlDataReader reader, string columnName) =>
      Int32.TryParse(reader.GetValue(reader.GetOrdinal(columnName)).ToString(), out var output)? output : -1;

    /*==========================================================================================================================
    | METHOD: GET TOPIC ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <see cref="Topic.Id"/> value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    internal static int GetTopicId(this SqlDataReader reader, string columnName = "TopicID") =>
      reader.GetInt32(reader.GetOrdinal(columnName));

    /*==========================================================================================================================
    | METHOD: GET STRING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a string value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    internal static string GetString(this SqlDataReader reader, string columnName) =>
      reader.GetString(reader.GetOrdinal(columnName));

    /*==========================================================================================================================
    | METHOD: GET VERSION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves the version column, with precisions appropriate for setting the <see cref="Topic.VersionHistory"/>.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> object.</param>
    internal static DateTime GetVersion(this SqlDataReader reader) =>
      reader.GetDateTime(reader.GetOrdinal("Version"));

  } //Class
} //Namespace
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
    | METHOD: GET INTEGER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves an integer value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    internal static int GetInteger(this SqlDataReader reader, string columnName) =>
      GetValue<int>(reader, columnName, reader.GetInt32);

    /*==========================================================================================================================
    | METHOD: GET STRING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a string value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    internal static string GetString(this SqlDataReader reader, string columnName) =>
      GetValue<string>(reader, columnName, reader.GetString);

    /*==========================================================================================================================
    | METHOD: GET DATE/TIME
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <see cref="DateTime"/> value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    internal static DateTime GetDateTime(this SqlDataReader reader, string columnName) =>
      GetValue<DateTime>(reader, columnName, reader.GetDateTime);

    /*==========================================================================================================================
    | METHOD: GET BOOLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <see cref="Boolean"/> value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    internal static bool GetBoolean(this SqlDataReader reader, string columnName) =>
      GetValue<bool>(reader, columnName, reader.GetBoolean);

    /*==========================================================================================================================
    | METHOD: GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a string value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    /// <param name="defaultValue">The default value, in case <see cref="DBNull"/> is returned.</param>
    private static T GetValue<T>(
      SqlDataReader reader,
      string columnName,
      Func<int, T> getter
    ) {
      Contract.Requires(columnName, nameof(columnName));
      var ordinal = reader.GetOrdinal(columnName);
      return getter(ordinal);
    }

  } //Class
} //Namespace
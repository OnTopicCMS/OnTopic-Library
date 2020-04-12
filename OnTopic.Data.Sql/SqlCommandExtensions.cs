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
  | CLASS: SQL COMMAND EXTENSIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Extension methods for the <see cref="SqlCommand"/> class.
  /// </summary>
  internal static class SqlCommandExtensions {

    /*==========================================================================================================================
    | METHOD: ADD OUTPUT PARAMETER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a SQL parameter to a command object, additionally setting the specified parameter direction.
    /// </summary>
    /// <param name="command">The SQL command object.</param>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="paramDirection">The SQL parameter's directional setting (input-only, output-only, etc.).</param>
    /// <param name="sqlDbType">The SQL field data type.</param>
    internal static void AddOutputParameter(this SqlCommand command, string sqlParameter) =>
      AddParameter(command, sqlParameter, null, SqlDbType.Int, ParameterDirection.ReturnValue);

    /*==========================================================================================================================
    | METHOD: ADD PARAMETER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Wrapper function that adds a SQL parameter to a command object.
    /// </summary>
    /// <param name="command">The SQL command object.</param>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="fieldValue">The SQL field value.</param>
    internal static void AddParameter(this SqlCommand command, string sqlParameter, int fieldValue)
      => AddParameter(command, sqlParameter, fieldValue, SqlDbType.Int);

    /// <summary>
    ///   Wrapper function that adds a SQL parameter to a command object.
    /// </summary>
    /// <param name="command">The SQL command object.</param>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="fieldValue">The SQL field value.</param>
    internal static void AddParameter(this SqlCommand command, string sqlParameter, bool fieldValue)
      => AddParameter(command, sqlParameter, fieldValue, SqlDbType.Bit);

    /// <summary>
    ///   Wrapper function that adds a SQL parameter to a command object.
    /// </summary>
    /// <param name="command">The SQL command object.</param>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="fieldValue">The SQL field value.</param>
    internal static void AddParameter(this SqlCommand command, string sqlParameter, DateTime fieldValue)
      => AddParameter(command, sqlParameter, fieldValue, SqlDbType.DateTime);

    /// <summary>
    ///   Wrapper function that adds a SQL parameter to a command object.
    /// </summary>
    /// <param name="command">The SQL command object.</param>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="fieldValue">The SQL field value.</param>
    internal static void AddParameter(this SqlCommand command, string sqlParameter, StringBuilder fieldValue)
      => AddParameter(command, sqlParameter, fieldValue.ToString(), SqlDbType.Xml);

    /// <summary>
    ///   Wrapper function that adds a SQL parameter to a command object.
    /// </summary>
    /// <param name="command">The SQL command object.</param>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="fieldValue">The SQL field value.</param>
    /// <param name="sqlDbType">The SQL field data type.</param>
    internal static void AddParameter(this SqlCommand command, string sqlParameter, string? fieldValue)
      => AddParameter(command, sqlParameter, (fieldValue?? "").Length.Equals(0)? null : fieldValue, SqlDbType.VarChar);

    /// <summary>
    ///   Adds a SQL parameter to a command object, additionally setting the specified SQL data length for the field.
    /// </summary>
    /// <param name="command">The SQL command object.</param>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="fieldValue">The SQL field value.</param>
    /// <param name="sqlDbType">The SQL field data type.</param>
    /// <param name="paramDirection">The SQL parameter's directional setting (input-only, output-only, etc.).</param>
    /// <param name="sqlLength">Length limit for the SQL field.</param>
    /// <requires description="The SQL command object must be specified." exception="T:System.ArgumentNullException">
    ///   command != null
    /// </requires>
    private static void AddParameter(
      SqlCommand command,
      string sqlParameter,
      object? fieldValue,
      SqlDbType sqlDbType,
      ParameterDirection paramDirection = ParameterDirection.Input
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(command, "The SQL command object must be specified.");
      Contract.Requires(command.Parameters, "The SQL command object's parameters collection must be available");

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish basic parameter
      \-----------------------------------------------------------------------------------------------------------------------*/
      var parameter             = new SqlParameter("@" + sqlParameter, sqlDbType) {
        Direction               = paramDirection
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Set parameter value
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (fieldValue is null) {
        parameter.Value         = (string?)null;
      }
      else if (paramDirection.Equals(ParameterDirection.Input)) {
        parameter.Value         = sqlDbType switch {
          SqlDbType.Bit         => (bool)fieldValue,
          SqlDbType.DateTime    => (DateTime)fieldValue,
          SqlDbType.Int         => (int)fieldValue,
          SqlDbType.Xml         => (string)fieldValue,
          _                     => (string)fieldValue,
        };
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Add parameter to command
      \-----------------------------------------------------------------------------------------------------------------------*/
      command.Parameters.Add(parameter);

    }

  } //Class
} //Namespace
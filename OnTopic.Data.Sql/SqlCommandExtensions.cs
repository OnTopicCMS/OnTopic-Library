/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Globalization;
using System.Text;

namespace OnTopic.Data.Sql {

  /*============================================================================================================================
  | CLASS: SQL COMMAND EXTENSIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Extension methods for the <see cref="SqlCommand"/> class.
  /// </summary>
  internal static class SqlCommandExtensions {

    /*==========================================================================================================================
    | METHOD: GET RETURN CODE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves the return code from a stored procedure.
    /// </summary>
    /// <param name="command">The SQL command object.</param>
    /// <param name="sqlParameter">The name of the SQL parameter to retrieve as the return code.</param>
    internal static int GetReturnCode(this SqlCommand command, string sqlParameter = "ReturnCode") {
      Contract.Assume(
        command.Parameters.Contains($"@{sqlParameter}"),
        $"The call to the {command.CommandText} stored procedure did not return the expected 'ReturnCode' parameter."
      );
      var returnCode = command.Parameters[$"@{sqlParameter}"].Value?.ToString();
      if (Int32.TryParse(returnCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out var returnValue)) {
        return returnValue;
      }
      return -1;
    }

    /*==========================================================================================================================
    | METHOD: ADD OUTPUT PARAMETER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a SQL parameter to a command object, additionally setting the specified parameter direction.
    /// </summary>
    /// <param name="command">The SQL command object.</param>
    /// <param name="sqlParameter">The name of the SQL output parameter.</param>
    internal static void AddOutputParameter(this SqlCommand command, string sqlParameter = "ReturnCode") =>
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
      => AddParameter(command, sqlParameter, fieldValue, SqlDbType.DateTime2);

    /// <summary>
    ///   Wrapper function that adds a SQL parameter to a command object.
    /// </summary>
    /// <param name="command">The SQL command object.</param>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="fieldValue">The SQL field value.</param>
    internal static void AddParameter(this SqlCommand command, string sqlParameter, DataTable fieldValue)
      => AddParameter(command, sqlParameter, fieldValue, SqlDbType.Structured);

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
    internal static void AddParameter(this SqlCommand command, string sqlParameter, string? fieldValue)
      => AddParameter(command, sqlParameter, String.IsNullOrEmpty(fieldValue)? null : fieldValue, SqlDbType.VarChar);

    /// <summary>
    ///   Adds a SQL parameter to a command object, additionally setting the specified SQL data length for the field.
    /// </summary>
    /// <param name="command">The SQL command object.</param>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="fieldValue">The SQL field value.</param>
    /// <param name="sqlDbType">The SQL field data type.</param>
    /// <param name="paramDirection">The SQL parameter's directional setting (input-only, output-only, etc.).</param>
    /// <requires description="The SQL command object must be specified." exception="T:System.ArgumentNullException">
    ///   command is not null
    /// </requires>
    private static void AddParameter(
      SqlCommand command,
      string sqlParameter,
      object? fieldValue,
      SqlDbType sqlDbType,
      ParameterDirection paramDirection = ParameterDirection.Input
    ) {

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
      else if (paramDirection is ParameterDirection.Input) {
        parameter.Value         = sqlDbType switch {
          SqlDbType.Bit         => (bool)fieldValue,
          SqlDbType.DateTime2   => (DateTime)fieldValue,
          SqlDbType.Int         => (int)fieldValue,
          SqlDbType.Xml         => (string)fieldValue,
          SqlDbType.Structured  => (DataTable)fieldValue,
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
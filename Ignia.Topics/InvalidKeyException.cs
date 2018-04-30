/*==============================================================================================================================
| Author Ignia, LLC
| Client Ignia, LLC
| Project Topics Library
\=============================================================================================================================*/
using System;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: INVALID KEY EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Exception to be thrown when a property referencing a <see cref="Topic.Key"/> is assigned with an invalidate value.
  /// </summary>
  /// <remarks>
  ///   <see cref="Topic.Key"/>s are alphanumeric, and may contain hyphens, periods, or underscores. Any other values, such as
  ///   spaces, slashes, or colons, are not permitted and will throw an exception.
  /// </remarks>
  public class InvalidKeyException: ArgumentException {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of a <see cref="InvalidKeyException"/> class with no parameters.
    /// </summary>
    /// <returns>A new <see cref="InvalidKeyException"/> instance.</returns>
    public InvalidKeyException(): base() {
    }

    /// <summary>
    ///   Instantiates a new instance of a <see cref="InvalidKeyException"/> class with a custom error message.
    /// </summary>
    /// <param name="message">An error message associated with this exception.</param>
    /// <returns>A new <see cref="InvalidKeyException"/> instance.</returns>
    public InvalidKeyException(string message): base(message) {
    }

    /// <summary>
    ///   Instantiates a new instance of a <see cref="InvalidKeyException"/> class with a custom error message and inner
    ///   exception.
    /// </summary>
    /// <param name="message">An error message associated with this exception.</param>
    /// <param name="inner">The inner exception associated with the current exception.</param>
    /// <returns>A new <see cref="InvalidKeyException"/> instance.</returns>
    public InvalidKeyException(string message, Exception inner): base(message, inner) {
    }

    /// <summary>
    ///   Instantiates a new instance of a <see cref="InvalidKeyException"/> class with a custom error message and associated
    ///   parameter name.
    /// </summary>
    /// <param name="message">An error message associated with this exception.</param>
    /// <param name="paramName">The name of the parameter that is associated with the exception.</param>
    /// <returns>A new <see cref="InvalidKeyException"/> instance.</returns>
    public InvalidKeyException(string message, string paramName) : base(message, paramName) {
    }

    /// <summary>
    ///   Instantiates a new instance of a <see cref="InvalidKeyException"/> class with a custom error message, associated
    ///   parameter name, and inner exception.
    /// </summary>
    /// <param name="message">An error message associated with this exception.</param>
    /// <param name="paramName">The name of the parameter that is associated with the exception.</param>
    /// <param name="inner">The inner exception associated with the current exception.</param>
    /// <returns>A new <see cref="InvalidKeyException"/> instance.</returns>
    public InvalidKeyException(string message, string paramName, Exception inner) : base(message, paramName, inner) {
    }


  }
}

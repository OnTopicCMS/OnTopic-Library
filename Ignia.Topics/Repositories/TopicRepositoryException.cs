/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Data.Common;
using Ignia.Topics.Diagnostics;
using System.Runtime.Serialization;

namespace Ignia.Topics.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC REPOSITORY EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicRepositoryException"/> provides a general exception that can be thrown for any persistence errors
  ///   that arise from concrete <see cref="ITopicRepository"/> implementations.
  /// </summary>
  /// <remarks>
  ///   Microsoft provides a set of <see cref="DbException"/> classes, such as <see cref="System.Data.SqlClient.SqlException"/>,
  ///   which are specific to their target implementations. Since <see cref="ITopicRepository"/>, however, is intended to be
  ///   database agnostic, none of these are appropriate to catch when implementing <see cref="ITopicRepository"/>. Instead, the
  ///   <see cref="TopicRepositoryException"/> provides a database agnostic version of an exception that can provide a wrapper
  ///   around any of these more concrete exceptions.
  /// </remarks>
  [Serializable]
  public class TopicRepositoryException : DbException {

    /*==========================================================================================================================
    | CONSTRUCTOR: TAXONOMY DELETE EVENT ARGS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="TopicRepositoryException" /> instance.
    /// </summary>
    /// <param name="message">The message to display for this exception.</param>
    public TopicRepositoryException() : base() { }

    /// <summary>
    ///   Initializes a new <see cref="TopicRepositoryException" /> instance with a specific error message.
    /// </summary>
    /// <param name="message">The message to display for this exception.</param>
    public TopicRepositoryException(string message) : base(message) {}

    /// <summary>
    ///   Initializes a new <see cref="TopicRepositoryException" /> instance with a specific error message and nested exception.
    /// </summary>
    /// <param name="message">The message to display for this exception.</param>
    /// <param name="innerException">The reference to the original, underlying exception.</param>
    public TopicRepositoryException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    ///   Instantiates a new <see cref="TopicRepositoryException"/> instance for serialization.
    /// </summary>
    /// <param name="info">A <see cref="SerializationInfo"/> instance with details about the serialization requirements.</param>
    /// <param name="context">A <see cref="StreamingContext"/> instance with details about the request context.</param>
    /// <returns>A new <see cref="InvalidKeyException"/> instance.</returns>
    protected TopicRepositoryException(SerializationInfo info, StreamingContext context) : base(info, context) {
      Contract.Requires<ArgumentNullException>(info != null);
    }

  } // Class

} // Namespace
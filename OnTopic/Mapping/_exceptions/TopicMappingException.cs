/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Runtime.Serialization;

namespace OnTopic.Mapping {

  /*============================================================================================================================
  | CLASS: TOPIC MAPPING EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicMappingException"/> provides a general exception that can be thrown for any errors that arise from
  ///   concrete implementations of <see cref="ITopicMappingService"/> as well as other mapping service interfaces.
  /// </summary>
  /// <remarks>
  ///   Having one (base) class used for all expected exceptions from the <see cref="ITopicMappingService"/> and other mapping
  ///   service interfaces allows implementors to capture all exceptions—while, potentially, catching more specific exceptions
  ///   based on derived classes, if we discover the need for more specific exceptions.
  /// </remarks>
  [Serializable]
  [ExcludeFromCodeCoverage]
  public class TopicMappingException : Exception {

    /*==========================================================================================================================
    | CONSTRUCTOR: TOPIC MAPPING EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="TopicMappingException" /> instance.
    /// </summary>
    public TopicMappingException() : base() { }

    /// <summary>
    ///   Initializes a new <see cref="TopicMappingException" /> instance with a specific error message.
    /// </summary>
    /// <param name="message">The message to display for this exception.</param>
    public TopicMappingException(string message) : base(message) { }

    /// <summary>
    ///   Initializes a new <see cref="TopicMappingException" /> instance with a specific error message and nested exception.
    /// </summary>
    /// <param name="message">The message to display for this exception.</param>
    /// <param name="innerException">The reference to the original, underlying exception.</param>
    public TopicMappingException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    ///   Instantiates a new <see cref="TopicMappingException"/> instance for serialization.
    /// </summary>
    /// <param name="info">A <see cref="SerializationInfo"/> instance with details about the serialization requirements.</param>
    /// <param name="context">A <see cref="StreamingContext"/> instance with details about the request context.</param>
    /// <returns>A new <see cref="InvalidKeyException"/> instance.</returns>
    protected TopicMappingException(SerializationInfo info, StreamingContext context) : base(info, context) {
      Contract.Requires(info);
    }

  } //Class
} //Namespace
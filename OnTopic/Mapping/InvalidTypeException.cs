/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Runtime.Serialization;
using OnTopic.Internal.Diagnostics;
using OnTopic.Lookup;

namespace OnTopic.Mapping {

  /*============================================================================================================================
  | CLASS: INVALID TYPE EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="InvalidTypeException"/> is thrown when an <see cref="ITopicMappingService"/> implementation requests a
  ///   target type that cannot be located using the supplied <see cref="ITypeLookupService"/>.
  /// </summary>
  /// <remarks>
  ///   Having one (base) class used for all expected exceptions from the <see cref="ITopicMappingService"/> and other mapping
  ///   service interfaces allows implementors to capture all exceptions—while, potentially, catching more specific exceptions
  ///   based on derived classes, if we discover the need for more specific exceptions.
  /// </remarks>
  [Serializable]
  public class InvalidTypeException: TopicMappingException {

    /*==========================================================================================================================
    | CONSTRUCTOR: INVALID TYPE EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="InvalidTypeException" /> instance.
    /// </summary>
    public InvalidTypeException() : base() { }

    /// <summary>
    ///   Initializes a new <see cref="InvalidTypeException" /> instance with a specific error message.
    /// </summary>
    /// <param name="message">The message to display for this exception.</param>
    public InvalidTypeException(string message) : base(message) { }

    /// <summary>
    ///   Initializes a new <see cref="InvalidTypeException" /> instance with a specific error message and nested exception.
    /// </summary>
    /// <param name="message">The message to display for this exception.</param>
    /// <param name="innerException">The reference to the original, underlying exception.</param>
    public InvalidTypeException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    ///   Instantiates a new <see cref="InvalidTypeException"/> instance for serialization.
    /// </summary>
    /// <param name="info">A <see cref="SerializationInfo"/> instance with details about the serialization requirements.</param>
    /// <param name="context">A <see cref="StreamingContext"/> instance with details about the request context.</param>
    /// <returns>A new <see cref="InvalidKeyException"/> instance.</returns>
    protected InvalidTypeException(SerializationInfo info, StreamingContext context) : base(info, context) {
      Contract.Requires(info);
    }

  } //Class
} //Namespace
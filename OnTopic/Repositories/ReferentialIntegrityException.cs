/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Runtime.Serialization;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: REFERENTIAL INTEGRITY EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="ReferentialIntegrityException"/> will be thrown when an operation violates the referential integrity of
  ///   the underlying persistence layer.
  /// </summary>
  /// <remarks>
  ///   Generally, the <see cref="ReferentialIntegrityException"/> will occur when a <see cref="Topic"/> is being
  ///   deleted, but the topic or one of its descendents is the <see cref="Topic.DerivedTopic"/> of another <see cref="Topic"/>.
  ///   In that case, deleting the topic will violate the referential integrity of the target topic.
  /// </remarks>
  [Serializable]
  public class ReferentialIntegrityException: TopicRepositoryException {

    /*==========================================================================================================================
    | CONSTRUCTOR: REFERENTIAL INTEGRITY EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="ReferentialIntegrityException" /> instance.
    /// </summary>
    public ReferentialIntegrityException() : base() { }

    /// <summary>
    ///   Initializes a new <see cref="ReferentialIntegrityException" /> instance based on the source <see cref=
    ///   "Topic"/>.
    /// </summary>
    /// <param name="sourceTopic">The source <see cref="Topic"/> which triggered the exception.</param>
    public ReferentialIntegrityException(Topic sourceTopic):
      base(
        $"The operation on the topic '{sourceTopic?.DerivedTopic?.GetUniqueKey()}' would introduce a referential integrity " +
        $"violation in the underlying persistence layer; the topic '{sourceTopic?.GetUniqueKey()}' depends upon it."
      ) { }

    /// <summary>
    ///   Initializes a new <see cref="ReferentialIntegrityException" /> instance based on the source <see cref=
    ///   "Topic"/>.
    /// </summary>
    /// <param name="sourceTopic">The source <see cref="Topic"/> which triggered the exception.</param>
    /// <param name="innerException">The reference to the original, underlying exception.</param>
    public ReferentialIntegrityException(Topic sourceTopic, Exception innerException):
      base(
        $"The operation on the topic '{sourceTopic?.DerivedTopic?.GetUniqueKey()}' would introduce a referential integrity " +
        $"violation in the underlying persistence layer; the topic '{sourceTopic?.GetUniqueKey()}' depends upon it.",
        innerException
      ) { }

    /// <summary>
    ///   Initializes a new <see cref="ReferentialIntegrityException" /> instance based on a <paramref name=
    ///   "message"/>.
    /// </summary>
    /// <param name="message">The error message to associate with the exception.</param>
    public ReferentialIntegrityException(string message) : base(message) { }

    /// <summary>
    ///   Initializes a new <see cref="ReferentialIntegrityException" /> instance based on a <paramref name=
    ///   "message"/>.
    /// </summary>
    /// <param name="message">The error message to associate with the exception.</param>
    /// <param name="innerException">The reference to the original, underlying exception.</param>
    public ReferentialIntegrityException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    ///   Instantiates a new <see cref="ReferentialIntegrityException"/> instance for serialization.
    /// </summary>
    /// <param name="info">A <see cref="SerializationInfo"/> instance with details about the serialization requirements.</param>
    /// <param name="context">A <see cref="StreamingContext"/> instance with details about the request context.</param>
    /// <returns>A new <see cref="InvalidKeyException"/> instance.</returns>
    protected ReferentialIntegrityException(SerializationInfo info, StreamingContext context) : base(info, context) {
      Contract.Requires(info);
    }

  } //Class
} //Namespace
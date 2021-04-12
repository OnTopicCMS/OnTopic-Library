/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC NOT FOUND EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicNotFoundException"/> will be thrown when a topic is requested from the repository, but one cannot be
  ///   found and there is not a logical fallback.
  /// </summary>
  /// <remarks>
  ///   As the <see cref="TopicNotFoundException"/> derives from both <see cref="TopicRepositoryException"/> and <see
  ///   cref="DbException"/>, it retains compatability with previous exception handling, while offering callers the option of
  ///   capturing a more specific error for this individual use case.
  /// </remarks>
  [Serializable]
  [ExcludeFromCodeCoverage]
  public class TopicNotFoundException: TopicRepositoryException {

    /*==========================================================================================================================
    | CONSTRUCTOR: TOPIC NOT FOUND EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="TopicNotFoundException" /> instance.
    /// </summary>
    public TopicNotFoundException() : base() { }

    /// <summary>
    ///   Initializes a new <see cref="TopicNotFoundException" /> instance based on a missing topic ID.
    /// </summary>
    /// <param name="topicId">The requested <see cref="Topic.Id"/> which cannot be found.</param>
    public TopicNotFoundException(int topicId):
      base($"The topic {topicId} cannot be found in the repository.") { }

    /// <summary>
    ///   Initializes a new <see cref="TopicNotFoundException" /> instance based on a missing topic ID.
    /// </summary>
    /// <param name="topicId">The requested <see cref="Topic.Id"/> which cannot be found.</param>
    /// <param name="innerException">The reference to the original, underlying exception.</param>
    public TopicNotFoundException(int topicId, Exception innerException):
      base($"The topic {topicId} cannot be found in the repository.", innerException) { }

    /// <summary>
    ///   Initializes a new <see cref="TopicNotFoundException" /> instance based on a missing topic key.
    /// </summary>
    /// <param name="topicKey">The requests <see cref="Topic.Key"/> which cannot be found.</param>
    public TopicNotFoundException(string? topicKey):
      base($"The topic {topicKey} cannot be found in the repository.") { }

    /// <summary>
    ///   Initializes a new <see cref="TopicNotFoundException" /> instance based on a missing topic key.
    /// </summary>
    /// <param name="topicKey">The requests <see cref="Topic.Key"/> which cannot be found.</param>
    /// <param name="innerException">The reference to the original, underlying exception.</param>
    public TopicNotFoundException(string? topicKey, Exception innerException):
      base($"The topic {topicKey} cannot be found in the repository.", innerException) { }

    /// <summary>
    ///   Instantiates a new <see cref="TopicRepositoryException"/> instance for serialization.
    /// </summary>
    /// <param name="info">A <see cref="SerializationInfo"/> instance with details about the serialization requirements.</param>
    /// <param name="context">A <see cref="StreamingContext"/> instance with details about the request context.</param>
    /// <returns>A new <see cref="InvalidKeyException"/> instance.</returns>
    protected TopicNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) {
      Contract.Requires(info);
    }

  } //Class
} //Namespace
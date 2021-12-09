/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using OnTopic.Internal.Diagnostics;
using OnTopic.Mapping.Reverse;
using OnTopic.Metadata;

namespace OnTopic.Mapping {

  /*============================================================================================================================
  | CLASS: MAPPING MODEL VALIDATION EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="MappingModelValidationException"/> is thrown when an <see cref="IReverseTopicMappingService"/>
  ///   implementation is provided a model that cannot be reliably mapped back to the target <see cref="Topic"/>, thus
  ///   introducing potential data integrity issues.
  /// </summary>
  /// <remarks>
  ///   The read-only mapping services, such as <see cref="ITopicMappingService"/>, are generally designed to be forgiving of
  ///   mismatches. By contrast, because the <see cref="IReverseTopicMappingService"/> is intended to update the persistence
  ///   store, it is generally designed to fail if there are any discrepancies between the source model and the target <see cref
  ///   ="ContentTypeDescriptor"/>. Given this, the <see cref="MappingModelValidationException"/> is expected to be thrown for
  ///   validation errors caused by e.g. the <see cref="ReverseTopicMappingService"/>.
  /// </remarks>
  [Serializable]
  [ExcludeFromCodeCoverage]
  public class MappingModelValidationException: TopicMappingException {

    /*==========================================================================================================================
    | CONSTRUCTOR: MAPPING MODEL VALIDATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="MappingModelValidationException" /> instance.
    /// </summary>
    public MappingModelValidationException() : base() { }

    /// <summary>
    ///   Initializes a new <see cref="MappingModelValidationException" /> instance with a specific error message.
    /// </summary>
    /// <param name="message">The message to display for this exception.</param>
    public MappingModelValidationException(string message) : base(message) { }

    /// <summary>
    ///   Initializes a new <see cref="MappingModelValidationException" /> instance with a specific error message and nested exception.
    /// </summary>
    /// <param name="message">The message to display for this exception.</param>
    /// <param name="innerException">The reference to the original, underlying exception.</param>
    public MappingModelValidationException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    ///   Instantiates a new <see cref="MappingModelValidationException"/> instance for serialization.
    /// </summary>
    /// <param name="info">A <see cref="SerializationInfo"/> instance with details about the serialization requirements.</param>
    /// <param name="context">A <see cref="StreamingContext"/> instance with details about the request context.</param>
    /// <returns>A new <see cref="InvalidKeyException"/> instance.</returns>
    protected MappingModelValidationException(SerializationInfo info, StreamingContext context) : base(info, context) {
      Contract.Requires(info);
    }

  } //Class
} //Namespace
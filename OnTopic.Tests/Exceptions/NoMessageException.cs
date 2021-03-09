/*==============================================================================================================================
| Author Ignia, LLC
| Client Ignia, LLC
| Project Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using OnTopic.Internal.Diagnostics;

#pragma warning disable CA1032 // Implement standard exception constructors

namespace OnTopic {

  /*============================================================================================================================
  | CLASS: NO MESSAGE EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   A test exception that doesn't include an expected constructor overload accepting the <c>message</c>.
  /// </summary>
  /// <remarks>
  ///   The <see cref="Contract.Requires{T}(Boolean, String?)"/> will attempt to initialize an exception of the specified type
  ///   with a message. This assumes an expected constructor overload exists that accepts a single <c>message</c> parameter. If
  ///   not, it will fallback to a <see cref="ArgumentException"/>.
  /// </remarks>
  [Serializable]
  [ExcludeFromCodeCoverage]
  public class NoMessageException: ArgumentException {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of a <see cref="NoMessageException"/> class with no parameters.
    /// </summary>
    /// <returns>A new <see cref="NoMessageException"/> instance.</returns>
    public NoMessageException(): base() {
    }

    /// <summary>
    ///   Instantiates a new instance of a <see cref="NoMessageException"/> class for serialization.
    /// </summary>
    /// <param name="info">A <see cref="SerializationInfo"/> instance with details about the serialization requirements.</param>
    /// <param name="context">A <see cref="StreamingContext"/> instance with details about the request context.</param>
    /// <returns>A new <see cref="NoMessageException"/> instance.</returns>
    protected NoMessageException(SerializationInfo info, StreamingContext context) : base(info, context) {
      Contract.Requires(info);
    }

  } //Class
} //Namespace

#pragma warning restore CA1032 // Implement standard exception constructors
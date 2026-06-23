/*==============================================================================================================================
| Author Ignia, LLC
| Client Ignia, LLC
| Project Topics Library
\=============================================================================================================================*/
namespace OnTopic {

  /*============================================================================================================================
  | CLASS: NO MESSAGE EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   A test exception that doesn't include an expected constructor overload accepting the <c>message</c>.
  /// </summary>
  /// <remarks>
  ///   The <see cref="Contract.Requires{T}(Boolean, String?, String?)"/> will attempt to initialize an exception of the specified type
  ///   with a message. This assumes an expected constructor overload exists that accepts a single <c>message</c> parameter. If
  ///   not, it will fallback to a <see cref="ArgumentException"/>.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "This exception intentionally omits the message constructor to test the fallback behavior in Contract.Requires{T}.")]
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

  } //Class
} //Namespace
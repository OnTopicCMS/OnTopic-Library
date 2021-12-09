/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: CONTRACT TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="Contract"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class ContractTest {

    /*==========================================================================================================================
    | TEST: REQUIRES: CONDITION IS TRUE: THROW NO EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a true condition using the <see cref="Contract"/> class, and validates that it correctly does nothing.
    /// </summary>
    [Fact]
    public void Requires_ConditionIsTrue_ThrowNoException()
      => Contract.Requires(true, "The argument cannot be null");

    /*==========================================================================================================================
    | TEST: REQUIRES: CONDITION IS FALSE: THROW ARGUMENT NULL EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a false condition using the <see cref="Contract"/> class, and validates that it correctly returns an <see
    ///   cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Requires_ConditionIsFalse_ThrowArgumentNullException() =>
      Assert.Throws<InvalidOperationException>(() =>
        Contract.Requires(false, "The argument cannot be null")
      );

    /*==========================================================================================================================
    | TEST: REQUIRES: OBJECT EXISTS: THROW NO EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a non-null argument using the <see cref="Contract"/> class, and validates that it correctly does nothing.
    /// </summary>
    [Fact]
    public void Requires_ObjectExists_ThrowNoException() =>
      Contract.Requires(new object(), "The argument cannot be null");

    /*==========================================================================================================================
    | TEST: REQUIRES: OBJECT IS NULL: THROW ARGUMENT NULL EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a null argument using the <see cref="Contract"/> class, and validates that it correctly returns an <see
    ///   cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Requires_ObjectIsNull_ThrowArgumentNullException() =>
      Assert.Throws<ArgumentNullException>(() =>
        Contract.Requires(null, "The argument cannot be null")
      );

    /*==========================================================================================================================
    | TEST: REQUIRES: MESSAGE EXISTS: THROW EXCEPTION WITH MESSAGE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a null argument using the <see cref="Contract"/> class, and validates that it correctly returns an <see
    ///   cref="ArgumentNullException"/> with the expected <see cref="ArgumentException.Message"/>.
    /// </summary>
    [Fact]
    public void Requires_MessageExists_ThrowExceptionWithMessage() {

      var errorMessage = "The argument cannot be null";

      try {
        Contract.Requires<ArgumentException>(false, errorMessage);
      }
      catch (ArgumentException ex) {
        Assert.Equal(errorMessage, ex.Message);
      }

    }

    /*==========================================================================================================================
    | TEST: REQUIRES: INVALID CONSTRUCTOR: THROW ARGUMENT EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a null argument using the <see cref="Contract"/> class, and attempts to throw a custom <see cref="
    ///   NoMessageException"/> with the expected <see cref="ArgumentException.Message"/>, but fails due to no overload with
    ///   a single <c>message</c> parameter. In this case, it should throw a <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void Requires_InvalidConstructor_ThrowArgumentException() {
      var errorMessage = "The argument cannot be null";
      Assert.Throws<ArgumentException>(() =>
        Contract.Requires<NoMessageException>(false, errorMessage)
      );
    }

    /*==========================================================================================================================
    | TEST: ASSUME: CONDITION IS TRUE: THROW NO EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a true condition using the <see cref="Contract"/> class, and validates that it correctly does nothing.
    /// </summary>
    [Fact]
    public void Assume_ConditionIsTrue_ThrowNoException()
      => Contract.Assume(true, "The argument cannot be null");

    /*==========================================================================================================================
    | TEST: ASSUME: CONDITION IS FALSE: THROW ARGUMENT NULL EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a false condition using the <see cref="Contract"/> class, and validates that it correctly returns an <see
    ///   cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Assume_ConditionIsFalse_ThrowArgumentNullException() =>
      Assert.Throws<InvalidOperationException>(() =>
        Contract.Assume(false, "The argument cannot be null")
      );

    /*==========================================================================================================================
    | TEST: ASSUME: CONDITION IS FALSE: THROW CUSTOM EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a false condition using the <see cref="Contract"/> class, and validates that it correctly throws the specified
    ///   <see cref="IndexOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void Assume_ConditionIsFalse_ThrowCustomExpection() =>
      Assert.Throws<IndexOutOfRangeException>(() =>
        Contract.Assume<IndexOutOfRangeException>(false, "The argument is out of range")
      );

    /*==========================================================================================================================
    | TEST: ASSUME: CONDITION IS FALSE: THROW CUSTOM EXCEPTION WITHOUT MESSAGE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a false condition using the <see cref="Contract"/> class, and validates that it correctly throws the specified
    ///   <see cref="IndexOutOfRangeException"/> using the empty constructor, since no <c>errorMessage</c> is included.
    /// </summary>
    [Fact]
    public void Assume_ConditionIsFalse_ThrowCustomExpectionWithoutMessage() =>
      Assert.Throws<IndexOutOfRangeException>(() =>
        Contract.Assume<IndexOutOfRangeException>(false)
      );

    /*==========================================================================================================================
    | TEST: ASSUME: OBJECT IS NULL: THROW INVALID OPERATION EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a null assignment using the <see cref="Contract"/> class, and validates that it correctly returns an <see
    ///   cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void Assume_ObjectIsNull_ThrowInvalidOperationException() =>
      Assert.Throws<InvalidOperationException>(() =>
        Contract.Assume(null, "The local runtime state is invalid.")
      );

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Internal.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: CONTRACT TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="Contract"/> class.
  /// </summary>
  [TestClass]
  public class ContractTest {

    /*==========================================================================================================================
    | TEST: REQUIRES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a non-null argument using the <see cref="Contract"/> class, and validates that it correctly does nothing.
    /// </summary>
    [TestMethod]
    public void Requires() {

      var argument = new Object();

      Contract.Requires(argument, "The argument cannot be null");

    }

    /*==========================================================================================================================
    | TEST: REQUIRES (ARGUMENT NULL EXCEPTION)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a null argument using the <see cref="Contract"/> class, and validates that it correctly returns an <see
    ///   cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RequiresArgumentNullException() {

      var argument = (object?)null;

      Contract.Requires(argument, "The argument cannot be null");

    }

    /*==========================================================================================================================
    | TEST: REQUIRES (EXCEPTION MESSAGE)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a null argument using the <see cref="Contract"/> class, and validates that it correctly returns an <see
    ///   cref="ArgumentNullException"/> with the expected <see cref="ArgumentException.Message"/>.
    /// </summary>
    [TestMethod]
    public void RequiresExceptionMessage() {

      var argument = (object?)null;
      var errorMessage = "The argument cannot be null";

      try {
        Contract.Requires<ArgumentException>(argument != null, errorMessage);
      }
      catch (ArgumentException ex) {
        Assert.AreEqual<String>(errorMessage, ex.Message);
      }

    }

    /*==========================================================================================================================
    | TEST: ASSUME (INVALID OPERATION EXCEPTION)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests a null assignment using the <see cref="Contract"/> class, and validates that it correctly returns an <see
    ///   cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void AssumeInvalidOperationException() {

      var variable = (object?)null;

      Contract.Assume(variable, "The local runtime state is invalid.");

    }


  } //Class
} //Namespace
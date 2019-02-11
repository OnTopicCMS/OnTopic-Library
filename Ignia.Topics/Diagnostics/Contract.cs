/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Reflection;

namespace Ignia.Topics.Diagnostics {

  /*============================================================================================================================
  | CLASS: CONTRACT [STATIC]
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   A static class for validating contract-style code declarations.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Initially, OnTopic used Microsoft's <see cref="System.Diagnostics.Contracts"/> references alongside their Code
  ///     Contracts analysis and rewrite modules. Those modules have seen been deprecated and are no longer actively maintained
  ///     by either Microsoft nor the community. Further, there is no evidence that Microsoft intends to support code contracts
  ///     analysis or rewriting in .NET Core. For this reason, code contracts is being removed from OnTopic.
  ///   </para>
  ///   <para>
  ///     <see cref="Contract"/> is not a replacement for Microsoft's Code Contract analysis and rewrite modules. Instead, it
  ///     aims to maintain basic synatactical and functional support for the most basic (and important) features of the rewrite
  ///     module, such as <code>Contract.Requires()</code>, which is necessary to ensure paramater validation in many methods
  ///     throughout the OnTopic library. <see cref="Contract"/> does not seek to functionally reproduce Code Contract's
  ///     <code>Ensures()</code>, <code>Assume()</code>, or <code>Invariant()</code>—any such functions are purely intended
  ///     to maintain temporary syntactical support, and will be marked as deprecated.
  ///   </para>
  ///   <para>
  ///     C# 8.0 will introduce nullable reference types. This largely mitigates the need for <code>Contract.Requires()</code>.
  ///     As such, this library can be seen as a temporary bridge to maintain parameter validation until C# 8.0 is released. At
  ///     that point, nullable reference types should be used in preference for many of the <code>Contract.Requires()</code>
  ///     calls—though, acknowledging, that there are some conditions that won't be satisfied by that (e.g., range checks).
  ///   </para>
  ///   <para>
  ///     <see href="https://stackoverflow.com/questions/40767941/does-vs2017-work-with-codecontracts/46412917#46412917"/>
  ///   </para>
  /// </remarks>
  public static class Contract {

    /*==========================================================================================================================
    | METHOD: REQUIRES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Will throw a <see cref="Exception"/> if the supplied expression evaluates to false.
    /// </summary>
    /// <param name="isValid">An expression resulting in a boolean value indicating if an exception should be thrown.</param>
    /// <param name="errorMessage">Optionally provides an error message in case an exception is thrown.</param>
    /// <exception cref="Exception">
    ///   Thrown when <paramref name="isValid"/> returns <see langword="true"/>.
    /// </exception>
    public static void Requires(bool isValid, string errorMessage = null) => Requires<Exception>(isValid, errorMessage);

    /// <summary>
    ///   Will throw the provided generic exception if the supplied expression evaluates to false.
    /// </summary>
    /// <remarks>
    ///   If the <paramref name="errorMessage"/> is included, this method assumes that the <typeparamref name="T"/> has a
    ///   constructor which accepts a single parameter of type <see cref="String"/>, representing the error message for the
    ///   exception. If no such constructor is available, an <see cref="ArgumentException"/> will be thrown.
    /// </remarks>
    /// <param name="isValid">An expression resulting in a boolean value indicating if an exception should be thrown.</param>
    /// <param name="errorMessage">Optionally provides an error message in case an exception is thrown.</param>
    /// <exception cref="T:Exception">
    ///   Thrown when <paramref name="isValid"/> returns <see langword="true"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when the <typeparamref name="T"/> does not a constructor accepting a sole <see cref="String"/> parameter
    ///   representing the error message, and the <paramref name="errorMessage"/> parameter was supplied.
    /// </exception>
    public static void Requires<T>(bool isValid, string errorMessage = null) where T : Exception, new() {
      if (isValid) return;
      if (String.IsNullOrEmpty(errorMessage)) {
        throw new T();
      }
      try {
        throw (T)Activator.CreateInstance(typeof(T), new object[] { errorMessage });
      }
      catch (Exception ex) when (
        ex is MissingMethodException
        || ex is MethodAccessException
        || ex is TargetInvocationException
        || ex is NotSupportedException
      ) {
        throw new ArgumentException(
          "The exception provided as the generic type argument does not have a constructor that accepts an error message as its sole argument",
          nameof(errorMessage),
          new T()
        );
      }

    }

    /*==========================================================================================================================
    | METHOD: ENSURES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Not implemented. Merely provided to maintain syntactical consistency with Code Contracts.
    /// </summary>
    /// <remarks>
    ///   It is not possible without code rewriting to validate the output of a method. As such, the <see cref="Ensures"/>
    ///   method cannot be properly implemented. For this reason, it is marked as deprecated.
    /// </remarks>
    /// <param name="isValid">An expression resulting in a boolean value indicating if an exception should be thrown.</param>
    /// <param name="errorMessage">Optionally provides an error message in case an exception is thrown.</param>
    [Obsolete("Not implemented. The Ensures method is maintained for syntactical consistency only. References should be removed.", true)]
    public static void Ensures(bool isValid, string errorMessage = null) {
    }

    /*==========================================================================================================================
    | METHOD: ASSUME
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Not implemented. Merely provided to maintain syntactical consistency with Code Contracts.
    /// </summary>
    /// <remarks>
    ///   It is not necessary without code analysis to assume that a condition is met. As a result, while this method always
    ///   returns <see langword="true"/>, this method serves no purpose and should be considered deprecated.
    /// </remarks>
    /// <param name="isValid">An expression resulting in a boolean value indicating if an exception should be thrown.</param>
    /// <param name="errorMessage">Optionally provides an error message in case an exception is thrown.</param>
    [Obsolete("Not implemented. The Assume method is maintained for syntactical consistency only. References should be removed.", true)]
    public static bool Assume(bool isValid, string errorMessage = null) => true;

    /// <summary>
    ///   Ensures that a condition is met. If not, the provided exception is thrown.
    /// </summary>
    /// <remarks>
    ///   Unlike the standard <c>Assumes()</c> method that ships with .NET, this custom oerload accepts a generic <typeparamref
    ///   name="T"/>, of type <see cref="Exception"/>, which will be thrown if the condition is not met. This is virtually
    ///   identical to <see cref="Requires{T}(Boolean, String)"/> except that, syntactically, it is expected to live within the
    ///   body of a method—where as <see cref="Requires{T}(Boolean, String)"/> is expected to live at the beginning of a method.
    ///   This communicates to readers that <see cref="Assume{T}(Boolean, String)"/> is validating runtime state, whereas <see
    ///   cref="Requires{T}(Boolean, String)"/> is validating preconditions.
    /// </remarks>
    /// <param name="isValid">An expression resulting in a boolean value indicating if an exception should be thrown.</param>
    /// <param name="errorMessage">Optionally provides an error message in case an exception is thrown.</param>
    /// <exception cref="T:Exception">
    ///   Thrown when <paramref name="isValid"/> returns <see langword="true"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when the <typeparamref name="T"/> does not a constructor accepting a sole <see cref="String"/> parameter
    ///   representing the error message, and the <paramref name="errorMessage"/> parameter was supplied.
    /// </exception>
    public static void Assume<T>(bool isValid, string errorMessage = null) where T : Exception, new() =>
      Requires<T>(isValid, errorMessage);

    /*==========================================================================================================================
    | METHOD: ASSERT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Not implemented. Merely provided to maintain syntactical consistency with Code Contracts.
    /// </summary>
    /// <remarks>
    ///   It is not necessary without code analysis to assert that a condition will be met. As a result, while this method
    ///   always returns <see langword="true"/>, this method serves no purpose and should be considered deprecated.
    /// </remarks>
    /// <param name="isValid">An expression resulting in a boolean value indicating if an exception should be thrown.</param>
    /// <param name="errorMessage">Optionally provides an error message in case an exception is thrown.</param>
    [Obsolete("Not implemented. The Assert method is maintained for syntactical consistency only. References should be removed.", true)]
    public static bool Assert(bool isValid, string errorMessage = null) => true;

    /*==========================================================================================================================
    | METHOD: RESULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Not implemented. Merely provided to maintain syntactical consistency with Code Contracts.
    /// </summary>
    /// <remarks>
    ///   It is not possible without code rewriting to validate the output of a method. As such, the <see cref="Result"/>
    ///   method cannot be properly implemented. For this reason, it is marked as deprecated.
    /// </remarks>
    [Obsolete("Not implemented. The Result method is maintained for syntactical consistency only. References should be removed.", true)]
    public static T Result<T>() => default(T);

  } //class
} //Namespace

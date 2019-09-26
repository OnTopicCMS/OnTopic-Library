/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Microsoft;

namespace Ignia.Topics.Internal.Diagnostics {

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
  ///     <code>Ensures()</code>, <code>Assume()</code>, or <code>Invariant()</code>—those methods are not implemented.
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
    public static void Requires(bool isValid, string? errorMessage = null) => Requires<Exception>(isValid, errorMessage);

    /// <summary>
    ///   Will throw an <see cref="ArgumentNullException"/> if the supplied object is <see langword="null"/>.
    /// </summary>
    /// <param name="requiredObject">An object that is required to be provided.</param>
    /// <param name="errorMessage">Optionally provides an error message in case an exception is thrown.</param>
    /// <exception cref="ArgumentNullException">
    ///   Thrown when <paramref name="requiredObject"/> is <see langword="null"/>.
    /// </exception>
      //###HACK JJC20190908: Roslyn's flow analysis doesn't accept the [NotNull] hint for parameters unless the variable has
      //been locally assigned, which is an issue for Requires() since it is intended to be used exclusively as a guard clause for
      //parameters. Assigning the parameter to itself mitigates this issue—though it does prompt its own warning in return.
      #pragma warning disable CS1717 // Assignment made to same variable
      requiredObject = requiredObject;
      #pragma warning restore CS1717 // Assignment made to same variable
    public static void Requires([ValidatedNotNull, NotNull]object? requiredObject, string? errorMessage = null) =>
      Requires<ArgumentNullException>(requiredObject != null, errorMessage);
    }

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
    /// <exception cref="Exception">
    ///   Thrown when <paramref name="isValid"/> returns <see langword="true"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when the <typeparamref name="T"/> does not a constructor accepting a sole <see cref="String"/> parameter
    ///   representing the error message, and the <paramref name="errorMessage"/> parameter was supplied.
    /// </exception>
    public static void Requires<T>(bool isValid, string? errorMessage = null) where T : Exception, new() {
      if (isValid) return;
      if (errorMessage is null || errorMessage.Length.Equals(0)) {
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
    | METHOD: ASSUME
    \-------------------------------------------------------------------------------------------------------------------------*/
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
    /// <exception cref="Exception">
    ///   Thrown when <paramref name="isValid"/> returns <see langword="true"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when the <typeparamref name="T"/> does not a constructor accepting a sole <see cref="String"/> parameter
    ///   representing the error message, and the <paramref name="errorMessage"/> parameter was supplied.
    /// </exception>
    public static void Assume<T>(bool isValid, string? errorMessage = null) where T : Exception, new() =>
      Requires<T>(isValid, errorMessage);

    /// <summary>
    ///   Will throw an <see cref="InvalidOperationException"/> if the supplied object is <see langword="null"/>.
    /// </summary>
    /// <param name="requiredObject">An object that is required to be provided.</param>
    /// <param name="errorMessage">Optionally provides an error message in case an exception is thrown.</param>
    /// <exception cref="InvalidOperationException">
    ///   Thrown when <paramref name="requiredObject"/> is <see langword="null"/>.
    /// </exception>
    public static void Assume([ValidatedNotNull, NotNull]object? requiredObject, string? errorMessage = null)
      => Requires<InvalidOperationException>(requiredObject != null, errorMessage);

  } //class
} //Namespace

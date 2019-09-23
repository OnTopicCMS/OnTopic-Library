/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Ignia.Topics.Internal.Diagnostics;

#if NETFRAMEWORK

namespace System.Diagnostics.CodeAnalysis {

  /*============================================================================================================================
  | CLASS: NOT NULL IF NOT NULL (ATTRIBUTE)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Marks that a method parameter is ensured not to return <see langword="null"/> if the annotated parameter is not <see 
  ///   langword="null"/>.
  /// </summary>
  /// <remarks>
  ///   This class will ship with .NET Standard 3.0. Once the project is updated to support that, we'll remove this class and 
  ///   instead allow implementers to use the out-of-the-box implementation. In the meanwhile, providing this class within the 
  ///   correct namespace satisfies the code analysis and allows the project to move forward with implementing the nullable 
  ///   annotation context.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true)]
  public class NotNullIfNotNullAttribute : Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of a <see cref="NotNullIfNotNullAttribute"/> object.
    /// </summary>
    public NotNullIfNotNullAttribute(string parameterName) {
      Contract.Requires(parameterName);
      ParameterName = parameterName;
    }

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of a <see cref="NotNullIfNotNullAttribute"/> object.
    /// </summary>
    public string ParameterName { get; }

  } //Class
} //Namespace

#endif
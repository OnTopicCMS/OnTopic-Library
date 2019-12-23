/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

#if NETSTANDARD2_0

namespace System.Diagnostics.CodeAnalysis {

  /*============================================================================================================================
  | CLASS: NOT NULL (ATTRIBUTE)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Marks that a method parameter is ensured not to return <see langword="null"/>, even if it is submitted as such.
  /// </summary>
  /// <remarks>
  ///   This class will ship with .NET Standard 3.0. Once the project is updated to support that, we'll remove this class and
  ///   instead allow implementers to use the out-of-the-box implementation. In the meanwhile, providing this class within the
  ///   correct namespace satisfies the code analysis and allows the project to move forward with implementing the nullable
  ///   annotation context.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue)]
  public sealed class NotNullAttribute: Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of a <see cref="NotNullAttribute"/> object.
    /// </summary>
    public NotNullAttribute() { }

  } //Class
} //Namespace

#endif
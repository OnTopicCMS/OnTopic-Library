/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

#if NETSTANDARD2_0

namespace System.Diagnostics.CodeAnalysis {

  /*============================================================================================================================
  | CLASS: ALLOW NULL (ATTRIBUTE)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Specifies that null is allowed as an input even if the corresponding type disallows it.
  /// </summary>
  /// <remarks>
  ///   This class will ship with .NET Standard 3.0. Once the project is updated to support that, we'll remove this class and
  ///   instead allow implementers to use the out-of-the-box implementation. In the meanwhile, providing this class within the
  ///   correct namespace satisfies the code analysis and allows the project to move forward with implementing the nullable
  ///   annotation context.
  /// </remarks>
  [AttributeUsage(
    AttributeTargets.Field |
    AttributeTargets.Method |
    AttributeTargets.Parameter |
    AttributeTargets.Property |
    AttributeTargets.ReturnValue,
    AllowMultiple = true
  )]
  public sealed class AllowNullAttribute : Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of a <see cref="AllowNullAttribute"/> object.
    /// </summary>
    public AllowNullAttribute() { }

  } //Class
} //Namespace

#endif
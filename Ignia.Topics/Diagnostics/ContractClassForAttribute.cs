/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace Ignia.Topics.Diagnostics {

  /*============================================================================================================================
  | CLASS: CONTRACT CLASS FOR ATTRIBUTE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Designates which class the code contracts in the decorated class are intended to apply to. Intended only for syntactical
  ///   consistency with Code Contracts.
  /// </summary>
  [Obsolete("Not implemented. The attribute is maintained for syntactical consistency only. References should be removed.", true)]
  public class ContractClassForAttribute : Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ContractClassAttribute"/> with an associated <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The class that contains the code contracts.</param>
    public ContractClassForAttribute(Type type) { }

  }
}
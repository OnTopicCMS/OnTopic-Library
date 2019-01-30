/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace Ignia.Topics.Diagnostics {

  /*============================================================================================================================
  | CLASS: CONTRACT CLASS ATTRIBUTE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Designates a separate class that will define the contracts on a class. Intended only for syntactical consistency with
  ///   Code Contracts.
  /// </summary>
  [Obsolete("Not implemented. The attribute is maintained for syntactical consistency only. References should be removed.", true)]
  public class ContractClassAttribute : Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ContractClassAttribute"/> with an associated <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The class that contains the code contracts.</param>
    public ContractClassAttribute(Type type) { }

  }
}
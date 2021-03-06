/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
namespace System.Diagnostics.CodeAnalysis {

  /*============================================================================================================================
  | CLASS: MEMBER NOT NULL (ATTRIBUTE)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Specifies that the method or property will ensure that the listed field and property members have not-null values.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
  internal sealed class MemberNotNullAttribute : Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>Initializes the attribute with a field or property member.</summary>
    /// <param name="member">
    /// The field or property member that is promised to be not-null.
    /// </param>
    #pragma warning disable CA1019 // Define accessors for attribute arguments
    public MemberNotNullAttribute(string member) {
      Members = new[] { member };
      }
    #pragma warning restore CA1019 // Define accessors for attribute arguments

    /// <summary>Initializes the attribute with the list of field and property members.</summary>
    /// <param name="members">
    /// The list of field and property members that are promised to be not-null.
    /// </param>
    public MemberNotNullAttribute(params string[] members) {
      Members = members;
    }

    /*==========================================================================================================================
    | PROPERTY: MEMBERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>Gets field or property member names.</summary>
    public string[] Members { get; }

  }
}
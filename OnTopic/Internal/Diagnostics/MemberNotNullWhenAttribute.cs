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
  ///   Specifies that the method or property will ensure that the listed field and property members have not-null values when
  ///   returning with the specified return value condition.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
  internal sealed class MemberNotNullWhenAttribute: Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>Initializes the attribute with the specified return value condition and a field or property member.</summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the associated parameter will not be null.
    /// </param>
    /// <param name="member">
    /// The field or property member that is promised to be not-null.
    /// </param>
    #pragma warning disable CA1019 // Define accessors for attribute arguments
    public MemberNotNullWhenAttribute(bool returnValue, string member) {
      ReturnValue = returnValue;
      Members = new[] { member };
    }
    #pragma warning restore CA1019 // Define accessors for attribute arguments

    /// <summary>
    ///   Initializes the attribute with the specified return value condition and list of field and property members.
    /// </summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the associated parameter will not be null.
    /// </param>
    /// <param name="members">
    /// The list of field and property members that are promised to be not-null.
    /// </param>
    public MemberNotNullWhenAttribute(bool returnValue, params string[] members) {
      ReturnValue = returnValue;
      Members = members;
    }

    /*==========================================================================================================================
    | PROPERTY: RETURN VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>Gets the return value condition.</summary>
    public bool ReturnValue { get; }

    /*==========================================================================================================================
    | PROPERTY: MEMBERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>Gets field or property member names.</summary>
    public string[] Members { get; }

  }
}
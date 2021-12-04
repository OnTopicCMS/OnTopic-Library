/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using OnTopic.ViewModels;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: MEMBER ACCESSOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing settable methods and gettable methods.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  public class MemberAccessorViewModel {

    public MemberAccessorViewModel() { }
    public int? NullableProperty { get; set; }
    public int NonNullableProperty { get; set; }
    public Type NonNullableReferenceGetter { get; set; } = typeof(MemberAccessorViewModel);
    public int? ReadOnlyProperty { get; }
    public int WriteOnlyProperty { set { } }
    public int GetMethod() => 0;
    public int InvalidGetMethod(int value) => value;
    public void SetMethod(int value) => _ = value;
    public void InvalidSetMethod(int value, int newValue) => _ = value == newValue;

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

#pragma warning disable CA1024 // Use properties where appropriate
#pragma warning disable CA1044 // Properties should not be write only
#pragma warning disable CA1822 // Mark members as static

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

    private int? _methodValue;

    public MemberAccessorViewModel() { }
    public int? NullableProperty { get; set; }
    public int NonNullableProperty { get; set; }
    public Type NonNullableReferenceGetter { get; set; } = typeof(MemberAccessorViewModel);
    public int? ReadOnlyProperty { get; }
    public int WriteOnlyProperty { set { } }
    public int? GetMethod() => _methodValue;
    public int InvalidGetMethod(int value) => value;
    public void SetMethod(int? value) => _methodValue = value;
    public void InvalidSetMethod(int value, int newValue) => _ = value == newValue;

  } //Class
} //Namespace

#pragma warning restore CA1024 // Use properties where appropriate
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore CA1044 // Properties should not be write only
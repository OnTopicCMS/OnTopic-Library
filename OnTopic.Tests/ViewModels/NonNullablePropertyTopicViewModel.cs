/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: NON-NULLABLE PROPERTY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing the mapping of non-nullable properties.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class NonNullablePropertyTopicViewModel {

    public string NonNullableString { get; set; } = "";

    public int NonNullableInteger { get; set; }

    public double NonNullableDouble { get; set; }

    public bool NonNullableBoolean { get; set; }

    public DateTime NonNullableDateTime { get; set; }

    public Uri NonNullableUrl { get; set; } = new("https://github.com/");

  } //Class
} //Namespace
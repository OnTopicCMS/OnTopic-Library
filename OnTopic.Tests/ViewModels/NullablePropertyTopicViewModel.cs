/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: NULLABLE PROPERTY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing the mapping of nullable properties.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class NullablePropertyTopicViewModel {

    public string? NullableString { get; set; }

    public int? NullableInteger { get; set; }

    public double? NullableDouble { get; set; }

    public bool? NullableBoolean { get; set; }

    public DateTime? NullableDateTime { get; set; }

    public Uri? NullableUrl { get; set; }

    public string? Title { get; set; }

    public bool? IsHidden { get; set; }

    public DateTime? LastModified { get; set; }


  } //Class
} //Namespace
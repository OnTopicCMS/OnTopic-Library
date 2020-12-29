/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: RECORD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple view model with a single property (<see cref="Key"/>), implemented as a <c>record</c> instead of a <c>
  ///   class</c>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Intended to validate that a <c>record</c> can be mapped using the <see cref="ITopicMappingService"/>.
  ///   </para>
  ///   <para>
  ///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  ///   </para>
  /// </remarks>
  public record RecordTopicViewModel {

    public string? Key { get; set; }

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: DISABLE MAPPING
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple view model with a single property (<see cref="Key" />) which is annotated with the <see
  ///   cref="DisableMappingAttribute"/>.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class DisableMappingTopicViewModel {

    [DisableMapping]
    public string? Key { get; set; }

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: MAP AS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple view model with two associations—a relationship collection and a topic reference—annotated with the
  ///   <see cref="MapAsAttribute"/>.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class MapAsTopicViewModel {

    [MapAs(typeof(AscendentTopicViewModel))]
    public KeyOnlyTopicViewModel? TopicReference { get; set; }

    [MapAs<AscendentTopicViewModel>()]
    public Collection<KeyOnlyTopicViewModel> Relationships { get; } = new();

  } //Class
} //Namespace
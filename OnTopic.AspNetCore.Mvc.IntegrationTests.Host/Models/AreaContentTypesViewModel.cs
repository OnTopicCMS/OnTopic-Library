/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.AspNetCore.Mvc.IntegrationTests.Models {

  /*============================================================================================================================
  | VIEW MODEL: AREA CONTENT TYPES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly typed view model for a topic with the <see cref="Topic.ContentType"/> of <c>AreaContentTypes</c>.
  /// </summary>
  /// <remarks>
  ///   This is used by the <see cref="TopicViewLocationExpander"/> test to differentiate between content types placed in <c>
  ///   /Areas/Area/ContentType/</c> and <c>/Area/Area/ContentTypes/</c>. If they used the same <see cref="Topic.ContentType"/>
  ///   then there would be no way to distinguish between these placements.
  /// </remarks>
  public class AreaContentTypesViewModel {

  } //Class
} //Namespace
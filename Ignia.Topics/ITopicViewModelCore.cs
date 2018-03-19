/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ignia.Topics {

  /*============================================================================================================================
  | INTERFACE: TOPIC VIEW MODEL CORE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for feeding views.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     It is not strictly required that topic view models implement the <see cref="ITopicViewModelCore"/> interface for the
  ///     default <see cref="TopicMappingService"/> to correctly identify and map <see cref="Topic"/>s to topic view models.
  ///     That said, the interface does define properties that presentation infrastructure may expect. As a result, if it isn't
  ///     provided via the public interface then it will instead need to be defined in some other way.
  ///   </para>
  ///   <para>
  ///     For instance, in the default MVC library, the <c>TopicViewResult</c> class requires that the <see
  ///     cref="Topic.ContentType"/> and <see cref="Topic.View"/> be supplied separately if they're not provided as part of a
  ///     <see cref="ITopicViewModelCore"/>. The exact details of this will obviously vary based on the implementation of the
  ///     presentation layer and any supporting libraries.
  ///   </para>
  /// </remarks>
  public interface ITopicViewModelCore {

    public string Key { get; set; }
    public string ContentType { get; set; }
    public string View { get; set; }
    public bool IsHidden { get; set; }

  } //Class
} //Namespace

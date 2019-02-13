/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Mapping;

namespace Ignia.Topics {

  /*============================================================================================================================
  | INTERFACE: TYPE LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides an interface for looking up <see cref="Type"/>s by string.
  /// </summary>
  /// <remarks>
  ///   Various areas of the OnTopic library require looking up <see cref="Type"/>s dynamically. For instance, the <see
  ///   cref="TopicFactory"/> and the <see cref="TopicMappingService"/> both determine whether or not there is a <see
  ///   cref="Type"/> corresponding to the <c>ContentType</c> (though the former is looking for a <see cref="Topic"/>, and the
  ///   latter is looking for a data transfer object). The <see cref="ITypeLookupService"/> provides a standard interface for
  ///   libraries capable of providing this functionality, allowing them to be injected into other services.
  /// </remarks>
  public interface ITypeLookupService {

    /*==========================================================================================================================
    | METHOD: LOOKUP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the requested <see cref="Type"/>.
    /// </summary>
    /// <param name="typeName">The name of the <see cref="Type"/> to retrieve.</param>
    Type Lookup(string typeName);

  }
}

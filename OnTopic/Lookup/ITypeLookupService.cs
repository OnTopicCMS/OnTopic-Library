/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Mapping;

namespace OnTopic.Lookup {

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
    ///   Attempts to retrieve a <see cref="Type"/> based on one or more supplied <paramref name="typeNames"/>.
    /// </summary>
    /// <remarks>
    ///   No matter how many <paramref name="typeNames"/> are entered, at most one <see cref="Type"/> will be returned. Each
    ///   subsequent <paramref name="typeNames"/> is treated as a fallback in case the previous one cannot be located. As such,
    ///   the <paramref name="typeNames"/> offers a way for callers to provide a prioritized list of fallbacks. This is useful
    ///   for scenarios where there are multiple accepted naming conventions, or there's a global default that can be accepted.
    /// </remarks>
    /// <param name="typeNames">
    ///   The name of the <see cref="Type"/> to retrieve. If multiple names are supplied, then the first match is returned.
    /// </param>
    /// <returns>
    ///   A <see cref="Type"/> corresponding to one of the specified <paramref name="typeNames"/>, if available.
    /// </returns>
    Type? Lookup(params string[] typeNames);

  } //Class
} //Namespace
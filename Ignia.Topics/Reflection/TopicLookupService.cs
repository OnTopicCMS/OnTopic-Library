/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace Ignia.Topics.Reflection {

  /*============================================================================================================================
  | CLASS: TOPIC LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicLookupService"/> will search all assemblies for <see cref="Type"/>s that derive from <see
  ///   cref="Topic"/>.
  /// </summary>
  public class TopicLookupService : DynamicTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="TopicLookupService"/>.
    /// </summary>
    internal TopicLookupService() : base(
      t => typeof(Topic).IsAssignableFrom(t),
      typeof(Topic)
    ) {}

  } //Class
} //Namespace
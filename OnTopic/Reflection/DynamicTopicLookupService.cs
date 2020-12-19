/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace OnTopic.Reflection {

  /*============================================================================================================================
  | CLASS: DYNAMIC TOPIC LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="DynamicTopicLookupService"/> will search all assemblies for <see cref="Type"/>s that derive from <see
  ///   cref="Topic"/>.
  /// </summary>
  public class DynamicTopicLookupService : DynamicTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="DynamicTopicLookupService"/>.
    /// </summary>
    public DynamicTopicLookupService() : base(
      t => typeof(Topic).IsAssignableFrom(t),
      typeof(Topic)
    ) { }

  } //Class
} //Namespace
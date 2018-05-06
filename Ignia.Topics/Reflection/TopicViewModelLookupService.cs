/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace Ignia.Topics.Reflection {

  /*============================================================================================================================
  | CLASS: TOPIC VIEW MODEL LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicViewModelLookupService"/> will search all assemblies for <see cref="Type"/>s that end with
  ///   "TopicViewModel"
  /// </summary>
  public class TopicViewModelLookupService : DynamicTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="TopicLookupService"/>.
    /// </summary>
    internal TopicViewModelLookupService() : base(
      t => t.Name.EndsWith("TopicViewModel"),
      typeof(object)
    ) {}

  } //Class
} //Namespace
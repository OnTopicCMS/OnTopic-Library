﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Lookup {

  /*============================================================================================================================
  | CLASS: DYNAMIC TOPIC VIEW MODEL LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="DynamicTopicViewModelLookupService"/> will search all assemblies for <see cref="Type"/>s that end with
  ///   "TopicViewModel"
  /// </summary>
  public class DynamicTopicViewModelLookupService : DynamicTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="DynamicTopicLookupService"/>.
    /// </summary>
    public DynamicTopicViewModelLookupService() : base(
      t => t.Name.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase)
    ) { }

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace Ignia.Topics.Reflection {

  /*============================================================================================================================
  | CLASS: TOPIC BINDING MODEL LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="DynamicTopicBindingModelLookupService"/> will search all assemblies for <see cref="Type"/>s that end with
  ///   "TopicBindingModel"
  /// </summary>
  public class DynamicTopicBindingModelLookupService : DynamicTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="DynamicTopicBindingModelLookupService"/>.
    /// </summary>
    public DynamicTopicBindingModelLookupService() : base(
      t => t.Name.EndsWith("TopicBindingModel", StringComparison.InvariantCultureIgnoreCase),
      typeof(object)
    ) { }

  } //Class
} //Namespace
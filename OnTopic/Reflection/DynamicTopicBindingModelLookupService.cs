/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Models;

namespace OnTopic.Reflection {

  /*============================================================================================================================
  | CLASS: DYNAMIC TOPIC BINDING MODEL LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="DynamicTopicBindingModelLookupService"/> will search all assemblies for <see cref="Type"/>s that
  ///   implement <see cref="ITopicBindingModel"/>.
  /// </summary>
  public class DynamicTopicBindingModelLookupService : DynamicTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="DynamicTopicBindingModelLookupService"/>.
    /// </summary>
    public DynamicTopicBindingModelLookupService() : base(
      t => typeof(ITopicBindingModel).IsAssignableFrom(t),
      typeof(object)
    ) { }

  } //Class
} //Namespace
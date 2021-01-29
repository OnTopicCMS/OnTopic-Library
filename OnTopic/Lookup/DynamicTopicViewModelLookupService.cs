/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Mapping;

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

    /*==========================================================================================================================
    | METHOD: LOOKUP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    /// <remarks>
    ///   The <see cref="DynamicTopicViewModelLookupService"/> version of <see cref="Lookup(String)"/> will first look for
    ///   the exact <paramref name="typeName"/>. If that fails, and the <paramref name="typeName"/> ends with <c>TopicViewModel
    ///   </c>, then it will automatically fall back to look for the <paramref name="typeName"/> with the <c>ViewModel</c>
    ///   suffix. If that cannot be found, it will return the <see cref="StaticTypeLookupService.DefaultType"/> of <see cref="
    ///   Object"/>. This allows implementors to use the shorter name, if preferred, without breaking compatibility with
    ///   implementations which default to looking for <c>TopicViewModel</c>, such as the <see cref="TopicMappingService"/>.
    /// </remarks>
    public override Type? Lookup(string typeName) {
      if (typeName is null) {
        return DefaultType;
      }
      else if (Contains(typeName)) {
        return base.Lookup(typeName);
      }
      else if (typeName.EndsWith("TopicViewModel", StringComparison.OrdinalIgnoreCase)) {
        return base.Lookup(typeName.Replace("TopicViewModel", "ViewModel", StringComparison.CurrentCultureIgnoreCase));
      }
      return DefaultType;
    }

  } //Class
} //Namespace
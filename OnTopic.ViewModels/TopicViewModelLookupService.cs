/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using OnTopic.Lookup;
using OnTopic.ViewModels.Items;

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | CLASS: TYPE INDEX
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicViewModelLookupService"/> can be configured to provide a lookup of .
  /// </summary>
  public class TopicViewModelLookupService : StaticTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="DefaultTopicLookupService"/>. Optionally accepts a list of <see
    ///   cref="Type"/> instances and a default <see cref="Type"/> value.
    /// </summary>
    /// <remarks>
    ///   Any <see cref="Type"/> instances submitted via <paramref name="types"/> should be unique by <see
    ///   cref="MemberInfo.Name"/>; if they are not, they will be removed.
    /// </remarks>
    /// <param name="types">The list of <see cref="Type"/> instances to expose as part of this service.</param>
    public TopicViewModelLookupService(IEnumerable<Type>? types = null) : base(types) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure local view models are accounted for
      \-----------------------------------------------------------------------------------------------------------------------*/
      TryAdd(typeof(ContentListTopicViewModel));
      TryAdd(typeof(IndexTopicViewModel));
      TryAdd(typeof(NavigationTopicViewModel));
      TryAdd(typeof(PageGroupTopicViewModel));
      TryAdd(typeof(PageTopicViewModel));
      TryAdd(typeof(SectionTopicViewModel));
      TryAdd(typeof(SlideshowTopicViewModel));
      TryAdd(typeof(TopicViewModel));
      TryAdd(typeof(VideoTopicViewModel));

      /*------------------------------------------------------------------------------------------------------------------------
      | Add item types
      \-----------------------------------------------------------------------------------------------------------------------*/
      TryAdd(typeof(ItemTopicViewModel));
      TryAdd(typeof(ContentItemTopicViewModel));
      TryAdd(typeof(ListTopicViewModel));
      TryAdd(typeof(LookupListItemTopicViewModel));
      TryAdd(typeof(SlideTopicViewModel));

      /*------------------------------------------------------------------------------------------------------------------------
      | Add support types
      \-----------------------------------------------------------------------------------------------------------------------*/

    }

    /*==========================================================================================================================
    | METHOD: LOOKUP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    /// <remarks>
    ///   The <see cref="TopicViewModelLookupService"/> version of <see cref="Lookup(String)"/> will first look for
    ///   the exact <paramref name="typeName"/>. If that fails, and the <paramref name="typeName"/> ends with <c>TopicViewModel
    ///   </c>, then it will automatically fall back to look for the <paramref name="typeName"/> with the <c>ViewModel</c>
    ///   suffix. If that cannot be found, it will return the <see cref="StaticTypeLookupService.DefaultType"/> of <see cref="
    ///   TopicViewModel"/>. This allows implementors to use the shorter name, if preferred, without breaking compatibility with
    ///   implementations which default to looking for <c>TopicViewModel</c>, such as the <see cref="TopicMappingService"/>.
    ///   While this convention is not used by the <see cref="ViewModels"/>, this fallback provides support for derived classes
    ///   which may prefer that convention.
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
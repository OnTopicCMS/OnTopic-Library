/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Reflection;
using OnTopic.Lookup;

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
      TryAdd(typeof(LookupListItemTopicViewModel));
      TryAdd(typeof(SlideTopicViewModel));

      /*------------------------------------------------------------------------------------------------------------------------
      | Add obsolete types
      >-------------------------------------------------------------------------------------------------------------------------
      | These will be removed in the next major version of OnTopic.
      \-----------------------------------------------------------------------------------------------------------------------*/
#pragma warning disable CS0618 // Type or member is obsolete
      TryAdd(typeof(ListTopicViewModel));
      #pragma warning restore CS0618 // Type or member is obsolete

      /*------------------------------------------------------------------------------------------------------------------------
      | Add support types
      \-----------------------------------------------------------------------------------------------------------------------*/

    }

  } //Class
} //Namespace
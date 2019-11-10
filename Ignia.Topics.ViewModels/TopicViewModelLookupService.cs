/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;

namespace Ignia.Topics.ViewModels {

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
    /// <param name="defaultType">The default type to return if no match can be found. Defaults to object.</param>
    public TopicViewModelLookupService(IEnumerable<Type>? types = null, Type? defaultType = null) :
      base(types, defaultType?? typeof(TopicViewModel)) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure local view models are accounted for
      \-----------------------------------------------------------------------------------------------------------------------*/
      AddIfMissing(typeof(ContentItemTopicViewModel));
      AddIfMissing(typeof(ContentListTopicViewModel));
      AddIfMissing(typeof(IndexTopicViewModel));
      AddIfMissing(typeof(ItemTopicViewModel));
      AddIfMissing(typeof(ListTopicViewModel));
      AddIfMissing(typeof(LookupListItemTopicViewModel));
      AddIfMissing(typeof(NavigationTopicViewModel));
      AddIfMissing(typeof(PageGroupTopicViewModel));
      AddIfMissing(typeof(PageTopicViewModel));
      AddIfMissing(typeof(SectionTopicViewModel));
      AddIfMissing(typeof(SlideTopicViewModel));
      AddIfMissing(typeof(SlideshowTopicViewModel));
      AddIfMissing(typeof(TopicViewModel));
      AddIfMissing(typeof(VideoTopicViewModel));

      /*------------------------------------------------------------------------------------------------------------------------
      | Function: Add If Missing
      \-----------------------------------------------------------------------------------------------------------------------*/
      void AddIfMissing(Type type) {
        if (!Contains(type.Name)) {
          Add(type);
        }
      }

    }

  } //Class
} //Namespace
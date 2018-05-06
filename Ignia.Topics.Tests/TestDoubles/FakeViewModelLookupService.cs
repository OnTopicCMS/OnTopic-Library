/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ignia.Topics.Tests.ViewModels;
using Ignia.Topics.ViewModels;

namespace Ignia.Topics.Tests.TestDoubles {

  /*============================================================================================================================
  | CLASS: FAKE TOPIC LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides access to derived types of <see cref="Topic"/> classes.
  /// </summary>
  /// <remarks>
  ///   Allows testing of services that depend on <see cref="ITypeLookupService"/> without using expensive reflection.
  /// </remarks>
  internal class FakeViewModelLookupService: StaticTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the <see cref="FakeTopicLookupService"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="FakeTopicLookupService"/>.</returns>
    internal FakeViewModelLookupService(): base(null, typeof(object)) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Add out-of-the-box view models
      \-----------------------------------------------------------------------------------------------------------------------*/
      Add(typeof(ContentItemTopicViewModel));
      Add(typeof(ContentListTopicViewModel));
      Add(typeof(IndexTopicViewModel));      Add(typeof(ItemTopicViewModel));      Add(typeof(LookupListItemTopicViewModel));      Add(typeof(NavigationTopicViewModel));
      Add(typeof(PageGroupTopicViewModel));      Add(typeof(PageTopicViewModel));
      Add(typeof(SectionTopicViewModel));      Add(typeof(SlideshowTopicViewModel));      Add(typeof(SlideTopicViewModel));      Add(typeof(TopicViewModel));      Add(typeof(VideoTopicViewModel));

      /*------------------------------------------------------------------------------------------------------------------------
      | Add test specific view models
      \-----------------------------------------------------------------------------------------------------------------------*/
      Add(typeof(CircularTopicViewModel));      Add(typeof(DefaultValueTopicViewModel));      Add(typeof(FilteredTopicViewModel));      Add(typeof(FlattenChildrenTopicViewModel));      Add(typeof(MetadataLookupTopicViewModel));      Add(typeof(MethodBasedViewModel));      Add(typeof(MinimumLengthPropertyTopicViewModel));      Add(typeof(RequiredObjectTopicViewModel));      Add(typeof(RequiredTopicViewModel));      Add(typeof(SampleTopicViewModel));

    }

  }
}

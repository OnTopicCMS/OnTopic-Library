/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Tests.ViewModels;
using OnTopic.Tests.ViewModels.Metadata;
using OnTopic.ViewModels;

namespace OnTopic.Tests.TestDoubles {

  /*============================================================================================================================
  | CLASS: FAKE TOPIC LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides access to derived types of <see cref="Topic"/> classes.
  /// </summary>
  /// <remarks>
  ///   Allows testing of services that depend on <see cref="ITypeLookupService"/> without using expensive reflection.
  /// </remarks>
  public class FakeViewModelLookupService: TopicViewModelLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the <see cref="FakeViewModelLookupService"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="FakeViewModelLookupService"/>.</returns>
    public FakeViewModelLookupService() : base(null, typeof(object)) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Add test specific view models
      \-----------------------------------------------------------------------------------------------------------------------*/
      Add(typeof(AmbiguousRelationTopicViewModel));
      Add(typeof(AscendentSpecializedTopicViewModel));
      Add(typeof(AscendentTopicViewModel));
      Add(typeof(CircularTopicViewModel));
      Add(typeof(DefaultValueTopicViewModel));
      Add(typeof(DescendentSpecializedTopicViewModel));
      Add(typeof(DescendentTopicViewModel));
      Add(typeof(FilteredTopicViewModel));
      Add(typeof(FlattenChildrenTopicViewModel));
      Add(typeof(InheritedPropertyTopicViewModel));
      Add(typeof(KeyOnlyTopicViewModel));
      Add(typeof(MapToParentTopicViewModel));
      Add(typeof(MethodBasedViewModel));
      Add(typeof(MinimumLengthPropertyTopicViewModel));
      Add(typeof(NestedTopicViewModel));
      Add(typeof(PropertyAliasTopicViewModel));
      Add(typeof(RelatedEntityTopicViewModel));
      Add(typeof(RelationTopicViewModel));
      Add(typeof(RelationWithChildrenTopicViewModel));
      Add(typeof(RequiredObjectTopicViewModel));
      Add(typeof(RequiredTopicViewModel));
      Add(typeof(TopicReferenceAttributeTopicViewModel));
      Add(typeof(TopicReferenceTopicViewModel));

      /*------------------------------------------------------------------------------------------------------------------------
      | Add test specific metadata view models
      \-----------------------------------------------------------------------------------------------------------------------*/
      Add(typeof(AttributeDescriptorTopicViewModel));
      Add(typeof(ContentTypeDescriptorTopicViewModel));
      Add(typeof(MetadataLookupTopicViewModel));
      Add(typeof(TextAttributeTopicViewModel));

    }

  } //Class
} //Namespace
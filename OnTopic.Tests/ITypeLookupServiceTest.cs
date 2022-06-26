/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Lookup;
using OnTopic.Tests.TestDoubles;
using OnTopic.Tests.ViewModels;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TYPE LOOKUP SERVICE TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="ITypeLookupService"/> interface and its implementations, including the
  ///   <see cref="StaticTypeLookupService"/> and <see cref="DynamicTypeLookupService"/>.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class ITypeLookupServiceTest {

    /*==========================================================================================================================
    | TEST: COMPOSITE: LOOKUP VALID TYPE: RETURNS TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="CompositeTypeLookupService"/> with two instances of a <see cref="ITypeLookupService"/> and
    ///   confirms that it returns the expected <see cref="Type"/> for a <see cref="ITypeLookupService.Lookup(String[])"/>
    ///   query.
    /// </summary>
    [Fact]
    public void Composite_LookupValidType_ReturnsType() {

      var lookupServiceA        = new FakeViewModelLookupService();
      var lookupServiceB        = new TopicViewModelLookupService();
      var compositeLookup       = new CompositeTypeLookupService(lookupServiceA, lookupServiceB);

      Assert.Equal(typeof(SlideshowTopicViewModel), compositeLookup.Lookup(nameof(SlideshowTopicViewModel)));
      Assert.Equal(typeof(MapToParentTopicViewModel), compositeLookup.Lookup(nameof(MapToParentTopicViewModel)));
      Assert.Null(compositeLookup.Lookup(nameof(Topic)));

    }

    /*==========================================================================================================================
    | TEST: DYNAMIC TOPIC VIEW MODEL LOOKUP SERVICE: LOOKUP TOPIC VIEW MODEL: RETURNS FALLBACK VIEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="DynamicTopicViewModelLookupService"/> and requests a type with the <c>TopicViewModel</c>
    ///   suffix; confirms it correctly falls back to a type with the <c>ViewModel</c> suffix.
    /// </summary>
    [Fact]
    public void DynamicTopicViewModelLookupService_LookupTopicViewModel_ReturnsFallbackViewModel() {

      var lookupService         = new DynamicTopicViewModelLookupService();
      var topicViewModel        = lookupService.Lookup("FallbackTopicViewModel", "FallbackViewModel");

      Assert.Equal(typeof(FallbackViewModel), topicViewModel);

    }

    /*==========================================================================================================================
    | TEST: DEFAULT TOPIC VIEW MODEL LOOKUP SERVICE: LOOKUP TOPIC VIEW MODEL: RETURNS FALLBACK VIEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicViewModelLookupService"/> and requests a type with the <c>TopicViewModel</c>
    ///   suffix; confirms it correctly falls back to a type with the <c>ViewModel</c> suffix.
    /// </summary>
    [Fact]
    public void TopicViewModelLookupService_LookupTopicViewModel_ReturnsFallbackViewModel() {

      var lookupService         = new FakeViewModelLookupService();
      var topicViewModel        = lookupService.Lookup("FallbackTopicViewModel", "FallbackViewModel");

      Assert.Equal(typeof(FallbackViewModel), topicViewModel);

    }

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Attributes;
using OnTopic.Data.Caching;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.Tests.Fixtures;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC REPOSITORY TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="AttributeCollection"/> class.
  /// </summary>
  /// <remarks>
  ///   These tests not only validate that the <see cref="StubTopicRepository"/> is functioning as expected, but also that the
  ///   underlying <see cref="TopicRepository"/> functions are also operating correctly.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  [Collection("Shared Repository")]
  public class ITopicRepositoryTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ITopicRepositoryTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="StubTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph.
    /// </remarks>
    public ITopicRepositoryTest(TopicInfrastructureFixture<StubTopicRepository> fixture) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(fixture, nameof(fixture));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository = fixture.CachedTopicRepository;

    }

    /*==========================================================================================================================
    | TEST: LOAD: DEFAULT: RETURNS ROOT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads the default topic and ensures there are the expected number of children.
    /// </summary>
    [Fact]
    public void Load_Default_ReturnsTopicTopic() {

      var rootTopic             = _topicRepository.Load();

      Assert.Equal(2, rootTopic?.Children.Count);
      Assert.Equal("Configuration", rootTopic?.Children.First().Key);
      Assert.Equal("Web", rootTopic?.Children.Last().Key);

    }

    /*==========================================================================================================================
    | TEST: LOAD: VALID UNIQUE KEY: RETURNS CORRECT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads topics and ensures there are the expected number of children.
    /// </summary>
    [Fact]
    public void Load_ValidUniqueKey_ReturnsCorrectTopic() =>
      Assert.Equal("Page", _topicRepository.Load("Root:Configuration:ContentTypes:Page")?.Key);

    /*==========================================================================================================================
    | TEST: LOAD: INVALID UNIQUE KEY: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads invalid topic key and ensures a null is returned.
    /// </summary>
    [Fact]
    public void Load_InvalidUniqueKey_ReturnsTopic() =>
      Assert.Null(_topicRepository.Load("Root:Configuration:ContentTypes:InvalidContentType"));

    /*==========================================================================================================================
    | TEST: LOAD: VALID TOPIC ID: RETURNS CORRECT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads topic by ID and ensures it is found.
    /// </summary>
    [Fact]
    public void Load_ValidTopicId_ReturnsCorrectTopic() {

      var topic                 = _topicRepository.Load(11111);

      Assert.NotNull(topic);
      Assert.Equal("Web_1_1_1_1", topic?.Key);

    }

    /*==========================================================================================================================
    | TEST: LOAD: INVALID TOPIC ID: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads topic by an incorrect ID and ensures it a null is returned.
    /// </summary>
    [Fact]
    public void Load_InvalidTopicId_ReturnsNull() =>
      Assert.Null(_topicRepository.Load(9999999));

    /*==========================================================================================================================
    | TEST: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Saves topics and ensures their identifiers are properly set.
    /// </summary>
    [Fact]
    public void Save() {

      var topic                 = new Topic("Test", "Page");
      var child                 = new Topic("Child", "Page", topic);

      _topicRepository.Save(topic);

      Assert.NotEqual(-1, topic.Id);
      Assert.Equal(-1, child.Id);

      _topicRepository.Save(topic, true);

      Assert.NotEqual(-1, child.Id);

    }

    /*==========================================================================================================================
    | TEST: MOVE: TO NEW PARENT: CONFIRMED MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Moves topics and ensures their parents are correctly set.
    /// </summary>
    [Fact]
    public void Move_ToNewParent_ConfirmedMove() {

      var source                = new Topic("OriginalParent", "Page");
      var destination           = new Topic("NewParent", "Page");
      var topic                 = new Topic("Topic", "Page", source);
      _                         = new Topic("Sibling", "Page", source);

      _topicRepository.Move(topic, destination);

      Assert.Equal(topic.Parent, destination);
      Assert.Single(source.Children);
      Assert.Single(destination.Children);

    }

    /*==========================================================================================================================
    | TEST: MOVE: TO NEW SIBLING: CONFIRMED MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Moves topic next to a different sibling and ensures it ends up in the correct location.
    /// </summary>
    [Fact]
    public void Move_ToNewSibling_ConfirmedMove() {

      var parent                = new Topic("OriginalParent", "Page");
      var topic                 = new Topic("Topic", "Page", parent);
      var sibling               = new Topic("Sibling", "Page", parent);

      _topicRepository.Move(topic, parent, sibling);

      Assert.Equal(topic.Parent, parent);
      Assert.Equal(2, parent.Children.Count);
      Assert.Equal("Sibling", parent.Children.First().Key);
      Assert.Equal("Topic", parent.Children[1].Key);

    }

    /*==========================================================================================================================
    | TEST: DELETE: TOPIC: REMOVED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes a topic to ensure it is properly removed.
    /// </summary>
    [Fact]
    public void Delete_Topic_Removed() {

      var parent                = new Topic("OriginalParent", "Page");
      var topic                 = new Topic("Topic", "Page", parent);
      _                         = new Topic("child", "Page", topic);

      _topicRepository.Delete(topic, true);

      Assert.Empty(parent.Children);

    }

    /*==========================================================================================================================
    | TEST: DELETE: DELETE EVENT: IS FIRED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> and then immediately deletes it. Ensures that the <see cref="ITopicRepository.
    ///   TopicDeleted"/> is fired, even though the original event is fired from the underlying <see cref="StubTopicRepository"/>
    ///   and not the immediate <see cref="CachedTopicRepository"/>.
    /// </summary>
    [Fact]
    public void Delete_DeleteEvent_IsFired() {

      var topic                 = new Topic("Test", "Page");
      var hasFired              = false;

      _topicRepository.Save(topic);
      _topicRepository.TopicDeleted += eventHandler;
      _topicRepository.Delete(topic);

      Assert.True(hasFired);

      void eventHandler(object? sender, TopicEventArgs eventArgs) => hasFired = true;

    }

  } //Class
} //Namespace
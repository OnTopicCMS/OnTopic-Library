/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.Contracts;
using Ignia.Topics.Collections;

namespace Ignia.Topics.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC DATA PROVIDER BASE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a base abstract class for taxonomy data providers.
  /// </summary>
  [ContractClass(typeof(TopicRepositoryBaseContract))]
  public abstract class TopicRepositoryBase {

    /*==========================================================================================================================
    | EVENT HANDLERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates the <see cref="DeleteEventArgs"/> event handler.
    /// </summary>
    public event EventHandler<DeleteEventArgs>        DeleteEvent;

    /// <summary>
    ///   Instantiates the <see cref="MoveEventArgs"/> event handler.
    /// </summary>
    public event EventHandler<MoveEventArgs>          MoveEvent;

    /// <summary>
    ///   Instantiates the <see cref="RenameEventArgs"/> event handler.
    /// </summary>
    public event EventHandler<RenameEventArgs>        RenameEvent;

    /*==========================================================================================================================
    | GET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a collection of Content Type Descriptor objects from the configuration section of the data provider.
    /// </summary>
    public abstract ContentTypeDescriptorCollection GetContentTypeDescriptors();

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads a topic (and, optionally, all of its descendents) based on the specified unique identifier.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    public abstract Topic Load(int topicId, bool isRecursive = true);

    /// <summary>
    ///   Loads a topic (and, optionally, all of its descendents) based on the specified key name.
    /// </summary>
    /// <param name="topicKey">The topic key.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    public abstract Topic Load(string topicKey = null, bool isRecursive = true);

    /// <summary>
    ///   Loads a specific version of a topic based on its version.
    /// </summary>
    /// <remarks>
    ///   This overload does not accept an argument for recursion; it will only load a single instance of a version. Further,
    ///   it will only load versions for which the unique identifier is known.
    /// </remarks>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="version">The version.</param>
    /// <returns>A topic object.</returns>
    public abstract Topic Load(int topicId, DateTime version);

    /*==========================================================================================================================
    | ###TODO JJC080314: An overload to Load() should be created to accept an XmlDocument or XmlNode based on the proposed
    | Import/Export schema.
    >---------------------------------------------------------------------------------------------------------------------------
    | ###NOTE JJC080313: If the topic already exists, return the existing node, by calling its Merge() function. Otherwise,
    | construct a new node using its XmlNode constructor.
    >---------------------------------------------------------------------------------------------------------------------------
      public static Topic Load(XmlNode node, ImportStrategy importStrategy = ImportStrategy.Merge) {
      //Process XML
      //Construct children objects
      //###NOTE JJC080314: May need to cross-reference with Load() and/or TopicRepository to validate against whatever objects
      //are already created and available.
      }
    \-------------------------------------------------------------------------------------------------------------------------*/

    /*==========================================================================================================================
    | METHOD: ROLLBACK
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Rolls back the current topic to a particular point in its version history by reloading legacy attributes and then
    ///   saving the new version.
    /// </summary>
    /// <param name="topic">The current version of the topic to rollback.</param>
    /// <param name="version">The selected Date/Time for the version to which to roll back.</param>
    /// <requires
    ///   description="The version requested for rollback does not exist in the version history."
    ///   exception="T:System.ArgumentNullException">
    ///   !VersionHistory.Contains(version)
    /// </requires>
    public void Rollback(Topic topic, DateTime version) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentException>(topic != null);
      Contract.Requires<ArgumentException>(version != null);

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      >-------------------------------------------------------------------------------------------------------------------------
      | ### NOTE JJC071417: We should use a code contract for this, but due to an intermittent error are throwing a standard
      | exception so we can include the version in the error message.
      \-----------------------------------------------------------------------------------------------------------------------*/
      /*
      Contract.Requires<ArgumentException>(
        !topic.VersionHistory.Contains(version),
        "The version requested for rollback does not exist in the version history"
      );
      */
      if (!topic.VersionHistory.Contains(version)) {
        throw new ArgumentException("The version requested for rollback ('" + version + "') does not exist in the version history", "version");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve topic from database
      \-----------------------------------------------------------------------------------------------------------------------*/
      var originalVersion = Load(topic.Id, version);
      Contract.Assume(originalVersion != null, "Assumes the originalVersion topic has been loaded from the repository.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Mark each attribute as dirty
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var attribute in originalVersion.Attributes) {
        if (!topic.Attributes.Contains(attribute.Key) || topic.Attributes.GetValue(attribute.Key) != attribute.Value) {
          attribute.IsDirty = true;
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Construct new AttributeCollection
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.Attributes.Clear();
      foreach (var attribute in originalVersion.Attributes) {
        topic.Attributes.Add(attribute);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Rename topic, if necessary
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Key == originalVersion.Key) {
        topic.Attributes.SetValue("Key", topic.Key, false);
      }
      else {
        topic.Key = originalVersion.Key;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure Parent, ContentType are maintained
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.Attributes.SetValue("ContentType", topic.ContentType, topic.ContentType != originalVersion.ContentType);
      topic.Attributes.SetValue("ParentId", topic.Parent.Id.ToString(), false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Save as new version
      \-----------------------------------------------------------------------------------------------------------------------*/
      Save(topic, false);

    }

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that saves topic attributes; also used for renaming a topic since name is stored as an attribute.
    /// </summary>
    /// <param name="topic">The topic object.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and save them as well.
    /// </param>
    /// <param name="isDraft">Boolean indicator as to the topic's publishing status.</param>
    /// <returns>The integer return value from the execution of the <c>topics_UpdateTopic</c> stored procedure.</returns>
    /// <requires description="The topic to save must be specified." exception="T:System.ArgumentNullException">topic != null</requires>
    /// <exception cref="ArgumentNullException">topic</exception>
    public virtual int Save(Topic topic, bool isRecursive = false, bool isDraft = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "topic");

      /*------------------------------------------------------------------------------------------------------------------------
      | Trigger event
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.OriginalKey != null && topic.OriginalKey != topic.Key) {
        var       args    = new RenameEventArgs(topic);
        RenameEvent?.Invoke(this, args);
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Perform reordering and/or move
      \---------------------------------------------------------------------------------------------------------------------*/
      if (topic.Parent != null && topic.Attributes.IsDirty("ParentId")) {
        var topicIndex = topic.Parent.Children.IndexOf(topic);
        if (topicIndex > 0) {
          ReorderSiblings(topic, topic.Parent.Children[topicIndex-1]);
        }
        else {
          ReorderSiblings(topic);
        }
        Move(topic, topic.Parent);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Reset original key
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.OriginalKey = null;
      return -1;

    }

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that supports moving a topic from one position to another.
    /// </summary>
    /// <param name="topic">The topic object to be moved.</param>
    /// <param name="target">A topic object under which to move the source topic.</param>
    /// <returns>Boolean value representing whether the operation completed successfully.</returns>
    /// <requires description="The topic to move must be provided." exception="T:System.ArgumentNullException">topic != null</requires>
    /// <requires description="The target under which to move the topic must be provided." exception="T:System.ArgumentNullException">
    ///   topic != null
    /// </requires>
    public virtual void Move(Topic topic, Topic target) {
      Contract.Requires<ArgumentNullException>(topic != null, "topic");
      Contract.Requires<ArgumentNullException>(target != null, "target");
      MoveEvent?.Invoke(this, new MoveEventArgs(topic, target));
      topic.Parent = target;
      ReorderSiblings(topic);
      //return true;
    }

    /// <summary>
    ///   Interface method that supports moving a topic from one position to another.
    /// </summary>
    /// <param name="topic">The topic object to be moved.</param>
    /// <param name="target">A topic object under which to move the source topic.</param>
    /// <param name="sibling">A topic object representing a sibling adjacent to which the topic should be moved.</param>
    /// <returns>Boolean value representing whether the operation completed successfully.</returns>
    public virtual void Move(Topic topic, Topic target, Topic sibling) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "topic");
      Contract.Requires<ArgumentNullException>(target != null, "target");
      Contract.Requires<ArgumentException>(topic != target, "A topic cannot be its own parent.");
      Contract.Requires<ArgumentException>(topic != sibling, "A topic cannot be moved relative to itself.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide cleanup related to moving topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      MoveEvent?.Invoke(this, new MoveEventArgs(topic, target));
      topic.Parent = target;
      ReorderSiblings(topic, sibling);

    }

    /*==========================================================================================================================
    | METHOD: REORDER SIBLINGS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Static method that updates the sort order of topics at a particular level.
    /// </summary>
    /// <param name="source">
    ///   The topic which is being moved.
    /// </param>
    /// <param name="sibling">
    ///   The topic object that if provided, represents the topic after which the source topic should be ordered.
    /// </param>
    /// <requires description="The source topic must be specified." exception="T:System.ArgumentNullException">
    ///   source != null
    /// </requires>
    /// <requires description="The source topic cannot be reordered relative to itself." exception="T:System.ArgumentException">
    ///   source != sibling
    /// </requires>
    private void ReorderSiblings(Topic source, Topic sibling = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentException>(source != sibling, "The source cannot be reordered relative to itself.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var parent = source.Parent;
      var sortOrder = 0;

      /*------------------------------------------------------------------------------------------------------------------------
      | If there is no sibling, inject the source at the beginning of the collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (sibling == null) {
        source.SortOrder = Int32.MaxValue;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through each topic to assign a new priority order
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var topic in parent.Children.Sorted) {
        // Assuming the topic isn't the source, or no sibling is present, increment the sortOrder
        if (topic != source || sibling == null) {
          topic.SortOrder = sortOrder++;
        }
        // If the topic is the sibling, then assign the next sortOrder to the source
        if (topic == sibling) {
          source.SortOrder = sortOrder++;
        }
      }

    }

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that deletes the provided topic from the tree
    /// </summary>
    /// <param name="topic">The topic object to delete.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and delete them as well.
    /// </param>
    /// <requires description="The topic to delete must be provided." exception="T:System.ArgumentNullException">topic != null</requires>
    /// <exception cref="ArgumentNullException">topic</exception>
    public virtual void Delete(Topic topic, bool isRecursive) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "topic");

      /*------------------------------------------------------------------------------------------------------------------------
      | Trigger event
      \-----------------------------------------------------------------------------------------------------------------------*/
      var         args    = new DeleteEventArgs(topic);
      DeleteEvent?.Invoke(this, args);

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove from parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Parent != null) {
        topic.Parent.Children.Remove(topic.Key);
      }

    }

  } // Class

} // Namespace

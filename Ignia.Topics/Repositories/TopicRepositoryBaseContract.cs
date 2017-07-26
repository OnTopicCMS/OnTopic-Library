/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.Contracts;

namespace Ignia.Topics.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC DATA PROVIDER BASE CONTRACT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines code contracts for the <c>TopicDataProviderBase</c> class' abstract methods <c>Load()</c> and <c>Move()</c>.
  /// </summary>
  [ContractClassFor(typeof(TopicRepositoryBase))]
  public abstract class TopicRepositoryBaseContract : TopicRepositoryBase {

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    ///   Loads a topic (and, optionally, all of its descendents) based on the specified unique identifier.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    public override Topic Load(int topicId, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      //No contractual requirements

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide dummy return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return new Topic();

    }

    /// <summary>
    ///   Loads a topic (and, optionally, all of its descendents) based on the specified key name.
    /// </summary>
    /// <param name="topicKey">The topic key.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    public override Topic Load(string topicKey = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      Topic.ValidateKey(topicKey, true);

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide dummy return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return new Topic();

    }

    /// <summary>
    ///   Interface method that loads topics into memory.
    /// </summary>
    /// <remarks>
    ///   Contract preconditions are defined in the <see cref="TopicRepositoryBaseContract"/> contract class.
    /// </remarks>
    protected override Topic Load(string topicKey, int topicId, int depth, DateTime? version = null) {
  
      /*------------------------------------------------------------------------------------------------------------------------
      | Provide dummy return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return new Topic();

    }

    /*==========================================================================================================================
    | GET CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a collection of Content Type objects from the configuration section of the data provider.
    /// </summary>
    public override ContentTypeCollection GetContentTypes() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Ensures(Contract.Result<ContentTypeCollection>() != null);

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide dummy return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return new ContentTypeCollection();

    }


  } // Class

} // Namespace
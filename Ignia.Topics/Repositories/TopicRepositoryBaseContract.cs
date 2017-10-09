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
    public override Topic Load(int topicId, bool isRecursive = true) => new Topic();

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
    ///   Loads a specific version of a topic based on its version.
    /// </summary>
    /// <remarks>
    ///   This overload does not accept an argument for recursion; it will only load a single instance of a version. Further,
    ///   it will only load versions for which the unique identifier is known.
    /// </remarks>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="version">The version.</param>
    /// <returns>A topic object.</returns>
    public override Topic Load(int topicId, DateTime version) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(version.Date < DateTime.Now, "The version requested must be a valid historical date.");
      Contract.Requires(version.Date > new DateTime(2014, 12, 9), "The version is expected to have been created since version support was introduced into the topic library.");

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
    public override TopicCollection<ContentType> GetContentTypes() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Ensures(Contract.Result<TopicCollection<ContentType>>() != null);

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide dummy return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return new TopicCollection<ContentType>();

    }


  } // Class

} // Namespace
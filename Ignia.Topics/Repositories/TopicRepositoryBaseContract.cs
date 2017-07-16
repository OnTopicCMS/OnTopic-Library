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
    | LOAD METHOD
    \-------------------------------------------------------------------------------------------------------------------------*/
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
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

  } // Class

} // Namespace
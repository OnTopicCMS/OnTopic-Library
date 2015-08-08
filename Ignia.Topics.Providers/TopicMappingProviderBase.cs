/*==============================================================================================================================
| Author        Casey Margell, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Editor
|
| Purpose       The ITopicImplementationProvider object defines an interface for implementation providers of the Topics system
|
\=============================================================================================================================*/
using System;
using System.Configuration.Provider;

namespace Ignia.Topics.Providers {

  /*=========================================================================================================================
  | CLASS: TOPIC MAPPING PROVIDER BASE
  >==========================================================================================================================
  | Defines a base class for topic mapping providers
  \------------------------------------------------------------------------------------------------------------------------*/
  public abstract class TopicMappingProviderBase : ProviderBase {

  /*=========================================================================================================================
  | PROPERTY: DATA PROVIDER
  >==========================================================================================================================
  | Upon setting the data provider we add this object as a listener to the events of that provider
  \------------------------------------------------------------------------------------------------------------------------*/
    public TopicDataProviderBase DataProvider {
      set {

      /*-----------------------------------------------------------------------------------------------------------------------
      | VALIDATE PARAMETERS
      \----------------------------------------------------------------------------------------------------------------------*/
        if (value == null) throw new ArgumentNullException("value");

      /*-----------------------------------------------------------------------------------------------------------------------
      | WIRE UP EVENTS
      \----------------------------------------------------------------------------------------------------------------------*/
        value.DeleteEvent += DeleteEventHandler;
        value.MoveEvent   += MoveEventHandler;
        value.RenameEvent += RenameEventHandler;

      }
    }

  /*=========================================================================================================================
  | METHOD: DELETE EVENT HANDLER
  >==========================================================================================================================
  | Abstranct definition of the DeleteEventHandler that inheriting objects must implement
  \------------------------------------------------------------------------------------------------------------------------*/
    public abstract void DeleteEventHandler(object sender, DeleteEventArgs args);

  /*=========================================================================================================================
  | METHOD: MOVE EVENT HANDLER
  >==========================================================================================================================
  | Abstranct definition of the MoveEventHandler that inheriting objects must implement
  \------------------------------------------------------------------------------------------------------------------------*/
    public abstract void MoveEventHandler(object sender, MoveEventArgs args);

  /*=========================================================================================================================
  | METHOD: RENAME EVENT HANDLER
  >==========================================================================================================================
  | Abstranct definition of the RenameEventHandler that inheriting objects must implement
  \------------------------------------------------------------------------------------------------------------------------*/
    public abstract void RenameEventHandler(object sender, RenameEventArgs args);

  } //Class

} //Namespace

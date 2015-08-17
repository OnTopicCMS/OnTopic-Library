/*==============================================================================================================================
| Author        Casey Margell, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Configuration.Provider;
using System.Diagnostics.Contracts;

namespace Ignia.Topics.Providers {

  /*============================================================================================================================
  | CLASS: TOPIC MAPPING PROVIDER BASE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a base class for topic mapping providers
  /// </summary>
  public abstract class TopicMappingProviderBase : ProviderBase {

    /*==========================================================================================================================
    | PROPERTY: DATA PROVIDER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Upon setting the data provider we add this object as a listener to the events of that provider.
    /// </summary>
    /// <requires description="The data provider must be available." exception="T:System.ArgumentNullException">value != null</requires>
    /// <exception cref="ArgumentNullException">value</exception>
    public TopicDataProviderBase DataProvider {
      set {

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate parameters
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Requires<ArgumentNullException>(value != null);

        /*----------------------------------------------------------------------------------------------------------------------
        | Wire up events
        \---------------------------------------------------------------------------------------------------------------------*/
        value.DeleteEvent += DeleteEventHandler;
        value.MoveEvent   += MoveEventHandler;
        value.RenameEvent += RenameEventHandler;

      }
    }

    /*==========================================================================================================================
    | METHOD: DELETE EVENT HANDLER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Abstranct definition of the <see cref="DeleteEventHandler(object, DeleteEventArgs)"/> that inheriting objects must
    ///   implement.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="DeleteEventArgs"/> instance containing the event data.</param>
    public abstract void DeleteEventHandler(object sender, DeleteEventArgs args);

    /*==========================================================================================================================
    | METHOD: MOVE EVENT HANDLER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Abstranct definition of the <see cref="MoveEventHandler(object, MoveEventArgs)"/> that inheriting objects must
    ///   implement.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="MoveEventArgs"/> instance containing the event data.</param>
    public abstract void MoveEventHandler(object sender, MoveEventArgs args);

    /*==========================================================================================================================
    | METHOD: RENAME EVENT HANDLER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Abstranct definition of the <see cref="RenameEventHandler(object, RenameEventArgs)"/> that inheriting objects must
    ///   implement.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="RenameEventArgs"/> instance containing the event data.</param>
    public abstract void RenameEventHandler(object sender, RenameEventArgs args);

  } // Class

} // Namespace

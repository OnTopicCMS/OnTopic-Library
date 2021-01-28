/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Collections.Specialized {

  /*============================================================================================================================
  | INTERFACE: TRACK DIRTY KEYS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines an interface for tracking dirty keys.
  /// </summary>
  public interface ITrackDirtyKeys {

    /*==========================================================================================================================
    | METHOD: IS DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether the collection is dirty.
    /// </summary>
    bool IsDirty();

    /// <summary>
    ///   Determines whether the provided <paramref name="key"/> in the collection is dirty.
    /// </summary>
    bool IsDirty(string key);

    /*==========================================================================================================================
    | METHOD: MARK CLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Marks the collection as clean.
    /// </summary>
    void MarkClean();

    /// <summary>
    ///   Marks the specified <paramref name="key"/> in the collection as clean.
    /// </summary>
    void MarkClean(string key);

  } //Interface
} //Namespace
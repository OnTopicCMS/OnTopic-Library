/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;

namespace OnTopic.Collections.Specialized {

  /*============================================================================================================================
  | CLASS: DIRTY KEY COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of dirty keys.
  /// </summary>
  /// <remarks>
  ///   This collection does not track the values of those keys or attempt to determine if a value is dirty. It simply provides
  ///   a convenient way for other collections to track dirty keys based on their own internal logic.
  /// </remarks>
  internal class DirtyKeyCollection : Collection<string>, ITrackDirtyKeys {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicCollection"/>.
    /// </summary>
    public DirtyKeyCollection() : base() {}

    /*==========================================================================================================================
    | METHOD: IS DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public bool IsDirty() => Count > 0;

    /// <inheritdoc/>
    public bool IsDirty(string key) => Contains(key);

    /*==========================================================================================================================
    | METHOD: MARK CLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public void MarkClean() => Clear();

    /// <inheritdoc/>
    public void MarkClean(string key) {
      if (Contains(key)) {
        Remove(key);
      }
    }

    /*==========================================================================================================================
    | METHOD: MARK DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Marks a specific <paramref name="key"/> as dirty, if it isn't already.
    /// </summary>
    public void MarkDirty(string key) {
      if (!Contains(key)) {
        Add(key);
      }
    }

    /*==========================================================================================================================
    | METHOD: MARK AS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Marks a specific <paramref name="key"/> as clean or dirty based on the <paramref name="markDirty"/> parameter.
    /// </summary>
    public void MarkAs(string key, bool markDirty) {
      if (markDirty) {
        MarkDirty(key);
      }
      else {
        MarkClean(key);
      }
    }

  } //Class
} //Namespace
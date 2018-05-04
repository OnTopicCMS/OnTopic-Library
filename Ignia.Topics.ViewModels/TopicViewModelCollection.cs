/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Ignia.Topics.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a basic collection interface for use with data transfer objects implementing <see cref="ITopicViewModelCore"/>,
  ///   including <see cref="TopicViewModel"/> and derivatives.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public class TopicViewModelCollection<TItem>: KeyedCollection<string, TItem> where TItem: ITopicViewModel {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicViewModelCollection{T}"/>.
    /// </summary>
    public TopicViewModelCollection() : base(StringComparer.OrdinalIgnoreCase) {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicViewModelCollection{T}"/>.
    /// </summary>
    /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
    public TopicViewModelCollection(IEnumerable<TItem> topics = null) : base(StringComparer.OrdinalIgnoreCase) {
      if (topics != null) {
        foreach(var item in topics) {
          this.Add(item);
        }
      }
    }

    /*==========================================================================================================================
    | METHOD: GET BY CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieve a filtered list of topics from this collection based on their content type.
    /// </summary>
    /// <param name="contentType">The name of the content type to filter by.</param>
    /// <returns>The filtered list of view models associated with the content type.</returns>
    public TopicViewModelCollection<TItem> GetByContentType(string contentType) {
      Contract.Assume(!String.IsNullOrWhiteSpace(contentType), "A content type argument is required.");
      return new TopicViewModelCollection<TItem>(
        Items.Where<TItem>(t => t.ContentType.Equals(contentType, StringComparison.InvariantCultureIgnoreCase))
      );
    }

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the EntityCollection to extract the keys from the items.
    /// </summary>
    /// <param name="item">The <see cref="Topic"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(TItem item) {
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");
      return item.Key;
    }

  }
}

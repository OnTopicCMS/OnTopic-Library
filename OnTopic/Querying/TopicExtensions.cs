/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Collections;
using OnTopic.Collections.Specialized;
using OnTopic.Metadata;
using OnTopic.Repositories;

namespace OnTopic.Querying;

/*==============================================================================================================================
| CLASS: TOPIC (EXTENSIONS)
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides extensions for querying <see cref="OnTopic.Topic"/>.
/// </summary>
/// <remarks>
///   These extensions, while powerful, were intended to be used against fully loaded, in-memory topic trees. Their usefulness
///   with lazy-loaded trees is limited and, potentially, even expensive, as innocent seeming queries may trigger lazy-loading
///   of attributes, relationships, references, children, etc. Traversal itself never triggers a load, as it reads <see cref=
///   "ITopicBackingAccessor.Children"/> directly, though the <c>predicate</c> parameter can easily call into any of these.
/// </remarks>
public static class TopicExtensions {

  /*============================================================================================================================
  | METHOD: FIND FIRST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Finds the first instance of a <see cref="Topic"/> in the topic tree that satisfies the delegate.
  /// </summary>
  /// <remarks>
  ///   Traverses via <see cref="ITopicBackingAccessor.Children"/>, the non-triggering backing field, so this never directly
  ///   causes a lazy load. The <paramref name="predicate"/> is not similarly guarded, however: When using this with a
  ///   lazy-loaded tree, be aware that it may trigger costly on-demand loading of attributes, relationships, references, and
  ///   children if they're included in the predicate. It is recommended to avoid use with lazy-loaded trees, or to use extreme
  ///   caution.
  /// </remarks>
  /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
  /// <param name="predicate">The function to validate whether a <see cref="Topic"/> should be included in the output.</param>
  /// <returns>The first instance of the topic to be satisfied.</returns>
  public static Topic? FindFirst(this Topic topic, Func<Topic, bool> predicate) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate contracts
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(topic, nameof(topic));
    Contract.Requires(predicate, nameof(predicate));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Search attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (predicate(topic)) {
      return topic;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Recurse over children
    \-------------------------------------------------------------------------------------------------------------------------*/
    foreach (var child in ((ITopicBackingAccessor)topic).Children) {
      var nestedResult          = child.FindFirst(predicate);
      if (nestedResult is not null) {
        return nestedResult;
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Indicate no results found
    \-------------------------------------------------------------------------------------------------------------------------*/
    return null;

  }

  /*============================================================================================================================
  | METHOD: FIND FIRST PARENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Finds the first instance of <see cref="Topic"/> among the parents that satisfies the delegate.
  /// </summary>
  /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
  /// <param name="predicate">The function to validate whether a <see cref="Topic"/> should be included in the output.</param>
  /// <returns>The first instance of a parent topic to be satisfied.</returns>
  public static Topic? FindFirstParent(this Topic topic, Func<Topic, bool> predicate) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate contracts
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(topic, nameof(topic));
    Contract.Requires(predicate, nameof(predicate));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Search attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (predicate(topic)) {
      return topic;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Find lowest common root
    \-------------------------------------------------------------------------------------------------------------------------*/
    while (topic.Parent is not  null) {
      if (predicate(topic.Parent)) {
        return topic.Parent;
      }
      topic                     = topic.Parent;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Indicate no results found
    \-------------------------------------------------------------------------------------------------------------------------*/
    return null;

  }

  /*============================================================================================================================
  | METHOD: FIND ALL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a collection of all in-memory topics in the <paramref name="topic"/> tree, descending from—and including—the
  ///   current topic.
  /// </summary>
  /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
  /// <returns>A collection of topics descending from the current topic.</returns>
  public static ReadOnlyTopicCollection FindAll(this Topic topic) => topic.FindAll(t => true);

  /// <summary>
  ///   Retrieves a collection of topics based on a supplied function.
  /// </summary>
  /// <remarks>
  ///   Traverses via <see cref="ITopicBackingAccessor.Children"/>, the non-triggering backing field, so this never directly
  ///   causes a lazy load. The <paramref name="predicate"/> is not similarly guarded, however: When using this with a
  ///   lazy-loaded tree, be aware that it may trigger costly on-demand loading of attributes, relationships, references, and
  ///   children if they're included in the predicate. It is recommended to avoid use with lazy-loaded trees, or to use extreme
  ///   caution.
  /// </remarks>
  /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
  /// <param name="predicate">The function to validate whether a <see cref="Topic"/> should be included in the output.</param>
  /// <returns>A collection of topics matching the input parameters.</returns>
  public static ReadOnlyTopicCollection FindAll(this Topic topic, Func<Topic, bool> predicate) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate contracts
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(topic, nameof(topic));
    Contract.Requires(predicate, nameof(predicate));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Search attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    var results                 = new TopicCollection();

    if (predicate(topic)) {
      results.Add(topic);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Recurse over children
    \-------------------------------------------------------------------------------------------------------------------------*/
    foreach (var child in ((ITopicBackingAccessor)topic).Children) {
      var nestedResults         = child.FindAll(predicate);
      foreach (var matchedTopic in nestedResults) {
        if (!results.Contains(matchedTopic)) {
          results.Add(matchedTopic);
        }
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return results
    \-------------------------------------------------------------------------------------------------------------------------*/
    return new(results);

  }

  /*============================================================================================================================
  | METHOD: FIND ALL BY ATTRIBUTE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a collection of topics based on an attribute name and value.
  /// </summary>
  /// <remarks>
  ///   If querying a lazy-loaded topic tree, this will trigger a query for any extended attributes that aren't yet (fully)
  ///   loaded. At the same time, however, it will not trigger lazy-loading of any children that aren't yet (fully) loaded. As a
  ///   result, queries against lazy-loaded topic trees can be slow while also being incomplete.
  /// </remarks>
  /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
  /// <param name="attributeKey">The string identifier for the <see cref="AttributeRecord"/> against which to be searched.</param>
  /// <param name="attributeValue">The text value for the <see cref="AttributeRecord"/> against which to be searched.</param>
  /// <returns>A collection of topics matching the input parameters.</returns>
  /// <requires description="The attribute name must be specified." exception="T:System.ArgumentNullException">
  ///   !String.IsNullOrWhiteSpace(name)
  /// </requires>
  /// <requires
  ///   decription="The name should be an alphanumeric sequence; it should not contain spaces or symbols."
  ///   exception="T:System.ArgumentException">
  ///   !name.Contains(" ")
  /// </requires>
  public static ReadOnlyTopicCollection FindAllByAttribute(this Topic topic, string attributeKey, string attributeValue) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate contracts
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(topic, nameof(topic));
    Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(attributeKey), nameof(attributeKey));
    Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(attributeValue), nameof(attributeValue));
    TopicFactory.ValidateKey(attributeKey);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return results
    \-------------------------------------------------------------------------------------------------------------------------*/
    return topic.FindAll(t =>
      !String.IsNullOrEmpty(t.Attributes.GetValue(attributeKey)) &&
      t.Attributes.GetValue(attributeKey, "").Contains(attributeValue, StringComparison.OrdinalIgnoreCase)
    );

  }

  /*============================================================================================================================
  | METHOD: GET TOPIC INDEX
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves all topics from the in-memory topic graph, and places them in a dictionary indexed by <see cref="Topic.Id"/>.
  /// </summary>
  /// <remarks>
  ///   This only loads topics from the in-memory topic graph. Any topics that aren't yet loaded in the in-memory topic graph
  ///   will not be included.
  /// </remarks>
  /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
  /// <returns>A dictionary of topics indexed by <see cref="Topic.Id"/>.</returns>
  public static TopicIndex GetTopicIndex(this Topic topic) => new(topic.FindAll());

  /*============================================================================================================================
  | METHOD: GET ROOT TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves the root <see cref="Topic"/> in the current topic graph.
  /// </summary>
  /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
  /// <returns>The <see cref="Topic"/> at the root o the current topic graph.</returns>
  public static Topic GetRootTopic(this Topic topic) => topic.FindFirstParent(t => t.Parent is null)!;

  /*============================================================================================================================
  | METHOD: GET BY UNIQUE KEY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a <see cref="Topic"/> with the specified <paramref name="uniqueKey"/>, if available.
  /// </summary>
  /// <remarks>
  ///   This will trigger synchronous lazy-loading calls to any topics in the chain whose children aren't yet loaded. That can
  ///   make initial calls to this unexpectedly expensive on a lazy-loaded topic tree, resulting in multiple calls to the
  ///   underlying persistance store.
  /// </remarks>
  /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
  /// <param name="uniqueKey">The <see cref="Topic.GetUniqueKey()"/> of the <see cref="Topic"/> to return.</param>
  /// <returns>A <see cref="Topic"/> with the specified <paramref name="uniqueKey"/>, if found.</returns>
  public static Topic? GetByUniqueKey(this Topic topic, string uniqueKey) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate contracts
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(topic, "The topic parameter must be specified.");
    Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(uniqueKey), nameof(uniqueKey));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Find lowest common root
    \-------------------------------------------------------------------------------------------------------------------------*/
    var currentTopic            = topic.GetRootTopic();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle request for root
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (currentTopic.Key.Equals(uniqueKey, StringComparison.OrdinalIgnoreCase)) {
      return currentTopic;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Process keys
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (uniqueKey.StartsWith(currentTopic.Key + ":", StringComparison.OrdinalIgnoreCase)) {
      uniqueKey                 = uniqueKey[(currentTopic.Key.Length + 1)..];
    }
    var keys                    = uniqueKey.Split(':', StringSplitOptions.RemoveEmptyEntries);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Navigate to the specific path
    \-------------------------------------------------------------------------------------------------------------------------*/
    foreach (var key in keys) {
      currentTopic              = currentTopic?.Children.GetValue(key);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return topic
    \-------------------------------------------------------------------------------------------------------------------------*/
    return currentTopic;

  }

  /*============================================================================================================================
  | METHOD: GET CONTENT TYPE DESCRIPTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves the <see cref="ContentTypeDescriptor"/> for the current <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   This assumes that the Configuration tree is fully loaded as part of the <paramref name="topic"/>'s graph. In a standard
  ///   configuration, this portion of the tree should be eagerly loaded as part of the cache initialization, since it's a
  ///   commonly referenced dependency with a lot of internal dependencies in terms of relationships and references.
  /// </remarks>
  /// <param name="topic">The instance of the <see cref="Topic"/> to operate against; populated automatically by .NET.</param>
  /// <returns>The <see cref="ContentTypeDescriptor"/> associated with the <see cref="Topic.ContentType"/>.</returns>
  public static ContentTypeDescriptor? GetContentTypeDescriptor(this Topic topic) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate contracts
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(topic, "The topic parameter must be specified.");

    /*--------------------------------------------------------------------------------------------------------------------------
    | Find the root
    \-------------------------------------------------------------------------------------------------------------------------*/
    var rootContentType         = topic.GetByUniqueKey("Root:Configuration:ContentTypes");

    /*--------------------------------------------------------------------------------------------------------------------------
    | Find content type
    \-------------------------------------------------------------------------------------------------------------------------*/
    var contentTypeDescriptor   = rootContentType?.FindFirst(t =>
      t.Key.Equals(topic.ContentType, StringComparison.OrdinalIgnoreCase) &&
      t is ContentTypeDescriptor
    ) as ContentTypeDescriptor;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return content type
    \-------------------------------------------------------------------------------------------------------------------------*/
    return contentTypeDescriptor;

  }

} //Class
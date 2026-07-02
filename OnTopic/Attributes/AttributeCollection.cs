/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Diagnostics.CodeAnalysis;
using OnTopic.Collections.Specialized;
using OnTopic.Repositories;

namespace OnTopic.Attributes;

/*==============================================================================================================================
| CLASS: ATTRIBUTE COLLECTION
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Represents a collection of <see cref="AttributeRecord"/> objects.
/// </summary>
/// <remarks>
///   <see cref="AttributeRecord"/> objects represent individual instances of attributes associated with particular topics.
///   The <see cref="Topic"/> class tracks these through its <see cref="Topic.Attributes"/> property, which is an instance of
///   the <see cref="AttributeCollection"/> class.
/// </remarks>
public class AttributeCollection : TrackedRecordCollection<AttributeRecord, string, AttributeSetterAttribute> {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private static readonly       List<string>                    _excludedAttributes             = new() {
    nameof(Topic.Title),
    nameof(Topic.LastModified)
  };

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="AttributeCollection"/> class.
  /// </summary>
  /// <remarks>
  ///   The <see cref="AttributeCollection"/> is intended exclusively for providing access to attributes via the <see cref="
  ///   Topic.Attributes"/> property. For this reason, the constructor is marked as internal.
  /// </remarks>
  /// <param name="parentTopic">A reference to the topic that the current attribute collection is bound to.</param>
  internal AttributeCollection(Topic parentTopic) : base(parentTopic) {
  }

  /*============================================================================================================================
  | PROPERTY: PARENT COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  protected override TrackedRecordCollection<AttributeRecord, string, AttributeSetterAttribute>? ParentCollection =>
    AssociatedTopic.Parent?.Attributes;

  /*============================================================================================================================
  | PROPERTY: BASE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  protected override TrackedRecordCollection<AttributeRecord, string, AttributeSetterAttribute>? BaseCollection =>
    AssociatedTopic.BaseTopic?.Attributes;

  /*============================================================================================================================
  | PROPERTY: LOAD STATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Indicates whether the collection has been populated from the underlying <see cref="Repositories.ITopicRepository" />,
  ///   allowing callers to distinguish data that is present and authoritative from data that must still be fetched.
  /// </summary>
  /// <remarks>
  ///   Defaults to <see cref="LoadState.Loaded"/>. When a topic is loaded without extended attributes (e.g., on a shallow
  ///   load), the repository conditionally sets this to <see cref="LoadState.NotLoaded"/> to indicate that the extended
  ///   attribute blob has not yet been retrieved. The persistence store may optionally provide an indicator of the count
  ///   without returning the full data, thus allowing this to be set to <see cref="LoadState.Loaded"/> if, in fact, there are
  ///   no extended attributes. Indexed attributes are never deferred regardless of this state.
  /// </remarks>
  public LoadState LoadState { get; set; } = LoadState.Loaded;

  /*============================================================================================================================
  | METHOD: IS DIRTY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Determine if <i>any</i> attributes in the <see cref="AttributeCollection"/> are dirty.
  /// </summary>
  /// <remarks>
  ///   This method is intended primarily for data storage providers, such as <see cref="ITopicRepository"/>, which may need
  ///   to determine if any attributes are dirty prior to saving them to the data storage medium. Be aware that this does
  ///   <i>not</i> track whether any <see cref="Topic.Relationships"/> have been modified; as such, it may still be necessary
  ///   to persist changes to the storage medium.
  /// </remarks>
  /// <param name="excludeLastModified">
  ///   Optionally excludes <see cref="AttributeRecord"/>s whose keys start with <c>LastModified</c>. This is useful for
  ///   excluding the byline (<c>LastModifiedBy</c>) and dateline (<c>LastModified</c>) since these values are automatically
  ///   generated by e.g. the OnTopic Editor and, thus, may be irrelevant updates if no other attribute values have changed.
  /// </param>
  /// <returns>True if the attribute value is marked as dirty; otherwise false.</returns>
  public bool IsDirty(bool excludeLastModified)
    => DeletedItems.Count > 0 || Items.Any(a =>
      a.IsDirty &&
      (!excludeLastModified ||  !a.Key.StartsWith("LastModified", StringComparison.OrdinalIgnoreCase))
    );

  /*============================================================================================================================
  | METHOD: GET VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves the value associated with the specified <paramref name="key"/>, autoloading the extended-attribute blob if the
  ///   key is not yet loaded and the extended-attribute boundary is <see cref="LoadState.NotLoaded"/>.
  /// </summary>
  /// <remarks>
  ///   Indexed attributes are always loaded in the local collection; the autoload is skipped for them. A deferred key that has
  ///   never been fetched triggers a single synchronous blob fill through the stamped resolver; all subsequent reads find the
  ///   boundary <see cref="LoadState.Loaded"/> and return immediately without an additional round-trip.
  /// </remarks>
  /// <param name="key">The string identifier for the <see cref="AttributeRecord"/>.</param>
  /// <param name="defaultValue">A string value to which to fall back in the case the value is not found.</param>
  /// <param name="inheritFromParent">
  ///   Determines if the value should be inherited from the parent topic when not found locally.
  /// </param>
  /// <param name="maxHops">The maximum number of ancestor hops when inheriting from parent topics.</param>
  [return: NotNullIfNotNull(nameof(defaultValue))]
  internal override string? GetValue(string key, string? defaultValue, bool inheritFromParent, int maxHops) {
    if (LoadState is LoadState.NotLoaded && !Contains(key)) {
      AssociatedTopic.EnsureLoaded(LoadBoundaries.ExtendedAttributes);
    }
    return base.GetValue(key, defaultValue, inheritFromParent, maxHops);
  }

  /*============================================================================================================================
  | METHOD: SET VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Helper method that either adds a new <see cref="AttributeRecord"/> object or updates the value of an existing one,
  ///   depending on whether that value already exists.
  /// </summary>
  /// <remarks>
  ///   Minimizes the need for defensive conditions throughout the library.
  /// </remarks>
  /// <param name="key">The string identifier for the <see cref="AttributeRecord"/>.</param>
  /// <param name="value">The text value for the <see cref="AttributeRecord"/>.</param>
  /// <param name="markDirty">
  ///   Specified whether the value should be marked as <see cref="TrackedRecord{T}.IsDirty"/>. By default, it will be marked
  ///   as dirty if the value is new or has changed from a previous value. By setting this parameter, that behavior is
  ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being
  ///   persisted to the data store on <see cref="Repositories.ITopicRepository.Save(Topic, Boolean)"/>.
  /// </param>
  /// <param name="version">
  ///   The <see cref="DateTime"/> value that the attribute was last modified. This is intended exclusively for use when
  ///   populating the topic graph from a persistent data store as a means of indicating the current version for each
  ///   attribute. This is used when e.g. importing values to determine if the existing value is newer than the source value.
  /// </param>
  /// <param name="isExtendedAttribute">Determines if the attribute originated from an extended attributes data store.</param>
  /// <requires
  ///   description="The key must be specified for the AttributeRecord key/value pair."
  ///   exception="T:System.ArgumentNullException">
  ///   !String.IsNullOrWhiteSpace(key)
  /// </requires>
  /// <requires
  ///   description="The value must be specified for the AttributeRecord key/value pair."
  ///   exception="T:System.ArgumentNullException">
  ///   !String.IsNullOrWhiteSpace(value)
  /// </requires>
  /// <requires
  ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols"
  ///   exception="T:System.ArgumentException">
  ///   !value.Contains(" ")
  /// </requires>
  public void SetValue(
    string key,
    string? value,
    bool? markDirty = null,
    DateTime? version = null,
    bool? isExtendedAttribute = null
  ) {
    base.SetValue(key, value, markDirty, version);
    if (Contains(key)) {
      var attributeValue        = this[key];
      var attributeIndex        = IndexOf(attributeValue);
      if (isExtendedAttribute is not null && isExtendedAttribute != attributeValue.IsExtendedAttribute) {
        attributeValue          = attributeValue with {
          IsExtendedAttribute = isExtendedAttribute
        };
        base[attributeIndex]    = attributeValue;
      }
    }
  }

  /*============================================================================================================================
  | METHOD: AS ATTRIBUTE DICTIONARY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Gets an <see cref="AttributeDictionary"/> based on the <see cref="Topic.Attributes"/> of the current <see cref="
  ///   AttributeCollection"/>. Optionall includes attributes from any <see cref="Topic.BaseTopic"/>s that the <see cref="
  ///   TrackedRecordCollection{TItem, TValue, TAttribute}.AssociatedTopic"/> derives from.
  /// </summary>
  /// <remarks>
  ///   The <see cref="AsAttributeDictionary(Boolean)"/> method will exclude attributes which correspond to properties on
  ///   <see cref="Topic"/> which contain specialized getter logic, such as <see cref="Topic.Title"/> and <see cref="Topic.
  ///   LastModified"/>.
  /// </remarks>
  /// <param name="inheritFromBase">
  ///   Determines if attributes from the <see cref="Topic.BaseTopic"/> should be included. Defaults to <c>false</c>.
  /// </param>
  /// <returns>A new <see cref="AttributeDictionary"/> containing attributes</returns>
  public AttributeDictionary AsAttributeDictionary(bool inheritFromBase = false) {
    var sourceAttributes        = (AttributeCollection?)this;
    var attributes              = new AttributeDictionary();
    var count                   = 0;
    while (sourceAttributes is  not null && ++count < 5) {
      foreach (var attribute in sourceAttributes) {
        if (count is 1 || !attributes.ContainsKey(attribute.Key)) {
          attributes.TryAdd(attribute.Key, attribute.Value);
        }
      }
      sourceAttributes = inheritFromBase? sourceAttributes.AssociatedTopic.BaseTopic?.Attributes : null;
    }
    foreach (var attribute in _excludedAttributes) {
      attributes.Remove(attribute);
    }
    return attributes;
  }

} //Class
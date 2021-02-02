/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using OnTopic.Attributes;
using OnTopic.Collections.Specialized;
using OnTopic.References;

namespace OnTopic.Internal.Reflection {

  /*============================================================================================================================
  | CLASS: TOPIC PROPERTY DISPATCHER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicPropertyDispatcher{TAttributeType, TValueType}"/> allows a collection on a <see cref="Topic"/>
  ///   entity to optionally route requests through properties on the corresponding <see cref="Topic"/> which correspond to the
  ///   item key, thus ensuring local state and any business logic enforced by the property setter are maintained.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Collections on <see cref="Topic"/>, such as <see cref="Topic.Attributes"/> and <see cref="Topic.References"/>, aren't
  ///     well-positioned to enforce attribute-specific business logic when adding or setting items in the collection. Instead,
  ///     this logic is typically handled by property setters on <see cref="Topic"/>, such as <see cref="Topic.View"/> or <see
  ///     cref="Topic.BaseTopic"/>. This introduces a potential backdoor, as updates made directly to the collection can
  ///     bypass any business logic—such as data validation or local state management—handled by those property setters. The
  ///     <see cref="TopicPropertyDispatcher{TAttributeType, TValueType}"/> class addresses this by allowing those collections
  ///     to route requests through appropriately decorated properties on <see cref="Topic"/> prior to adding or setting a
  ///     value.
  ///   </para>
  ///   <para>
  ///     The <see cref="TopicPropertyDispatcher{TAttributeType, TValueType}"/> requires two type arguments. <typeparamref name=
  ///     "TAttributeType"/> represents an attribute which must be present on each property setter. This helps avoid potential
  ///     ambiguities. For instance, if both <see cref="Topic.Attributes"/> and <see cref="Topic.References"/> have the same
  ///     key, and that key maps to the identify of a <see cref="Topic"/> property, the usage will be restricted based on
  ///     whether the expected attribute is used to decorate the property—for instance, the <see cref="AttributeSetterAttribute"
  ///     /> or <see cref="ReferenceSetterAttribute"/>. In practice, this is an unexpected situation since a) individual content
  ///     types cannot use the same key for both <see cref="Topic.Attributes"/> and <see cref="Topic.References"/>, and b) even
  ///     if they did, these properties support different data types, and thus are not intercompatible. Nevertheless, these
  ///     attributes provide an additional level of explicitness to avoid any ambiguity, and provide both developers as well as
  ///     the <see cref="TopicPropertyDispatcher{TAttributeType, TValueType}"/> hints about what a <see cref="Topic"/> property
  ///     is intended for.
  ///   </para>
  ///   <para>
  ///     The <typeparamref name="TValueType"/> represents the value that is stored in the corresponding collection. This value
  ///     is saved as part of either <see cref="Enforce(String, TValueType?)"/> or <see cref="Register(String, TValueType?)"/>,
  ///     and can optionally be retrieved via <see cref="IsRegistered(String, out TValueType?)"/>. This is useful in case there
  ///     is data from the original request that might be lost when routed through the property setter. For example, the <see
  ///     cref="Topic.Attributes"/> collection works with <see cref="AttributeValue"/> instances, which contain metadata such as
  ///     <see cref="TrackedItem{T}.IsDirty"/>, <see cref="AttributeValue.IsExtendedAttribute"/>, and <see cref="TrackedItem{T}.
  ///     LastModified"/>, which are not passed to the corresponding property decorated with <see cref="AttributeSetterAttribute
  ///     "/>. As such, by saving a reference to those as part of the <see cref="Register(String, TValueType?)"/> process, we
  ///     allow the source collection to retrieve the original request in order to ensure that data isn't lost. This isn't as
  ///     critical for e.g. <see cref="Topic.References"/> since the <typeparamref name="TValueType"/> will be the same <see
  ///     cref="Topic"/> that's sent to the corresponding property, and thus is expected to be the same as the value set by the
  ///     <see cref="Topic"/> property itself.
  ///   </para>
  ///   <para>
  ///     In a typical workflow, the <see cref="Enforce(String, TValueType?)"/> method will end up getting once or twice. The
  ///     first occurs when a caller attempts to insert an item directly into a collection; if a corresponding property setter
  ///     is requested, then <see cref="Enforce(String, TValueType?)"/> will call <see cref="Register(String, TValueType?)"/>,
  ///     trigger the call to the corresponding property, and return <c>false</c>—indicating that the item should not be added
  ///     to the collection. In this case, the property setter will run its own business logic, then attempt to add or set the
  ///     item into the collection again. This time, the call to <see cref="Register(String, TValueType?)"/> will prevent the
  ///     second call to <see cref="Enforce(String, TValueType?)"/> from enforcing the business logic, thus preventing an
  ///     infinite loop.
  ///   </para>
  ///   <para>
  ///     One caveat to this are cases where the caller attempts to set the value via the <see cref="Topic"/> property directly,
  ///     instead of adding the item directly to the corresponding collection—e.g., they call <see cref="Topic.View"/> instead
  ///     of e.g. the <see cref="AttributeValueCollection.SetValue(String, String?, Boolean?, DateTime?, Boolean?)"/> method
  ///     from <see cref="Topic.Attributes"/>. In that case, the business logic will already have been enforced, but the <see
  ///     cref="Register(String, TValueType?)"/> method will not have been called. To mitigate the property setter getting
  ///     called twice, collection implementors are advised to offer an internal overload that allows an item to be added to the
  ///     collection while bypassing the business logic. For instance, this can be done using <see cref="
  ///     TrackedCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, Boolean, DateTime?)"/> or <see cref="
  ///     TrackedCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, Boolean, DateTime?)"/>; in each case,
  ///     the internally accessible <c>enforceBusinessLogic</c> parameter allows a property setter to disable business logic.
  ///     Internally, this is done by calling <see cref="Register(String, TValueType?)"/>, thus assuring <see cref="Enforce(
  ///     String, TValueType?)"/> that the business logic has already occurred.
  ///   </para>
  /// </remarks>
  internal class TopicPropertyDispatcher<TAttributeType, TValueType>
    where TAttributeType: Attribute
    where TValueType: class
  {

    /*==========================================================================================================================
    | STATIC VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    static readonly             MemberDispatcher                _typeCache                      = new(typeof(TAttributeType));

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            Topic                           _associatedTopic;
    private                     int                             _setCounter;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicPropertyDispatcher{TAttributeType, TValueType}"/> class associated
    ///   with a specific <see cref="Topic"/>.
    /// </summary>
    /// <param name="associatedTopic">The <see cref="Topic"/> whose properties should be called, when appropriate.</param>
    internal TopicPropertyDispatcher(Topic associatedTopic) {
      _associatedTopic = associatedTopic;
    }

    /*==========================================================================================================================
    | PROPERTY: PROPERTY CACHE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a local cache of <typeparamref name="TValueType"/> objects, keyed by an associated <c>itemKey</c>, prior
    ///   to having their business logic enforced.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Whenever a property setter has been called, a record in the <see cref="PropertyCache"/> is added containing a) the
    ///     key associated with the item (and, therefore, property), and b) the original <typeparamref name="TValueType"/> that
    ///     was to be added to the collection. This registers that the business logic for that property has been enforced.
    ///   </para>
    ///   <para>
    ///     There are two ways that a record is created in the <see cref="PropertyCache"/>. The typical way is to call <see cref
    ///     ="Enforce(String, TValueType?)"/>, which will check to see if there is a corresponding property setter and, if there
    ///     is, will add a record to the cache, and call the property. The second way is to call <see cref="Register(String,
    ///     TValueType?)"/> to directly register that the property has already been executed. This is typically done by special
    ///     internal methods, called exclusively by the property setters themselves, with a <c>enforceBusinessLogic</c>
    ///     parameter that is set to <c>false</c>; that prevents calls made directly to the property setter to bypass the
    ///     dispatcher.
    ///   </para>
    ///   <para>
    ///     There is only one way to remove an item from the <see cref="PropertyCache"/> once it's been created. This happens
    ///     when <see cref="Enforce(String, TValueType?)"/> is called, and a record already exists. When this occurs, the record
    ///     is removed, and <see cref="Enforce(String, TValueType?)"/> returns <c>true</c> without any further action. This
    ///     instructs the caller—i.e., a method on a collection responsible for adding or setting an item—that it can complete
    ///     the request.
    ///   </para>
    /// </remarks>
    private Dictionary<string, TValueType?> PropertyCache { get; } = new();

    /*==========================================================================================================================
    | REGISTER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instructs the <see cref="TopicPropertyDispatcher{TAttributeType, TValueType}"/> that the business logic for a
    ///   corresponding property has been set, and does not need to be executed again.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     The <see cref="Register(String, TValueType?)"/> method is called by <see cref="Enforce(String, TValueType?)"/> right
    ///     before it triggers a call to a corresponding property setter. This allows it to track that the business logic has
    ///     been enforced, and it doesn't need to make the call again on a round trip.
    ///   </para>
    ///   <para>
    ///     The <see cref="Register(String, TValueType?)"/> method can also be called directly by a collection to tell the
    ///     <see cref="TopicPropertyDispatcher{TAttributeType, TValueType}"/> that business logic should not be enforced when
    ///     adding or setting an item. The typical use case for this is an internal method which allows the property setters
    ///     themselves to bypass business logic, thus preventing them from being called twice. These methods should be marked
    ///     internal to prevent external actors from bypassing the business logic; the purpose is to confirm that the business
    ///     logic has already been enforced, not to make the business logic optional. Two examples of this are the internal
    ///     <c>enforceBusinessLogic</c> parameters on <see cref="TrackedCollection{TItem, TValue, TAttribute}.SetValue(String,
    ///     TValue, Boolean?, Boolean, DateTime?)"/> and <see cref="TrackedCollection{TItem, TValue, TAttribute}.SetValue(
    ///     String, TValue, Boolean?, Boolean, DateTime?)"/>.
    ///   </para>
    ///   <para>
    ///     It's worth noting that any calls to <see cref="Register(String, TValueType?)"/> are invalidated the next time <see
    ///     cref="Enforce(String, TValueType?)"/> is called. As such, <see cref="Register(String, TValueType?)"/> is not a way
    ///     to permanently disable calling a property setter. (The correct way to do that is to remove the property setter, or
    ///     at least its corresponding <typeparamref name="TAttributeType"/>.) Instead, it only disables the next attempt to add
    ///     an item corresponding to that key—which, if correctly implemented, will be when the current <paramref name="
    ///     initialValue"/> is added to the collection.
    ///   </para>
    /// </remarks>
    /// <param name="itemKey">
    ///   The key of the <typeparamref name="TValueType"/>, which potentially corresponds to a <see cref="Topic"/> property.
    /// </param>
    /// <param name="initialValue">The <typeparamref name="TValueType"/> object which is being inserted.</param>
    internal bool Register(string itemKey, TValueType? initialValue) {
      var type = initialValue?.GetType();
      if (typeof(AttributeValue).IsAssignableFrom(type)) {
        type = null;
      }
      if (
        _typeCache.HasSettableProperty(_associatedTopic.GetType(), itemKey, type) &&
        !PropertyCache.ContainsKey(itemKey)
      ) {
        PropertyCache.Add(itemKey, initialValue);
        return true;
      }
      return false;
    }

    /*==========================================================================================================================
    | IS REGISTERED?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Identifies whether the given <paramref name="itemKey"/> has been registered—and, thus, has already had its business
    ///   logic enforced.
    /// </summary>
    /// <param name="itemKey">
    ///   The key of the <typeparamref name="TValueType"/>, which potentially corresponds to a <see cref="Topic"/> property.
    /// </param>
    /// <returns>Returns <c>true</c> if the <paramref name="itemKey"/> has been registered, otherwise <c>false</c>.</returns>
    internal bool IsRegistered(string itemKey) => IsRegistered(itemKey, out var _);

    /// <summary>
    ///   Identifies whether the given <paramref name="itemKey"/> has been registered—and, thus, has already had its business
    ///   logic enforced. Returns the <paramref name="initialObject"/> that was registered as an out parameter.
    /// </summary>
    /// <param name="itemKey">
    ///   The key of the <typeparamref name="TValueType"/>, which potentially corresponds to a <see cref="Topic"/> property.
    /// </param>
    /// <param name="initialObject">The <typeparamref name="TValueType"/> object which is being inserted.</param>
    /// <returns>Returns <c>true</c> if the <paramref name="itemKey"/> has been registered, otherwise <c>false</c>.</returns>
    internal bool IsRegistered(string itemKey, [NotNullWhen(true)] out TValueType? initialObject) =>
      PropertyCache.TryGetValue(itemKey, out initialObject!);

    /*==========================================================================================================================
    | METHOD: ENFORCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Inspects the requested <paramref name="itemKey"/> to determine if the corresponding <paramref name="initialObject"
    ///   /> should be routed through the associated <see cref="Topic"/> in order to enforce business logic.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     If a settable property is available on the associated <see cref="Topic"/> corresponding to the <paramref name="
    ///     itemKey"/>, the call should be routed through that property to ensure that local business logic is enforced. This
    ///     is determined by looking for <typeparamref name="TAttributeType"/> attribute, which confirms that a property with a
    ///     matching name is aware of and intended to operate with a given collection.
    ///   </para>
    ///   <para>
    ///     The <see cref="Enforce(String, TValueType?)"/> method should be called from an implementing collection prior to
    ///     committing an add, insert, or set operation. That operation should only be completed if <see cref="Enforce(String,
    ///     TValueType?)"/> returns <c>true</c>; otherwise, the request will be routed through the corresponding property on
    ///     <see cref="Topic"/> in order to enforce any business logic, after which the property will attempt to add the
    ///     property to the collection again. When <see cref="Enforce(String, TValueType?)"/> is called a second time for the
    ///     same <paramref name="itemKey"/>, it won't enforce the business logic, and will instead return <c>true</c>.
    ///   </para>
    /// </remarks>
    /// <param name="itemKey">
    ///   The key of the <typeparamref name="TValueType"/>, which potentially corresponds to a <see cref="Topic"/> property
    ///   setter.
    /// </param>
    /// <param name="initialObject">The <typeparamref name="TValueType"/> object which is being inserted.</param>
    /// <returns>Returns <c>true</c> if the business logic has been enfored; otherwise <c>false</c>.</returns>
    internal bool Enforce(string itemKey, TValueType? initialObject) {
      if (PropertyCache.ContainsKey(itemKey)) {
        PropertyCache.Remove(itemKey);
        return true;
      }
      else if (Register(itemKey, initialObject)) {
        _setCounter++;
        if (_setCounter > 3) {
          throw new InvalidOperationException(
            $"An infinite loop has occurred when setting '{itemKey}'; be sure that you are referencing " +
            $"`Topic.SetAttributeValue()` when setting attributes from `Topic` properties."
          );
        }
        var attribute = initialObject as AttributeValue;
        try {
          if (attribute is not null) {
            _typeCache.SetPropertyValue(_associatedTopic, itemKey, attribute.Value);
          }
          else {
            _typeCache.SetPropertyValue(_associatedTopic, itemKey, initialObject);
          }
        }
        catch (TargetInvocationException ex) {
          if (PropertyCache.ContainsKey(itemKey)) {
            PropertyCache.Remove(itemKey);
          }
          if (ex.InnerException is not null) {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
          }
          throw;
        }
        _setCounter = 0;
        return false;
      }
      return true;
    }

  } //Class
} //Namespace
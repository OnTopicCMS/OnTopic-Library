﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Collections;
using OnTopic.Collections.Specialized;

namespace OnTopic.Attributes {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE SETTER [ATTRIBUTE]
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Flags that a property should be used when setting an attribute via <see cref="AttributeCollection.SetValue(String,
  ///   String?, Boolean?, DateTime?, Boolean?)"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     When a call is made to <see cref="AttributeCollection.SetValue(String, String, Boolean?, DateTime?, Boolean?)"/>, the
  ///     code will check to see if a property with the same name as the attribute key exists, and whether that property is
  ///     decorated with the <see cref="AttributeSetterAttribute"/> (i.e., <c>[AttributeSetter]</c>). If it is, then the update
  ///     will be routed through that property. This ensures that business logic is enforced by local properties, instead of
  ///     allowing business logic to be potentially bypassed by writing directly to the <see cref="Topic.Attributes"/>
  ///     collection.
  ///   </para>
  ///   <para>
  ///     As an example, the <see cref="Topic.Key"/> property is adorned with the <see cref="AttributeSetterAttribute"/>. As a
  ///     result, if a client calls <c>topic.Attributes.SetValue("Key", "NewKey")</c> then that update will be routed through
  ///     <see cref="Topic.Key"/>, thus enforcing key validation, and calling <see cref="KeyedTopicCollection{T}.ChangeKey(T,
  ///     String)"/>. Similarly, if <c>topic.Attributes.SetValue("Key", ":/? ")</c> were called, a contract exception will be
  ///     thrown since <c>:/? </c> violates <see cref="TopicFactory.ValidateKey(String, Boolean)"/>.
  ///   </para>
  ///   <para>
  ///     To ensure this logic, it is critical that implementers of <see cref="AttributeSetterAttribute"/> ensure that the
  ///     property setters call <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?,
  ///     Boolean, DateTime?)"/> overload with the final parameter set to false to disable the enforcement of business logic.
  ///     Otherwise, an infinite loop will occur. Calling that overload tells <see cref="AttributeCollection"/> that the
  ///     business logic has already been enforced by the caller. As this is an internal overload, implementers should use the
  ///     local proxy at <see cref="Topic.SetAttributeValue(String, String, Boolean?)"/>, which ensures that final parameter is
  ///     set to <c>false</c>.
  ///   </para>
  /// </remarks>
  [AttributeUsage(AttributeTargets.Property)]
  public sealed class AttributeSetterAttribute : Attribute {

  } //Class
} //Namespace
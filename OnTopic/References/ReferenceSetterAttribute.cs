/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Collections.Specialized;

namespace OnTopic.References {

  /*============================================================================================================================
  | CLASS: REFERENCE SETTER [ATTRIBUTE]
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Flags that a property should be used when setting a reference via <see cref="TrackedCollection{TItem, TValue, TAttribute
  ///   }.SetValue(String, TValue, Boolean?, DateTime?)"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     When a call is made to <see cref="TrackedCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?,
  ///     DateTime?)"/> the code will check to see if a property with the same name as the reference key exists, and whether
  ///     that property is decorated with the <see cref="ReferenceSetterAttribute"/> (i.e., <code>[ReferenceSetter]</code>). If
  ///     it is, then the update will be routed through that property. This ensures that business logic is enforced by local
  ///     properties, instead of allowing business logic to be potentially bypassed by writing directly to the <see cref="Topic.
  ///     References"/> collection.
  ///   </para>
  ///   <para>
  ///     As an example, the <see cref="Topic.BaseTopic"/> property is adorned with the <see cref="ReferenceSetterAttribute"/>.
  ///     As a result, if a client calls <code>topic.References.SetTopic("BaseTopic", topic)</code>, then that update will be
  ///     routed through <see cref="Topic.BaseTopic"/>, thus enforcing any validation.
  ///   </para>
  ///   <para>
  ///     To ensure this logic, it is critical that implementers of <see cref="ReferenceSetterAttribute"/> ensure that the
  ///     property setters call the <see cref="TrackedCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?,
  ///     DateTime?)"/> overload with the final parameter set to <c>false</c> to disable the enforcement of business logic.
  ///     Otherwise, an infinite loop will occur. Calling that overload tells <see cref="TopicReferenceCollection"/> that the
  ///     business logic has already been enforced by the caller.
  ///   </para>
  /// </remarks>
  [AttributeUsage(AttributeTargets.Property)]
  public sealed class ReferenceSetterAttribute : Attribute {

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Collections;

namespace OnTopic.Attributes {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE SETTER [ATTRIBUTE]
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Flags that a property should be used when setting an attribute via
  ///   <see cref="AttributeValueCollection.SetValue(String, String, Boolean?)"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     When a call is made to <see cref="AttributeValueCollection.SetValue(String, String, Boolean?)"/>, the code will check
  ///     to see if a property with the same name as the attribute key exists, and whether that property is decorated with the
  ///     <see cref="AttributeSetterAttribute"/> (i.e., <code>[AttributeSetter]</code>). If it is, then the update will be
  ///     routed through that property. This ensures that business logic is enforced by local properties, instead of allowing
  ///     business logic to be potentially bypassed by writing directly to the <see cref="Topic.Attributes"/> collection.
  ///   </para>
  ///   <para>
  ///     As an example, the <see cref="Topic.Key"/> property is adorned with the <see cref="AttributeSetterAttribute"/>. As a
  ///     result, if a client calls <code>topic.Attributes.SetValue("Key", "NewKey")</code> then that update will be routed
  ///     through <see cref="Topic.Key"/>, thus enforcing key validation, and calling
  ///     <see cref="TopicCollection{T}.ChangeKey(T, String)"/>. Similarly, if <code>topic.Attributes.SetValue("Key", ":/? ")
  ///     </code> were called, a contract exception will be thrown since <code>:/? </code> violates
  ///     <see cref="TopicFactory.ValidateKey(String, Boolean)"/>.
  ///   </para>
  ///   <para>
  ///     To ensure this logic, it is critical that implementers of <see cref="AttributeSetterAttribute"/> ensure that the
  ///     property setters call <see cref="AttributeValueCollection.SetValue(String, String, Boolean?, Boolean)"/> overload with
  ///     the final parameter set to false to disable the enforcement of business logic. Otherwise, an infinite loop will occur.
  ///     Calling that overload tells <see cref="AttributeValueCollection"/> that the business logic has already been enforced
  ///     by the caller. As this is an internal overload, implementers should use the local proxy at
  ///     <see cref="Topic.SetAttributeValue(String, String, Boolean?)"/>, which ensures that final parameter is set to false.
  ///   </para>
  /// </remarks>
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public sealed class AttributeSetterAttribute : System.Attribute {

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Collections;

namespace Ignia.Topics.Mapping.Annotations {

  /*============================================================================================================================
  | ATTRIBUTE: INHERIT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Flags that a property should be inherit its value from <see cref="Topic.Parent"/> when calling <see
  ///   cref="AttributeValueCollection.GetValue(String, Boolean)"/>.
  /// </summary>
  /// <remarks>
  ///   By default, <see cref="ITopicMappingService"/> implementations will call <see
  ///   cref="AttributeValueCollection.GetValue(string, bool)"/> with the <c>inheritFromParent</c> parameter set to its default
  ///   of <c>false</c>. This attribute instructs it to instead set that parameter to <c>true</c>, which in turn causes the
  ///   <see cref="AttributeValueCollection"/> to crawl up the <see cref="Topic.Parent"/> tree until a value is found. This is
  ///   useful when an attribute is expected to be inherited by all child topics.
  /// </remarks>
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public sealed class InheritAttribute : System.Attribute {

  } //Class
} //Namespace
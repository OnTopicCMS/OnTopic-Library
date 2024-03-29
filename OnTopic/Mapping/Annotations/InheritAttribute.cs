﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Collections.Specialized;

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ATTRIBUTE: INHERIT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Flags that a property should be inherit its value from <see cref="Topic.Parent"/> when calling <see
  ///   cref="TrackedRecordCollection{TItem, TValue, TAttribute}.GetValue(String, Boolean)"/>.
  /// </summary>
  /// <remarks>
  ///   By default, <see cref="ITopicMappingService"/> implementations will call <see cref="TrackedRecordCollection{TItem,
  ///   TValue, TAttribute}.GetValue(String, Boolean)"/> with the <c>inheritFromParent</c> parameter set to its default of <c>
  ///   false</c>. This attribute instructs it to instead set that parameter to <c>true</c>, which in turn causes the <see cref=
  ///   "TrackedRecordCollection{TItem, TValue, TAttribute}"/> to crawl up the <see cref="Topic.Parent"/> tree until a value is
  ///   found. This is useful when an attribute is expected to be inherited by all child topics.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
  public sealed class InheritAttribute : Attribute {

  } //Class
} //Namespace
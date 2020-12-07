// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "Invalid overload; known bug in code analysis", Scope = "member", Target = "~M:OnTopic.Topic.GetWebPath~System.String")]
[assembly: SuppressMessage("Usage", "CA2249:Consider using 'string.Contains' instead of 'string.IndexOf'", Justification = "StringComparison overload not supported by .NET Standard 2.0.", Scope = "member", Target = "~M:OnTopic.Querying.TopicExtensions.FindAllByAttribute(OnTopic.Topic,System.String,System.String)~OnTopic.Collections.ReadOnlyTopicCollection{OnTopic.Topic}")]
[assembly: SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Expected by convention for OnTopic Editor", Scope = "namespaceanddescendants", Target = "~N:OnTopic")]
[assembly: SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Breaking change; deferred to major version.", Scope = "namespaceanddescendants", Target = "~N:OnTopic")]
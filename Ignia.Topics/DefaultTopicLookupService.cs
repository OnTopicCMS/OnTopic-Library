/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Metadata;
using System.Reflection;
using Ignia.Topics.Metadata.AttributeTypes;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: TYPE INDEX
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="DefaultTopicLookupService"/> can be configured to provide a lookup of .
  /// </summary>
  public class DefaultTopicLookupService: StaticTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="DefaultTopicLookupService"/>. Optionally accepts a list of <see cref="Type"/>
    ///   instances and a default <see cref="Type"/> value.
    /// </summary>
    /// <remarks>
    ///   Any <see cref="Type"/> instances submitted via <paramref name="types"/> should be unique by <see
    ///   cref="MemberInfo.Name"/>; if they are not, they will be removed.
    /// </remarks>
    /// <param name="types">The list of <see cref="Type"/> instances to expose as part of this service.</param>
    /// <param name="defaultType">The default type to return if no match can be found. Defaults to object.</param>
    public DefaultTopicLookupService(IEnumerable<Type> types = null, Type defaultType = null) :
      base(types, defaultType?? typeof(Topic)) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure editor types are accounted for
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!Contains(nameof(ContentTypeDescriptor)))             Add(typeof(ContentTypeDescriptor));
      if (!Contains(nameof(AttributeDescriptor)))               Add(typeof(AttributeDescriptor));
      if (!Contains(nameof(BooleanAttribute)))                  Add(typeof(BooleanAttribute));
      if (!Contains(nameof(DateTimeAttribute)))                 Add(typeof(DateTimeAttribute));
      if (!Contains(nameof(FileListAttribute)))                 Add(typeof(FileListAttribute));
      if (!Contains(nameof(FilePathAttribute)))                 Add(typeof(FilePathAttribute));
      if (!Contains(nameof(HtmlAttribute)))                     Add(typeof(HtmlAttribute));
      if (!Contains(nameof(LastModifiedAttribute)))             Add(typeof(LastModifiedAttribute));
      if (!Contains(nameof(LastModifiedByAttribute)))           Add(typeof(LastModifiedByAttribute));
      if (!Contains(nameof(NestedTopicListAttribute)))          Add(typeof(NestedTopicListAttribute));
      if (!Contains(nameof(NumberAttribute)))                   Add(typeof(NumberAttribute));
      if (!Contains(nameof(QueryableTopicListAttribute)))       Add(typeof(QueryableTopicListAttribute));
      if (!Contains(nameof(RelationshipAttribute)))             Add(typeof(RelationshipAttribute));
      if (!Contains(nameof(TextAreaAttribute)))                 Add(typeof(TextAreaAttribute));
      if (!Contains(nameof(TextAttribute)))                     Add(typeof(TextAttribute));
      if (!Contains(nameof(TokenizedTopicListAttribute)))       Add(typeof(TokenizedTopicListAttribute));
      if (!Contains(nameof(TopicListAttribute)))                Add(typeof(TopicListAttribute));
      if (!Contains(nameof(TopicReferenceAttribute)))           Add(typeof(TopicReferenceAttribute));

    }

  } //Class
} //Namespace
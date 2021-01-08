/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Reflection;
using OnTopic.Metadata;
using OnTopic.Metadata.AttributeTypes;

namespace OnTopic.Lookup {

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
    ///   Establishes a new instance of a <see cref="DefaultTopicLookupService"/>. Optionally accepts a list of <see
    ///   cref="Type"/> instances and a default <see cref="Type"/> value.
    /// </summary>
    /// <remarks>
    ///   Any <see cref="Type"/> instances submitted via <paramref name="types"/> should be unique by <see
    ///   cref="MemberInfo.Name"/>; if they are not, they will be removed.
    /// </remarks>
    /// <param name="types">The list of <see cref="Type"/> instances to expose as part of this service.</param>
    /// <param name="defaultType">The default type to return if no match can be found. Defaults to object.</param>
    public DefaultTopicLookupService(IEnumerable<Type>? types = null, Type? defaultType = null) :
      base(types, defaultType?? typeof(Topic)) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure editor types are accounted for
      \-----------------------------------------------------------------------------------------------------------------------*/
      TryAdd(typeof(ContentTypeDescriptor));
      TryAdd(typeof(AttributeDescriptor));
      TryAdd(typeof(BooleanAttribute));
      TryAdd(typeof(DateTimeAttribute));
      TryAdd(typeof(FileListAttribute));
      TryAdd(typeof(FilePathAttribute));
      TryAdd(typeof(HtmlAttribute));
      TryAdd(typeof(IncomingRelationshipAttribute));
      TryAdd(typeof(InstructionAttribute));
      TryAdd(typeof(LastModifiedAttribute));
      TryAdd(typeof(LastModifiedByAttribute));
      TryAdd(typeof(NestedTopicListAttribute));
      TryAdd(typeof(NumberAttribute));
      TryAdd(typeof(QueryableTopicListAttribute));
      TryAdd(typeof(RelationshipAttribute));
      TryAdd(typeof(TextAreaAttribute));
      TryAdd(typeof(TextAttribute));
      TryAdd(typeof(TokenizedTopicListAttribute));
      TryAdd(typeof(TopicListAttribute));
      TryAdd(typeof(TopicReferenceAttribute));

    }

  } //Class
} //Namespace
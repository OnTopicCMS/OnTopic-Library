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
      if (!Contains("ContentTypeDescriptor")) Add(typeof(ContentTypeDescriptor));
      if (!Contains("AttributeDescriptor")) Add(typeof(AttributeDescriptor));

    }

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ignia.Topics.Reflection {

  /*============================================================================================================================
  | CLASS: MEMBER INFO COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides keyed access to a collection of <see cref="MemberInfoCollection"/> instances.
  /// </summary>
  internal class MemberInfoCollection : MemberInfoCollection<MemberInfo> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="MemberInfoCollection"/> class associated with a <see cref="Type"/>
    ///   name.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> associated with the collection.</param>
    internal MemberInfoCollection(Type type) : base(type) {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="MemberInfoCollection"/> class associated with a <see cref="Type"/>
    ///   name and prepopulates it with a predetermined set of <see cref="MemberInfo"/> instances.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> associated with the collection.</param>
    /// <param name="members">
    ///   An <see cref="IEnumerable{MemberInfo}"/> of <see cref="MemberInfo"/> instances to populate the collection.
    /// </param>
    internal MemberInfoCollection(Type type, IEnumerable<MemberInfo> members) : base(type, members) {
    }

  } //Class

} //Namespace
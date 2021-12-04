/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Internal.Reflection {

  /*============================================================================================================================
  | CLASS: MEMBER ACCESSOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  internal class TypeAccessor {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            Dictionary<string, MemberAccessor>                              _members;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="TypeAccessor"/> based on a given <paramref name="type"/>.
    /// </summary>
    /// <remarks>
    ///   The <see cref="TypeAccessor"/> constructor automatically identifies each supported <see cref="MemberInfo"/> on the
    ///   <see cref="Type"/> and adds an associated <see cref="MemberAccessor"/> to the <see cref="TypeAccessor"/> for each.
    /// </remarks>
    /// <param name="type"></param>
    internal TypeAccessor(Type type) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(type, nameof(type));

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize fields, properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      _members                  = new(StringComparer.OrdinalIgnoreCase);
      Type                      = type;

      /*------------------------------------------------------------------------------------------------------------------------
      | Get members from type
      \-----------------------------------------------------------------------------------------------------------------------*/
      var members               = type.GetMembers(
          BindingFlags.Instance |
          BindingFlags.FlattenHierarchy |
          BindingFlags.NonPublic |
          BindingFlags.Public
      );
      foreach (var member in members.Where(t => MemberAccessor.IsValid(t))) {
        _members.Add(member.Name, new(member));
      }

    }

    /*==========================================================================================================================
    | TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the return type of a getter or the argument type of a setter.
    /// </summary>
    internal Type Type { get; }

    /*==========================================================================================================================
    | GET MEMBERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of properties and methods supported by the <see cref="TypeAccessor"/>.
    /// </summary>
    /// <param name="memberTypes">Optionally filters the list of members by a <see cref="MemberTypes"/> list.</param>
    /// <returns>A list of <see cref="MemberAccessor"/> instances.</returns>
    internal List<MemberAccessor> GetMembers(MemberTypes memberTypes = MemberTypes.All) =>
      _members.Values.Where(m => memberTypes == MemberTypes.All || memberTypes.HasFlag(m.MemberType)).ToList();

    /*==========================================================================================================================
    | GET MEMBER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a property or method supported by the <see cref="TypeAccessor"/> by <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="MemberAccessor"/>, derived from <see cref="MemberInfo.Name"/>.</param>
    /// <returns>A <see cref="MemberAccessor"/> instance for getting or setting values on a given member.</returns>
    internal MemberAccessor? GetMember(string name) => _members.GetValueOrDefault(name);

    /*==========================================================================================================================
    | HAS GETTER?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if a <see cref="MemberAccessor"/> with a getter exists for a member with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="MemberAccessor"/>, derived from <see cref="MemberInfo.Name"/>.</param>
    /// <returns>True if a gettable member exists; otherwise false.</returns>
    internal bool HasGetter(string name) => GetMember(name)?.IsGettable ?? false;

    /*==========================================================================================================================
    | HAS SETTER?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if a <see cref="MemberAccessor"/> with a setter exists for a member with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="MemberAccessor"/>, derived from <see cref="MemberInfo.Name"/>.</param>
    /// <returns>True if a settable member exists; otherwise false.</returns>
    internal bool HasSetter(string name) => GetMember(name)?.IsSettable ?? false;

    /*==========================================================================================================================
    | GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves the value from a member named <paramref name="memberName"/> from the supplied <paramref name="source"/>
    ///   object.
    /// </summary>
    /// <param name="source">The <see cref="Object"/> instance from which the value should be retrieved.</param>
    /// <param name="memberName">The name of the method or property from which the value should be retrieved.</param>
    /// <returns>The value returned from the member.</returns>
    internal object? GetValue(object source, string memberName) => GetMember(memberName)?.GetValue(source);

    /*==========================================================================================================================
    | SET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets the value of a member named <paramref name="memberName"/> on the supplied <paramref name="source"/> object.
    /// </summary>
    /// <param name="source">The <see cref="Object"/> instance on which the value should be set.</param>
    /// <param name="memberName">The name of the method or property on which the value should be set.</param>
    /// <param name="value">The <see cref="Object"/> value to set the member to.</param>
    internal void SetValue(object source, string memberName, object? value) => GetMember(memberName)?.SetValue(source, value);

  } //Class
} //Namespace
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnTopic.Data.Caching;
using OnTopic.Internal.Reflection;
using OnTopic.Lookup;
using OnTopic.Mapping;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.Tests.TestDoubles;
using OnTopic.ViewModels;

namespace OnTopic.Tests.Fixtures {

  /*============================================================================================================================
  | CLASS: TYPE ACCESSOR FIXTURE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Introduces a shared context to use for unit tests depending on an <see cref="TypeAccessor"/>.
  /// </summary>
  public class TypeAccessorFixture<T> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    public TypeAccessorFixture() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Create type accessor
      \-----------------------------------------------------------------------------------------------------------------------*/
      TypeAccessor              = new TypeAccessor(typeof(T));

    }

    /*==========================================================================================================================
    | TYPE ACCESSOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A <see cref="TypeAccessor"/> for accessing <see cref="MemberAccessor"/> instances.
    /// </summary>
    internal TypeAccessor TypeAccessor { get; private set; }

  }
}

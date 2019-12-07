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

namespace Ignia.Topics.Metadata {

  /*============================================================================================================================
  | ENUM: MODEL TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides options describing how a specific attribute is exposed in terms of the Topic Library's object model.
  /// </summary>
  public enum ModelType {

    /*--------------------------------------------------------------------------------------------------------------------------
    | SCALAR VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A standard attribute value, exposed primarily through the <see cref="Topic.Attributes"/> collection, or a
    ///   corresponding property.
    /// </summary>
    ScalarValue                 = 1,

    /*--------------------------------------------------------------------------------------------------------------------------
    | RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A relationship to a collection of separate <see cref="Topic"/>s. Relationships are exposed as strongly-typed
    ///   collections through the <see cref="Topic.Relationships"/> property.
    /// </summary>
    Relationship                = 2,

    /*--------------------------------------------------------------------------------------------------------------------------
    | TOPIC REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A reference to a separate <see cref="Topic"/>. References are ideally exposed and populated as strongly-typed
    ///   properties on <see cref="Topic"/>—or a derived class—as done with e.g. <see cref="Topic.DerivedTopic"/>.
    /// </summary>
    Reference                   = 3,

    /*--------------------------------------------------------------------------------------------------------------------------
    | NESTED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A set of child <see cref="Topic"/>s packaged as part of the current <see cref="Topic"/>.
    /// </summary>
    NestedTopic                 = 4

  } //Enum
} //Namespace
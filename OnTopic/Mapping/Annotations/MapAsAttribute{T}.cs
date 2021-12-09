/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ATTRIBUTE: MAP AS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc cref="MapAsAttribute"/>
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
  public sealed class MapAsAttribute<T> : Attribute {

    /*==========================================================================================================================
    | PROPERTY: TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the value of the <see cref="Type"/> that the association should be mapped to.
    /// </summary>
    public Type Type { get; } = typeof(T);

  } //Class
} //Namespace
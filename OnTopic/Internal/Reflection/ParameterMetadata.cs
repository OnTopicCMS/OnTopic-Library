/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Reflection;

namespace OnTopic.Internal.Reflection {

  /*============================================================================================================================
  | CLASS: PARAMETER METADATA
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides metadata associated with a given parameter.
  /// </summary>
  internal class ParameterMetadata: ItemMetadata {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ParameterMetadata"/> class associated with a <see cref="ParameterInfo"/>
    ///   instance.
    /// </summary>
    /// <param name="parameterInfo">The <see cref="ParameterInfo"/> associated with this instance.</param>
    internal ParameterMetadata(ParameterInfo parameterInfo): base (parameterInfo.Name, parameterInfo) {
      ParameterInfo             = parameterInfo;
      Type                      = parameterInfo.ParameterType;
    }

    /*==========================================================================================================================
    | PARAMETER INFO
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the <see cref="ParameterInfo"/> associated with this instance of <see cref="ParameterMetadata"/>.
    /// </summary>
    public ParameterInfo ParameterInfo { get; }

  } //Class
} //Namespace
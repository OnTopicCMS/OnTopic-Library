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

    ScalarValue                 = 1,
    Relationship                = 2,
    Reference                   = 3,
    NestedTopic                 = 4

  } //Enum
} //Namespace

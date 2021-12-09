﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ENUM: RELATIONSHIPS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc cref="AssociationTypes"/>
  [Flags]
  [Obsolete($"The {nameof(Relationships)} enum has been renamed to {nameof(AssociationTypes)}.", true)]
  public enum Relationships {

    /*==========================================================================================================================
    | NONE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Do not follow any associations.
    /// </summary>
    None                        = 0

  } //Enum
} //Namespace
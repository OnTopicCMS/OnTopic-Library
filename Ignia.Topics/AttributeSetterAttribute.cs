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
using Ignia.Topics.Collections;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Flags that a property should be used when setting an attribute via
  ///   <see cref="AttributeValueCollection.SetValue(String, String, Boolean?)"/>.
  /// </summary>
  [System.AttributeUsage(System.AttributeTargets.Property)]
  sealed class AttributeSetterAttribute : System.Attribute {

  } //Class

} //Namespace

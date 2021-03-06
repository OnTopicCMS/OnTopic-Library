/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

/*==============================================================================================================================
| DEFINE ASSEMBLY ATTRIBUTES
>===============================================================================================================================
| Declare and define attributes used in the compiling of the finished assembly.
\-----------------------------------------------------------------------------------------------------------------------------*/
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: Guid("27632801-bfe3-41d9-8678-3c4bbe45e6c9")]

/*==============================================================================================================================
| HANDLE SUPPRESSIONS
>===============================================================================================================================
| Suppress warnings from code analysis that are either false positives or not relevant for this assembly.
\-----------------------------------------------------------------------------------------------------------------------------*/
[assembly: SuppressMessage("Naming", "CA1707: Remove the underscores from member name", Justification = "Used by convention for unit tests", Scope = "namespaceanddescendants", Target = "~N:OnTopic.Tests")]
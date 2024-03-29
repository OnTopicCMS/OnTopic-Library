﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: METHOD BASED REFERENCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing settable methods and gettable methods.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class MethodBasedReferenceViewModel {

    private TopicViewModel? _methodValue;

    public void SetMethod(TopicViewModel? methodValue) => _methodValue = methodValue;

    #pragma warning disable CA1024 // Use properties where appropriate
    public TopicViewModel? GetMethod() => _methodValue;
    #pragma warning restore CA1024 // Use properties where appropriate

  } //Class
} //Namespace
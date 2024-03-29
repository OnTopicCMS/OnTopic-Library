﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1024 // Use properties where appropriate

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: METHOD BASED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing settable methods and gettable methods.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  public class MethodBasedViewModel {

    private int _methodValue;

    public void SetMethod(int methodValue) => _methodValue = methodValue;
    public int GetMethod() => _methodValue;
    [DisplayName("Get Annotated Method")]
    public int GetAnnotatedMethod() => _methodValue;
    public TopicViewModel GetComplexMethod() => new();
    public void SetParametersMethod(int methodValue, int additionalValue) => _methodValue = methodValue + additionalValue;
    public void SetComplexMethod(NavigationTopicViewModel model) => _methodValue = model?.Children.Count?? 0;

  } //Class
} //Namespace

#pragma warning restore CA1024 // Use properties where appropriate
#pragma warning restore CA1822 // Mark members as static
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using OnTopic.Metadata;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: COMPATIBLE PROPERTY TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing views with a property that maps to a source object, but isn't
  ///   otherwise mapped by any of the conversion functions.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  [SuppressMessage("Usage", "CA2227", Justification = "This is intended to be initialized by the mapping service.")]
  public class CompatiblePropertyTopicViewModel {

    public ModelType ModelType { get; set; }

    public Collection<DateTime>? VersionHistory { get; set; }

  } //Class
} //Namespace
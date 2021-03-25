/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OnTopic.Mapping;
using OnTopic.Mapping.Annotations;
using OnTopic.Tests.BindingModels;
using OnTopic.ViewModels;

#pragma warning disable CA2227 // Collection properties should be read only
#pragma warning disable CA1034 // Nested types should not be visible

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: NULL COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a view model with null collections to determine if they can be correctly mapped.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     As a best practice, view models—as with all classes—should have collection properties initialized during construction,
  ///     and non-settable. If this isn't possible, however, the <see cref="TopicMappingService"/> optionally supports creating
  ///     collections based on a number of rules.
  ///   </para>
  ///   <para>
  ///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  ///   </para>
  /// </remarks>
  public class NullCollectionTopicViewModel {

    [Collection("Collection")]
    public Collection<KeyOnlyTopicViewModel>? NullCollection { get; set; }

    [Collection("Collection")]
    public ICollection<KeyOnlyTopicViewModel>? NullICollection { get; set; }

    [Collection("Collection")]
    public IList<KeyOnlyTopicViewModel>? NullIList { get; set; }

    [Collection("Collection")]
    public IEnumerable<KeyOnlyTopicViewModel>? NullIEnumerable { get; set; }

    [Collection("Collection")]
    public TypedTopicCollection? NullTypedCollection { get; set; }

    public class TypedTopicCollection: Collection<KeyOnlyTopicViewModel> { }

  } //Class
} //Namespace

#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CA1034 // Nested types should not be visible
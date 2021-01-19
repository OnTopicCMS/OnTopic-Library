/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia
| Project       Website
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
using OnTopic.Models;

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: NAVIGATION TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for feeding views with information about the navigation.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     No topics are expected to have a <c>Navigation</c> content type. Instead, this view model is expected to be manually
  ///     constructed by e.g. a <c>LayoutController</c>.
  ///   </para>
  ///   <para>
  ///     Since C# doesn't support return-type covariance, this class can't be derived in a meaningful way (i.e., if it were to
  ///     be, the <see cref="NavigationTopicViewModel.Children"/> property would still return a <see cref="Collection{T}"/> of
  ///     <see cref="NavigationTopicViewModel"/> instances). Instead, the preferred way to extend the functionality is to create
  ///     a new implementation of <see cref="INavigationTopicViewModel{T}"/>. To help communicate this, the <see
  ///     cref="NavigationTopicViewModel"/> class is marked as <c>sealed</c>.
  ///   </para>
  /// </remarks>
  public sealed record NavigationTopicViewModel : TopicViewModel, INavigationTopicViewModel<NavigationTopicViewModel> {

    /*==========================================================================================================================
    | SHORT TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a short title to be used in the navigation, for cases where the normal title is too long.
    /// </summary>
    public string? ShortTitle { get; init; }

    /*==========================================================================================================================
    | CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a list of nested <see cref="NavigationTopicViewModel"/> objects, for handling hierarchical navigation.
    /// </summary>
    public Collection<NavigationTopicViewModel> Children { get; } = new();

    /*==========================================================================================================================
    | IS SELECTED?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether or not the node represented by this <see cref="NavigationTopicViewModel"/> is currently selected,
    ///   typically meaning the user is on the page this object is pointing to.
    /// </summary>
    public bool IsSelected(string uniqueKey) =>
      $"{uniqueKey}:".StartsWith($"{UniqueKey}:", StringComparison.InvariantCultureIgnoreCase);

  } //Class
} //Namespace
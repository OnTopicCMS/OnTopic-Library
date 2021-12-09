/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.AspNetCore.Mvc.Components;
using OnTopic.Models;

namespace OnTopic.AspNetCore.Mvc.Models {

  /*============================================================================================================================
  | VIEW MODEL: NAVIGATION TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed view model for feeding views with information expected to be required for each node in of
  ///   navigation.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     No topics are expected to have a <c>Navigation</c> content type. Instead, this view model is expected to be manually
  ///     constructed by the <see cref="NavigationTopicViewComponentBase{T}"/>.
  ///   </para>
  ///   <para>
  ///     The <see cref="NavigationRoot"/> can be any view model that implements <see cref="IHierarchicalTopicViewModel{T}"/>,
  ///     which ensures support for hierarchical coverage. In practice, we expect most implementers will choose to implement
  ///     <see cref="INavigationTopicViewModel{T}"/> instead, which addresses not only <see cref="IHierarchicalTopicViewModel{T}
  ///     "/>, but also provides a base level of support for properties that most navigation views will need, as well as a
  ///     method for determining if a given <see cref="IHierarchicalTopicViewModel{T}"/> instance is the currently-selected
  ///     topic. Derived implementations may introduce additional properties, as appropriate.
  ///   </para>
  /// </remarks>
  public class NavigationViewModel<T> where T : class, IHierarchicalTopicViewModel<T> {

    /*==========================================================================================================================
    | NAVIGATION ROOT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents the root element in the navigation.
    /// </summary>
    /// <remarks>
    ///   Since this implements <see cref="IHierarchicalTopicViewModel{T}"/>, it  may include multiple levels of children. By
    ///   implementing it as a generic, each site or application can provide its own <see cref="IHierarchicalTopicViewModel{T}"
    ///   /> implementation, thus potentially extending the schema with properties relevant to that site's navigation. For
    ///   example, a site may optionally add an <c>IconUrl</c> property if it wishes to assign unique icons to each link in the
    ///   navigation.
    /// </remarks>
    public T? NavigationRoot { get; set; }

    /*==========================================================================================================================
    | CURRENT WEB PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The <see cref="Topic.GetWebPath()"/> representing the path to the current <see cref="Topic"/>.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     In order to determine whether any given <see cref="INavigationTopicViewModel{T}.IsSelected(String)"/>, the views
    ///     will need to know where in the hierarchy the user currently is. By storing this on the <see cref="
    ///     NavigationViewModel{T}"/> used as the root view model for every navigation component, we ensure that the views
    ///     always have access to this information.
    ///   </para>
    ///   <para>
    ///     It's worth noting that while this <i>could</i> be stored on the <see cref="IHierarchicalTopicViewModel{T}"/> itself,
    ///     that would prevent those values from being cached. As such, it's preferrable to keep the navigation nodes stateless,
    ///     and maintaining state exclusively in the <see cref="NavigationViewModel{T}"/> itself.
    ///   </para>
    /// </remarks>
    public string CurrentWebPath { get; set; } = default!;

    /*==========================================================================================================================
    | CURRENT KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The <see cref="Topic.GetWebPath()"/> representing the path to the current <see cref="Topic"/>.
    /// </summary>
    /// <inheritdoc cref="CurrentWebPath"/>
    [ExcludeFromCodeCoverage]
    [Obsolete($"The {nameof(CurrentKey)} property has been replaced in favor of {nameof(CurrentWebPath)}.", true)]
    public string CurrentKey { get; set; } = default!;

  } //Class
} //Namespace
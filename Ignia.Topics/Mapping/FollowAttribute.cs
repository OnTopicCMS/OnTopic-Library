/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | ATTRIBUTE: FOLLOW
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Instructs the <see cref="ITopicMappingService"/> to continue following relationships on that property. Optionally
  ///   specifies which relationships should be followed.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     By default, <see cref="ITopicMappingService"/> will populate all relationships on the initial data transfer object,
  ///     but won't continue to do so on related objects. So, for instance, a <c>Children</c> collection will cause all children
  ///     to be loaded, but the mapper won't populate their <c>Children</c> (assuming that property is set).
  ///   </para>
  ///   <para>
  ///     The <see cref="FollowAttribute"/> overrides this behavior. If set, the <see cref="ITopicMappingService"/> will
  ///     populate the <see cref="Relationships"/> specified on the related topics. By default, it will crawl <i>all</i>
  ///     relationships, but the <see cref="Relationships"/> flag can optionally be used to specify one or multiple
  ///     relationship types, thus providing fine-tune control.
  ///   </para>
  /// </remarks>
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public sealed class FollowAttribute : System.Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="FollowAttribute"/> by providing an <paramref name="relationships"/>.
    /// </summary>
    /// <param name="relationships">The specific relationships that should be crawled.</param>
    public FollowAttribute(Relationships relationships) {
      Relationships = relationships;
    }

    /*==========================================================================================================================
    | PROPERTY: RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the type(s) of relationships that should be recursed over.
    /// </summary>
    public Relationships Relationships { get; }

  } //Class

} //Namespace

/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Diagnostics.CodeAnalysis;

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ATTRIBUTE: FOLLOW
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc cref="IncludeAttribute"/>
  [ExcludeFromCodeCoverage]
  [AttributeUsage(AttributeTargets.Property)]
  [Obsolete($"The {nameof(FollowAttribute)} has been renamed to {nameof(IncludeAttribute)}.", true)]
  public sealed class FollowAttribute : Attribute {

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
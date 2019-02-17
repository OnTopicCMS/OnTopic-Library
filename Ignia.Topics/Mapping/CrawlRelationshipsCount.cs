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
using Mapping = Ignia.Topics.Mapping;

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | CLASS: CRAWL RELATIONSHIPS COUNT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Determines what types of relationships should be crawled and how far.
  /// </summary>
  /// <remarks>
  ///   For each type of relationship, a property of type <see cref="Int32"/> is exposed. If the value is 0, then the
  ///   relationship should not be crawled. If the count is positive, then it should be reduced by one each time a relationship
  ///   is successfully navigated.
  /// </remarks>
  public struct CrawlRelationshipsCount {

    /*==========================================================================================================================
    | CONSTRUCTOR: CRAWL RELATIONSHIPS COUNT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a new <see cref="CrawlRelationships"/> value object by specifying the (remaining) count for each relationship.
    /// </summary>
    /// <param name="children">Number of remaining permitted iterations for children.</param>
    /// <param name="relationships">Number of remaining permitted iterations for relationships.</param>
    /// <param name="incomingRelationships">Number of remaining permitted iterations for incoming relationships.</param>
    /// <param name="references">Number of remaining permitted iterations for references.</param>
    /// <param name="parents">Number of remaining permitted iterations for parents.</param>
    public CrawlRelationshipsCount(
      int children,
      int relationships         = 0,
      int incomingRelationships = 0,
      int references            = 0,
      int parents               = 0
    ) {
      Children                  = children;
      Relationships             = relationships;
      IncomingRelationships     = incomingRelationships;
      References                = references;
      Parents                   = parents;
    }

    /// <summary>
    ///   Constructs a new <see cref="CrawlRelationships"/> value object by providing a <see cref="Relationships"/> enum. If the
    ///   flag is set for a specific relationship type, then the iteration could it set to 10.
    /// </summary>
    public CrawlRelationshipsCount(Relationships relationships) {
      Children                  = relationships.HasFlag(Mapping.Relationships.Children) ? 10 : 0;
      Relationships             = relationships.HasFlag(Mapping.Relationships.Relationships) ? 10 : 0;
      IncomingRelationships     = relationships.HasFlag(Mapping.Relationships.Children) ? 10 : 0;
      References                = relationships.HasFlag(Mapping.Relationships.References) ? 10 : 0;
      Parents                   = relationships.HasFlag(Mapping.Relationships.Parents) ? 10 : 0;
    }

    /// <summary>
    ///   Constructs a new <see cref="CrawlRelationships"/> value object by specifying the <see cref="PropertyConfiguration"/>
    ///   (for attribute values) and the current values for the <see cref="CrawlRelationshipsCount"/>.
    /// </summary>
    /// <remarks>
    ///   If a relationship count is specified via an attribute on the current target, as collated by the <see
    ///   cref="PropertyConfiguration"/>, and that value is lower than the current values, then the remaining iteration count
    ///   will be reduced to the lower number.
    /// </remarks>
    public CrawlRelationshipsCount(PropertyConfiguration configuration, CrawlRelationshipsCount previousCount) {
      this = previousCount;
    }

    /*==========================================================================================================================
    | PROPERTY: CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents the number of remaining iterations that are permitted for expanding child topics.
    /// </summary>
    public int Children { get; }

    /*==========================================================================================================================
    | PROPERTY: RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents the number of remaining iterations that are permitted for expanding related topics.
    /// </summary>
    public int Relationships { get; }

    /*==========================================================================================================================
    | PROPERTY: INCOMING RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents the number of remaining iterations that are permitted for expanding topics related to the current topic.
    /// </summary>
    public int IncomingRelationships { get; }

    /*==========================================================================================================================
    | PROPERTY: REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents the number of remaining iterations that are permitted for expanding referenced topics.
    /// </summary>
    public int References { get; }

    /*==========================================================================================================================
    | PROPERTY: PARENTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents the number of remaining iterations that are permitted for expanding parent topics.
    /// </summary>
    public int Parents { get; }


    /*==========================================================================================================================
    | OPERATOR: EQUALITY (==)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Customizes the logic for evaluating whether two <see cref="CrawlRelationshipsCount"/> value objects are equal to one
    ///   another.
    /// </summary>
    public static bool operator ==(CrawlRelationshipsCount x, CrawlRelationshipsCount y) =>
      x.Children == y.Children &&
      x.Relationships == y.Relationships &&
      x.IncomingRelationships == y.IncomingRelationships &&
      x.References == y.References &&
      x.Parents == y.Parents;

    /*==========================================================================================================================
    | OPERATOR: INEQUALITY (==)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Customizes the logic for evaluating whether two <see cref="CrawlRelationshipsCount"/> value objects are not equal to
    ///   one another.
    /// </summary>
    public static bool operator !=(CrawlRelationshipsCount x, CrawlRelationshipsCount y) => !(x == y);

  } //Class
} //Namespace

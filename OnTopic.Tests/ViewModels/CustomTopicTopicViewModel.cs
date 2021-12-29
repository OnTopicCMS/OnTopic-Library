/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Attributes;
using OnTopic.Internal.Reflection;
using OnTopic.Tests.Entities;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: CUSTOM TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a topic view model which maps to the <see cref="CustomTopic"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  ///   </para>
  ///   <para>
  ///     Typically, topics wouldn't end in "Topic" (outside of <see cref="Topic"/>). Because <see cref="CustomTopic"/> does,
  ///     the corresponding topic view model must be named <see cref="CustomTopicTopicViewModel"/> (with two <c>Topic</c>).
  ///     Otherwise, when the <see cref="TypeAccessor"/> attempts to match it to its implied content type, it will strip the
  ///     <c>TopicViewModel</c>, per conventions, and discover that there isn't a match for <c>Custom</c>. This is a peculiarity
  ///     of the test case, and not something we'd expect in real-life scenarios.
  ///   </para>
  /// </remarks>
  public class CustomTopicTopicViewModel {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    public CustomTopicTopicViewModel(
      string title,
      string webPath,
      string? textAttribute,
      bool booleanAttribute,
      string booleanAsStringAttribute,
      int numericAttribute,
      string nonNullableAttribute,
      DateTime dateTimeAttribute,
      Topic? topicReference,
      string? unmatchedMember,
      DateTime? isHidden
    ) {
      Title                     = title;
      WebPath                   = webPath;
      TextAttribute             = textAttribute;
      BooleanAttribute          = booleanAttribute;
      BooleanAsStringAttribute  = booleanAsStringAttribute;
      NumericAttribute          = numericAttribute;
      NonNullableAttribute      = nonNullableAttribute;
      DateTimeAttribute         = dateTimeAttribute;
      TopicReference            = topicReference;
      UnmatchedMember           = unmatchedMember;
      IsHidden                  = isHidden;
    }

    /*==========================================================================================================================
    | TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the <see cref="Topic.Title"/> property.
    /// </summary>
    public string Title { get; init; }

    /*==========================================================================================================================
    | WEB PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the root-relative web path of the Topic, based on an assumption that the root topic is bound to the root of
    ///   the site.
    /// </summary>
    public string WebPath { get; init; }

    /*==========================================================================================================================
    | TEXT ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a text property which is intended to be mapped to a text attribute.
    /// </summary>
    public string? TextAttribute { get; init; }

    /*==========================================================================================================================
    | BOOLEAN ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a Boolean property which is intended to be mapped to a Boolean attribute.
    /// </summary>
    public bool BooleanAttribute { get; init; }

    /*==========================================================================================================================
    | BOOLEAN AS STRING ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a string property which is intended to be mapped to a Boolean attribute.
    /// </summary>
    public string BooleanAsStringAttribute { get; init; }

    /*==========================================================================================================================
    | NUMERIC ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a numeric property which is intended to be mapped to a numeric attribute.
    /// </summary>
    public int NumericAttribute { get; init; }

    /*==========================================================================================================================
    | NON-NULLABLE ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a property which does not permit null values as a means of ensuring the business logic is enforced when
    ///   removing values.
    /// </summary>
    public string NonNullableAttribute { get; init; }

    /*==========================================================================================================================
    | DATE/TIME ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a date/time property which is intended to be mapped to a date/time attribute.
    /// </summary>
    public DateTime DateTimeAttribute { get; init; }

    /*==========================================================================================================================
    | TOPIC REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a topic reference property which is intended to be mapped to a topic reference.
    /// </summary>
    public Topic? TopicReference { get; init; }

    /*==========================================================================================================================
    | UNMATCHED MEMBER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a member that doesn't match any members of either <see cref="CustomTopic"/> or <see cref="Topic"/>.
    /// </summary>
    public string? UnmatchedMember { get; init; }

    /*==========================================================================================================================
    | MISMATCHED MEMBER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a member that corresponds to a property on <see cref="Topic"/>, but with an incompatible type.
    /// </summary>
    public DateTime? IsHidden { get; init; }


  } //Class
} //Namespace
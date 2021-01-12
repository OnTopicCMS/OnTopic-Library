/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: KEY VALUES PAIR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a 1:n relationship used for implementations of <see cref="IDictionary{TKey, TValue}"/> as a means of
  ///   supporting a multimap.
  /// </summary>
  /// <remarks>
  ///   Out of the box, the .NET CLR includes a similar <see cref="KeyValuePair{TKey, TValue}"/> class, which serves an
  ///   identical purpose. The <see cref="KeyValuesPair{TKey, TValue}"/> class, however, provides more intuitive semantics for
  ///   working with multimap—i.e., 1:n—scenarios.
  /// </remarks>
  /// <example>
  ///   As an example, the <see cref="KeyValuesPair{TKey, TValue}"/> supports the following:
  ///   <code>
  ///     foreach (var relationship in topic.Relationships) {
  ///       foreach (var topic in relationship.Values) {
  ///         Console.Log(topic.Key);
  ///       }
  ///     }
  ///   </code>
  /// </example>
  public class KeyValuesPair<TKey, TValue> where TValue: class, ICollection<Topic> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a new instance of a <see cref="KeyValuesPair{TKey, TValue}"/> class with a <paramref name="key"/> and,
    ///   optionally, a <paramref name="values"/>.
    /// </summary>
    /// <param name="key">The key for the given <see cref="KeyValuesPair{TKey, TValue}"/> instance.</param>
    /// <param name="values">The optional set of values for the given <see cref="KeyValuesPair{TKey, TValue}"/> instance.</param>
    public KeyValuesPair(TKey key, TValue values) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(key, nameof(key));
      Contract.Requires(values, nameof(values));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      Key                       = key;
      Values                    = values;

    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the <see cref="Key"/> for a given item.
    /// </summary>
    public TKey Key { get; init; }

    /*==========================================================================================================================
    | PROPERTY: VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the <see cref="Values"/> for the given item.
    /// </summary>
    public TValue Values { get; init; }

  } //Class
} //Namespace
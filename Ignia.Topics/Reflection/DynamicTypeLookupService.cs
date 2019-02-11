/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;

namespace Ignia.Topics.Reflection {

  /*============================================================================================================================
  | CLASS: TYPE INDEX
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="DynamicTypeLookupService"/> will search all assemblies for <see cref="Type"/> instances that match a
  ///   predicate.
  /// </summary>
  public class DynamicTypeLookupService : StaticTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="DynamicTypeLookupService"/> based on a <paramref name="predicate"/> and,
    ///   optionally, a default <see cref="Type"/> object to return if none is specified.
    /// </summary>
    /// <param name="predicate">The search condition to use to identify target classes.</param>
    /// <param name="defaultType">The default type to return if no match can be found. Defaults to object.</param>
    public DynamicTypeLookupService(Func<Type, bool> predicate, Type defaultType = null) : base(null, defaultType) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Find target classes
      \---------------------------------------------------------------------------------------------------------------------*/
      var matchedTypes = AppDomain
        .CurrentDomain
        .GetAssemblies()
        .SelectMany(t => t.GetTypes())
        .Where(t => t.IsClass && predicate(t))
        .OrderBy(t => t.Namespace.StartsWith("Ignia.Topics", StringComparison.InvariantCultureIgnoreCase))
        .ToList();

      /*----------------------------------------------------------------------------------------------------------------------
      | Populate collection
      \---------------------------------------------------------------------------------------------------------------------*/
      foreach (var type in matchedTypes) {
        if (!Contains(type)) {
          Add(type);
        }
      }

    }

  } //Class
} //Namespace
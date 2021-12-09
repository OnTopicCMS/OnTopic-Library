/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Concurrent;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Internal.Reflection {

  /*============================================================================================================================
  | CLASS: TYPE ACCESSOR CACHE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides access to a cached set of <see cref="TypeAccessor"/> instances across the application.
  /// </summary>
  internal static class TypeAccessorCache {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private static readonly ConcurrentDictionary<Type, TypeAccessor> _cache = new();

    /*==========================================================================================================================
    | GET TYPE ACCESSOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <see cref="TypeAccessor"/> for a given <see cref="Type"/>.
    /// </summary>
    /// <remarks>
    ///   As each <see cref="Type"/> is fixed at runtime, and we expect types that are mapped once to be mapped multiple times,
    ///   a static cache is maintained of <see cref="TypeAccessor"/> instances. If the provided <see cref="Type"/> doesn't yet
    ///   exist in the cache, it will be created.
    /// </remarks>
    /// <param name="type">The <see cref="Type"/> that needs to be dynamically accessed.</param>
    /// <returns>A <see cref="TypeAccessor"/> for dynamically accessing the supplied <paramref name="type"/>.</returns>
    internal static TypeAccessor GetTypeAccessor(Type type) {
      Contract.Requires(type, nameof(type));
      return _cache.GetOrAdd(type, t => new(t));
    }

    /// <inheritdoc cref="GetTypeAccessor(Type)"/>
    /// <typeparam name="T">The <see cref="Type"/> that needs to be dynamically accessed.</typeparam>
    internal static TypeAccessor GetTypeAccessor<T>() => GetTypeAccessor(typeof(T));

  } //Class
} //Namespace
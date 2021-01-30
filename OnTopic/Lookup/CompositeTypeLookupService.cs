/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnTopic.Lookup {

  /*============================================================================================================================
  | CLASS: COMPOSITE TYPE LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="CompositeTypeLookupService"/> allows <see cref="Type"/>s from multiple <see cref="ITypeLookupService"/>s
  ///   to be merged into a single <see cref="ITypeLookupService"/>.
  /// </summary>
  /// <remarks>
  ///   Different libraries—such as the OnTopic View Models or OnTopic Editors—may have their own <see cref="ITypeLookupService"
  ///   /> for managing <see cref="Type"/>s that are specific to that library. The <see cref="CompositeTypeLookupService"/>
  ///   allows those to be combined into a single <see cref="ITypeLookupService"/> by passing each of them to the constructor.
  ///   If conflicts occur, then the last <see cref="Type"/> entered will take precidence, overriding any initial definitions.
  /// </remarks>
  public class CompositeTypeLookupService: ITypeLookupService {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            List<ITypeLookupService>        _typeLookupServices             = new();

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="CompositeTypeLookupService"/>. Accepts any number of <see cref=
    ///   "ITypeLookupService"/> instanced to be passed to the constructor.
    /// </summary>
    /// <param name="typeLookupServices">
    ///   The list of <see cref="ITypeLookupService"/> instances to expose as part of this service.
    /// </param>
    public CompositeTypeLookupService(params ITypeLookupService[] typeLookupServices) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Add types to internal collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      _typeLookupServices.AddRange(typeLookupServices.Reverse());

    }

    /*==========================================================================================================================
    | METHOD: LOOKUP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public Type? Lookup(params string[] typeNames) {
      var type = typeof(object);
      if (typeNames is not null) {
        foreach (var typeName in typeNames) {
          foreach (var typeLookupService in _typeLookupServices) {
            type = typeLookupService.Lookup(typeName);
            if (type is not null && type.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)) {
              return type;
            }
          }
        }
      }
      //Default to default return type of last query
      return type;
    }

  } //Class
} //Namespace
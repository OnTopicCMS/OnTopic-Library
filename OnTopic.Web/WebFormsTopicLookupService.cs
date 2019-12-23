/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using OnTopic.Metadata;
using OnTopic.Web.Editor;

namespace OnTopic {

  /*============================================================================================================================
  | CLASS: TOPIC LOOKUP SERVICE (WEB FORMS)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="WebFormsTopicLookupService"/> provides access to all of the standard <see cref="Topic"/> types available
  ///   in the <see cref="DefaultTopicLookupService"/>, as well as custom <see cref="Topic"/>s available for WebForms.
  /// </summary>
  public class WebFormsTopicLookupService: DefaultTopicLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="WebFormsTopicLookupService"/>. Optionally accepts a list of <see
    ///   cref="Type"/> instances and a default <see cref="Type"/> value.
    /// </summary>
    /// <remarks><inheritdoc /></remarks>
    /// <param name="types"><inheritdoc /></param>
    /// <param name="defaultType"><inheritdoc /></param>
    public WebFormsTopicLookupService(IEnumerable<Type> types = null, Type defaultType = null) :
      base(types, defaultType?? typeof(Topic)) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure editor types are accounted for
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!Contains(nameof(ContentTypeDescriptor)))             Add(typeof(ConfigurableAttributeDescriptor));

    }

  } //Class
} //Namespace
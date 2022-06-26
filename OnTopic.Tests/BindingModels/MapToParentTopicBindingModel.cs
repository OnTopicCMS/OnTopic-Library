/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: MAP TO PARENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a minimal implementation of a custom topic binding model with mapped complex properties.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class MapToParentTopicBindingModel : BasicTopicBindingModel {

    [MapToParent(AttributePrefix="")]
    public ContactTopicBindingModel PrimaryContact { get; set; } = new();

    [MapToParent(AttributePrefix="Alternate")]
    public EmailTopicBindingModel AlternateContact { get; set; } = new();

    [MapToParent]
    public EmailTopicBindingModel BillingContact { get; set; } = new();

  } //Class
} //Namespace
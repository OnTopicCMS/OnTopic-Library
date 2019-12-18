/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace Ignia.Topics.Tests.BindingModels {

  /*============================================================================================================================
  | BINDING MODEL: CONTACT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a minimal implementation of a custom topic binding model with a single property intended to be called from
  ///   <see cref="MapToParentTopicBindingModel"/>.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class ContactTopicBindingModel {

    public string? Email { get; set; }

  } //Class
} //Namespace
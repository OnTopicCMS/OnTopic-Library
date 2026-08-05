/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.ViewModels.BindingModels;

namespace OnTopic.Tests.BindingModels;

/*==============================================================================================================================
| BINDING MODEL: NESTED REFERENCE ATTRIBUTE TOPIC
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a minimal implementation of a custom topic binding model with both a scalar value and a reference property, for
///   use as an item within a <see cref="ContentTypeDescriptorTopicBindingModel.Attributes"/> collection.
/// </summary>
/// <remarks>
///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
/// </remarks>
public class NestedReferenceAttributeTopicBindingModel : AttributeDescriptorTopicBindingModel {

  public NestedReferenceAttributeTopicBindingModel(string key) : base(key, "TextAttributeDescriptor") { }

  public AssociatedTopicBindingModel? BaseTopic { get; set; }

} //Class
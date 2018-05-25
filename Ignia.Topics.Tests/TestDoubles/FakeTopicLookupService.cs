/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace Ignia.Topics.Tests.TestDoubles {

  /*============================================================================================================================
  | CLASS: FAKE TOPIC LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides access to derived types of <see cref="Topic"/> classes.
  /// </summary>
  /// <remarks>
  ///   Allows testing of services that depend on <see cref="ITypeLookupService"/> without using expensive reflection.
  /// </remarks>
  internal class FakeTopicLookupService: StaticTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the <see cref="FakeTopicLookupService"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="FakeTopicLookupService"/>.</returns>
    internal FakeTopicLookupService(): base(null, typeof(Topic)) {
      Add(typeof(Topic));
      Add(typeof(ContentTypeDescriptor));
      Add(typeof(AttributeDescriptor));
    }

  }
}

/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Globalization;
using Ignia.Topics.Collections;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: SERIALIZATION TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for JSON.NET's serialization—such as via the <see cref="AttributeValueCollectionJsonConverter"/>.
  /// </summary>
  [TestClass]
  public class SerializationTest {

    /*==========================================================================================================================
    | TEST: WRITE ATTRIBUTE VALUE COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests the serialization of a <see cref="AttributeValueCollection"/> to confirm that it properly utilizes the <see
    ///   cref="AttributeValueCollectionJsonConverter"/>.
    /// </summary>
    [TestMethod]
    public void WriteAttributeValueCollection() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var json                  = JsonConvert.SerializeObject(topic);
      var jObject               = JObject.Parse(json);

      Assert.AreEqual<string>("Page", jObject["Attributes"]["ContentType"].ToString());

    }

  } //Class

} //Namespace

/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Attributes;
using OnTopic.Collections.Specialized;
using OnTopic.Tests.Entities;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE COLLECTION TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="AttributeValueCollection"/> class.
  /// </summary>
  [TestClass]
  public class AttributeValueCollectionTest {

    /*==========================================================================================================================
    | TEST: GET VALUE: CORRECT VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new attribute via an <c>[AttributeSetter]</c> and ensures that the attribute can be returned.
    /// </summary>
    [TestMethod]
    public void GetValue_CorrectValue_IsReturned() {
      var topic = TopicFactory.Create("Test", "Container");
      topic.View = "Test";
      Assert.AreEqual<string>("Test", topic.Attributes.GetValue("View"));
    }

    /*==========================================================================================================================
    | TEST: GET VALUE: MISSING VALUE: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new topic and requests an invalid attribute; ensures falls back to the default.
    /// </summary>
    [TestMethod]
    public void GetValue_MissingValue_ReturnsDefault() {
      var topic = TopicFactory.Create("Test", "Container");
      Assert.AreEqual<string>("Foo", topic.Attributes.GetValue("InvalidAttribute", "Foo"));
    }

    /*==========================================================================================================================
    | TEST: GET INTEGER: CORRECT VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that integer values can be set and retrieved as expected.
    /// </summary>
    [TestMethod]
    public void GetInteger_CorrectValue_IsReturned() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetInteger("Number1", 1);

      Assert.AreEqual<int>(1, topic.Attributes.GetInteger("Number1", 5));

    }

    /*==========================================================================================================================
    | TEST: GET INTEGER: INCORRECT VALUE: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that invalid values return the default.
    /// </summary>
    [TestMethod]
    public void GetInteger_IncorrectValue_ReturnsDefault() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Number3", "Invalid");

      Assert.AreEqual<int>(5, topic.Attributes.GetInteger("Number3", 5));
      Assert.AreEqual<int>(0, topic.Attributes.GetInteger("Number3"));

    }

    /*==========================================================================================================================
    | TEST: GET INTEGER: INCORRECT KEY: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that incorrect key names return the default.
    /// </summary>
    [TestMethod]
    public void GetInteger_IncorrectKey_ReturnsDefault() {

      var topic = TopicFactory.Create("Test", "Container");

      Assert.AreEqual<int>(5, topic.Attributes.GetInteger("InvalidKey", 5));
      Assert.AreEqual<int>(0, topic.Attributes.GetInteger("InvalidKey"));

    }

    /*==========================================================================================================================
    | TEST: GET DOUBLE: CORRECT VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that double values can be set and retrieved as expected.
    /// </summary>
    [TestMethod]
    public void GetDouble_CorrectValue_IsReturned() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetDouble("Number1", 1);

      Assert.AreEqual<double>(1.0, topic.Attributes.GetDouble("Number1", 5.0));

    }

    /*==========================================================================================================================
    | TEST: GET DOUBLE: INCORRECT VALUE: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that double values return the default.
    /// </summary>
    [TestMethod]
    public void GetDouble_IncorrectValue_ReturnsDefault() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Number3", "Invalid");

      Assert.AreEqual<double>(5.0, topic.Attributes.GetDouble("Number3", 5.0));
      Assert.AreEqual<double>(0, topic.Attributes.GetDouble("Number3"));

    }

    /*==========================================================================================================================
    | TEST: GET DOUBLE: INCORRECT KEY: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that double key names return the default.
    /// </summary>
    [TestMethod]
    public void GetDouble_IncorrectKey_ReturnsDefault() {

      var topic = TopicFactory.Create("Test", "Container");

      Assert.AreEqual<double>(5.0, topic.Attributes.GetDouble("InvalidKey", 5.0));
      Assert.AreEqual<double>(0, topic.Attributes.GetDouble("InvalidKey"));

    }

    /*==========================================================================================================================
    | TEST: GET DATETIME: CORRECT VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that integer values can be set and retrieved as expected.
    /// </summary>
    [TestMethod]
    public void GetDateTime_CorrectValue_IsReturned() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var dateTime1             = new DateTime(1976, 10, 15);

      topic.Attributes.SetDateTime("DateTime1", dateTime1);

      Assert.AreEqual<DateTime>(dateTime1, topic.Attributes.GetDateTime("DateTime1", DateTime.MinValue));

    }

    /*==========================================================================================================================
    | TEST: GET DATETIME: INCORRECT VALUE: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that invalid values return the default.
    /// </summary>
    [TestMethod]
    public void GetDateTime_IncorrectValue_ReturnsDefault() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var dateTime1             = new DateTime(1976, 10, 15);
      var dateTime2             = new DateTime(1981, 06, 03);

      topic.Attributes.SetDateTime("DateTime2", dateTime2);

      Assert.AreEqual<DateTime>(dateTime1, topic.Attributes.GetDateTime("DateTime3", dateTime1));
      Assert.AreEqual<DateTime>(new DateTime(), topic.Attributes.GetDateTime("DateTime3"));

    }

    /*==========================================================================================================================
    | TEST: GET DATETIME: INCORRECT KEY: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that incorrect key names return the default.
    /// </summary>
    [TestMethod]
    public void GetDateTime_IncorrectKey_ReturnsDefault() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var dateTime1             = new DateTime(1976, 10, 15);
      var dateTime2             = new DateTime(1981, 06, 03);

      topic.Attributes.SetDateTime("DateTime2", dateTime2);

      Assert.AreEqual<DateTime>(dateTime1, topic.Attributes.GetDateTime("DateTime3", dateTime1));
      Assert.AreEqual<DateTime>(new DateTime(), topic.Attributes.GetDateTime("DateTime3"));

    }

    /*==========================================================================================================================
    | TEST: GET BOOLEAN: CORRECT VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that boolean values can be set and retrieved as expected.
    /// </summary>
    [TestMethod]
    public void GetBoolean_CorrectValue_IsReturned() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetBoolean("IsValue1", true);
      topic.Attributes.SetBoolean("IsValue2", false);

      Assert.IsTrue(topic.Attributes.GetBoolean("IsValue1", false));
      Assert.IsFalse(topic.Attributes.GetBoolean("IsValue2", true));

    }

    /*==========================================================================================================================
    | TEST: GET BOOLEAN: INCORRECT VALUE: RETURN DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that invalid values return the default.
    /// </summary>
    [TestMethod]
    public void GetBoolean_IncorrectValue_ReturnDefault() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("IsValue", "Invalid");

      Assert.IsTrue(topic.Attributes.GetBoolean("IsValue", true));
      Assert.IsFalse(topic.Attributes.GetBoolean("IsValue", false));
      Assert.IsFalse(topic.Attributes.GetBoolean("IsValue"));

    }

    /*==========================================================================================================================
    | TEST: GET BOOLEAN: INCORRECT KEY: RETURN DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that incorrect key names return the default.
    /// </summary>
    [TestMethod]
    public void GetBoolean_IncorrectKey_ReturnDefault() {

      var topic = TopicFactory.Create("Test", "Container");

      Assert.IsTrue(topic.Attributes.GetBoolean("InvalidKey", true));
      Assert.IsFalse(topic.Attributes.GetBoolean("InvalidKey", false));
      Assert.IsFalse(topic.Attributes.GetBoolean("InvalidKey"));

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: CORRECT VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a custom attribute on a topic and ensures it can be retrieved.
    /// </summary>
    [TestMethod]
    public void SetValue_CorrectValue_IsReturned() {
      var topic = TopicFactory.Create("Test", "Container");
      topic.Attributes.SetValue("Foo", "Bar");
      Assert.AreEqual<string>("Bar", topic.Attributes.GetValue("Foo"));
    }

    /*==========================================================================================================================
    | TEST: SET VALUE: VALUE CHANGED: IS DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Modifies the value of a custom attribute on a topic and ensures it is marked as IsDirty.
    /// </summary>
    [TestMethod]
    public void SetValue_ValueChanged_IsDirty() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Foo", "Bar", false);
      topic.Attributes.SetValue("Foo", "Baz");

      Assert.IsTrue(topic.Attributes["Foo"].IsDirty);

    }

    /*==========================================================================================================================
    | TEST: CLEAR: EXISTING VALUES: IS DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="IList.Clear()"/> and confirms that the collection is marked as dirty, due to deleted attrbutes.
    /// </summary>
    [TestMethod]
    public void Clear_ExistingValues_IsDirty() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Foo", "Bar", false);

      topic.Attributes.Clear();

      Assert.IsTrue(topic.Attributes.IsDirty());
      Assert.IsTrue(topic.Attributes.DeletedItems.Contains("Foo"));

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: VALUE UNCHANGED: IS NOT DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets the value of a custom <see cref="AttributeValue"/> to the existing value and ensures it is <i>not</i> marked as
    ///   <see cref="TrackedItem{T}.IsDirty"/>.
    /// </summary>
    [TestMethod]
    public void SetValue_ValueUnchanged_IsNotDirty() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Fah", "Bar", false);
      topic.Attributes.SetValue("Fah", "Bar");

      Assert.IsFalse(topic.Attributes["Fah"].IsDirty);

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: DIRTY VALUES: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeValueCollection"/> with a <see cref="AttributeValue"/> that is marked as <see
    ///   cref="TrackedItem{T}.IsDirty"/>. Confirms that <see cref="AttributeValueCollection.IsDirty(Boolean)"/> returns
    ///   <c>true</c>.
    /// </summary>
    [TestMethod]
    public void IsDirty_DirtyValues_ReturnsTrue() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Foo", "Bar");

      Assert.IsTrue(topic.Attributes.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: DELETED VALUES: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeValueCollection"/> with a <see cref="AttributeValue"/> and then deletes it. Confirms
    ///   that <see cref="AttributeValueCollection.IsDirty(Boolean)"/> returns <c>true</c>.
    /// </summary>
    [TestMethod]
    public void IsDirty_DeletedValues_ReturnsTrue() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Foo", "Bar");
      topic.Attributes.Remove("Foo");

      Assert.IsTrue(topic.Attributes.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: NO DIRTY VALUES: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeValueCollection"/> with a <see cref="AttributeValue"/> that is <i>not</i> marked as
    ///   <see cref="TrackedItem{T}.IsDirty"/>. Confirms that <see cref="AttributeValueCollection.IsDirty(Boolean)"/> returns
    ///   <c>false</c>/
    /// </summary>
    [TestMethod]
    public void IsDirty_NoDirtyValues_ReturnsFalse() {

      var topic = TopicFactory.Create("Test", "Container", 1);

      topic.Attributes.SetValue("Foo", "Bar", false);

      Assert.IsFalse(topic.Attributes.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: EXCLUDE LAST MODIFIED: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeValueCollection"/> with a <see cref="AttributeValue"/> that is <i>not</i> marked as
    ///   <see cref="TrackedItem{T}.IsDirty"/> as well as a <c>LastModified</c> <see cref="AttributeValue"/> that is. Confirms
    ///   that <see cref="AttributeValueCollection.IsDirty(Boolean)"/> returns <c>false</c>.
    /// </summary>
    [TestMethod]
    public void IsDirty_ExcludeLastModified_ReturnsFalse() {

      var topic = TopicFactory.Create("Test", "Container", 1);

      topic.Attributes.SetValue("Foo", "Bar", false);
      topic.Attributes.SetValue("LastModified", DateTime.Now.ToString(CultureInfo.InvariantCulture));
      topic.Attributes.SetValue("LastModifiedBy", "System");

      Assert.IsFalse(topic.Attributes.IsDirty(true));

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MARK CLEAN: UPDATES LAST MODIFIED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeValueCollection"/> with a <see cref="AttributeValue"/> and then deletes it. Confirms
    ///   that the <see cref="TrackedItem{T}.LastModified"/> returns the new <c>version</c> after calling <see cref="
    ///   TrackedCollection{TItem, TValue, TAttribute}.MarkClean(DateTime?)"/>.
    /// </summary>
    [TestMethod]
    public void IsDirty_MarkClean_UpdatesLastModified() {

      var topic = TopicFactory.Create("Test", "Container");
      var version = DateTime.Now.AddDays(5);

      topic.Attributes.SetValue("Baz", "Foo");
      topic.Attributes.MarkClean(version);
      topic.Attributes.TryGetValue("Baz", out var cleanedAttribute);

      Assert.AreEqual<DateTime>(version, cleanedAttribute.LastModified);

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MARK CLEAN: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeValueCollection"/> with a <see cref="AttributeValue"/> and then deletes it. Confirms
    ///   that <see cref="AttributeValueCollection.IsDirty(Boolean)"/> returns <c>false</c> after calling <see cref="
    ///   TrackedCollection{TItem, TValue, TAttribute}.MarkClean(DateTime?)"/>.
    /// </summary>
    [TestMethod]
    public void IsDirty_MarkClean_ReturnsFalse() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Foo", "Bar");
      topic.Attributes.SetValue("Baz", "Foo");

      topic.Attributes.Remove("Foo");

      topic.Attributes.MarkClean();

      Assert.IsFalse(topic.Attributes.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MARK ATTRIBUTE CLEAN: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeValueCollection"/> with a <see cref="AttributeValue"/> and then confirms that <see
    ///   cref="TrackedCollection{TItem, TValue, TAttribute}.IsDirty(String)"/> returns <c>false</c> for that attribute after
    ///   calling <see cref="TrackedCollection{TItem, TValue, TAttribute}.MarkClean(String, DateTime?)"/>.
    /// </summary>
    [TestMethod]
    public void IsDirty_MarkAttributeClean_ReturnsFalse() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Foo", "Bar");
      topic.Attributes.MarkClean("Foo");

      Assert.IsFalse(topic.Attributes.IsDirty("Foo"));

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: INVALID VALUE: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to violate the business logic by bypassing the property setter; ensures that business logic is enforced.
    /// </summary>
    [TestMethod]
    [ExpectedException(
      typeof(InvalidKeyException),
      "The topic allowed a view to be set via a back door, without routing it through the View property."
    )]
    public void SetValue_InvalidValue_ThrowsException() {
      var topic = TopicFactory.Create("Test", "Container");
      topic.Attributes.SetValue("View", "# ?");
    }

    /*==========================================================================================================================
    | TEST: ADD: VALID ATTRIBUTE VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a custom attribute on a topic by directly adding an <see cref="AttributeValue"/> instance; ensures it can be
    ///   retrieved.
    /// </summary>
    [TestMethod]
    public void Add_ValidAttributeValue_IsReturned() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.Add(new("View", "NewKey", false));

      Assert.AreEqual<string>("NewKey", topic.View);

    }

    /*==========================================================================================================================
    | TEST: ADD: NUMERIC VALUE WITH BUSINESS LOGIC: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a numeric attribute on a topic instance; ensures it is routed through the corresponding property and correctly
    ///   retrieved.
    /// </summary>
    [TestMethod]
    public void Add_NumericValueWithBusinessLogic_IsReturned() {

      var topic = new CustomTopic("Test", "Page");

      topic.Attributes.SetInteger("NumericAttribute", 1);

      Assert.AreEqual<int>(1, topic.NumericAttribute);

    }

    /*==========================================================================================================================
    | TEST: ADD: BOOLEAN VALUE WITH BUSINESS LOGIC: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a Boolean attribute on a topic instance; ensures it is routed through the corresponding property and correctly
    ///   retrieved.
    /// </summary>
    [TestMethod]
    public void Add_BooleanValueWithBusinessLogic_IsReturned() {

      var topic = new CustomTopic("Test", "Page");

      topic.Attributes.SetBoolean("BooleanAttribute", true);

      Assert.IsTrue(topic.BooleanAttribute);

    }

    /*==========================================================================================================================
    | TEST: ADD: NUMERIC VALUE WITH BUSINESS LOGIC: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a numeric attribute on a topic instance with an invalid value; ensures an exception is thrown.
    /// </summary>
    [TestMethod]
    [ExpectedException(
      typeof(ArgumentOutOfRangeException),
      "The topic allowed a key to be set via a back door, without routing it through the NumericValue property."
    )]
    public void Add_NumericValueWithBusinessLogic_ThrowsException() {

      var topic = new CustomTopic("Test", "Page");

      topic.Attributes.SetInteger("NumericAttribute", -1);

    }

    /*==========================================================================================================================
    | TEST: ADD: DATE/TIME VALUE WITH BUSINESS LOGIC: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a Date/Time attribute on a topic instance; ensures it is routed through the corresponding property and correctly
    ///   retrieved.
    /// </summary>
    [TestMethod]
    public void Add_DateTimeValueWithBusinessLogic_IsReturned() {

      var topic = new CustomTopic("Test", "Page");
      var dateTime = new DateTime(2021, 1, 5);

      topic.Attributes.SetDateTime("DateTimeAttribute", dateTime);

      Assert.AreEqual<DateTime>(dateTime, topic.DateTimeAttribute);

    }

    /*==========================================================================================================================
    | TEST: ADD: DATE/TIME VALUE WITH BUSINESS LOGIC: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a Date/Time attribute on a topic instance with an invalid value; ensures an exception is thrown.
    /// </summary>
    [TestMethod]
    [ExpectedException(
      typeof(ArgumentOutOfRangeException),
      "The topic allowed a key to be set via a back door, without routing it through the NumericValue property."
    )]
    public void Add_DateTimeValueWithBusinessLogic_ThrowsException() {

      var topic = new CustomTopic("Test", "Page");

      topic.Attributes.SetDateTime("DateTimeAttribute", DateTime.MinValue);

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: INSERT INVALID ATTRIBUTE VALUE: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to violate the business logic by bypassing SetValue() entirely; ensures that business logic is enforced.
    /// </summary>
    [TestMethod]
    [ExpectedException(
      typeof(InvalidKeyException),
      "The topic allowed a key to be set via a back door, without routing it through the View property."
    )]
    public void Add_InvalidAttributeValue_ThrowsException() {
      var topic = TopicFactory.Create("Test", "Container");
      topic.Attributes.Add(new("View", "# ?"));
    }

    /*==========================================================================================================================
    | TEST: REPLACE VALUE: WITH BUSINESS LOGIC: MAINTAINS ISDIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a new <see cref="AttributeValue"/> which maps to <see cref="Topic.Key"/> directly to a <see cref=
    ///   "AttributeValueCollection"/> and confirms that the original <see cref="TrackedItem{T}.IsDirty"/> is replaced if the
    ///   <see cref="TrackedItem{T}.Value"/> changes.
    /// </summary>
    [TestMethod]
    public void Add_WithBusinessLogic_MaintainsIsDirty() {

      var topic = TopicFactory.Create("Test", "Container", 1);

      topic.View = "Test";
      topic.Attributes.TryGetValue("View", out var originalValue);

      var index = topic.Attributes.IndexOf(originalValue);

      topic.Attributes[index] = new AttributeValue("View", "NewValue", false);
      topic.Attributes.TryGetValue("View", out var newAttribute);

      topic.Attributes.SetValue("View", "NewerValue", false);
      topic.Attributes.TryGetValue("View", out var newerAttribute);

      Assert.IsFalse(newAttribute.IsDirty);
      Assert.IsFalse(newerAttribute.IsDirty);

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: EMPTY ATTRIBUTE VALUE: SKIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a new attribute with an empty value, and confirms that it is <i>not</i> added as a new <see
    ///   cref="AttributeValue"/>. Empty values are treated as the same as non-existent attributes. They are stored for the sake
    ///   of tracking <i>deleted</i> attributes, but should not be stored for <i>new</i> attributes.
    /// </summary>
    [TestMethod]
    public void SetValue_EmptyAttributeValue_Skips() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Attribute", "");

      Assert.IsFalse(topic.Attributes.Contains("Attribute"));

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: UPDATE EMPTY ATTRIBUTE VALUE: REPLACES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a new attribute with an empty value, and confirms that it is <i>is</i> added as a new <see
    ///   cref="AttributeValue"/> assuming the value previously existed. Empty values are treated as the same as non-existent
    ///   attributes, but they should be stored for the sake of tracking <i>deleted</i> attributes.
    /// </summary>
    [TestMethod]
    public void SetValue_EmptyAttributeValue_Replaces() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Attribute", "New Value");
      topic.Attributes.SetValue("Attribute", "");

      Assert.IsTrue(topic.Attributes.Contains("Attribute"));

    }

    /*==========================================================================================================================
    | TEST: GET VALUE: INHERIT FROM PARENT: RETURNS PARENT VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an attribute on the parent of a topic and ensures it can be retrieved using inheritance.
    /// </summary>
    [TestMethod]
    public void GetValue_InheritFromParent_ReturnsParentValue() {

      var topics = new Topic[8];

      for (var i = 0; i <= 7; i++) {
        var topic = TopicFactory.Create("Topic" + i, "Container");
        if (i > 0) topic.Parent = topics[i - 1];
        topics[i] = topic;
      }

      topics[0].Attributes.SetValue("Foo", "Bar");

      Assert.IsNull(topics[4].Attributes.GetValue("Foo", null));
      Assert.AreEqual<string>("Bar", topics[7].Attributes.GetValue("Foo", true));
      Assert.AreNotEqual<string>("Bar", topics[7].Attributes.GetValue("Foo", false));

    }

    /*==========================================================================================================================
    | TEST: GET VALUE: INHERIT FROM BASE: RETURNS INHERITED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a long tree of derived topics, and ensures that the inherited value is returned.
    /// </summary>
    [TestMethod]
    public void GetValue_InheritFromBase_ReturnsInheritedValue() {

      var topics = new Topic[5];

      for (var i = 0; i <= 4; i++) {
        var topic = TopicFactory.Create("Topic" + i, "Container");
        if (i > 0) topics[i - 1].BaseTopic = topic;
        topics[i] = topic;
      }

      topics[4].Attributes.SetValue("Foo", "Bar");

      Assert.AreEqual<string>("Bar", topics[0].Attributes.GetValue("Foo", null, true, true));

    }

    /*==========================================================================================================================
    | TEST: GET VALUE: EXCEEDS MAX HOPS: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a long tree of derives topics, and ensures that inheritance will pursue no more than five hops.
    /// </summary>
    [TestMethod]
    public void GetValue_ExceedsMaxHops_ReturnsDefault() {

      var topics = new Topic[8];

      for (var i = 0; i <= 7; i++) {
        var topic = TopicFactory.Create("Topic" + i, "Container");
        if (i > 0) topics[i - 1].BaseTopic = topic;
        topics[i] = topic;
      }

      topics[7].Attributes.SetValue("Foo", "Bar");

      Assert.IsNull(topics[0].Attributes.GetValue("Foo", null, true, true));

    }

  } //Class
} //Namespace
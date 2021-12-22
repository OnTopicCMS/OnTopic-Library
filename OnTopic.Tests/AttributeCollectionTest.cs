/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections;
using System.Globalization;
using OnTopic.Attributes;
using OnTopic.Collections.Specialized;
using OnTopic.Tests.Entities;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE COLLECTION TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="AttributeCollection"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class AttributeCollectionTest {

    /*==========================================================================================================================
    | TEST: GET VALUE: CORRECT VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new attribute via an <c>[AttributeSetter]</c> and ensures that the attribute can be returned.
    /// </summary>
    [Fact]
    public void GetValue_CorrectValue_IsReturned() {
      var topic                 = new Topic("Test", "Container") {
        View                    = "Test"
      };
      Assert.Equal("Test", topic.Attributes.GetValue("View"));
    }

    /*==========================================================================================================================
    | TEST: GET VALUE: INHERITED VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that values can be set and retrieved as expected via inheritance, both via <see cref="Topic.Parent"/> and <see
    ///   cref="Topic.BaseTopic"/>.
    /// </summary>
    [Fact]
    public void GetValue_InheritedValue_IsReturned() {

      var baseTopic             = new Topic("Base", "Container");
      var topic                 = new Topic("Test", "Container");
      var childTopic            = new Topic("Child", "Container", topic);

      topic.BaseTopic           = baseTopic;

      baseTopic.View            = "Test";

      Assert.Equal("Test", topic.Attributes.GetValue("View"));
      Assert.Equal("Test", childTopic.Attributes.GetValue("View", "Invalid", true));
      Assert.Null(topic.Attributes.GetValue("View", null, inheritFromBase: false));

    }

    /*==========================================================================================================================
    | TEST: GET VALUE: MISSING VALUE: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new topic and requests an invalid attribute; ensures falls back to the default.
    /// </summary>
    [Fact]
    public void GetValue_MissingValue_ReturnsDefault() {

      var topic = new Topic("Test", "Container");

      Assert.Null(topic.Attributes.GetValue("InvalidAttribute"));
      Assert.Equal("Foo", topic.Attributes.GetValue("InvalidAttribute", "Foo"));

    }

    /*==========================================================================================================================
    | TEST: GET VALUE: EMPTY VALUE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   If the <see cref="TrackedRecord{T}.Value"/> is <see cref="String.Empty"/>, then <see cref="TrackedRecordCollection{
    ///   TItem, TValue, TAttribute}.GetValue(String, Boolean)"/> should return <c>null</c>.
    /// </summary>
    [Fact]
    public void GetValue_EmptyValue_ReturnsNull() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.Add(new("EmptyValue", ""));

      Assert.Null(topic.Attributes.GetValue("EmptyValue"));

    }

    /*==========================================================================================================================
    | TEST: GET INTEGER: CORRECT VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that integer values can be set and retrieved as expected.
    /// </summary>
    [Fact]
    public void GetInteger_CorrectValue_IsReturned() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetInteger("Number1", 1);

      Assert.Equal(1, topic.Attributes.GetInteger("Number1", 5));

    }

    /*==========================================================================================================================
    | TEST: GET DOUBLE: INHERITED VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that integer values can be set and retrieved as expected via inheritance, both via <see cref="Topic.Parent"/>
    ///   and <see cref="Topic.BaseTopic"/>.
    /// </summary>
    [Fact]
    public void GetInteger_InheritedValue_IsReturned() {

      var baseTopic             = new Topic("Base", "Container");
      var topic                 = new Topic("Test", "Container");
      var childTopic            = new Topic("Child", "Container", topic);

      topic.BaseTopic           = baseTopic;

      baseTopic.Attributes.SetInteger("Number1", 1);

      Assert.Equal(1, topic.Attributes.GetInteger("Number1", 5));
      Assert.Equal(1, childTopic.Attributes.GetInteger("Number1", 5, true));
      Assert.Equal(0, topic.Attributes.GetInteger("Number1", inheritFromBase: false));

    }

    /*==========================================================================================================================
    | TEST: GET INTEGER: INCORRECT VALUE: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that invalid values return the default.
    /// </summary>
    [Fact]
    public void GetInteger_IncorrectValue_ReturnsDefault() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetValue("Number3", "Invalid");

      Assert.Equal(0, topic.Attributes.GetInteger("Number3"));
      Assert.Equal(5, topic.Attributes.GetInteger("Number3", 5));

    }

    /*==========================================================================================================================
    | TEST: GET INTEGER: INCORRECT KEY: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that incorrect key names return the default.
    /// </summary>
    [Fact]
    public void GetInteger_IncorrectKey_ReturnsDefault() {

      var topic = new Topic("Test", "Container");

      Assert.Equal(0, topic.Attributes.GetInteger("InvalidKey"));
      Assert.Equal(5, topic.Attributes.GetInteger("InvalidKey", 5));

    }

    /*==========================================================================================================================
    | TEST: GET DOUBLE: CORRECT VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that double values can be set and retrieved as expected.
    /// </summary>
    [Fact]
    public void GetDouble_CorrectValue_IsReturned() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetDouble("Number1", 1);

      Assert.Equal(1.0, topic.Attributes.GetDouble("Number1", 5.0));

    }

    /*==========================================================================================================================
    | TEST: GET DOUBLE: INHERITED VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that double values can be set and retrieved as expected via inheritance, both via <see cref="Topic.Parent"/>
    ///   and <see cref="Topic.BaseTopic"/>.
    /// </summary>
    [Fact]
    public void GetDouble_InheritedValue_IsReturned() {

      var baseTopic             = new Topic("Base", "Container");
      var topic                 = new Topic("Test", "Container");
      var childTopic            = new Topic("Child", "Container", topic);

      topic.BaseTopic           = baseTopic;

      baseTopic.Attributes.SetDouble("Number1", 1);

      Assert.Equal(1.0, topic.Attributes.GetDouble("Number1", 5.0));
      Assert.Equal(1.0, childTopic.Attributes.GetDouble("Number1", 5.0, true));
      Assert.Equal(0.0, topic.Attributes.GetInteger("Number1", inheritFromBase: false));

    }

    /*==========================================================================================================================
    | TEST: GET DOUBLE: INCORRECT VALUE: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that double values return the default.
    /// </summary>
    [Fact]
    public void GetDouble_IncorrectValue_ReturnsDefault() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetValue("Number3", "Invalid");

      Assert.Equal(0.0, topic.Attributes.GetDouble("Number3"));
      Assert.Equal(5.0, topic.Attributes.GetDouble("Number3", 5.0));

    }

    /*==========================================================================================================================
    | TEST: GET DOUBLE: INCORRECT KEY: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that double key names return the default.
    /// </summary>
    [Fact]
    public void GetDouble_IncorrectKey_ReturnsDefault() {

      var topic = new Topic("Test", "Container");

      Assert.Equal(0.0, topic.Attributes.GetDouble("InvalidKey"));
      Assert.Equal(5.0, topic.Attributes.GetDouble("InvalidKey", 5.0));

    }

    /*==========================================================================================================================
    | TEST: GET DATETIME: CORRECT VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that Date/Time values can be set and retrieved as expected.
    /// </summary>
    [Fact]
    public void GetDateTime_CorrectValue_IsReturned() {

      var topic                 = new Topic("Test", "Container");
      var dateTime1             = new DateTime(1976, 10, 15);

      topic.Attributes.SetDateTime("DateTime1", dateTime1);

      Assert.Equal(dateTime1, topic.Attributes.GetDateTime("DateTime1", DateTime.MinValue));

    }

    /*==========================================================================================================================
    | TEST: GET DATETIME: INHERITED VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that Date/Time values can be set and retrieved as expected via inheritance, both via <see cref="Topic.Parent"/>
    ///   and <see cref="Topic.BaseTopic"/>.
    /// </summary>
    [Fact]
    public void GetDateTime_InheritedValue_IsReturned() {

      var baseTopic             = new Topic("Base", "Container");
      var topic                 = new Topic("Test", "Container");
      var childTopic            = new Topic("Child", "Container", topic);
      var dateTime1             = new DateTime(1976, 10, 15);

      topic.BaseTopic           = baseTopic;

      baseTopic.Attributes.SetDateTime("DateTime1", dateTime1);

      Assert.Equal(dateTime1, topic.Attributes.GetDateTime("DateTime1", DateTime.Now));
      Assert.Equal(dateTime1, childTopic.Attributes.GetDateTime("DateTime1", DateTime.Now, true));
      Assert.Equal(new DateTime(), topic.Attributes.GetDateTime("DateTime1", inheritFromBase: false));

    }

    /*==========================================================================================================================
    | TEST: GET DATETIME: INCORRECT VALUE: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that invalid values return the default.
    /// </summary>
    [Fact]
    public void GetDateTime_IncorrectValue_ReturnsDefault() {

      var topic                 = new Topic("Test", "Container");
      var dateTime1             = new DateTime(1976, 10, 15);

      topic.Attributes.SetValue("DateTime2", "IncorrectValue");

      Assert.Equal(new DateTime(), topic.Attributes.GetDateTime("DateTime2"));
      Assert.Equal(dateTime1, topic.Attributes.GetDateTime("DateTime2", dateTime1));

    }

    /*==========================================================================================================================
    | TEST: GET DATETIME: INCORRECT KEY: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that incorrect key names return the default.
    /// </summary>
    [Fact]
    public void GetDateTime_IncorrectKey_ReturnsDefault() {

      var topic                 = new Topic("Test", "Container");
      var dateTime1             = new DateTime(1976, 10, 15);
      var dateTime2             = new DateTime(1981, 06, 03);

      topic.Attributes.SetDateTime("DateTime2", dateTime2);

      Assert.Equal(new DateTime(), topic.Attributes.GetDateTime("DateTime3"));
      Assert.Equal(dateTime1, topic.Attributes.GetDateTime("DateTime3", dateTime1));

    }

    /*==========================================================================================================================
    | TEST: GET BOOLEAN: CORRECT VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that boolean values can be set and retrieved as expected.
    /// </summary>
    [Fact]
    public void GetBoolean_CorrectValue_IsReturned() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetBoolean("IsValue1", true);
      topic.Attributes.SetBoolean("IsValue2", false);

      Assert.True(topic.Attributes.GetBoolean("IsValue1"));
      Assert.True(topic.Attributes.GetBoolean("IsValue1", false));
      Assert.False(topic.Attributes.GetBoolean("IsValue2", true));

    }

    /*==========================================================================================================================
    | TEST: GET BOOLEAN: INHERITED VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that boolean values can be set and retrieved as expected via inheritance, both via <see cref="Topic.Parent"/>
    ///   and <see cref="Topic.BaseTopic"/>.
    /// </summary>
    [Fact]
    public void GetBoolean_InheritedValue_IsReturned() {

      var baseTopic             = new Topic("Base", "Container");
      var topic                 = new Topic("Test", "Container");
      var childTopic            = new Topic("Child", "Container", topic);

      topic.BaseTopic           = baseTopic;

      baseTopic.Attributes.SetBoolean("IsValue1", true);

      Assert.True(topic.Attributes.GetBoolean("IsValue1"));
      Assert.True(childTopic.Attributes.GetBoolean("IsValue1", false, true));
      Assert.False(topic.Attributes.GetBoolean("IsValue1", inheritFromBase: false));

    }

    /*==========================================================================================================================
    | TEST: GET BOOLEAN: INCORRECT VALUE: RETURN DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that invalid values return the default.
    /// </summary>
    [Fact]
    public void GetBoolean_IncorrectValue_ReturnDefault() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetValue("IsValue", "Invalid");

      Assert.False(topic.Attributes.GetBoolean("IsValue"));
      Assert.True(topic.Attributes.GetBoolean("IsValue", true));
      Assert.False(topic.Attributes.GetBoolean("IsValue", false));

    }

    /*==========================================================================================================================
    | TEST: GET BOOLEAN: INCORRECT KEY: RETURN DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that incorrect key names return the default.
    /// </summary>
    [Fact]
    public void GetBoolean_IncorrectKey_ReturnDefault() {

      var topic = new Topic("Test", "Container");

      Assert.False(topic.Attributes.GetBoolean("InvalidKey"));
      Assert.True(topic.Attributes.GetBoolean("InvalidKey", true));
      Assert.False(topic.Attributes.GetBoolean("InvalidKey", false));

    }

    /*==========================================================================================================================
    | TEST: GET URI: INHERITED VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that URI values can be set and retrieved as expected via inheritance, both via <see cref="Topic.Parent"/>
    ///   and <see cref="Topic.BaseTopic"/>.
    /// </summary>
    [Fact]
    public void GetUri_InheritedValue_IsReturned() {

      var baseTopic             = new Topic("Base", "Container");
      var topic                 = new Topic("Test", "Container");
      var childTopic            = new Topic("Child", "Container", topic);
      var url                   = "https://www.github.com/OnTopicCMS/";
      var uri                   = new Uri(url);

      topic.BaseTopic           = baseTopic;

      baseTopic.Attributes.SetUri("Url", uri);

      Assert.Equal(uri, topic.Attributes.GetUri("Url"));
      Assert.Equal(uri, childTopic.Attributes.GetUri("Url", inheritFromParent: true));
      Assert.Null(topic.Attributes.GetUri("Url", inheritFromBase: false));

    }

    /*==========================================================================================================================
    | TEST: GET URI: INCORRECT VALUE: RETURN DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that invalid values return the default.
    /// </summary>
    [Fact]
    public void GetUri_IncorrectValue_ReturnDefault() {

      var topic                 = new Topic("Test", "Container");
      var url                   = "https://www.github.com/OnTopicCMS/";
      var uri                   = new Uri(url);

      Assert.Null(topic.Attributes.GetUri("InvalidUrl"));
      Assert.Equal(uri, topic.Attributes.GetUri("InvalidUrl", uri));

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: CORRECT VALUE: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a custom attribute on a topic and ensures it can be retrieved.
    /// </summary>
    [Fact]
    public void SetValue_CorrectValue_IsReturned() {
      var topic = new Topic("Test", "Container");
      topic.Attributes.SetValue("Foo", "Bar");
      Assert.Equal("Bar", topic.Attributes.GetValue("Foo"));
    }

    /*==========================================================================================================================
    | TEST: SET VALUE: VALUE CHANGED: IS DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Modifies the value of a custom attribute on a topic and ensures it is marked as IsDirty.
    /// </summary>
    [Fact]
    public void SetValue_ValueChanged_IsDirty() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetValue("Foo", "Bar", false);
      topic.Attributes.SetValue("Foo", "Baz");

      Assert.True(topic.Attributes["Foo"].IsDirty);

    }

    /*==========================================================================================================================
    | TEST: CLEAR: NON-NULLABLE VALUE WITH BUSINESS LOGIC: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an attribute on a topic instance with an valid value, and then attempts to clear all attributes. Ensures that the
    ///   business logic is enforced by throwing an exception on the non-nullable property.
    /// </summary>
    [Fact]
    public void Clear_NonNullableValueWithBusinessLogic_ThrowsException() {

      var topic                 = new CustomTopic("Test", "Page") {
        NonNullableAttribute    = "Test"
      };

      Assert.Throws<ArgumentNullException>(() =>
        topic.Attributes.Clear()
      );

    }

    /*==========================================================================================================================
    | TEST: CLEAR: EXISTING VALUES: IS DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="IList.Clear()"/> and confirms that the collection is marked as dirty, due to deleted attrbutes.
    /// </summary>
    [Fact]
    public void Clear_ExistingValues_IsDirty() {

      var topic = new Topic("Test", "Container", null, 1);

      topic.Attributes.SetValue("Foo", "Bar", false);

      topic.Attributes.Clear();

      Assert.True(topic.Attributes.IsDirty());
      Assert.Contains("Foo", topic.Attributes.DeletedItems);

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: VALUE UNCHANGED: IS NOT DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets the value of a custom <see cref="AttributeRecord"/> to the existing value and ensures it is <i>not</i> marked as
    ///   <see cref="TrackedRecord{T}.IsDirty"/>.
    /// </summary>
    [Fact]
    public void SetValue_ValueUnchanged_IsNotDirty() {

      var topic = new Topic("Test", "Container", null, 1);

      topic.Attributes.SetValue("Fah", "Bar", false);
      topic.Attributes.SetValue("Fah", "Bar");

      Assert.False(topic.Attributes["Fah"].IsDirty);

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: DIRTY VALUES: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeCollection"/> with a <see cref="AttributeRecord"/> that is marked as <see cref="
    ///   TrackedRecord{T}.IsDirty"/>. Confirms that <see cref="AttributeCollection.IsDirty(Boolean)"/> returns <c>true</c>.
    /// </summary>
    [Fact]
    public void IsDirty_DirtyValues_ReturnsTrue() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetValue("Foo", "Bar");

      Assert.True(topic.Attributes.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: IS NEW: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeCollection"/> with a <see cref="AttributeRecord"/> that is <i>not</i> marked as <see
    ///   cref="TrackedRecord{T}.IsDirty"/>. Confirms that <see cref="AttributeCollection.IsDirty(Boolean)"/> returns
    ///   <c>true</c> if <see cref="Topic.IsNew"/>, even if <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.
    ///   MarkClean(String)"/> was called.
    /// </summary>
    [Fact]
    public void IsDirty_IsNew_ReturnsTrue() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetValue("Foo", "Bar", false);

      topic.Attributes.MarkClean("Foo");
      topic.Attributes.MarkClean();

      Assert.True(topic.Attributes.IsDirty());
      Assert.True(topic.Attributes.IsDirty(true));

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: DELETED VALUES: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeCollection"/> with a <see cref="AttributeRecord"/> and then deletes it. Confirms
    ///   that <see cref="AttributeCollection.IsDirty(Boolean)"/> returns <c>true</c>.
    /// </summary>
    [Fact]
    public void IsDirty_DeletedValues_ReturnsTrue() {

      var topic = new Topic("Test", "Container", null, 1);

      topic.Attributes.SetValue("Foo", "Bar");
      topic.Attributes.Remove("Foo");

      Assert.True(topic.Attributes.IsDirty());
      Assert.True(topic.Attributes.IsDirty(true));

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: UNDELETED VALUES: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeCollection"/> with a <see cref="AttributeRecord"/> and then deletes it. Confirms
    ///   that <see cref="AttributeCollection.IsDirty(Boolean)"/> returns <c>false</c> after recreating the value.
    /// </summary>
    [Fact]
    public void IsDirty_UndeletedValues_ReturnsFalse() {

      var topic = new Topic("Test", "Container", null, 1);

      topic.Attributes.SetValue("Foo", "Bar");
      topic.Attributes.Remove("Foo");
      topic.Attributes.SetValue("Foo", "Bar", false);

      Assert.False(topic.Attributes.IsDirty());
      Assert.False(topic.Attributes.IsDirty(true));
      Assert.DoesNotContain("Foo", topic.Attributes.DeletedItems);

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: NO DIRTY VALUES: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeCollection"/> with a <see cref="AttributeRecord"/> that is <i>not</i> marked as <see
    ///   cref="TrackedRecord{T}.IsDirty"/>. Confirms that <see cref="AttributeCollection.IsDirty(Boolean)"/> returns
    ///   <c>false</c>.
    /// </summary>
    [Fact]
    public void IsDirty_NoDirtyValues_ReturnsFalse() {

      var topic = new Topic("Test", "Container", null, 1);

      topic.Attributes.SetValue("Foo", "Bar", false);

      Assert.False(topic.Attributes.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: IS NEW: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeCollection"/> with a <see cref="AttributeRecord"/> that is <i>not</i> marked as <see
    ///   cref="TrackedRecord{T}.IsDirty"/>. Confirms that <see cref="AttributeCollection.IsDirty(Boolean)"/> still returns
    ///   <c>true</c> if the associated <see cref="Topic"/> is <see cref="Topic.IsNew"/>. New topics cannot be clean.
    /// </summary>
    [Fact]
    public void IsDirty_IsNew_ReturnsFalse() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetValue("Foo", "Bar", false);

      Assert.True(topic.Attributes.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MISSING KEY: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Confirms that calling <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty(String)"/> with an invalid
    ///   <c>key</c> returns <c>false</c>.
    /// </summary>
    [Fact]
    public void IsDirty_MissingKey_ReturnsFalse() {

      var topic = new Topic("Test", "Container");

      Assert.False(topic.Attributes.IsDirty("MissingKey"));

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: EXCLUDE LAST MODIFIED: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeCollection"/> with a <see cref="AttributeRecord"/> that is <i>not</i> marked as <see
    ///   cref="TrackedRecord{T}.IsDirty"/>. Updates the <c>LastModified</c> attributes, thus marking the collection as <see
    ///   cref="TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty()"/>. Confirms that <see cref="AttributeCollection.
    ///   IsDirty(Boolean)"/> returns <c>false</c>.
    /// </summary>
    [Fact]
    public void IsDirty_ExcludeLastModified_ReturnsFalse() {

      var topic = new Topic("Test", "Container", null, 1);

      topic.Attributes.SetValue("Foo", "Bar", false);
      topic.Attributes.SetValue("LastModified", DateTime.Now.ToString(CultureInfo.InvariantCulture));
      topic.Attributes.SetValue("LastModifiedBy", "System");

      Assert.True(topic.Attributes.IsDirty());
      Assert.False(topic.Attributes.IsDirty(true));

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MARK CLEAN: UPDATES LAST MODIFIED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeCollection"/> with a <see cref="AttributeRecord"/> and then deletes it. Confirms
    ///   that the <see cref="TrackedRecord{T}.LastModified"/> returns the new <c>version</c> after calling <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.MarkClean(DateTime?)"/>.
    /// </summary>
    [Fact]
    public void IsDirty_MarkClean_UpdatesLastModified() {

      var topic                 = new Topic("Test", "Container", null, 1);
      var firstVersion          = DateTime.Now.AddDays(5);
      var secondVersion         = DateTime.Now.AddDays(10);
      var thirdVersion          = DateTime.Now.AddDays(15);

      topic.Attributes.SetValue("Foo", "Qux", false, firstVersion);
      topic.Attributes.SetValue("Bar", "Quux");
      topic.Attributes.SetValue("Baz", "Quuz");

      topic.Attributes.MarkClean("Bar", secondVersion);
      topic.Attributes.MarkClean(thirdVersion);

      topic.Attributes.TryGetValue("Foo", out var cleanedAttribute1);
      topic.Attributes.TryGetValue("Bar", out var cleanedAttribute2);
      topic.Attributes.TryGetValue("Baz", out var cleanedAttribute3);

      Assert.Equal(firstVersion, cleanedAttribute1?.LastModified);
      Assert.Equal(secondVersion, cleanedAttribute2?.LastModified);
      Assert.Equal(thirdVersion, cleanedAttribute3?.LastModified);

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MARK CLEAN: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeCollection"/> with a <see cref="AttributeRecord"/> and then deletes it. Confirms
    ///   that <see cref="AttributeCollection.IsDirty(Boolean)"/> returns <c>false</c> after calling <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.MarkClean(DateTime?)"/>.
    /// </summary>
    [Fact]
    public void IsDirty_MarkClean_ReturnsFalse() {

      var topic = new Topic("Test", "Container", null, 1);

      topic.Attributes.SetValue("Foo", "Bar");
      topic.Attributes.SetValue("Baz", "Foo");

      topic.Attributes.Remove("Foo");

      topic.Attributes.MarkClean();

      Assert.False(topic.Attributes.IsDirty());
      Assert.Empty(topic.Attributes.DeletedItems);

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MARK CLEAN: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds an <see cref="AttributeRecord"/> to a <see cref="AttributeCollection"/> associated with a <see cref="Topic"/>
    ///   that is <see cref="Topic.IsNew"/>. Confirms that <see cref="AttributeCollection.IsDirty(Boolean)"/> returns <c>true
    ///   </c> even after calling <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.MarkClean(String)"/> since new
    ///   topics cannot be clean.
    /// </summary>
    [Fact]
    public void IsDirty_MarkClean_ReturnsTrue() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetValue("Foo", "Bar");

      topic.Attributes.MarkClean("Foo");

      Assert.True(topic.Attributes.IsDirty("Foo"));

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MARK ATTRIBUTE CLEAN: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates the <see cref="AttributeCollection"/> with a <see cref="AttributeRecord"/> and then confirms that <see cref=
    ///   "TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty(String)"/> returns <c>false</c> for that attribute after
    ///   calling <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.MarkClean(String, DateTime?)"/>.
    /// </summary>
    [Fact]
    public void IsDirty_MarkAttributeClean_ReturnsFalse() {

      var topic = new Topic("Test", "Container", null, 1);

      topic.Attributes.SetValue("Foo", "Bar");
      topic.Attributes.MarkClean("Foo");

      Assert.False(topic.Attributes.IsDirty("Foo"));

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: ADD CLEAN ATTRIBUTE TO NEW TOPIC: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates a <see cref="AttributeCollection"/> associated with an <see cref="Topic.IsNew"/> <see cref="Topic"/> with a
    ///   <see cref="AttributeRecord"/> that is not marked as <see cref="TrackedRecord{T}.IsDirty"/> and then confirms that <see
    ///   cref="TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty()"/> returns <c>true</c>.
    /// </summary>
    [Fact]
    public void IsDirty_AddCleanAttributeToNewTopic_ReturnsTrue() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.Add(
        new() {
          Key                   = "Foo",
          Value                 = "Bar",
          IsDirty               = false
        }
      );

      Assert.True(topic.Attributes.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MARK NEW TOPIC AS CLEAN: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Populates a <see cref="AttributeCollection"/> associated with an <see cref="Topic.IsNew"/> <see cref="Topic"/> with a
    ///   <see cref="AttributeRecord"/> and then confirms that <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.
    ///   IsDirty(String)"/> returns <c>true</c> for that attribute after calling <see cref="TrackedRecordCollection{TItem,
    ///   TValue, TAttribute}.MarkClean(String, DateTime?)"/>.
    /// </summary>
    [Fact]
    public void IsDirty_MarkNewTopicAsClean_ReturnsTrue() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetValue("Foo", "Bar");
      topic.Attributes.MarkClean();

      Assert.True(topic.Attributes.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: INVALID VALUE: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to violate the business logic by bypassing the property setter; ensures that business logic is enforced.
    /// </summary>
    [Fact]
    public void SetValue_InvalidValue_ThrowsException() {

      var topic = new Topic("Test", "Container");

      Assert.Throws<InvalidKeyException>(() =>
        topic.Attributes.SetValue("View", "# ?")
      );

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: DUPLICATE VALUE: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to insert a <see cref="TrackedRecord{T}"/> with the same <see cref="TrackedRecord{T}.Key"/> as an existing
    ///   value, and confirms that the expected <see cref="ArgumentException"/> is thrown.
    /// </summary>
    [Fact]
    public void SetValue_DuplicateValue_ThrowsException() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.Add(new("Test", "Original"));

      Assert.Throws<ArgumentException>(() =>
        topic.Attributes.Add(new("Test", "New"))
      );

    }

    /*==========================================================================================================================
    | TEST: ADD: VALID ATTRIBUTE RECORD: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a custom attribute on a topic by directly adding an <see cref="AttributeRecord"/> instance; ensures it can be
    ///   retrieved.
    /// </summary>
    [Fact]
    public void Add_ValidAttributeRecord_IsReturned() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.Add(new("View", "NewKey", false));

      Assert.Equal("NewKey", topic.View);

    }

    /*==========================================================================================================================
    | TEST: ADD: NUMERIC VALUE WITH BUSINESS LOGIC: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a numeric attribute on a topic instance; ensures it is routed through the corresponding property and correctly
    ///   retrieved.
    /// </summary>
    [Fact]
    public void Add_NumericValueWithBusinessLogic_IsReturned() {

      var topic = new CustomTopic("Test", "Page");

      topic.Attributes.SetInteger("NumericAttribute", 1);

      Assert.Equal(1, topic.NumericAttribute);

    }

    /*==========================================================================================================================
    | TEST: ADD: BOOLEAN VALUE WITH BUSINESS LOGIC: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a Boolean attribute on a topic instance; ensures it is routed through the corresponding property and correctly
    ///   retrieved.
    /// </summary>
    [Fact]
    public void Add_BooleanValueWithBusinessLogic_IsReturned() {

      var topic = new CustomTopic("Test", "Page");

      topic.Attributes.SetBoolean("BooleanAttribute", true);

      Assert.True(topic.BooleanAttribute);

    }

    /*==========================================================================================================================
    | TEST: ADD: NUMERIC VALUE WITH BUSINESS LOGIC: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a numeric attribute on a topic instance with an invalid value; ensures an exception is thrown.
    /// </summary>
    [Fact]
    public void Add_NumericValueWithBusinessLogic_ThrowsException() {

      var topic = new CustomTopic("Test", "Page");

      Assert.Throws<ArgumentOutOfRangeException>(() =>
        topic.Attributes.SetInteger("NumericAttribute", -1)
      );

    }

    /*==========================================================================================================================
    | TEST: ADD: DATE/TIME VALUE WITH BUSINESS LOGIC: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a Date/Time attribute on a topic instance; ensures it is routed through the corresponding property and correctly
    ///   retrieved.
    /// </summary>
    [Fact]
    public void Add_DateTimeValueWithBusinessLogic_IsReturned() {

      var topic = new CustomTopic("Test", "Page");
      var dateTime = new DateTime(2021, 1, 5);

      topic.Attributes.SetDateTime("DateTimeAttribute", dateTime);

      Assert.Equal(dateTime, topic.DateTimeAttribute);

    }

    /*==========================================================================================================================
    | TEST: ADD: DATE/TIME VALUE WITH BUSINESS LOGIC: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a <see cref="DateTime"/> attribute on a topic instance with an invalid value; ensures an exception is thrown.
    /// </summary>
    [Fact]
    public void Add_DateTimeValueWithBusinessLogic_ThrowsException() {

      var topic = new CustomTopic("Test", "Page");

      Assert.Throws<ArgumentOutOfRangeException>(() =>
        topic.Attributes.SetDateTime("DateTimeAttribute", DateTime.MinValue)
      );

    }

    /*==========================================================================================================================
    | TEST: ATTRIBUTE RECORD: LAST MODIFIED: DEFAULT VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new <see cref="AttributeRecord"/> and ensures that the <see cref="TrackedRecord{T}.LastModified"/> is set to
    ///   <see cref="DateTime.UtcNow"/>.
    /// </summary>
    [Fact]
    public void AttributeRecord_LastModified_DefaultValue() {

      var beforeDate            = DateTime.UtcNow;
      var attribute             = new AttributeRecord("Test", "Value");
      var afterDate             = DateTime.UtcNow;

      Assert.True(attribute.LastModified > beforeDate);
      Assert.True(attribute.LastModified < afterDate);

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: INSERT INVALID ATTRIBUTE RECORD: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to violate the business logic by bypassing <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.
    ///   SetValue(String, TValue, Boolean?, DateTime?)"/> entirely; ensures that business logic is enforced.
    /// </summary>
    [Fact]
    public void Add_InvalidAttributeRecord_ThrowsException() {

      var topic = new Topic("Test", "Container");

      Assert.Throws<InvalidKeyException>(() =>
        topic.Attributes.Add(new("View", "# ?"))
      );

    }

    /*==========================================================================================================================
    | TEST: REPLACE VALUE: WITH BUSINESS LOGIC: MAINTAINS ISDIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a new <see cref="AttributeRecord"/> which maps to <see cref="Topic.Key"/> directly to a <see cref=
    ///   "AttributeCollection"/> and confirms that the original <see cref="TrackedRecord{T}.IsDirty"/> is replaced if the <see
    ///   cref="TrackedRecord{T}.Value"/> changes.
    /// </summary>
    [Fact]
    public void Add_WithBusinessLogic_MaintainsIsDirty() {

      var topic                 = new Topic("Test", "Container", null, 1) {
        View                    = "Test"
      };

      topic.Attributes.TryGetValue("View", out var originalValue);

      Contract.Assume(originalValue);

      var index = topic.Attributes.IndexOf(originalValue);

      topic.Attributes[index] = new AttributeRecord("View", "NewValue", false);
      topic.Attributes.TryGetValue("View", out var newAttribute);

      topic.Attributes.SetValue("View", "NewerValue", false);
      topic.Attributes.TryGetValue("View", out var newerAttribute);

      Assert.False(newAttribute?.IsDirty);
      Assert.False(newerAttribute?.IsDirty);

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: EMPTY ATTRIBUTE RECORD: SKIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a new attribute with an empty value, and confirms that it is <i>not</i> added as a new <see cref="AttributeRecord
    ///   "/>. Empty values are treated as the same as non-existent attributes. They are stored for the sake of tracking <i>
    ///   deleted</i> attributes, but should not be stored for <i>new</i> attributes.
    /// </summary>
    [Fact]
    public void SetValue_EmptyAttributeRecord_Skips() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetValue("Attribute", "");

      Assert.False(topic.Attributes.Contains("Attribute"));

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: UPDATE EMPTY ATTRIBUTE RECORD: REPLACES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a new attribute with an empty value, and confirms that it is <i>is</i> added as a new <see cref="AttributeRecord
    ///   "/> assuming the value previously existed. Empty values are treated as the same as non-existent attributes, but they
    ///   should be stored for the sake of tracking <i>deleted</i> attributes.
    /// </summary>
    [Fact]
    public void SetValue_EmptyAttributeRecord_Replaces() {

      var topic = new Topic("Test", "Container");

      topic.Attributes.SetValue("Attribute", "New Value");
      topic.Attributes.SetValue("Attribute", "");

      Assert.True(topic.Attributes.Contains("Attribute"));

    }

    /*==========================================================================================================================
    | TEST: GET VALUE: INHERIT FROM PARENT: RETURNS PARENT VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an attribute on the parent of a topic and ensures it can be retrieved using inheritance.
    /// </summary>
    [Fact]
    public void GetValue_InheritFromParent_ReturnsParentValue() {

      var topics = new Topic[8];

      for (var i = 0; i <= 7; i++) {
        var topic = new Topic("Topic" + i, "Container");
        if (i > 0) topic.Parent = topics[i - 1];
        topics[i] = topic;
      }

      topics[0].Attributes.SetValue("Foo", "Bar");

      Assert.Null(topics[4].Attributes.GetValue("Foo", null));
      Assert.Equal("Bar", topics[7].Attributes.GetValue("Foo", true));
      Assert.NotEqual("Bar", topics[7].Attributes.GetValue("Foo", false));

    }

    /*==========================================================================================================================
    | TEST: GET VALUE: INHERIT FROM BASE: RETURNS INHERITED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a long tree of derived topics, and ensures that the inherited value is returned.
    /// </summary>
    [Fact]
    public void GetValue_InheritFromBase_ReturnsInheritedValue() {

      var topics = new Topic[5];

      for (var i = 0; i <= 4; i++) {
        var topic = new Topic("Topic" + i, "Container");
        if (i > 0) topics[i - 1].BaseTopic = topic;
        topics[i] = topic;
      }

      topics[4].Attributes.SetValue("Foo", "Bar");

      Assert.Equal("Bar", topics[0].Attributes.GetValue("Foo", null, true, true));

    }

    /*==========================================================================================================================
    | TEST: GET VALUE: EXCEEDS MAX HOPS: RETURNS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a long tree of derives topics, and ensures that inheritance will pursue no more than five hops.
    /// </summary>
    [Fact]
    public void GetValue_ExceedsMaxHops_ReturnsDefault() {

      var topics = new Topic[8];

      for (var i = 0; i <= 7; i++) {
        var topic = new Topic("Topic" + i, "Container");
        if (i > 0) topics[i - 1].BaseTopic = topic;
        topics[i] = topic;
      }

      topics[7].Attributes.SetValue("Foo", "Bar");

      Assert.Null(topics[0].Attributes.GetValue("Foo", null, true, true));

    }

  } //Class
} //Namespace
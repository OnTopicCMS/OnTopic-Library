using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.Data.Tools.Schema.Sql.UnitTesting;
using Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OnTopic.Data.Sql.Database.Tests {
  [TestClass()]
  public class Functions: SqlDatabaseTestClass {

    public Functions() {
      InitializeComponent();
    }

    [TestInitialize()]
    public void TestInitialize() {
      base.InitializeTest();
    }
    [TestCleanup()]
    public void TestCleanup() {
      base.CleanupTest();
    }

    #region Designer support code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetExtendedAttributeTest_TestAction;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Functions));
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition inconclusiveCondition1;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetParentIDTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetTopicIDTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetUniqueKeyTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition inconclusiveCondition4;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_FindTopicIDsTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition findTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetAttributesTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition getAttributeValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetChildTopicIDsTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetAttributesTest_PretestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preGetAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetAttributesTest_PosttestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition postGetAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction testInitializeAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preFunctionTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction testCleanupAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition postFunctionTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_FindTopicIDsTest_PretestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preFindAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getChildTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition getParentIDValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition getIDTopicValue;
      this.dbo_GetExtendedAttributeTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_GetParentIDTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_GetTopicIDTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_GetUniqueKeyTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_FindTopicIDsTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_GetAttributesTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_GetChildTopicIDsTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      dbo_GetExtendedAttributeTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      inconclusiveCondition1 = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition();
      dbo_GetParentIDTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      dbo_GetTopicIDTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      dbo_GetUniqueKeyTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      inconclusiveCondition4 = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition();
      dbo_FindTopicIDsTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      findTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_GetAttributesTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      getAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getAttributeValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      dbo_GetChildTopicIDsTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      dbo_GetAttributesTest_PretestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preGetAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_GetAttributesTest_PosttestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      postGetAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      testInitializeAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preFunctionTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      testCleanupAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      postFunctionTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_FindTopicIDsTest_PretestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preFindAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getChildTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getParentIDValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      getIDTopicValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      // 
      // dbo_GetExtendedAttributeTest_TestAction
      // 
      dbo_GetExtendedAttributeTest_TestAction.Conditions.Add(inconclusiveCondition1);
      resources.ApplyResources(dbo_GetExtendedAttributeTest_TestAction, "dbo_GetExtendedAttributeTest_TestAction");
      // 
      // inconclusiveCondition1
      // 
      inconclusiveCondition1.Enabled = true;
      inconclusiveCondition1.Name = "inconclusiveCondition1";
      // 
      // dbo_GetParentIDTest_TestAction
      // 
      dbo_GetParentIDTest_TestAction.Conditions.Add(getParentIDValue);
      resources.ApplyResources(dbo_GetParentIDTest_TestAction, "dbo_GetParentIDTest_TestAction");
      // 
      // dbo_GetTopicIDTest_TestAction
      // 
      dbo_GetTopicIDTest_TestAction.Conditions.Add(getIDTopicValue);
      resources.ApplyResources(dbo_GetTopicIDTest_TestAction, "dbo_GetTopicIDTest_TestAction");
      // 
      // dbo_GetUniqueKeyTest_TestAction
      // 
      dbo_GetUniqueKeyTest_TestAction.Conditions.Add(inconclusiveCondition4);
      resources.ApplyResources(dbo_GetUniqueKeyTest_TestAction, "dbo_GetUniqueKeyTest_TestAction");
      // 
      // inconclusiveCondition4
      // 
      inconclusiveCondition4.Enabled = true;
      inconclusiveCondition4.Name = "inconclusiveCondition4";
      // 
      // dbo_FindTopicIDsTest_TestAction
      // 
      dbo_FindTopicIDsTest_TestAction.Conditions.Add(findTopicCount);
      resources.ApplyResources(dbo_FindTopicIDsTest_TestAction, "dbo_FindTopicIDsTest_TestAction");
      // 
      // findTopicCount
      // 
      findTopicCount.Enabled = true;
      findTopicCount.Name = "findTopicCount";
      findTopicCount.ResultSet = 1;
      findTopicCount.RowCount = 2;
      // 
      // dbo_GetAttributesTest_TestAction
      // 
      dbo_GetAttributesTest_TestAction.Conditions.Add(getAttributeCount);
      dbo_GetAttributesTest_TestAction.Conditions.Add(getAttributeValue);
      resources.ApplyResources(dbo_GetAttributesTest_TestAction, "dbo_GetAttributesTest_TestAction");
      // 
      // getAttributeCount
      // 
      getAttributeCount.Enabled = true;
      getAttributeCount.Name = "getAttributeCount";
      getAttributeCount.ResultSet = 1;
      getAttributeCount.RowCount = 4;
      // 
      // getAttributeValue
      // 
      getAttributeValue.ColumnNumber = 1;
      getAttributeValue.Enabled = true;
      getAttributeValue.ExpectedValue = "GetAttributesTest4";
      getAttributeValue.Name = "getAttributeValue";
      getAttributeValue.NullExpected = false;
      getAttributeValue.ResultSet = 1;
      getAttributeValue.RowNumber = 4;
      // 
      // dbo_GetChildTopicIDsTest_TestAction
      // 
      dbo_GetChildTopicIDsTest_TestAction.Conditions.Add(getChildTopicCount);
      resources.ApplyResources(dbo_GetChildTopicIDsTest_TestAction, "dbo_GetChildTopicIDsTest_TestAction");
      // 
      // dbo_GetAttributesTest_PretestAction
      // 
      dbo_GetAttributesTest_PretestAction.Conditions.Add(preGetAttributeCount);
      resources.ApplyResources(dbo_GetAttributesTest_PretestAction, "dbo_GetAttributesTest_PretestAction");
      // 
      // preGetAttributeCount
      // 
      preGetAttributeCount.Enabled = true;
      preGetAttributeCount.Name = "preGetAttributeCount";
      preGetAttributeCount.ResultSet = 1;
      preGetAttributeCount.RowCount = 3;
      // 
      // dbo_GetAttributesTest_PosttestAction
      // 
      dbo_GetAttributesTest_PosttestAction.Conditions.Add(postGetAttributeCount);
      resources.ApplyResources(dbo_GetAttributesTest_PosttestAction, "dbo_GetAttributesTest_PosttestAction");
      // 
      // postGetAttributeCount
      // 
      postGetAttributeCount.Enabled = true;
      postGetAttributeCount.Name = "postGetAttributeCount";
      postGetAttributeCount.ResultSet = 1;
      postGetAttributeCount.RowCount = 0;
      // 
      // testInitializeAction
      // 
      testInitializeAction.Conditions.Add(preFunctionTopicCount);
      resources.ApplyResources(testInitializeAction, "testInitializeAction");
      // 
      // preFunctionTopicCount
      // 
      preFunctionTopicCount.Enabled = true;
      preFunctionTopicCount.Name = "preFunctionTopicCount";
      preFunctionTopicCount.ResultSet = 1;
      preFunctionTopicCount.RowCount = 8;
      // 
      // testCleanupAction
      // 
      testCleanupAction.Conditions.Add(postFunctionTopicCount);
      resources.ApplyResources(testCleanupAction, "testCleanupAction");
      // 
      // postFunctionTopicCount
      // 
      postFunctionTopicCount.Enabled = true;
      postFunctionTopicCount.Name = "postFunctionTopicCount";
      postFunctionTopicCount.ResultSet = 1;
      postFunctionTopicCount.RowCount = 1;
      // 
      // dbo_FindTopicIDsTest_PretestAction
      // 
      dbo_FindTopicIDsTest_PretestAction.Conditions.Add(preFindAttributeCount);
      resources.ApplyResources(dbo_FindTopicIDsTest_PretestAction, "dbo_FindTopicIDsTest_PretestAction");
      // 
      // preFindAttributeCount
      // 
      preFindAttributeCount.Enabled = true;
      preFindAttributeCount.Name = "preFindAttributeCount";
      preFindAttributeCount.ResultSet = 1;
      preFindAttributeCount.RowCount = 3;
      // 
      // dbo_GetExtendedAttributeTestData
      // 
      this.dbo_GetExtendedAttributeTestData.PosttestAction = null;
      this.dbo_GetExtendedAttributeTestData.PretestAction = null;
      this.dbo_GetExtendedAttributeTestData.TestAction = dbo_GetExtendedAttributeTest_TestAction;
      // 
      // dbo_GetParentIDTestData
      // 
      this.dbo_GetParentIDTestData.PosttestAction = null;
      this.dbo_GetParentIDTestData.PretestAction = null;
      this.dbo_GetParentIDTestData.TestAction = dbo_GetParentIDTest_TestAction;
      // 
      // dbo_GetTopicIDTestData
      // 
      this.dbo_GetTopicIDTestData.PosttestAction = null;
      this.dbo_GetTopicIDTestData.PretestAction = null;
      this.dbo_GetTopicIDTestData.TestAction = dbo_GetTopicIDTest_TestAction;
      // 
      // dbo_GetUniqueKeyTestData
      // 
      this.dbo_GetUniqueKeyTestData.PosttestAction = null;
      this.dbo_GetUniqueKeyTestData.PretestAction = null;
      this.dbo_GetUniqueKeyTestData.TestAction = dbo_GetUniqueKeyTest_TestAction;
      // 
      // dbo_FindTopicIDsTestData
      // 
      this.dbo_FindTopicIDsTestData.PosttestAction = null;
      this.dbo_FindTopicIDsTestData.PretestAction = dbo_FindTopicIDsTest_PretestAction;
      this.dbo_FindTopicIDsTestData.TestAction = dbo_FindTopicIDsTest_TestAction;
      // 
      // dbo_GetAttributesTestData
      // 
      this.dbo_GetAttributesTestData.PosttestAction = dbo_GetAttributesTest_PosttestAction;
      this.dbo_GetAttributesTestData.PretestAction = dbo_GetAttributesTest_PretestAction;
      this.dbo_GetAttributesTestData.TestAction = dbo_GetAttributesTest_TestAction;
      // 
      // dbo_GetChildTopicIDsTestData
      // 
      this.dbo_GetChildTopicIDsTestData.PosttestAction = null;
      this.dbo_GetChildTopicIDsTestData.PretestAction = null;
      this.dbo_GetChildTopicIDsTestData.TestAction = dbo_GetChildTopicIDsTest_TestAction;
      // 
      // getChildTopicCount
      // 
      getChildTopicCount.Enabled = true;
      getChildTopicCount.Name = "getChildTopicCount";
      getChildTopicCount.ResultSet = 1;
      getChildTopicCount.RowCount = 3;
      // 
      // getParentIDValue
      // 
      getParentIDValue.ColumnNumber = 1;
      getParentIDValue.Enabled = true;
      getParentIDValue.ExpectedValue = "0";
      getParentIDValue.Name = "getParentIDValue";
      getParentIDValue.NullExpected = false;
      getParentIDValue.ResultSet = 1;
      getParentIDValue.RowNumber = 1;
      // 
      // getIDTopicValue
      // 
      getIDTopicValue.ColumnNumber = 1;
      getIDTopicValue.Enabled = true;
      getIDTopicValue.ExpectedValue = "0";
      getIDTopicValue.Name = "getIDTopicValue";
      getIDTopicValue.NullExpected = false;
      getIDTopicValue.ResultSet = 1;
      getIDTopicValue.RowNumber = 1;
      // 
      // Functions
      // 
      this.TestCleanupAction = testCleanupAction;
      this.TestInitializeAction = testInitializeAction;
    }

    #endregion


    #region Additional test attributes
    //
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    #endregion

    [TestMethod()]
    public void dbo_GetExtendedAttributeTest() {
      SqlDatabaseTestActions testActions = this.dbo_GetExtendedAttributeTestData;
      // Execute the pre-test script
      // 
      System.Diagnostics.Trace.WriteLineIf((testActions.PretestAction != null), "Executing pre-test script...");
      SqlExecutionResult[] pretestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PretestAction);
      try {
        // Execute the test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.TestAction != null), "Executing test script...");
        SqlExecutionResult[] testResults = TestService.Execute(this.ExecutionContext, this.PrivilegedContext, testActions.TestAction);
      }
      finally {
        // Execute the post-test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.PosttestAction != null), "Executing post-test script...");
        SqlExecutionResult[] posttestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PosttestAction);
      }
    }

    [TestMethod()]
    public void dbo_GetParentIDTest() {
      SqlDatabaseTestActions testActions = this.dbo_GetParentIDTestData;
      // Execute the pre-test script
      // 
      System.Diagnostics.Trace.WriteLineIf((testActions.PretestAction != null), "Executing pre-test script...");
      SqlExecutionResult[] pretestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PretestAction);
      try {
        // Execute the test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.TestAction != null), "Executing test script...");
        SqlExecutionResult[] testResults = TestService.Execute(this.ExecutionContext, this.PrivilegedContext, testActions.TestAction);
      }
      finally {
        // Execute the post-test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.PosttestAction != null), "Executing post-test script...");
        SqlExecutionResult[] posttestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PosttestAction);
      }
    }

    [TestMethod()]
    public void dbo_GetTopicIDTest() {
      SqlDatabaseTestActions testActions = this.dbo_GetTopicIDTestData;
      // Execute the pre-test script
      // 
      System.Diagnostics.Trace.WriteLineIf((testActions.PretestAction != null), "Executing pre-test script...");
      SqlExecutionResult[] pretestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PretestAction);
      try {
        // Execute the test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.TestAction != null), "Executing test script...");
        SqlExecutionResult[] testResults = TestService.Execute(this.ExecutionContext, this.PrivilegedContext, testActions.TestAction);
      }
      finally {
        // Execute the post-test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.PosttestAction != null), "Executing post-test script...");
        SqlExecutionResult[] posttestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PosttestAction);
      }
    }

    [TestMethod()]
    public void dbo_GetUniqueKeyTest() {
      SqlDatabaseTestActions testActions = this.dbo_GetUniqueKeyTestData;
      // Execute the pre-test script
      // 
      System.Diagnostics.Trace.WriteLineIf((testActions.PretestAction != null), "Executing pre-test script...");
      SqlExecutionResult[] pretestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PretestAction);
      try {
        // Execute the test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.TestAction != null), "Executing test script...");
        SqlExecutionResult[] testResults = TestService.Execute(this.ExecutionContext, this.PrivilegedContext, testActions.TestAction);
      }
      finally {
        // Execute the post-test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.PosttestAction != null), "Executing post-test script...");
        SqlExecutionResult[] posttestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PosttestAction);
      }
    }

    [TestMethod()]
    public void dbo_FindTopicIDsTest() {
      SqlDatabaseTestActions testActions = this.dbo_FindTopicIDsTestData;
      // Execute the pre-test script
      // 
      System.Diagnostics.Trace.WriteLineIf((testActions.PretestAction != null), "Executing pre-test script...");
      SqlExecutionResult[] pretestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PretestAction);
      try {
        // Execute the test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.TestAction != null), "Executing test script...");
        SqlExecutionResult[] testResults = TestService.Execute(this.ExecutionContext, this.PrivilegedContext, testActions.TestAction);
      }
      finally {
        // Execute the post-test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.PosttestAction != null), "Executing post-test script...");
        SqlExecutionResult[] posttestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PosttestAction);
      }
    }

    [TestMethod()]
    public void dbo_GetAttributesTest() {
      SqlDatabaseTestActions testActions = this.dbo_GetAttributesTestData;
      // Execute the pre-test script
      // 
      System.Diagnostics.Trace.WriteLineIf((testActions.PretestAction != null), "Executing pre-test script...");
      SqlExecutionResult[] pretestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PretestAction);
      try {
        // Execute the test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.TestAction != null), "Executing test script...");
        SqlExecutionResult[] testResults = TestService.Execute(this.ExecutionContext, this.PrivilegedContext, testActions.TestAction);
      }
      finally {
        // Execute the post-test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.PosttestAction != null), "Executing post-test script...");
        SqlExecutionResult[] posttestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PosttestAction);
      }
    }

    [TestMethod()]
    public void dbo_GetChildTopicIDsTest() {
      SqlDatabaseTestActions testActions = this.dbo_GetChildTopicIDsTestData;
      // Execute the pre-test script
      // 
      System.Diagnostics.Trace.WriteLineIf((testActions.PretestAction != null), "Executing pre-test script...");
      SqlExecutionResult[] pretestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PretestAction);
      try {
        // Execute the test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.TestAction != null), "Executing test script...");
        SqlExecutionResult[] testResults = TestService.Execute(this.ExecutionContext, this.PrivilegedContext, testActions.TestAction);
      }
      finally {
        // Execute the post-test script
        // 
        System.Diagnostics.Trace.WriteLineIf((testActions.PosttestAction != null), "Executing post-test script...");
        SqlExecutionResult[] posttestResults = TestService.Execute(this.PrivilegedContext, this.PrivilegedContext, testActions.PosttestAction);
      }
    }
    private SqlDatabaseTestActions dbo_GetExtendedAttributeTestData;
    private SqlDatabaseTestActions dbo_GetParentIDTestData;
    private SqlDatabaseTestActions dbo_GetTopicIDTestData;
    private SqlDatabaseTestActions dbo_GetUniqueKeyTestData;
    private SqlDatabaseTestActions dbo_FindTopicIDsTestData;
    private SqlDatabaseTestActions dbo_GetAttributesTestData;
    private SqlDatabaseTestActions dbo_GetChildTopicIDsTestData;
  }
}

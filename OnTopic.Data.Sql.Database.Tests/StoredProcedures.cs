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
  public class StoredProcedures: SqlDatabaseTestClass {

    public StoredProcedures() {
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
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_CreateTopicTest_TestAction;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StoredProcedures));
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition createTopicTotal;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_DeleteTopicTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition deleteTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition deleteAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition deleteRelationshipCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition deleteReferenceCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetTopicVersionTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getVersionTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getVersionAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getVersionExtendedAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getVersionRelationshipCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getVersionReferenceCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getVersionVersionHistoryCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetTopicsTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getExtendedAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getRelationshipCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getReferenceCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getVersionHistoryCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_MoveTopicTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateAttributesTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition inconclusiveCondition6;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateExtendedAttributesTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition inconclusiveCondition7;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateReferencesTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition inconclusiveCondition8;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateRelationshipsTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition inconclusiveCondition9;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateTopicTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition inconclusiveCondition10;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_CreateTopicTest_PosttestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition postCreateTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_DeleteTopicTest_PretestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preDeleteTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preDeleteAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preDeleteRelationshipCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preDeleteReferenceCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetTopicsTest_PretestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preGetTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preGetAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preGetRelationshipCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preGetReferenceCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetTopicsTest_PosttestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetTopicVersionTest_PretestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preGetVersionTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preGetVersionAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preGetVersionRelationshipCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preGetVersionReferenceCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetTopicVersionTest_PosttestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition getVersionAttributeValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition getVersionRelationshipValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition getVersionReferenceValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_MoveTopicTest_PretestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preMoveTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition moveTopicValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_MoveTopicTest_PosttestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition postMoveTopicCount;
      this.dbo_CreateTopicTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_DeleteTopicTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_GetTopicVersionTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_GetTopicsTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_MoveTopicTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_UpdateAttributesTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_UpdateExtendedAttributesTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_UpdateReferencesTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_UpdateRelationshipsTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      this.dbo_UpdateTopicTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
      dbo_CreateTopicTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      createTopicTotal = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_DeleteTopicTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      deleteTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      deleteAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      deleteRelationshipCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      deleteReferenceCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_GetTopicVersionTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      getVersionTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getVersionAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getVersionExtendedAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getVersionRelationshipCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getVersionReferenceCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getVersionVersionHistoryCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_GetTopicsTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      getTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getExtendedAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getRelationshipCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getReferenceCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getVersionHistoryCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_MoveTopicTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      dbo_UpdateAttributesTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      inconclusiveCondition6 = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition();
      dbo_UpdateExtendedAttributesTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      inconclusiveCondition7 = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition();
      dbo_UpdateReferencesTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      inconclusiveCondition8 = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition();
      dbo_UpdateRelationshipsTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      inconclusiveCondition9 = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition();
      dbo_UpdateTopicTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      inconclusiveCondition10 = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.InconclusiveCondition();
      dbo_CreateTopicTest_PosttestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      postCreateTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_DeleteTopicTest_PretestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preDeleteTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      preDeleteAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      preDeleteRelationshipCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      preDeleteReferenceCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_GetTopicsTest_PretestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preGetTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      preGetAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      preGetRelationshipCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      preGetReferenceCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_GetTopicsTest_PosttestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      dbo_GetTopicVersionTest_PretestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preGetVersionTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      preGetVersionAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      preGetVersionRelationshipCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      preGetVersionReferenceCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_GetTopicVersionTest_PosttestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      getVersionAttributeValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      getVersionRelationshipValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      getVersionReferenceValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      dbo_MoveTopicTest_PretestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preMoveTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      moveTopicValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      dbo_MoveTopicTest_PosttestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      postMoveTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      // 
      // dbo_CreateTopicTest_TestAction
      // 
      dbo_CreateTopicTest_TestAction.Conditions.Add(createTopicTotal);
      resources.ApplyResources(dbo_CreateTopicTest_TestAction, "dbo_CreateTopicTest_TestAction");
      // 
      // createTopicTotal
      // 
      createTopicTotal.Enabled = true;
      createTopicTotal.Name = "createTopicTotal";
      createTopicTotal.ResultSet = 1;
      createTopicTotal.RowCount = 1;
      // 
      // dbo_DeleteTopicTest_TestAction
      // 
      dbo_DeleteTopicTest_TestAction.Conditions.Add(deleteTopicCount);
      dbo_DeleteTopicTest_TestAction.Conditions.Add(deleteAttributeCount);
      dbo_DeleteTopicTest_TestAction.Conditions.Add(deleteRelationshipCount);
      dbo_DeleteTopicTest_TestAction.Conditions.Add(deleteReferenceCount);
      resources.ApplyResources(dbo_DeleteTopicTest_TestAction, "dbo_DeleteTopicTest_TestAction");
      // 
      // deleteTopicCount
      // 
      deleteTopicCount.Enabled = true;
      deleteTopicCount.Name = "deleteTopicCount";
      deleteTopicCount.ResultSet = 1;
      deleteTopicCount.RowCount = 0;
      // 
      // deleteAttributeCount
      // 
      deleteAttributeCount.Enabled = true;
      deleteAttributeCount.Name = "deleteAttributeCount";
      deleteAttributeCount.ResultSet = 2;
      deleteAttributeCount.RowCount = 0;
      // 
      // deleteRelationshipCount
      // 
      deleteRelationshipCount.Enabled = true;
      deleteRelationshipCount.Name = "deleteRelationshipCount";
      deleteRelationshipCount.ResultSet = 3;
      deleteRelationshipCount.RowCount = 0;
      // 
      // deleteReferenceCount
      // 
      deleteReferenceCount.Enabled = true;
      deleteReferenceCount.Name = "deleteReferenceCount";
      deleteReferenceCount.ResultSet = 4;
      deleteReferenceCount.RowCount = 0;
      // 
      // dbo_GetTopicVersionTest_TestAction
      // 
      dbo_GetTopicVersionTest_TestAction.Conditions.Add(getVersionTopicCount);
      dbo_GetTopicVersionTest_TestAction.Conditions.Add(getVersionAttributeCount);
      dbo_GetTopicVersionTest_TestAction.Conditions.Add(getVersionExtendedAttributeCount);
      dbo_GetTopicVersionTest_TestAction.Conditions.Add(getVersionRelationshipCount);
      dbo_GetTopicVersionTest_TestAction.Conditions.Add(getVersionReferenceCount);
      dbo_GetTopicVersionTest_TestAction.Conditions.Add(getVersionVersionHistoryCount);
      dbo_GetTopicVersionTest_TestAction.Conditions.Add(getVersionAttributeValue);
      dbo_GetTopicVersionTest_TestAction.Conditions.Add(getVersionRelationshipValue);
      dbo_GetTopicVersionTest_TestAction.Conditions.Add(getVersionReferenceValue);
      resources.ApplyResources(dbo_GetTopicVersionTest_TestAction, "dbo_GetTopicVersionTest_TestAction");
      // 
      // getVersionTopicCount
      // 
      getVersionTopicCount.Enabled = true;
      getVersionTopicCount.Name = "getVersionTopicCount";
      getVersionTopicCount.ResultSet = 1;
      getVersionTopicCount.RowCount = 1;
      // 
      // getVersionAttributeCount
      // 
      getVersionAttributeCount.Enabled = true;
      getVersionAttributeCount.Name = "getVersionAttributeCount";
      getVersionAttributeCount.ResultSet = 2;
      getVersionAttributeCount.RowCount = 2;
      // 
      // getVersionExtendedAttributeCount
      // 
      getVersionExtendedAttributeCount.Enabled = true;
      getVersionExtendedAttributeCount.Name = "getVersionExtendedAttributeCount";
      getVersionExtendedAttributeCount.ResultSet = 3;
      getVersionExtendedAttributeCount.RowCount = 1;
      // 
      // getVersionRelationshipCount
      // 
      getVersionRelationshipCount.Enabled = true;
      getVersionRelationshipCount.Name = "getVersionRelationshipCount";
      getVersionRelationshipCount.ResultSet = 4;
      getVersionRelationshipCount.RowCount = 1;
      // 
      // getVersionReferenceCount
      // 
      getVersionReferenceCount.Enabled = true;
      getVersionReferenceCount.Name = "getVersionReferenceCount";
      getVersionReferenceCount.ResultSet = 5;
      getVersionReferenceCount.RowCount = 1;
      // 
      // getVersionVersionHistoryCount
      // 
      getVersionVersionHistoryCount.Enabled = true;
      getVersionVersionHistoryCount.Name = "getVersionVersionHistoryCount";
      getVersionVersionHistoryCount.ResultSet = 6;
      getVersionVersionHistoryCount.RowCount = 1;
      // 
      // dbo_GetTopicsTest_TestAction
      // 
      dbo_GetTopicsTest_TestAction.Conditions.Add(getTopicCount);
      dbo_GetTopicsTest_TestAction.Conditions.Add(getAttributeCount);
      dbo_GetTopicsTest_TestAction.Conditions.Add(getExtendedAttributeCount);
      dbo_GetTopicsTest_TestAction.Conditions.Add(getRelationshipCount);
      dbo_GetTopicsTest_TestAction.Conditions.Add(getReferenceCount);
      dbo_GetTopicsTest_TestAction.Conditions.Add(getVersionHistoryCount);
      resources.ApplyResources(dbo_GetTopicsTest_TestAction, "dbo_GetTopicsTest_TestAction");
      // 
      // getTopicCount
      // 
      getTopicCount.Enabled = true;
      getTopicCount.Name = "getTopicCount";
      getTopicCount.ResultSet = 1;
      getTopicCount.RowCount = 2;
      // 
      // getAttributeCount
      // 
      getAttributeCount.Enabled = true;
      getAttributeCount.Name = "getAttributeCount";
      getAttributeCount.ResultSet = 2;
      getAttributeCount.RowCount = 4;
      // 
      // getExtendedAttributeCount
      // 
      getExtendedAttributeCount.Enabled = true;
      getExtendedAttributeCount.Name = "getExtendedAttributeCount";
      getExtendedAttributeCount.ResultSet = 3;
      getExtendedAttributeCount.RowCount = 2;
      // 
      // getRelationshipCount
      // 
      getRelationshipCount.Enabled = true;
      getRelationshipCount.Name = "getRelationshipCount";
      getRelationshipCount.ResultSet = 4;
      getRelationshipCount.RowCount = 1;
      // 
      // getReferenceCount
      // 
      getReferenceCount.Enabled = true;
      getReferenceCount.Name = "getReferenceCount";
      getReferenceCount.ResultSet = 5;
      getReferenceCount.RowCount = 1;
      // 
      // getVersionHistoryCount
      // 
      getVersionHistoryCount.Enabled = true;
      getVersionHistoryCount.Name = "getVersionHistoryCount";
      getVersionHistoryCount.ResultSet = 6;
      getVersionHistoryCount.RowCount = 2;
      // 
      // dbo_MoveTopicTest_TestAction
      // 
      dbo_MoveTopicTest_TestAction.Conditions.Add(moveTopicValue);
      resources.ApplyResources(dbo_MoveTopicTest_TestAction, "dbo_MoveTopicTest_TestAction");
      // 
      // dbo_UpdateAttributesTest_TestAction
      // 
      dbo_UpdateAttributesTest_TestAction.Conditions.Add(inconclusiveCondition6);
      resources.ApplyResources(dbo_UpdateAttributesTest_TestAction, "dbo_UpdateAttributesTest_TestAction");
      // 
      // inconclusiveCondition6
      // 
      inconclusiveCondition6.Enabled = true;
      inconclusiveCondition6.Name = "inconclusiveCondition6";
      // 
      // dbo_UpdateExtendedAttributesTest_TestAction
      // 
      dbo_UpdateExtendedAttributesTest_TestAction.Conditions.Add(inconclusiveCondition7);
      resources.ApplyResources(dbo_UpdateExtendedAttributesTest_TestAction, "dbo_UpdateExtendedAttributesTest_TestAction");
      // 
      // inconclusiveCondition7
      // 
      inconclusiveCondition7.Enabled = true;
      inconclusiveCondition7.Name = "inconclusiveCondition7";
      // 
      // dbo_UpdateReferencesTest_TestAction
      // 
      dbo_UpdateReferencesTest_TestAction.Conditions.Add(inconclusiveCondition8);
      resources.ApplyResources(dbo_UpdateReferencesTest_TestAction, "dbo_UpdateReferencesTest_TestAction");
      // 
      // inconclusiveCondition8
      // 
      inconclusiveCondition8.Enabled = true;
      inconclusiveCondition8.Name = "inconclusiveCondition8";
      // 
      // dbo_UpdateRelationshipsTest_TestAction
      // 
      dbo_UpdateRelationshipsTest_TestAction.Conditions.Add(inconclusiveCondition9);
      resources.ApplyResources(dbo_UpdateRelationshipsTest_TestAction, "dbo_UpdateRelationshipsTest_TestAction");
      // 
      // inconclusiveCondition9
      // 
      inconclusiveCondition9.Enabled = true;
      inconclusiveCondition9.Name = "inconclusiveCondition9";
      // 
      // dbo_UpdateTopicTest_TestAction
      // 
      dbo_UpdateTopicTest_TestAction.Conditions.Add(inconclusiveCondition10);
      resources.ApplyResources(dbo_UpdateTopicTest_TestAction, "dbo_UpdateTopicTest_TestAction");
      // 
      // inconclusiveCondition10
      // 
      inconclusiveCondition10.Enabled = true;
      inconclusiveCondition10.Name = "inconclusiveCondition10";
      // 
      // dbo_CreateTopicTest_PosttestAction
      // 
      dbo_CreateTopicTest_PosttestAction.Conditions.Add(postCreateTopicCount);
      resources.ApplyResources(dbo_CreateTopicTest_PosttestAction, "dbo_CreateTopicTest_PosttestAction");
      // 
      // postCreateTopicCount
      // 
      postCreateTopicCount.Enabled = true;
      postCreateTopicCount.Name = "postCreateTopicCount";
      postCreateTopicCount.ResultSet = 1;
      postCreateTopicCount.RowCount = 0;
      // 
      // dbo_DeleteTopicTest_PretestAction
      // 
      dbo_DeleteTopicTest_PretestAction.Conditions.Add(preDeleteTopicCount);
      dbo_DeleteTopicTest_PretestAction.Conditions.Add(preDeleteAttributeCount);
      dbo_DeleteTopicTest_PretestAction.Conditions.Add(preDeleteRelationshipCount);
      dbo_DeleteTopicTest_PretestAction.Conditions.Add(preDeleteReferenceCount);
      resources.ApplyResources(dbo_DeleteTopicTest_PretestAction, "dbo_DeleteTopicTest_PretestAction");
      // 
      // preDeleteTopicCount
      // 
      preDeleteTopicCount.Enabled = true;
      preDeleteTopicCount.Name = "preDeleteTopicCount";
      preDeleteTopicCount.ResultSet = 1;
      preDeleteTopicCount.RowCount = 2;
      // 
      // preDeleteAttributeCount
      // 
      preDeleteAttributeCount.Enabled = true;
      preDeleteAttributeCount.Name = "preDeleteAttributeCount";
      preDeleteAttributeCount.ResultSet = 2;
      preDeleteAttributeCount.RowCount = 4;
      // 
      // preDeleteRelationshipCount
      // 
      preDeleteRelationshipCount.Enabled = true;
      preDeleteRelationshipCount.Name = "preDeleteRelationshipCount";
      preDeleteRelationshipCount.ResultSet = 3;
      preDeleteRelationshipCount.RowCount = 1;
      // 
      // preDeleteReferenceCount
      // 
      preDeleteReferenceCount.Enabled = true;
      preDeleteReferenceCount.Name = "preDeleteReferenceCount";
      preDeleteReferenceCount.ResultSet = 4;
      preDeleteReferenceCount.RowCount = 1;
      // 
      // dbo_GetTopicsTest_PretestAction
      // 
      dbo_GetTopicsTest_PretestAction.Conditions.Add(preGetTopicCount);
      dbo_GetTopicsTest_PretestAction.Conditions.Add(preGetAttributeCount);
      dbo_GetTopicsTest_PretestAction.Conditions.Add(preGetRelationshipCount);
      dbo_GetTopicsTest_PretestAction.Conditions.Add(preGetReferenceCount);
      resources.ApplyResources(dbo_GetTopicsTest_PretestAction, "dbo_GetTopicsTest_PretestAction");
      // 
      // preGetTopicCount
      // 
      preGetTopicCount.Enabled = true;
      preGetTopicCount.Name = "preGetTopicCount";
      preGetTopicCount.ResultSet = 1;
      preGetTopicCount.RowCount = 2;
      // 
      // preGetAttributeCount
      // 
      preGetAttributeCount.Enabled = true;
      preGetAttributeCount.Name = "preGetAttributeCount";
      preGetAttributeCount.ResultSet = 2;
      preGetAttributeCount.RowCount = 4;
      // 
      // preGetRelationshipCount
      // 
      preGetRelationshipCount.Enabled = true;
      preGetRelationshipCount.Name = "preGetRelationshipCount";
      preGetRelationshipCount.ResultSet = 3;
      preGetRelationshipCount.RowCount = 1;
      // 
      // preGetReferenceCount
      // 
      preGetReferenceCount.Enabled = true;
      preGetReferenceCount.Name = "preGetReferenceCount";
      preGetReferenceCount.ResultSet = 4;
      preGetReferenceCount.RowCount = 1;
      // 
      // dbo_GetTopicsTest_PosttestAction
      // 
      resources.ApplyResources(dbo_GetTopicsTest_PosttestAction, "dbo_GetTopicsTest_PosttestAction");
      // 
      // dbo_GetTopicVersionTest_PretestAction
      // 
      dbo_GetTopicVersionTest_PretestAction.Conditions.Add(preGetVersionTopicCount);
      dbo_GetTopicVersionTest_PretestAction.Conditions.Add(preGetVersionAttributeCount);
      dbo_GetTopicVersionTest_PretestAction.Conditions.Add(preGetVersionRelationshipCount);
      dbo_GetTopicVersionTest_PretestAction.Conditions.Add(preGetVersionReferenceCount);
      resources.ApplyResources(dbo_GetTopicVersionTest_PretestAction, "dbo_GetTopicVersionTest_PretestAction");
      // 
      // preGetVersionTopicCount
      // 
      preGetVersionTopicCount.Enabled = true;
      preGetVersionTopicCount.Name = "preGetVersionTopicCount";
      preGetVersionTopicCount.ResultSet = 1;
      preGetVersionTopicCount.RowCount = 2;
      // 
      // preGetVersionAttributeCount
      // 
      preGetVersionAttributeCount.Enabled = true;
      preGetVersionAttributeCount.Name = "preGetVersionAttributeCount";
      preGetVersionAttributeCount.ResultSet = 2;
      preGetVersionAttributeCount.RowCount = 6;
      // 
      // preGetVersionRelationshipCount
      // 
      preGetVersionRelationshipCount.Enabled = true;
      preGetVersionRelationshipCount.Name = "preGetVersionRelationshipCount";
      preGetVersionRelationshipCount.ResultSet = 3;
      preGetVersionRelationshipCount.RowCount = 2;
      // 
      // preGetVersionReferenceCount
      // 
      preGetVersionReferenceCount.Enabled = true;
      preGetVersionReferenceCount.Name = "preGetVersionReferenceCount";
      preGetVersionReferenceCount.ResultSet = 4;
      preGetVersionReferenceCount.RowCount = 2;
      // 
      // dbo_GetTopicVersionTest_PosttestAction
      // 
      resources.ApplyResources(dbo_GetTopicVersionTest_PosttestAction, "dbo_GetTopicVersionTest_PosttestAction");
      // 
      // dbo_CreateTopicTestData
      // 
      this.dbo_CreateTopicTestData.PosttestAction = dbo_CreateTopicTest_PosttestAction;
      this.dbo_CreateTopicTestData.PretestAction = null;
      this.dbo_CreateTopicTestData.TestAction = dbo_CreateTopicTest_TestAction;
      // 
      // dbo_DeleteTopicTestData
      // 
      this.dbo_DeleteTopicTestData.PosttestAction = null;
      this.dbo_DeleteTopicTestData.PretestAction = dbo_DeleteTopicTest_PretestAction;
      this.dbo_DeleteTopicTestData.TestAction = dbo_DeleteTopicTest_TestAction;
      // 
      // dbo_GetTopicVersionTestData
      // 
      this.dbo_GetTopicVersionTestData.PosttestAction = dbo_GetTopicVersionTest_PosttestAction;
      this.dbo_GetTopicVersionTestData.PretestAction = dbo_GetTopicVersionTest_PretestAction;
      this.dbo_GetTopicVersionTestData.TestAction = dbo_GetTopicVersionTest_TestAction;
      // 
      // dbo_GetTopicsTestData
      // 
      this.dbo_GetTopicsTestData.PosttestAction = dbo_GetTopicsTest_PosttestAction;
      this.dbo_GetTopicsTestData.PretestAction = dbo_GetTopicsTest_PretestAction;
      this.dbo_GetTopicsTestData.TestAction = dbo_GetTopicsTest_TestAction;
      // 
      // dbo_MoveTopicTestData
      // 
      this.dbo_MoveTopicTestData.PosttestAction = dbo_MoveTopicTest_PosttestAction;
      this.dbo_MoveTopicTestData.PretestAction = dbo_MoveTopicTest_PretestAction;
      this.dbo_MoveTopicTestData.TestAction = dbo_MoveTopicTest_TestAction;
      // 
      // dbo_UpdateAttributesTestData
      // 
      this.dbo_UpdateAttributesTestData.PosttestAction = null;
      this.dbo_UpdateAttributesTestData.PretestAction = null;
      this.dbo_UpdateAttributesTestData.TestAction = dbo_UpdateAttributesTest_TestAction;
      // 
      // dbo_UpdateExtendedAttributesTestData
      // 
      this.dbo_UpdateExtendedAttributesTestData.PosttestAction = null;
      this.dbo_UpdateExtendedAttributesTestData.PretestAction = null;
      this.dbo_UpdateExtendedAttributesTestData.TestAction = dbo_UpdateExtendedAttributesTest_TestAction;
      // 
      // dbo_UpdateReferencesTestData
      // 
      this.dbo_UpdateReferencesTestData.PosttestAction = null;
      this.dbo_UpdateReferencesTestData.PretestAction = null;
      this.dbo_UpdateReferencesTestData.TestAction = dbo_UpdateReferencesTest_TestAction;
      // 
      // dbo_UpdateRelationshipsTestData
      // 
      this.dbo_UpdateRelationshipsTestData.PosttestAction = null;
      this.dbo_UpdateRelationshipsTestData.PretestAction = null;
      this.dbo_UpdateRelationshipsTestData.TestAction = dbo_UpdateRelationshipsTest_TestAction;
      // 
      // dbo_UpdateTopicTestData
      // 
      this.dbo_UpdateTopicTestData.PosttestAction = null;
      this.dbo_UpdateTopicTestData.PretestAction = null;
      this.dbo_UpdateTopicTestData.TestAction = dbo_UpdateTopicTest_TestAction;
      // 
      // getVersionAttributeValue
      // 
      getVersionAttributeValue.ColumnNumber = 3;
      getVersionAttributeValue.Enabled = true;
      getVersionAttributeValue.ExpectedValue = "Value";
      getVersionAttributeValue.Name = "getVersionAttributeValue";
      getVersionAttributeValue.NullExpected = false;
      getVersionAttributeValue.ResultSet = 2;
      getVersionAttributeValue.RowNumber = 1;
      // 
      // getVersionRelationshipValue
      // 
      getVersionRelationshipValue.ColumnNumber = 5;
      getVersionRelationshipValue.Enabled = true;
      getVersionRelationshipValue.ExpectedValue = "2020-01-01 12:00:00";
      getVersionRelationshipValue.Name = "getVersionRelationshipValue";
      getVersionRelationshipValue.NullExpected = false;
      getVersionRelationshipValue.ResultSet = 4;
      getVersionRelationshipValue.RowNumber = 1;
      // 
      // getVersionReferenceValue
      // 
      getVersionReferenceValue.ColumnNumber = 4;
      getVersionReferenceValue.Enabled = true;
      getVersionReferenceValue.ExpectedValue = "2020-01-01 12:00:00";
      getVersionReferenceValue.Name = "getVersionReferenceValue";
      getVersionReferenceValue.NullExpected = false;
      getVersionReferenceValue.ResultSet = 5;
      getVersionReferenceValue.RowNumber = 1;
      // 
      // dbo_MoveTopicTest_PretestAction
      // 
      dbo_MoveTopicTest_PretestAction.Conditions.Add(preMoveTopicCount);
      resources.ApplyResources(dbo_MoveTopicTest_PretestAction, "dbo_MoveTopicTest_PretestAction");
      // 
      // preMoveTopicCount
      // 
      preMoveTopicCount.Enabled = true;
      preMoveTopicCount.Name = "preMoveTopicCount";
      preMoveTopicCount.ResultSet = 1;
      preMoveTopicCount.RowCount = 7;
      // 
      // moveTopicValue
      // 
      moveTopicValue.ColumnNumber = 1;
      moveTopicValue.Enabled = true;
      moveTopicValue.ExpectedValue = "MoveTopicChildTest3";
      moveTopicValue.Name = "moveTopicValue";
      moveTopicValue.NullExpected = false;
      moveTopicValue.ResultSet = 2;
      moveTopicValue.RowNumber = 4;
      // 
      // dbo_MoveTopicTest_PosttestAction
      // 
      dbo_MoveTopicTest_PosttestAction.Conditions.Add(postMoveTopicCount);
      resources.ApplyResources(dbo_MoveTopicTest_PosttestAction, "dbo_MoveTopicTest_PosttestAction");
      // 
      // postMoveTopicCount
      // 
      postMoveTopicCount.Enabled = true;
      postMoveTopicCount.Name = "postMoveTopicCount";
      postMoveTopicCount.ResultSet = 1;
      postMoveTopicCount.RowCount = 0;
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
    public void dbo_CreateTopicTest() {
      SqlDatabaseTestActions testActions = this.dbo_CreateTopicTestData;
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
    public void dbo_DeleteTopicTest() {
      SqlDatabaseTestActions testActions = this.dbo_DeleteTopicTestData;
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
    public void dbo_GetTopicVersionTest() {
      SqlDatabaseTestActions testActions = this.dbo_GetTopicVersionTestData;
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
    public void dbo_GetTopicsTest() {
      SqlDatabaseTestActions testActions = this.dbo_GetTopicsTestData;
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
    public void dbo_MoveTopicTest() {
      SqlDatabaseTestActions testActions = this.dbo_MoveTopicTestData;
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
    public void dbo_UpdateAttributesTest() {
      SqlDatabaseTestActions testActions = this.dbo_UpdateAttributesTestData;
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
    public void dbo_UpdateExtendedAttributesTest() {
      SqlDatabaseTestActions testActions = this.dbo_UpdateExtendedAttributesTestData;
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
    public void dbo_UpdateReferencesTest() {
      SqlDatabaseTestActions testActions = this.dbo_UpdateReferencesTestData;
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
    public void dbo_UpdateRelationshipsTest() {
      SqlDatabaseTestActions testActions = this.dbo_UpdateRelationshipsTestData;
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
    public void dbo_UpdateTopicTest() {
      SqlDatabaseTestActions testActions = this.dbo_UpdateTopicTestData;
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
    private SqlDatabaseTestActions dbo_CreateTopicTestData;
    private SqlDatabaseTestActions dbo_DeleteTopicTestData;
    private SqlDatabaseTestActions dbo_GetTopicVersionTestData;
    private SqlDatabaseTestActions dbo_GetTopicsTestData;
    private SqlDatabaseTestActions dbo_MoveTopicTestData;
    private SqlDatabaseTestActions dbo_UpdateAttributesTestData;
    private SqlDatabaseTestActions dbo_UpdateExtendedAttributesTestData;
    private SqlDatabaseTestActions dbo_UpdateReferencesTestData;
    private SqlDatabaseTestActions dbo_UpdateRelationshipsTestData;
    private SqlDatabaseTestActions dbo_UpdateTopicTestData;
  }
}

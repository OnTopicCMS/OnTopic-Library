﻿using System;
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
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition getVersionAttributeValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition getVersionRelationshipValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition getVersionReferenceValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetTopicsTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getExtendedAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getRelationshipCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getReferenceCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getVersionHistoryCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_MoveTopicTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition moveTopicValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateAttributesTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition updateAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition updateAttributeValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateExtendedAttributesTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition updateExtendedAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition updateExtendedAttributeValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateReferencesTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition updateReferenceCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition updateReferenceValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateRelationshipsTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition updateRelationshipCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition updateRelationshipValue;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateTopicTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition updateTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition updateTopicValue;
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
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_MoveTopicTest_PretestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preMoveTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_MoveTopicTest_PosttestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition postMoveTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateAttributesTest_PretestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preUpdateAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateAttributesTest_PosttestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition postUpdateAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateReferencesTest_PretestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preUpdateReferenceCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateReferencesTest_PosttestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateRelationshipsTest_PretestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preUpdateRelationshipCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateRelationshipsTest_PosttestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateTopicTest_PosttestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition postUpdateTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateTopicTest_PretestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preUpdateTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateExtendedAttributesTest_PretestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preUpdateExtendedAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_UpdateExtendedAttributesTest_PosttestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition postUpdateExtendedAttributeTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition postUpdateExtendedAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction testInitializeAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetTopicUpdatesTest_TestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getUpdatesTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getUpdatesAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getUpdatesExtendedAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getUpdatesRelationshipCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getUpdatesReferenceCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition getUpdatesVersionHistoryCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetTopicUpdatesTest_PretestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preGetUpdatesTopicCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preGetUpdatesAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preGetUpdatesRelationshipCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition preGetUpdatesReferenceCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction dbo_GetTopicUpdatesTest_PosttestAction;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition postGetUpdatesAttributeCount;
      Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction testCleanupAction;
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
      this.dbo_GetTopicUpdatesTestData = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestActions();
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
      getVersionAttributeValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      getVersionRelationshipValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      getVersionReferenceValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      dbo_GetTopicsTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      getTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getExtendedAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getRelationshipCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getReferenceCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getVersionHistoryCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_MoveTopicTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      moveTopicValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      dbo_UpdateAttributesTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      updateAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      updateAttributeValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      dbo_UpdateExtendedAttributesTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      updateExtendedAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      updateExtendedAttributeValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      dbo_UpdateReferencesTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      updateReferenceCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      updateReferenceValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      dbo_UpdateRelationshipsTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      updateRelationshipCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      updateRelationshipValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
      dbo_UpdateTopicTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      updateTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      updateTopicValue = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.ScalarValueCondition();
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
      dbo_MoveTopicTest_PretestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preMoveTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_MoveTopicTest_PosttestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      postMoveTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_UpdateAttributesTest_PretestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preUpdateAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_UpdateAttributesTest_PosttestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      postUpdateAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_UpdateReferencesTest_PretestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preUpdateReferenceCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_UpdateReferencesTest_PosttestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      dbo_UpdateRelationshipsTest_PretestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preUpdateRelationshipCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_UpdateRelationshipsTest_PosttestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      dbo_UpdateTopicTest_PosttestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      postUpdateTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_UpdateTopicTest_PretestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preUpdateTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_UpdateExtendedAttributesTest_PretestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preUpdateExtendedAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_UpdateExtendedAttributesTest_PosttestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      postUpdateExtendedAttributeTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      postUpdateExtendedAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      testInitializeAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      dbo_GetTopicUpdatesTest_TestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      getUpdatesTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getUpdatesAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getUpdatesExtendedAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getUpdatesRelationshipCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getUpdatesReferenceCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      getUpdatesVersionHistoryCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_GetTopicUpdatesTest_PretestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      preGetUpdatesTopicCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      preGetUpdatesAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      preGetUpdatesRelationshipCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      preGetUpdatesReferenceCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      dbo_GetTopicUpdatesTest_PosttestAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
      postGetUpdatesAttributeCount = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.Conditions.RowCountCondition();
      testCleanupAction = new Microsoft.Data.Tools.Schema.Sql.UnitTesting.SqlDatabaseTestAction();
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
      // dbo_UpdateAttributesTest_TestAction
      // 
      dbo_UpdateAttributesTest_TestAction.Conditions.Add(updateAttributeCount);
      dbo_UpdateAttributesTest_TestAction.Conditions.Add(updateAttributeValue);
      resources.ApplyResources(dbo_UpdateAttributesTest_TestAction, "dbo_UpdateAttributesTest_TestAction");
      // 
      // updateAttributeCount
      // 
      updateAttributeCount.Enabled = true;
      updateAttributeCount.Name = "updateAttributeCount";
      updateAttributeCount.ResultSet = 1;
      updateAttributeCount.RowCount = 3;
      // 
      // updateAttributeValue
      // 
      updateAttributeValue.ColumnNumber = 1;
      updateAttributeValue.Enabled = true;
      updateAttributeValue.ExpectedValue = "UpdateAttributesTest4";
      updateAttributeValue.Name = "updateAttributeValue";
      updateAttributeValue.NullExpected = false;
      updateAttributeValue.ResultSet = 1;
      updateAttributeValue.RowNumber = 3;
      // 
      // dbo_UpdateExtendedAttributesTest_TestAction
      // 
      dbo_UpdateExtendedAttributesTest_TestAction.Conditions.Add(updateExtendedAttributeCount);
      dbo_UpdateExtendedAttributesTest_TestAction.Conditions.Add(updateExtendedAttributeValue);
      resources.ApplyResources(dbo_UpdateExtendedAttributesTest_TestAction, "dbo_UpdateExtendedAttributesTest_TestAction");
      // 
      // updateExtendedAttributeCount
      // 
      updateExtendedAttributeCount.Enabled = true;
      updateExtendedAttributeCount.Name = "updateExtendedAttributeCount";
      updateExtendedAttributeCount.ResultSet = 1;
      updateExtendedAttributeCount.RowCount = 2;
      // 
      // updateExtendedAttributeValue
      // 
      updateExtendedAttributeValue.ColumnNumber = 1;
      updateExtendedAttributeValue.Enabled = true;
      updateExtendedAttributeValue.ExpectedValue = "<attributes><attribute key=\"Body\">New</attribute></attributes>";
      updateExtendedAttributeValue.Name = "updateExtendedAttributeValue";
      updateExtendedAttributeValue.NullExpected = false;
      updateExtendedAttributeValue.ResultSet = 1;
      updateExtendedAttributeValue.RowNumber = 2;
      // 
      // dbo_UpdateReferencesTest_TestAction
      // 
      dbo_UpdateReferencesTest_TestAction.Conditions.Add(updateReferenceCount);
      dbo_UpdateReferencesTest_TestAction.Conditions.Add(updateReferenceValue);
      resources.ApplyResources(dbo_UpdateReferencesTest_TestAction, "dbo_UpdateReferencesTest_TestAction");
      // 
      // updateReferenceCount
      // 
      updateReferenceCount.Enabled = true;
      updateReferenceCount.Name = "updateReferenceCount";
      updateReferenceCount.ResultSet = 1;
      updateReferenceCount.RowCount = 4;
      // 
      // updateReferenceValue
      // 
      updateReferenceValue.ColumnNumber = 1;
      updateReferenceValue.Enabled = true;
      updateReferenceValue.ExpectedValue = "-1";
      updateReferenceValue.Name = "updateReferenceValue";
      updateReferenceValue.NullExpected = false;
      updateReferenceValue.ResultSet = 1;
      updateReferenceValue.RowNumber = 3;
      // 
      // dbo_UpdateRelationshipsTest_TestAction
      // 
      dbo_UpdateRelationshipsTest_TestAction.Conditions.Add(updateRelationshipCount);
      dbo_UpdateRelationshipsTest_TestAction.Conditions.Add(updateRelationshipValue);
      resources.ApplyResources(dbo_UpdateRelationshipsTest_TestAction, "dbo_UpdateRelationshipsTest_TestAction");
      // 
      // updateRelationshipCount
      // 
      updateRelationshipCount.Enabled = true;
      updateRelationshipCount.Name = "updateRelationshipCount";
      updateRelationshipCount.ResultSet = 1;
      updateRelationshipCount.RowCount = 4;
      // 
      // updateRelationshipValue
      // 
      updateRelationshipValue.ColumnNumber = 1;
      updateRelationshipValue.Enabled = true;
      updateRelationshipValue.ExpectedValue = "True";
      updateRelationshipValue.Name = "updateRelationshipValue";
      updateRelationshipValue.NullExpected = false;
      updateRelationshipValue.ResultSet = 1;
      updateRelationshipValue.RowNumber = 3;
      // 
      // dbo_UpdateTopicTest_TestAction
      // 
      dbo_UpdateTopicTest_TestAction.Conditions.Add(updateTopicCount);
      dbo_UpdateTopicTest_TestAction.Conditions.Add(updateTopicValue);
      resources.ApplyResources(dbo_UpdateTopicTest_TestAction, "dbo_UpdateTopicTest_TestAction");
      // 
      // updateTopicCount
      // 
      updateTopicCount.Enabled = true;
      updateTopicCount.Name = "updateTopicCount";
      updateTopicCount.ResultSet = 1;
      updateTopicCount.RowCount = 1;
      // 
      // updateTopicValue
      // 
      updateTopicValue.ColumnNumber = 1;
      updateTopicValue.Enabled = true;
      updateTopicValue.ExpectedValue = "TestNew";
      updateTopicValue.Name = "updateTopicValue";
      updateTopicValue.NullExpected = false;
      updateTopicValue.ResultSet = 1;
      updateTopicValue.RowNumber = 1;
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
      // 
      // dbo_UpdateAttributesTest_PretestAction
      // 
      dbo_UpdateAttributesTest_PretestAction.Conditions.Add(preUpdateAttributeCount);
      resources.ApplyResources(dbo_UpdateAttributesTest_PretestAction, "dbo_UpdateAttributesTest_PretestAction");
      // 
      // preUpdateAttributeCount
      // 
      preUpdateAttributeCount.Enabled = true;
      preUpdateAttributeCount.Name = "preUpdateAttributeCount";
      preUpdateAttributeCount.ResultSet = 1;
      preUpdateAttributeCount.RowCount = 3;
      // 
      // dbo_UpdateAttributesTest_PosttestAction
      // 
      dbo_UpdateAttributesTest_PosttestAction.Conditions.Add(postUpdateAttributeCount);
      resources.ApplyResources(dbo_UpdateAttributesTest_PosttestAction, "dbo_UpdateAttributesTest_PosttestAction");
      // 
      // postUpdateAttributeCount
      // 
      postUpdateAttributeCount.Enabled = true;
      postUpdateAttributeCount.Name = "postUpdateAttributeCount";
      postUpdateAttributeCount.ResultSet = 1;
      postUpdateAttributeCount.RowCount = 0;
      // 
      // dbo_UpdateReferencesTest_PretestAction
      // 
      dbo_UpdateReferencesTest_PretestAction.Conditions.Add(preUpdateReferenceCount);
      resources.ApplyResources(dbo_UpdateReferencesTest_PretestAction, "dbo_UpdateReferencesTest_PretestAction");
      // 
      // preUpdateReferenceCount
      // 
      preUpdateReferenceCount.Enabled = true;
      preUpdateReferenceCount.Name = "preUpdateReferenceCount";
      preUpdateReferenceCount.ResultSet = 1;
      preUpdateReferenceCount.RowCount = 3;
      // 
      // dbo_UpdateReferencesTest_PosttestAction
      // 
      resources.ApplyResources(dbo_UpdateReferencesTest_PosttestAction, "dbo_UpdateReferencesTest_PosttestAction");
      // 
      // dbo_UpdateRelationshipsTest_PretestAction
      // 
      dbo_UpdateRelationshipsTest_PretestAction.Conditions.Add(preUpdateRelationshipCount);
      resources.ApplyResources(dbo_UpdateRelationshipsTest_PretestAction, "dbo_UpdateRelationshipsTest_PretestAction");
      // 
      // preUpdateRelationshipCount
      // 
      preUpdateRelationshipCount.Enabled = true;
      preUpdateRelationshipCount.Name = "preUpdateRelationshipCount";
      preUpdateRelationshipCount.ResultSet = 1;
      preUpdateRelationshipCount.RowCount = 3;
      // 
      // dbo_UpdateRelationshipsTest_PosttestAction
      // 
      resources.ApplyResources(dbo_UpdateRelationshipsTest_PosttestAction, "dbo_UpdateRelationshipsTest_PosttestAction");
      // 
      // dbo_UpdateTopicTest_PosttestAction
      // 
      dbo_UpdateTopicTest_PosttestAction.Conditions.Add(postUpdateTopicCount);
      resources.ApplyResources(dbo_UpdateTopicTest_PosttestAction, "dbo_UpdateTopicTest_PosttestAction");
      // 
      // postUpdateTopicCount
      // 
      postUpdateTopicCount.Enabled = true;
      postUpdateTopicCount.Name = "postUpdateTopicCount";
      postUpdateTopicCount.ResultSet = 1;
      postUpdateTopicCount.RowCount = 0;
      // 
      // dbo_UpdateTopicTest_PretestAction
      // 
      dbo_UpdateTopicTest_PretestAction.Conditions.Add(preUpdateTopicCount);
      resources.ApplyResources(dbo_UpdateTopicTest_PretestAction, "dbo_UpdateTopicTest_PretestAction");
      // 
      // preUpdateTopicCount
      // 
      preUpdateTopicCount.Enabled = true;
      preUpdateTopicCount.Name = "preUpdateTopicCount";
      preUpdateTopicCount.ResultSet = 1;
      preUpdateTopicCount.RowCount = 1;
      // 
      // dbo_UpdateExtendedAttributesTest_PretestAction
      // 
      dbo_UpdateExtendedAttributesTest_PretestAction.Conditions.Add(preUpdateExtendedAttributeCount);
      resources.ApplyResources(dbo_UpdateExtendedAttributesTest_PretestAction, "dbo_UpdateExtendedAttributesTest_PretestAction");
      // 
      // preUpdateExtendedAttributeCount
      // 
      preUpdateExtendedAttributeCount.Enabled = true;
      preUpdateExtendedAttributeCount.Name = "preUpdateExtendedAttributeCount";
      preUpdateExtendedAttributeCount.ResultSet = 1;
      preUpdateExtendedAttributeCount.RowCount = 1;
      // 
      // dbo_UpdateExtendedAttributesTest_PosttestAction
      // 
      dbo_UpdateExtendedAttributesTest_PosttestAction.Conditions.Add(postUpdateExtendedAttributeTopicCount);
      dbo_UpdateExtendedAttributesTest_PosttestAction.Conditions.Add(postUpdateExtendedAttributeCount);
      resources.ApplyResources(dbo_UpdateExtendedAttributesTest_PosttestAction, "dbo_UpdateExtendedAttributesTest_PosttestAction");
      // 
      // postUpdateExtendedAttributeTopicCount
      // 
      postUpdateExtendedAttributeTopicCount.Enabled = true;
      postUpdateExtendedAttributeTopicCount.Name = "postUpdateExtendedAttributeTopicCount";
      postUpdateExtendedAttributeTopicCount.ResultSet = 1;
      postUpdateExtendedAttributeTopicCount.RowCount = 0;
      // 
      // postUpdateExtendedAttributeCount
      // 
      postUpdateExtendedAttributeCount.Enabled = true;
      postUpdateExtendedAttributeCount.Name = "postUpdateExtendedAttributeCount";
      postUpdateExtendedAttributeCount.ResultSet = 1;
      postUpdateExtendedAttributeCount.RowCount = 0;
      // 
      // testInitializeAction
      // 
      resources.ApplyResources(testInitializeAction, "testInitializeAction");
      // 
      // dbo_GetTopicUpdatesTest_TestAction
      // 
      dbo_GetTopicUpdatesTest_TestAction.Conditions.Add(getUpdatesTopicCount);
      dbo_GetTopicUpdatesTest_TestAction.Conditions.Add(getUpdatesAttributeCount);
      dbo_GetTopicUpdatesTest_TestAction.Conditions.Add(getUpdatesExtendedAttributeCount);
      dbo_GetTopicUpdatesTest_TestAction.Conditions.Add(getUpdatesRelationshipCount);
      dbo_GetTopicUpdatesTest_TestAction.Conditions.Add(getUpdatesReferenceCount);
      dbo_GetTopicUpdatesTest_TestAction.Conditions.Add(getUpdatesVersionHistoryCount);
      resources.ApplyResources(dbo_GetTopicUpdatesTest_TestAction, "dbo_GetTopicUpdatesTest_TestAction");
      // 
      // getUpdatesTopicCount
      // 
      getUpdatesTopicCount.Enabled = true;
      getUpdatesTopicCount.Name = "getUpdatesTopicCount";
      getUpdatesTopicCount.ResultSet = 5;
      getUpdatesTopicCount.RowCount = 1;
      // 
      // getUpdatesAttributeCount
      // 
      getUpdatesAttributeCount.Enabled = true;
      getUpdatesAttributeCount.Name = "getUpdatesAttributeCount";
      getUpdatesAttributeCount.ResultSet = 2;
      getUpdatesAttributeCount.RowCount = 4;
      // 
      // getUpdatesExtendedAttributeCount
      // 
      getUpdatesExtendedAttributeCount.Enabled = true;
      getUpdatesExtendedAttributeCount.Name = "getUpdatesExtendedAttributeCount";
      getUpdatesExtendedAttributeCount.ResultSet = 3;
      getUpdatesExtendedAttributeCount.RowCount = 2;
      // 
      // getUpdatesRelationshipCount
      // 
      getUpdatesRelationshipCount.Enabled = true;
      getUpdatesRelationshipCount.Name = "getUpdatesRelationshipCount";
      getUpdatesRelationshipCount.ResultSet = 4;
      getUpdatesRelationshipCount.RowCount = 1;
      // 
      // getUpdatesReferenceCount
      // 
      getUpdatesReferenceCount.Enabled = true;
      getUpdatesReferenceCount.Name = "getUpdatesReferenceCount";
      getUpdatesReferenceCount.ResultSet = 5;
      getUpdatesReferenceCount.RowCount = 1;
      // 
      // getUpdatesVersionHistoryCount
      // 
      getUpdatesVersionHistoryCount.Enabled = true;
      getUpdatesVersionHistoryCount.Name = "getUpdatesVersionHistoryCount";
      getUpdatesVersionHistoryCount.ResultSet = 6;
      getUpdatesVersionHistoryCount.RowCount = 2;
      // 
      // dbo_GetTopicUpdatesTest_PretestAction
      // 
      dbo_GetTopicUpdatesTest_PretestAction.Conditions.Add(preGetUpdatesTopicCount);
      dbo_GetTopicUpdatesTest_PretestAction.Conditions.Add(preGetUpdatesAttributeCount);
      dbo_GetTopicUpdatesTest_PretestAction.Conditions.Add(preGetUpdatesRelationshipCount);
      dbo_GetTopicUpdatesTest_PretestAction.Conditions.Add(preGetUpdatesReferenceCount);
      resources.ApplyResources(dbo_GetTopicUpdatesTest_PretestAction, "dbo_GetTopicUpdatesTest_PretestAction");
      // 
      // preGetUpdatesTopicCount
      // 
      preGetUpdatesTopicCount.Enabled = true;
      preGetUpdatesTopicCount.Name = "preGetUpdatesTopicCount";
      preGetUpdatesTopicCount.ResultSet = 1;
      preGetUpdatesTopicCount.RowCount = 3;
      // 
      // preGetUpdatesAttributeCount
      // 
      preGetUpdatesAttributeCount.Enabled = true;
      preGetUpdatesAttributeCount.Name = "preGetUpdatesAttributeCount";
      preGetUpdatesAttributeCount.ResultSet = 2;
      preGetUpdatesAttributeCount.RowCount = 8;
      // 
      // preGetUpdatesRelationshipCount
      // 
      preGetUpdatesRelationshipCount.Enabled = true;
      preGetUpdatesRelationshipCount.Name = "preGetUpdatesRelationshipCount";
      preGetUpdatesRelationshipCount.ResultSet = 3;
      preGetUpdatesRelationshipCount.RowCount = 2;
      // 
      // preGetUpdatesReferenceCount
      // 
      preGetUpdatesReferenceCount.Enabled = true;
      preGetUpdatesReferenceCount.Name = "preGetUpdatesReferenceCount";
      preGetUpdatesReferenceCount.ResultSet = 3;
      preGetUpdatesReferenceCount.RowCount = 2;
      // 
      // dbo_GetTopicUpdatesTest_PosttestAction
      // 
      dbo_GetTopicUpdatesTest_PosttestAction.Conditions.Add(postGetUpdatesAttributeCount);
      resources.ApplyResources(dbo_GetTopicUpdatesTest_PosttestAction, "dbo_GetTopicUpdatesTest_PosttestAction");
      // 
      // postGetUpdatesAttributeCount
      // 
      postGetUpdatesAttributeCount.Enabled = true;
      postGetUpdatesAttributeCount.Name = "postGetUpdatesAttributeCount";
      postGetUpdatesAttributeCount.ResultSet = 1;
      postGetUpdatesAttributeCount.RowCount = 0;
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
      this.dbo_UpdateAttributesTestData.PosttestAction = dbo_UpdateAttributesTest_PosttestAction;
      this.dbo_UpdateAttributesTestData.PretestAction = dbo_UpdateAttributesTest_PretestAction;
      this.dbo_UpdateAttributesTestData.TestAction = dbo_UpdateAttributesTest_TestAction;
      // 
      // dbo_UpdateExtendedAttributesTestData
      // 
      this.dbo_UpdateExtendedAttributesTestData.PosttestAction = dbo_UpdateExtendedAttributesTest_PosttestAction;
      this.dbo_UpdateExtendedAttributesTestData.PretestAction = dbo_UpdateExtendedAttributesTest_PretestAction;
      this.dbo_UpdateExtendedAttributesTestData.TestAction = dbo_UpdateExtendedAttributesTest_TestAction;
      // 
      // dbo_UpdateReferencesTestData
      // 
      this.dbo_UpdateReferencesTestData.PosttestAction = dbo_UpdateReferencesTest_PosttestAction;
      this.dbo_UpdateReferencesTestData.PretestAction = dbo_UpdateReferencesTest_PretestAction;
      this.dbo_UpdateReferencesTestData.TestAction = dbo_UpdateReferencesTest_TestAction;
      // 
      // dbo_UpdateRelationshipsTestData
      // 
      this.dbo_UpdateRelationshipsTestData.PosttestAction = dbo_UpdateRelationshipsTest_PosttestAction;
      this.dbo_UpdateRelationshipsTestData.PretestAction = dbo_UpdateRelationshipsTest_PretestAction;
      this.dbo_UpdateRelationshipsTestData.TestAction = dbo_UpdateRelationshipsTest_TestAction;
      // 
      // dbo_UpdateTopicTestData
      // 
      this.dbo_UpdateTopicTestData.PosttestAction = dbo_UpdateTopicTest_PosttestAction;
      this.dbo_UpdateTopicTestData.PretestAction = dbo_UpdateTopicTest_PretestAction;
      this.dbo_UpdateTopicTestData.TestAction = dbo_UpdateTopicTest_TestAction;
      // 
      // dbo_GetTopicUpdatesTestData
      // 
      this.dbo_GetTopicUpdatesTestData.PosttestAction = dbo_GetTopicUpdatesTest_PosttestAction;
      this.dbo_GetTopicUpdatesTestData.PretestAction = dbo_GetTopicUpdatesTest_PretestAction;
      this.dbo_GetTopicUpdatesTestData.TestAction = dbo_GetTopicUpdatesTest_TestAction;
      // 
      // testCleanupAction
      // 
      resources.ApplyResources(testCleanupAction, "testCleanupAction");
      // 
      // StoredProcedures
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
    [TestMethod()]
    public void dbo_GetTopicUpdatesTest() {
      SqlDatabaseTestActions testActions = this.dbo_GetTopicUpdatesTestData;
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
    private SqlDatabaseTestActions dbo_GetTopicUpdatesTestData;
  }
}

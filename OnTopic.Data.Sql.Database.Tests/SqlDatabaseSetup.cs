using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Tools.Schema.Sql.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OnTopic.Data.Sql.Database.Tests {
  [TestClass()]
  public class SqlDatabaseSetup {

    [AssemblyInitialize()]
    #pragma warning disable IDE0060 // Remove unused parameter
    public static void InitializeAssembly(TestContext ctx) {
      // Setup the test database based on setting in the
      // configuration file
      SqlDatabaseTestClass.TestService.DeployDatabaseProject();
      SqlDatabaseTestClass.TestService.GenerateData();
    }
    #pragma warning restore IDE0060 // Remove unused parameter

  }
}

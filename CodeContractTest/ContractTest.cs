using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeContractTest {
  public class ContractTest {

    public ContractTest () { }
 
    public string MethodTest() {

      return System.Web.HttpContext.Current?.Request.QueryString["Test"];

    }

  }
}

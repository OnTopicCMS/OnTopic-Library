using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CodeContractTest {
  public class ContractTest {

    public ContractTest () { }
 
    public void Init() {
      var test = MethodTest(HttpContext.Current);
    }

    [Pure]
    public static bool ValidateInput(HttpContext context) {
      Contract.Requires<ArgumentNullException>(context != null);
      Contract.Requires<ArgumentNullException>(context.User != null);
      return true;
    }

    public string MethodTest(HttpContext context) {

      ValidateInput(context);

      var user = context.User.Identity.Name;

      return HttpContext.Current?.Request.QueryString["Test"] ?? user;

    }

  }
}

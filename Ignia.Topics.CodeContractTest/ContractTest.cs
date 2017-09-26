using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
      Contract.Assert(context.User.Identity != null);
      return true;
    }

    public string MethodTest(HttpContext context) {

      Contract.Ensures(Contract.Result<string>() != null);
      ValidateInput(context);

      var user = context.User.Identity.Name;
      var collection = new FauxObjectCollection {
        new FauxObject("Test")
      };

      if (collection.Contains(user)) {
        user += collection[user]?.Value;
      }

      if (context.Request.QueryString.AllKeys.Contains<string>("Test")) {
        user += context.Request.QueryString["Test"];
      }

      return HttpContext.Current?.Request.QueryString["Test"] ?? user;

    }

  }
}

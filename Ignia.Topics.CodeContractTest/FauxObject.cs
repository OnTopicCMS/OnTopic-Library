using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeContractTest {
  public class FauxObject {

    public FauxObject(string key) {
      Key = key;
    }

    public string Key { get; }

    public string Value { get; set; }

  }
}

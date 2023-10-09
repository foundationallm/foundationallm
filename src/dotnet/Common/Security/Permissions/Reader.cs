using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Security.Permissions
{
    public class Reader : Role
    {
        public Reader() {

            this.Id = Guid.Parse("a3f8a6aa-6a8b-48d4-9b1a-000000000002");
            this.Name = "Reader";
            this.Actions = new List<Action>();
            this.Actions.Add(Action.Read);
        }
    }
}

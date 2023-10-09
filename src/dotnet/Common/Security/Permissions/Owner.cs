using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Security.Permissions
{
    public class Owner : Role
    {
        public Owner() {

            this.Id = Guid.Parse("a3f8a6aa-6a8b-48d4-9b1a-000000000000");
            this.Name = "Owner";
            this.Actions = new List<Action>();
            this.Actions.Add(Action.All);
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Security
{
    public class Role
    {
        public Role() { }

        public string Name { get; set; }

        public Guid Id { get; set; }

        public List<Action> Actions { get; set; }
    }
}

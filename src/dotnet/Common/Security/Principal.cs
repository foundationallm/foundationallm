using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Security
{
    public class Principal
    {
        public Principal() { }

        public string Id { get; set; }

        public PrincipalType Type { get; set; }
    }
}

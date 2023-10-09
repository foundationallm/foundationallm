using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Security
{
    public class PermissionedObject
    {

        public PermissionedObject() { }

        public List<RoleAssignment> GetPermissions()
        {
            //base off type and name...
            return SecurityManager.GetAccess(this);
        }
    }
}

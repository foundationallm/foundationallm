using FoundationaLLM.Common.Models.System.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Security.Store
{
    public class SqlDatabase : SecurityDatastore, ISecurityDatastore
    {
        public void AddRoleAssignments(RoleAssignment ra)
        {
            throw new NotImplementedException();
        }

        public void DeleteRoleAssignments(RoleAssignment ra)
        {
            throw new NotImplementedException();
        }

        public Principal GetGroup(string upn)
        {
            throw new NotImplementedException();
        }

        public List<RoleAssignment> GetScopeAssignments(string scope)
        {
            throw new NotImplementedException();
        }

        public Principal GetUser(string upn)
        {
            throw new NotImplementedException();
        }

        public bool IsGroupMember(string groupId, string principalId)
        {
            throw new NotImplementedException();
        }
    }
}

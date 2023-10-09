using FoundationaLLM.Common.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.System.Interfaces
{
    public interface ISecurityDatastore
    {
        public Task<List<RoleAssignment>> GetScopeAssignments(string scope);

        public void AddRoleAssignments(RoleAssignment ra);

        public void DeleteRoleAssignments(RoleAssignment ra);

        public Principal GetUser(string upn);

        public Principal GetGroup(string upn);

        public bool IsActive(Principal p);

        public bool IsGroupMember(string groupId, string principalId);
    }
}

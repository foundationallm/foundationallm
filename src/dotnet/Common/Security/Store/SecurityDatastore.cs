using FoundationaLLM.Common.Models.System.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Security.Store
{
    public class SecurityDatastore : ISecurityDatastore
    {
        protected string _path;

        public string GetFilePath(string scope, bool getRoot = false)
        {
            scope = scope.Replace("*", "all");

            if (getRoot)
            {
                string path = "";
                string[] vals = scope.Split('/');

                for (int i = 0; i < vals.Length - 1; i++)
                {
                    path += $"/{vals[i]}";
                }

                return $"{_path}/{path}/all".Replace("//", "/");
            }
            else
                return $"{_path}/{scope}".Replace("//", "/");
        }

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

        virtual public Task<List<RoleAssignment>> GetScopeAssignments(string scope)
        {
            throw new NotImplementedException();
        }

        public Principal GetUser(string upn)
        {
            throw new NotImplementedException();
        }

        public bool IsActive(Principal p)
        {
            throw new NotImplementedException();
        }

        public bool IsGroupMember(string groupId, string principalId)
        {
            throw new NotImplementedException();
        }
    }
}

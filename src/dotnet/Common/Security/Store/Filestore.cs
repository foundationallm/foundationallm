using FoundationaLLM.Common.Models.System.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Security.Store
{
    public class Filestore : SecurityDatastore, ISecurityDatastore
    {
        public Filestore(string path) {
            this._path = path;
        }

        public void AddRoleAssignments(RoleAssignment ra)
        {
            //save to filepath based on scope...
            string dirPath = $"{GetFilePath(ra.Scope)}";
            string filePath = $"{dirPath}/{ra.Name}.json";

            Directory.CreateDirectory(dirPath);

            var sw = File.CreateText(filePath);
            sw.WriteLine(JsonConvert.SerializeObject(ra));
            sw.Close();
        }

        public void DeleteRoleAssignments(RoleAssignment ra)
        {
            string dirPath = $"{GetFilePath(ra.Scope)}";
            string filePath = $"{dirPath}/{ra.Name}.json";

            File.Delete(filePath);
        }

        public Principal GetGroup(string upn)
        {
            string filePath = $"/groups/{upn}";

            string strUser = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<Principal>(strUser);
        }

        public List<RoleAssignment> GetScopeAssignments(string scope)
        {
            List<RoleAssignment> assignments = new List<RoleAssignment>();

            string dirPath = $"{GetFilePath(scope)}";
            
            foreach (string f in Directory.GetFiles(dirPath))
            {
                assignments.Add(JsonConvert.DeserializeObject<RoleAssignment>(File.ReadAllText(f)));
            }

            //get the wildcard scope...
            dirPath = $"{GetFilePath(scope, true)}";

            foreach (string f in Directory.GetFiles(dirPath))
            {
                assignments.Add(JsonConvert.DeserializeObject<RoleAssignment>(File.ReadAllText(f)));
            }

            //get the top wildcard scope...
            dirPath = $"{_path}/All";

            foreach (string f in Directory.GetFiles(dirPath))
            {
                assignments.Add(JsonConvert.DeserializeObject<RoleAssignment>(File.ReadAllText(f)));
            }

            return assignments;
        }

        public Principal GetUser(string upn)
        {
            string filePath = $"/users/{upn}";

            string strUser = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<Principal>(strUser);
        }

        public bool IsGroupMember(string groupId, string principalId)
        {
            throw new NotImplementedException();
        }
    }
}

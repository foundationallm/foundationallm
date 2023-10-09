using Azure.Identity;
using FoundationaLLM.Common.Models.System.Interfaces;
using Microsoft.Graph;
using Microsoft.Graph.Devices.GetByIds;
using Microsoft.Graph.Groups.Item.Onenote.Notebooks.GetNotebookFromWebUrl;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Security.Store
{
    public class AzureEntra : SecurityDatastore, ISecurityDatastore
    {
        string _clientId;
        string _clientSecret;

        // Multi-tenant apps can use "common",
        // single-tenant apps must use the tenant ID from the Azure portal
        string _tenantId = "common";
        GraphServiceClient _graphClient;

        public AzureEntra(string clientId, string clientSecret, string tenantId)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _tenantId = tenantId;
        }

        public void AddRoleAssignments(RoleAssignment ra)
        {
            throw new NotImplementedException();
        }

        public void DeleteRoleAssignments(RoleAssignment ra)
        {
            throw new NotImplementedException();
        }

        public List<RoleAssignment> GetScopeAssignments(string scope)
        {
            throw new NotImplementedException();
        }

        public void Authenticate()
        {
            //get an access token to azure entra...
            var scopes = new[] { "User.Read" };

            var options = new DefaultAzureCredentialOptions();
            options.AuthorityHost = AzureAuthorityHosts.AzurePublicCloud;
            options.TenantId = _tenantId;
            //options.ClientId = _clientId;
            options.ExcludeManagedIdentityCredential = true;
            var credentials = new Azure.Identity.DefaultAzureCredential(options);

            _graphClient = new GraphServiceClient(credentials, scopes);
        }

        async public Task<Principal> GetUser(string upn)
        {
            //make call to graph to get a user and their id
            Microsoft.Graph.Users.GetByIds.GetByIdsPostRequestBody req = new Microsoft.Graph.Users.GetByIds.GetByIdsPostRequestBody();
            req.Ids = new List<string> { upn };
            Microsoft.Graph.Users.GetByIds.GetByIdsPostResponse res = await _graphClient.Users.GetByIds.PostAsGetByIdsPostResponseAsync(req);

            Principal p = new Principal();
            p.Id = res.Value.ToString();
            p.Type = PrincipalType.User;
            return p;
        }

        public Principal GetGroup(string upn)
        {
            //make call to graph to get a user and their id


            Principal p = new Principal();
            p.Id = "";
            p.Type = PrincipalType.Group;
            return p;
        }

        public Principal GetServicePrincipal(string upn)
        {
            //make call to graph to get a user and their id


            Principal p = new Principal();
            p.Id = "";
            p.Type = PrincipalType.ServicePrincipal;
            return p;
        }

        public bool IsGroupMember(string groupId, string principalId)
        {
            throw new NotImplementedException();
        }
    }
}

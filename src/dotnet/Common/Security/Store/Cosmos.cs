using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.System.Interfaces;

namespace FoundationaLLM.Common.Security.Store
{
    public class Cosmos : SecurityDatastore, ISecurityDatastore
    {
        string _connection;
        Database _database;
        Container _container;
        public Cosmos(string endpoint, string key, string databaseName, string containerName)
        {
            CosmosClient client = new CosmosClientBuilder(endpoint, key).Build();

            Database? database = client?.GetDatabase(databaseName);

            _database = database ?? throw new ArgumentException("Unable to connect to existing Azure Cosmos DB database.");

            _container = database?.GetContainer(containerName.Trim()) ??
                                       throw new ArgumentException("Unable to connect to existing Azure Cosmos DB container or database.");
        }
        async public void AddRoleAssignments(RoleAssignment ra)
        {
            PartitionKey partitionKey = new(ra.Id);
            
            await _container.CreateItemAsync(
                item: ra,
                partitionKey: partitionKey
            );
        }

        async public void DeleteRoleAssignments(RoleAssignment ra)
        {
            PartitionKey partitionKey = new(ra.Id);

            await _container.DeleteItemAsync<RoleAssignment>(ra.Name.ToString(), partitionKey);
        }

        public Principal GetGroup(string upn)
        {
            throw new NotImplementedException();
        }

        async public Task<List<RoleAssignment>> GetScopeAssignments(string scope)
        {
            QueryDefinition query = new QueryDefinition("SELECT DISTINCT * FROM c WHERE c.scope = @scope")
                //.WithParameter("@type", nameof(RoleAssignment))
                .WithParameter("@scope", scope);

            FeedIterator<RoleAssignment> response = _container.GetItemQueryIterator<RoleAssignment>(query);

            List<RoleAssignment> output = new();
            while (response.HasMoreResults)
            {
                FeedResponse<RoleAssignment> results = await response.ReadNextAsync();
                output.AddRange(results);
            }

            return output;
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

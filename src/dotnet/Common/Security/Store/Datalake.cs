using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using FoundationaLLM.Common.Models.System.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Security.Store
{
    public class Datalake : SecurityDatastore, ISecurityDatastore
    {
        BlobServiceClient _blobServiceClient;
        BlobContainerClient _blobClient;
        string _container;

        public Datalake(string connectionString, string container="Security")
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _blobClient = _blobServiceClient.GetBlobContainerClient(container);
            _container = container;
        }

        async public void AddRoleAssignments(RoleAssignment ra)
        {
            //save to filepath based on scope...
            string dirPath = $"{GetFilePath(ra.Scope)}";
            string filePath = $"{dirPath}/{ra.Name}.json";

            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ra))))
            {
                await _blobClient.UploadBlobAsync(filePath, ms);
            }
        }

        public void DeleteRoleAssignments(RoleAssignment ra)
        {
            //save to filepath based on scope...
            string dirPath = $"{GetFilePath(ra.Scope)}";
            string filePath = $"{dirPath}/{ra.Name}.json".Replace("//", "/");

            try
            {
                _blobClient.DeleteBlob(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public Principal GetGroup(string upn)
        {
            string filePath = $"/groups/{upn}";

            string strUser = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<Principal>(strUser);
        }

        async public Task<List<string>> GetBlobs(string prefix, int segmentSize = 10)
        {
            List<string> blobs = new List<string>();

            try
            {

                // Call the listing operation and return pages of the specified size.
                //var resultSegment = _blobClient.GetBlobsByHierarchyAsync(prefix: prefix.Trim('/'), delimiter: "/");
                //var resultSegment = _blobClient.GetBlobs(prefix: prefix);

                // Call the listing operation and return pages of the specified size.
                var resultSegment = _blobClient.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/")
                    .AsPages(default, segmentSize);

                // Enumerate the blobs returned for each page.
                await foreach (Azure.Page<BlobHierarchyItem> blobPage in resultSegment)
                {
                    // A hierarchical listing may return both virtual directories and blobs.
                    foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
                    {
                        if (blobhierarchyItem.IsPrefix)
                        {
                            // Write out the prefix of the virtual directory.
                            // Call recursively with the prefix to traverse the virtual directory.
                            blobs.AddRange(await GetBlobs(blobhierarchyItem.Prefix, segmentSize));
                        }
                        else if (blobhierarchyItem.IsBlob)
                        {
                            blobs.Add(blobhierarchyItem.Blob.Name);
                        }
                    }
                }
            }
            catch (RequestFailedException e)
            {
                throw;
            }

            return blobs;
        }

        override async public Task<List<RoleAssignment>> GetScopeAssignments(string scope)
        {
            List<RoleAssignment> assignments = new List<RoleAssignment>();

            string dirPath = $"{GetFilePath(scope)}";

            while(dirPath.StartsWith("/"))
                dirPath = dirPath.Substring(1);

            foreach (string f in await GetBlobs(dirPath))
            {
                var _blob = _blobClient.GetBlockBlobClient(f);
                var reader = new StreamReader(await _blob.OpenReadAsync());
                var data = await reader.ReadToEndAsync();
                assignments.Add(JsonConvert.DeserializeObject<RoleAssignment>(data));
            }

            //get the wildcard scope...
            dirPath = $"{GetFilePath(scope, true)}";

            foreach (string f in await GetBlobs(dirPath))
            {
                var _blob = _blobClient.GetBlockBlobClient(f);
                var reader = new StreamReader(await _blob.OpenReadAsync());
                var data = await reader.ReadToEndAsync();
                assignments.Add(JsonConvert.DeserializeObject<RoleAssignment>(data));
            }

            //get the top wildcard scope...
            dirPath = $"{_path}/All";

            foreach (string f in await GetBlobs(dirPath))
            {
                var _blob = _blobClient.GetBlockBlobClient(f);
                var reader = new StreamReader(await _blob.OpenReadAsync());
                var data = await reader.ReadToEndAsync();
                assignments.Add(JsonConvert.DeserializeObject<RoleAssignment>(data));
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

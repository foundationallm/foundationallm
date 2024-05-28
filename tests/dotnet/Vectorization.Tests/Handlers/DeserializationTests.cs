using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Vectorization.Models.Resources;
using System.Text.Json;

namespace Vectorization.Tests.Handlers
{
    public class DeserializationTests
    {
        [Fact]
        public async void TestDeserializeVectorizationStore()
        {
            string json = @"
			{
				""Resources"": [
					{
						""type"": null,
						""name"": ""Test"",
						""object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/vectorizationPipelines/Test"",
						""display_name"": null,
						""description"": ""Vectorization data pipeline dedicated to the Test agent."",
						""active"": false,
						""data_source_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.DataSource/dataSources/AzureDataLake"",
						""text_partitioning_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/Test"",
						""text_embedding_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/AzureOpenAI_Embedding"",
						""indexing_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/indexingProfiles/Fabric-Conf"",
						""trigger_type"": ""Event"",
						""trigger_cron_schedule"": null,
						""created_on"": ""0001-01-01T00:00:00+00:00"",
						""updated_on"": ""0001-01-01T00:00:00+00:00"",
						""created_by"": null,
						""updated_by"": null,
						""deleted"": false
					},
					{
						""type"": null,
						""name"": ""KMTest"",
						""object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/vectorizationPipelines/KMTest"",
						""display_name"": null,
						""description"": ""Vectorization data pipeline dedicated to the KMTest agent."",
						""active"": false,
						""data_source_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.DataSource/dataSources/datalake02"",
						""text_partitioning_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/KMTest"",
						""text_embedding_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/AzureOpenAI_Embedding"",
						""indexing_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/indexingProfiles/Fabric-Conf"",
						""trigger_type"": ""Event"",
						""trigger_cron_schedule"": null,
						""created_on"": ""0001-01-01T00:00:00+00:00"",
						""updated_on"": ""0001-01-01T00:00:00+00:00"",
						""created_by"": null,
						""updated_by"": null,
						""deleted"": false
					},
					{
						""type"": null,
						""name"": ""sc-test-1"",
						""object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/vectorizationPipelines/sc-test-1"",
						""display_name"": null,
						""description"": ""Vectorization data pipeline dedicated to the sc-test-1 agent."",
						""active"": false,
						""data_source_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.DataSource/dataSources/AzureDataLake"",
						""text_partitioning_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/sc-test-1"",
						""text_embedding_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/AzureOpenAI_Embedding"",
						""indexing_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/indexingProfiles/Fabric-Conf"",
						""trigger_type"": ""Event"",
						""trigger_cron_schedule"": null,
						""created_on"": ""0001-01-01T00:00:00+00:00"",
						""updated_on"": ""0001-01-01T00:00:00+00:00"",
						""created_by"": null,
						""updated_by"": null,
						""deleted"": false
					},
					{
						""type"": null,
						""name"": ""sc-test-2"",
						""object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/vectorizationPipelines/sc-test-2"",
						""display_name"": null,
						""description"": ""Vectorization data pipeline dedicated to the sc-test-2 agent."",
						""active"": false,
						""data_source_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.DataSource/dataSources/AzureDataLake"",
						""text_partitioning_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/sc-test-2"",
						""text_embedding_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/AzureOpenAI_Embedding"",
						""indexing_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/indexingProfiles/Fabric-Conf"",
						""trigger_type"": ""Event"",
						""trigger_cron_schedule"": null,
						""created_on"": ""0001-01-01T00:00:00+00:00"",
						""updated_on"": ""0001-01-01T00:00:00+00:00"",
						""created_by"": null,
						""updated_by"": null,
						""deleted"": false
					},
					{
						""type"": null,
						""name"": ""FabricCon01"",
						""object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/vectorizationPipelines/FabricCon01"",
						""display_name"": null,
						""description"": ""Vectorization data pipeline dedicated to the FabricCon01 agent."",
						""active"": false,
						""data_source_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.DataSource/dataSources/AzureDataLake"",
						""text_partitioning_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/FabricCon01"",
						""text_embedding_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/AzureOpenAI_Embedding"",
						""indexing_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/indexingProfiles/Fabric-Conf"",
						""trigger_type"": ""Event"",
						""trigger_cron_schedule"": null,
						""created_on"": ""0001-01-01T00:00:00+00:00"",
						""updated_on"": ""0001-01-01T00:00:00+00:00"",
						""created_by"": null,
						""updated_by"": null,
						""deleted"": false
					},
					{
						""type"": null,
						""name"": ""CJ-Test-01"",
						""object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/vectorizationPipelines/CJ-Test-01"",
						""display_name"": null,
						""description"": ""Vectorization data pipeline dedicated to the CJ-Test-01 agent."",
						""active"": false,
						""data_source_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.DataSource/dataSources/AzureDataLake"",
						""text_partitioning_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/CJ-Test-01"",
						""text_embedding_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/AzureOpenAI_Embedding"",
						""indexing_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/indexingProfiles/Fabric-Conf"",
						""trigger_type"": ""Event"",
						""trigger_cron_schedule"": null,
						""created_on"": ""0001-01-01T00:00:00+00:00"",
						""updated_on"": ""0001-01-01T00:00:00+00:00"",
						""created_by"": null,
						""updated_by"": null,
						""deleted"": false
					},
					{
						""type"": ""vectorization-pipeline"",
						""name"": ""MSDFGM"",
						""object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/vectorizationPipelines/MSDFGM"",
						""display_name"": null,
						""description"": ""Vectorization data pipeline dedicated to the MSDFGM agent."",
						""active"": false,
						""data_source_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.DataSource/dataSources/msdfgm"",
						""text_partitioning_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/DefaultTokenTextPartition_Small"",
						""text_embedding_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/AzureOpenAI_Embedding_Gateway"",
						""indexing_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/indexingProfiles/AzureAISearch_MSDFGM"",
						""trigger_type"": ""Event"",
						""trigger_cron_schedule"": null,
						""created_on"": ""0001-01-01T00:00:00+00:00"",
						""updated_on"": ""0001-01-01T00:00:00+00:00"",
						""created_by"": null,
						""updated_by"": null,
						""deleted"": false
					},
					{
						""type"": null,
						""name"": ""CJ-Test-02"",
						""object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/vectorizationPipelines/CJ-Test-02"",
						""display_name"": null,
						""description"": ""Vectorization data pipeline dedicated to the CJ-Test-02 agent."",
						""active"": false,
						""data_source_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.DataSource/dataSources/AzureDataLake"",
						""text_partitioning_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/CJ-Test-02"",
						""text_embedding_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/AzureOpenAI_Embedding"",
						""indexing_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/indexingProfiles/Fabric-Conf"",
						""trigger_type"": ""Event"",
						""trigger_cron_schedule"": null,
						""created_on"": ""0001-01-01T00:00:00+00:00"",
						""updated_on"": ""0001-01-01T00:00:00+00:00"",
						""created_by"": null,
						""updated_by"": null,
						""deleted"": false
					},
					{
						""type"": ""vectorization-pipeline"",
						""name"": ""Fabric-Conf-03"",
						""object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/vectorizationPipelines/Fabric-Conf-03"",
						""display_name"": null,
						""description"": ""Vectorization data pipeline dedicated to the Fabric-Conf-03 agent."",
						""active"": false,
						""data_source_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.DataSource/dataSources/AzureDataLake"",
						""text_partitioning_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/Fabric-Conf-03"",
						""text_embedding_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/AzureOpenAI_Embedding"",
						""indexing_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/indexingProfiles/Fabric-Conf"",
						""trigger_type"": ""Event"",
						""trigger_cron_schedule"": null,
						""created_on"": ""0001-01-01T00:00:00+00:00"",
						""updated_on"": ""0001-01-01T00:00:00+00:00"",
						""created_by"": null,
						""updated_by"": null,
						""deleted"": false
					},
					{
						""type"": ""vectorization-pipeline"",
						""name"": ""Fabric-Conf-01"",
						""object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/vectorizationPipelines/Fabric-Conf-01"",
						""display_name"": null,
						""description"": ""Vectorization data pipeline dedicated to the Fabric-Conf-01 agent."",
						""active"": false,
						""data_source_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.DataSource/dataSources/AzureDataLake"",
						""text_partitioning_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/Fabric-Conf-01"",
						""text_embedding_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/AzureOpenAI_Embedding"",
						""indexing_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/indexingProfiles/Fabric-Conf"",
						""trigger_type"": ""Event"",
						""trigger_cron_schedule"": null,
						""created_on"": ""0001-01-01T00:00:00+00:00"",
						""updated_on"": ""0001-01-01T00:00:00+00:00"",
						""created_by"": null,
						""updated_by"": null,
						""deleted"": false
					},
					{
						""type"": ""vectorization-pipeline"",
						""name"": ""Fabric-Conf-02"",
						""object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/vectorizationPipelines/Fabric-Conf-02"",
						""display_name"": null,
						""description"": ""Vectorization data pipeline dedicated to the Fabric-Conf-02 agent."",
						""active"": false,
						""data_source_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.DataSource/dataSources/AzureDataLake"",
						""text_partitioning_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/Fabric-Conf-02"",
						""text_embedding_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/AzureOpenAI_Embedding"",
						""indexing_profile_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/indexingProfiles/Fabric-Conf"",
						""trigger_type"": ""Schedule"",
						""trigger_cron_schedule"": ""* 5 * * * *"",
						""created_on"": ""0001-01-01T00:00:00+00:00"",
						""updated_on"": ""0001-01-01T00:00:00+00:00"",
						""created_by"": null,
						""updated_by"": null,
						""deleted"": false
					}
				],
				""DefaultResourceName"": null
			}
				

            ";
			ResourceStore<VectorizationPipeline> store = JsonSerializer.Deserialize<ResourceStore<VectorizationPipeline>>(json);
            Assert.Equal(11, store.Resources.Count);
        }
        [Fact]
        public async void TestDeserializeVectorizationRequest()
        {
            string json = @"
            {
                ""id"": ""c15a3ac5-b540-4466-8b9b-fc4cbf94b71c"",
                ""object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/vectorizationRequests/c15a3ac5-b540-4466-8b9b-fc4cbf94b71c"",
                ""expired"": false,
                ""resource_filepath"": ""requests/20240426/20240426-c15a3ac5-b540-4466-8b9b-fc4cbf94b71c.json"",
                ""content_identifier"": {
                    ""data_source_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.DataSource/dataSources/msdfgm"",
                    ""multipart_id"": [
                        ""dbo"",
                        ""File"",
                        ""FileContents"",
                        ""FileID"",
                        ""350C3136-DEC8-41A3-9375-DF131DFFC6B0"",
                        ""mydoc.docx""
                    ],
                    ""canonical_id"": ""msdfgm/dbo/File/FileContents/FileID/350C3136-DEC8-41A3-9375-DF131DFFC6B0/mydoc.docx""
                },
                ""processing_type"": ""Asynchronous"",
                ""pipeline_object_id"": ""/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/vectorizationPipelines/MSDFGM"",
                ""pipeline_execution_id"": ""19fd589e-fcfb-4b1c-997c-8456227a55a7"",
                ""processing_state"": ""New"",
                ""error_messages"": [],
                ""steps"": [
                    {
                        ""id"": ""extract"",
                        ""parameters"": {}
                    },
                    {
                        ""id"": ""partition"",
                        ""parameters"": {
                            ""text_partitioning_profile_name"": ""DefaultTokenTextPartition_Small""
                        }
                    },
                    {
                        ""id"": ""embed"",
                        ""parameters"": {
                            ""text_embedding_profile_name"": ""AzureOpenAI_Embedding_Gateway""
                        }
                    },
                    {
                        ""id"": ""index"",
                        ""parameters"": {
                            ""indexing_profile_name"": ""AzureAISearch_MSDFGM""
                        }
                    }
                ],
                ""completed_steps"": [
                    ""extract"",
                    ""partition""
                ],
                ""remaining_steps"": [
                    ""embed"",
                    ""index""
                ],
                ""current_step"": ""embed"",
                ""error_count"": 0,
                ""running_operations"": {
                    ""embed"": {
                        ""operation_id"": ""edd1197e-43c6-4eaf-aa8b-51eecb67642b"",
                        ""first_response_time"": ""2024-04-26T21:23:15.4092786Z"",
                        ""last_response_time"": ""2024-04-26T21:23:15.4092786Z"",
                        ""complete"": false,
                        ""pollingCount"": 0
                    }
                },
                ""last_successful_step_time"": ""2024-04-26T21:22:14.9690748Z""
            }
            ";

            VectorizationRequest request = JsonSerializer.Deserialize<VectorizationRequest>(json);
            Assert.Equal("c15a3ac5-b540-4466-8b9b-fc4cbf94b71c", request.Name);
        }
    }
}

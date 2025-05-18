'''
Management API client for FoundationLLM.
This client is used to interact with the FoundationLLM Management API.
'''

from .api_client import APIClient

class ManagementClient(APIClient):
    '''
    Management API client for FoundationLLM.
    This client is used to interact with the FoundationLLM Management API.
    '''

    def __init__(
        self,
        api_scope: str,
        api_endpoint: str,
        foundationallm_instance_id: str):

        super().__init__(
            api_scope,
            api_endpoint,
            foundationallm_instance_id
        )

        self.default_text_partitioning_profile = "text_partition_default"
        self.default_text_embedding_profile = "text_embedding_profile_gateway_embedding3large"

    # region: Agents

    # region: Agent workflows

    def get_agent_workflows(self) -> dict:
        '''
        Retrieves the list of agent workflows.
        '''

        result = self.get_request(
            "providers/FoundationaLLM.Agent/workflows"
        )

        return result

    # endregion

    # endregion

    # region: Vectorization pipelines

    def create_vectorization_pipeline(
        self,
        pipeline_name: str,
        pipeline_description: str,
        data_source_name: str,
        vector_store_name: str
    ) -> dict:
        '''
        Creates a vectorization pipeline.
        - pipeline_name: The name of the pipeline.
        - pipeline_description: The description of the pipeline.
        - data_source_name: The name of the data source.
        - vector_store_name: The name of the vector store.
        '''
        
        result = self.post_request(
            f"providers/FoundationaLLM.Vectorization/vectorizationPipelines/{pipeline_name}",
            {
                "name": pipeline_name,
                "description": pipeline_description,
                "data_source_object_id": f"/instances/{self.__foundationallm_instance_id}/providers/FoundationaLLM.DataSource/dataSources/{data_source_name}",
                "text_partitioning_profile_object_id": f"/instances/{self.__foundationallm_instance_id}/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/{self.default_text_partitioning_profile}",
                "text_embedding_profile_object_id": f"/instances/{self.__foundationallm_instance_id}/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/{self.default_text_embedding_profile}",
                "indexing_profile_object_id": f"/instances/{self.__foundationallm_instance_id}/providers/FoundationaLLM.Vectorization/indexingProfiles/{vector_store_name}",
                "trigger_type": "Manual"
            }
        )

        return result

    def get_vectorization_pipeline(
        self,
        pipeline_name: str
    ) -> dict:
        '''
        Retrieves a vectorization pipeline.
        - pipeline_name: The name of the pipeline.
        '''

        result = self.get_request(
            f"providers/FoundationaLLM.Vectorization/vectorizationPipelines/{pipeline_name}"
        )

        return result

    def activate_vectorization_pipeline(
        self,
        pipeline_name: str
    ) -> dict:
        '''
        Activates a vectorization pipeline.
        - pipeline_name: The name of the pipeline.
        '''

        result = self.post_request(
            f"providers/FoundationaLLM.Vectorization/vectorizationPipelines/{pipeline_name}/activate",
            {}
        )

        return result

    def get_vectorization_pipeline_execution(
        self,
        pipeline_name: str,
        execution_id: str
    ) -> dict:
        '''
        Retrieves a vectorization pipeline execution.
        - pipeline_name: The name of the pipeline.
        - execution_id: The ID of the execution.
        '''

        result = self.get_request(
            f"providers/FoundationaLLM.Vectorization/vectorizationPipelines/{pipeline_name}/vectorizationPipelineExecutions/{execution_id}"
        )

        return result
    
    # endregion

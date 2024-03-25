"""
Class: ResourceProvider
Description:
    Responsible for retrieving resources.
    Supporting:
        - FoundationaLLM.Prompt
        - FoundationaLLM.Vectorization.indexingprofiles
        - FoundationaLLM.Vectorization.textembeddingprofiles
"""
import re
import json
from foundationallm.config import Configuration
from foundationallm.storage import BlobStorageManager
from foundationallm.models.resource_providers.prompts import Prompt
from foundationallm.models.resource_providers.vectorization import (
    AzureAISearchIndexingProfile,
    AzureOpenAIEmbeddingProfile
)

class ResourceProvider:
    """
    Responsible for read-only access to resource metadata.
    """
    def __init__(
            self,
            config: Configuration
            ):
        self.blob_storage_manager = BlobStorageManager(
            blob_connection_string=config.get_value(
                "FoundationaLLM:Vectorization:ResourceProviderService:Storage:ConnectionString"
            ), 
            container_name="resource-provider"
        )

    def get_resource(self, object_id:str):
        """
        Factory method that returns the concrete object of the resource
        with the given object id.

        If a concrete object is not found, the method returns a dictionary
        of the resource configuration (or None if the resource is not found).
        """
        if object_id is not None and object_id != '':
            obj_dict = self.get_resource_as_dict(object_id)
        
            if obj_dict is not None:
                tokens = object_id.split("/")
                # the last token is resource
                resource = tokens[-1]
                # the second to last token is resource type
                resource_type = tokens[-2]
                # the third to last token is the resource provider type
                provider_type = tokens[-3]

                # match case on resource type
                match provider_type:
                    case "FoundationaLLM.Prompt":
                        prompt_resource = Prompt(**obj_dict)
                        return prompt_resource
                    case "FoundationaLLM.Vectorization":                
                        if resource_type == "indexingProfiles":
                            if obj_dict["indexer"]=="AzureAISearchIndexer":
                                indexing_resource = AzureAISearchIndexingProfile(**obj_dict)
                                return indexing_resource
                        elif resource_type == "textEmbeddingProfiles":
                            if obj_dict["text_embedding"]=="SemanticKernelTextEmbedding":
                                embedding_resource = AzureOpenAIEmbeddingProfile(**obj_dict)
                                return embedding_resource

            return obj_dict
        return None
    
    def get_resource_as_dict(self, object_id:str):
        """
        Retrieves the resource with the given object id.

        Parameters
        ----------
        object_id : str
            The id of the resource to retrieve.

        Returns
        -------
        Any
            The resource with the given id.
        """
        if object_id is None or object_id == '':
            return None
        
        tokens = object_id.split("/")
        # the last token is resource
        resource = tokens[-1]
        # the second to last token is resource type
        resource_type = tokens[-2]
        # the third to last token is the resource provider type
        provider_type = tokens[-3]

        # match case on resource type
        match provider_type:
            case "FoundationaLLM.Prompt":
                # return the content of the referenced prompt file
                full_path = f"{provider_type}/{resource}.json"
                file_content = self.blob_storage_manager.read_file_content(full_path)
                if file_content is not None:
                    return json.loads(file_content.decode("utf-8"))
                
            case "FoundationaLLM.Vectorization":
                full_path = None
                if resource_type == "indexingProfiles":
                    full_path = f"{provider_type}/vectorization-indexing-profiles.json"

                elif resource_type == "textEmbeddingProfiles":
                    full_path = f"{provider_type}/vectorization-text-embedding-profiles.json"

                if full_path is not None:
                    file_content = self.blob_storage_manager.read_file_content(full_path)
                    if file_content is not None:
                        decoded_content = file_content.decode("utf-8")
                        profiles = json.loads(decoded_content).get("Profiles", [])
                        filtered = next(filter(lambda profile: profile.get("name","") == resource, profiles), None)
                        if filtered is not None:
                            filtered = self.__translate_keys(filtered)
                            return filtered
        return None

    def __pascal_to_snake(self, name):  
        # Convert PascalCase or camelCase to snake_case  
        s1 = re.sub('(.)([A-Z][a-z]+)', r'\1_\2', name)  
        return re.sub('([a-z0-9])([A-Z])', r'\1_\2', s1).lower()  
  
    def __translate_keys(self, obj):  
        if isinstance(obj, dict):  
            new_dict = {}  
            for key, value in obj.items():  
                new_key = self.__pascal_to_snake(key)  
                new_dict[new_key] = self.__translate_keys(value)  # Recursively apply to values  
            return new_dict  
        elif isinstance(obj, list):  
            return [self.__translate_keys(item) for item in obj]  # Apply to each item in the list  
        else:  
            return obj  # Return the item itself if it's not a dict or list 

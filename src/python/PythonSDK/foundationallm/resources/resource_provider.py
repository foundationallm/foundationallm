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
        authentication_type = 'AzureIdentity'
        try:
            authentication_type = config.get_value(
                "FoundationaLLM:Vectorization:ResourceProviderService:Storage:AuthenticationType"
            )
        except:
            # Default to AzureIdentity for authentication
            authentication_type = 'AzureIdentity'

        if authentication_type == 'AzureIdentity':
            account_name = None
            try:
                account_name = config.get_value(
                    "FoundationaLLM:Vectorization:ResourceProviderService:Storage:AccountName"
                )
            except:
                raise ValueError('The authentication type is set to AzureIdentity. Therefore, the FoundationaLLM:Vectorization:ResourceProviderService:Storage:AccountName app configuration setting must be set to a valid account name.')

            self.blob_storage_manager = BlobStorageManager(
                account_name=account_name,
                container_name="resource-provider",
                authentication_type='AzureIdentity'
            )
        elif authentication_type == 'ConnectionString':
            blob_connection_string = None
            try:
                blob_connection_string = config.get_value(
                    "FoundationaLLM:Vectorization:ResourceProviderService:Storage:ConnectionString"
                )
            except:
                raise ValueError('The authentication type is set to ConnectionString. Therefore, the FoundationaLLM:Vectorization:ResourceProviderService:Storage:ConnectionString app configuration setting must be set to a valid connection string.')

            self.blob_storage_manager = BlobStorageManager(
                blob_connection_string=blob_connection_string, 
                container_name="resource-provider",
                authentication_type='ConnectionString'
            )
        else:
            raise ValueError(f'The authentication type {authentication_type} is not supported.')

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
                        if resource_type.lower() == "indexingprofiles":
                            if obj_dict["indexer"]=="AzureAISearchIndexer":
                                indexing_resource = AzureAISearchIndexingProfile(**obj_dict)
                                return indexing_resource
                        elif resource_type.lower() == "textembeddingprofiles":
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
                if resource_type.lower() == "indexingprofiles":
                    full_path = f"{provider_type}/vectorization-indexing-profiles.json"
                elif resource_type.lower() == "textembeddingprofiles":
                    full_path = f"{provider_type}/vectorization-text-embedding-profiles.json"

                if full_path is not None:
                    file_content = self.blob_storage_manager.read_file_content(full_path)
                    if file_content is not None:
                        decoded_content = file_content.decode("utf-8")
                        resources = json.loads(decoded_content).get("Resources", [])
                        filtered = next(filter(lambda profile: profile.get("name","") == resource, resources), None)
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

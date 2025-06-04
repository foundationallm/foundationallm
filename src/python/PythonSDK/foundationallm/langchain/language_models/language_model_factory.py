import boto3
import botocore
import json
from azure.core.credentials import AzureKeyCredential
from azure.identity import DefaultAzureCredential, get_bearer_token_provider
from google.oauth2 import service_account
from langchain_azure_ai.chat_models import AzureAIChatCompletionsModel
from langchain_core.language_models import BaseLanguageModel
from langchain_aws import ChatBedrockConverse
from langchain_google_vertexai import ChatVertexAI
from langchain_openai import AzureChatOpenAI, ChatOpenAI, OpenAI
from openai import AsyncAzureOpenAI as async_aoi
from foundationallm.config import Configuration
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.models.authentication import AuthenticationTypes
from foundationallm.models.language_models import LanguageModelProvider
from foundationallm.models.operations import OperationTypes
from foundationallm.models.resource_providers.ai_models import AIModelBase
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.utils import ObjectUtils

class LanguageModelFactory:

    def __init__(self, objects:dict, config: Configuration):
        self.objects = objects
        self.config = config

    def get_language_model(self,
                           ai_model_object_id:str,
                           override_operation_type: OperationTypes = None,
                           agent_model_parameter_overrides:dict = None,
                          ) -> BaseLanguageModel:
        """
        Create a language model using the specified endpoint settings.

        override_operation_type : OperationTypes - internally override the operation type for the API endpoint.

        Returns
        -------
        BaseLanguageModel
            Returns an API connector for a chat completion model.
        """
        language_model:BaseLanguageModel = None
        api_key = None

        ai_model = ObjectUtils.get_object_by_id(ai_model_object_id, self.objects, AIModelBase)
        if ai_model is None:
            raise LangChainException("AI model configuration settings are missing.", 400)

        api_endpoint = ObjectUtils.get_object_by_id(ai_model.endpoint_object_id, self.objects, APIEndpointConfiguration)
        if api_endpoint is None:
            raise LangChainException("API endpoint configuration settings are missing.", 400)

        match api_endpoint.provider:
            case LanguageModelProvider.AZUREAI:
                if api_endpoint.authentication_type == AuthenticationTypes.AZURE_IDENTITY:
                    try:
                        scope = api_endpoint.authentication_parameters.get('scope', 'https://cognitiveservices.azure.com/.default')
                        credential = DefaultAzureCredential(exclude_environment_credential=True)
                        language_model = AzureAIChatCompletionsModel(
                            endpoint=api_endpoint.url,
                            credential=credential,
                            model_name=ai_model.deployment_name,
                            client_kwargs={
                                "credential_scopes": [scope],
                                "timeout": api_endpoint.timeout_seconds
                            }
                        )
                        if api_endpoint.api_version is not None:
                            language_model.api_version = api_endpoint.api_version

                    except Exception as e:
                        raise LangChainException(f"Failed to create Azure OpenAI API connector: {str(e)}", 500)
                else: # Key-based authentication
                    try:
                        api_key = self.config.get_value(api_endpoint.authentication_parameters.get('api_key_configuration_name'))
                    except Exception as e:
                        raise LangChainException(f"Failed to retrieve API key: {str(e)}", 500)

                    if api_key is None:
                        raise LangChainException("API key is missing from the configuration settings.", 400)
                    credential = AzureKeyCredential(api_key)
                    language_model = AzureAIChatCompletionsModel(
                        endpoint=api_endpoint.url,
                        credential = credential,
                        model_name=ai_model.deployment_name,
                        client_kwargs={
                            "timeout": api_endpoint.timeout_seconds
                        }
                    )
                    if api_endpoint.api_version is not None:
                            language_model.api_version = api_endpoint.api_version
            case LanguageModelProvider.MICROSOFT:
                op_type = api_endpoint.operation_type
                if override_operation_type is not None:
                    op_type = override_operation_type
                if api_endpoint.authentication_type == AuthenticationTypes.AZURE_IDENTITY:
                    try:
                        scope = api_endpoint.authentication_parameters.get('scope', 'https://cognitiveservices.azure.com/.default')
                        # Set up a Azure AD token provider.
                        token_provider = get_bearer_token_provider(
                            DefaultAzureCredential(exclude_environment_credential=True),
                            scope
                        )

                        if op_type == OperationTypes.CHAT:
                            language_model = AzureChatOpenAI(
                                azure_endpoint=api_endpoint.url,
                                api_version=api_endpoint.api_version,
                                openai_api_type='azure_ad',
                                azure_ad_token_provider=token_provider,
                                azure_deployment=ai_model.deployment_name,
                                request_timeout=api_endpoint.timeout_seconds
                            )
                        elif op_type == OperationTypes.ASSISTANTS_API or op_type == OperationTypes.IMAGE_SERVICES:
                            # Assistants API clients can't have deployment as that is assigned at the assistant level.
                            language_model = async_aoi(
                                azure_endpoint=api_endpoint.url,
                                api_version=api_endpoint.api_version,
                                azure_ad_token_provider=token_provider,
                                timeout=api_endpoint.timeout_seconds
                            )
                        else:
                            raise LangChainException(f"Unsupported operation type: {op_type}", 400)

                    except Exception as e:
                        raise LangChainException(f"Failed to create Azure OpenAI API connector: {str(e)}", 500)
                else: # Key-based authentication
                    try:
                        api_key = self.config.get_value(api_endpoint.authentication_parameters.get('api_key_configuration_name'))
                    except Exception as e:
                        raise LangChainException(f"Failed to retrieve API key: {str(e)}", 500)

                    if api_key is None:
                        raise LangChainException("API key is missing from the configuration settings.", 400)

                    if op_type == OperationTypes.CHAT:
                        language_model = AzureChatOpenAI(
                            azure_endpoint=api_endpoint.url,
                            api_key=api_key,
                            api_version=api_endpoint.api_version,
                            azure_deployment=ai_model.deployment_name,
                            request_timeout=api_endpoint.timeout_seconds
                        )
                    elif op_type == OperationTypes.ASSISTANTS_API or op_type == OperationTypes.IMAGE_SERVICES:
                        # Assistants API clients can't have deployment as that is assigned at the assistant level.
                        language_model = async_aoi(
                            azure_endpoint=api_endpoint.url,
                            api_key=api_key,
                            api_version=api_endpoint.api_version,
                            timeout=api_endpoint.timeout_seconds
                        )
                    else:
                        raise LangChainException(f"Unsupported operation type: {op_type}", 400)
            case LanguageModelProvider.OPENAI:
                try:
                    api_key = self.config.get_value(api_endpoint.authentication_parameters.get('api_key_configuration_name'))
                except Exception as e:
                    raise LangChainException(f"Failed to retrieve API key: {str(e)}", 500)

                if api_key is None:
                    raise LangChainException("API key is missing from the configuration settings.", 400)

                language_model = (
                    ChatOpenAI(base_url=api_endpoint.url, api_key=api_key, request_timeout=api_endpoint.timeout_seconds)
                    if api_endpoint.operation_type == OperationTypes.CHAT
                    else OpenAI(base_url=api_endpoint.url, api_key=api_key, request_timeout=api_endpoint.timeout_seconds)
                )
            case LanguageModelProvider.BEDROCK:
                boto3_config = botocore.config.Config(connect_timeout=60, read_timeout=api_endpoint.timeout_seconds)

                if api_endpoint.authentication_type == AuthenticationTypes.AZURE_IDENTITY:
                    # Get Azure scope for federated authentication as well as the AWS role ARN (Amazon Resource Name).
                    try:
                        scope = self.config.get_value(api_endpoint.authentication_parameters.get('scope'))
                    except Exception as e:
                        raise LangChainException(f"Failed to retrieve scope: {str(e)}", 500)

                    if scope is None:
                        raise LangChainException("Scope is missing from the configuration settings.", 400)

                    try:
                        role_arn = self.config.get_value(api_endpoint.authentication_parameters.get('role_arn'))
                    except Exception as e:
                        raise LangChainException(f"Failed to retrieve Role ARN: {str(e)}", 500)

                    if role_arn is None:
                        raise LangChainException("Role ARN is missing from the configuration settings.", 400)

                    # Get Azure token for designated scope.
                    az_creds = DefaultAzureCredential(exclude_environment_credential=True)
                    azure_token = az_creds.get_token(scope)

                    # Get AWS STS credentials using Azure token.
                    sts_client = boto3.client('sts')
                    sts_response = sts_client.assume_role_with_web_identity(
                        RoleArn=role_arn,
                        RoleSessionName='assume-role',
                        WebIdentityToken=azure_token.token
                    )
                    creds = sts_response['Credentials']

                    # parse region from the URL, ex: https://bedrock-runtime.us-east-1.amazonaws.com/
                    region = api_endpoint.url.split('.')[1]
                    language_model = ChatBedrockConverse(
                        model= ai_model.deployment_name,
                        region_name = region,
                        aws_access_key_id = creds["AccessKeyId"],
                        aws_secret_access_key = creds["SecretAccessKey"],
                        aws_session_token= creds["SessionToken"],
                        config=boto3_config
                    )
                else: # Key-based authentication
                    try:
                        access_key = self.config.get_value(api_endpoint.authentication_parameters.get('access_key'))
                    except Exception as e:
                        raise LangChainException(f"Failed to retrieve access key: {str(e)}", 500)

                    if access_key is None:
                        raise LangChainException("Access key is missing from the configuration settings.", 400)

                    try:
                        secret_key = self.config.get_value(api_endpoint.authentication_parameters.get('secret_key'))
                    except Exception as e:
                        raise LangChainException(f"Failed to retrieve secret key: {str(e)}", 500)

                    if secret_key is None:
                        raise LangChainException("Secret key is missing from the configuration settings.", 400)

                    # parse region from the URL, ex: https://bedrock-runtime.us-east-1.amazonaws.com/
                    region = api_endpoint.url.split('.')[1]
                    language_model = ChatBedrockConverse(
                        model= ai_model.deployment_name,
                        region_name = region,
                        aws_access_key_id = access_key,
                        aws_secret_access_key = secret_key,
                        config=boto3_config
                    )
            case LanguageModelProvider.VERTEXAI:
                # Only supports service account authentication via JSON credentials stored in key vault.
                # Uses the authentication parameter: service_account_credentials to get the application configuration key for this value.
                try:
                    service_account_credentials_definition = json.loads(self.config.get_value(api_endpoint.authentication_parameters.get('service_account_credentials')))
                except Exception as e:
                    raise LangChainException(f"Failed to retrieve service account credentials: {str(e)}", 500)

                if not service_account_credentials_definition:
                    raise LangChainException("Service account credentials are missing from the configuration settings.", 400)

                service_account_credentials = service_account.Credentials.from_service_account_info(service_account_credentials_definition)
                language_model = ChatVertexAI(
                    model=ai_model.deployment_name,
                    temperature=0,
                    max_tokens=None,
                    max_retries=6,
                    stop=None,
                    credentials=service_account_credentials
                )

        # Set model parameters.
        for key, value in ai_model.model_parameters.items():
            if hasattr(language_model, key):
                setattr(language_model, key, value)

        # Set agent model overrides.
        if agent_model_parameter_overrides is not None:
            for key, value in agent_model_parameter_overrides.items():
                if hasattr(language_model, key):
                    setattr(language_model, key, value)

        return language_model

export type ResourceProviderGetResult<T> = {
	/**
	 * Represents the result of a fetch operation.
	 */
	resource: T;

	/**
	 * List of authorized actions on the resource.
	 */
	actions: string[];

	/**
	 * List of roles on the resource.
	 */
	roles: string[];
};

export interface ContentArtifact {
	id: string;
	title: string;
	filepath: string;
}

export interface ResourceProviderUpsertResult {
	object_id: string;
	resource_exists: boolean;
}

export interface MessageContent {
	type: string;
	fileName: string;
	value: string;
	blobUrl?: string;
	loading?: boolean;
	error?: boolean;
}

export interface AttachmentDetail {
	objectId: string;
	displayName: string;
	contentType: string;
}

export interface AnalysisResult {
	tool_input: string;
	tool_output: string;
	agent_capability_category: string;
	tool_name: string;
}

export interface Message {
	id: string;
	type: string;
	sessionId: string;
	timeStamp: string;
	sender: 'User' | 'Agent';
	senderDisplayName: string | null;
	tokens: number;
	text: string;
	rating: boolean | null;
	ratingComments: string | null;
	vector: Array<Number>;
	completionPromptId: string | null;
	contentArtifacts: Array<ContentArtifact>;
	content: Array<MessageContent>;
	attachments: Array<string>;
	attachmentDetails: Array<AttachmentDetail>;
	analysisResults: Array<AnalysisResult>;
	processingTime: number; // Calculated in milliseconds - not from the API
}

export interface MessageRatingRequest {
	rating: boolean | null;
	comments: string | null;
}

export interface Session {
	id: string;
	type: string;
	sessionId: string;
	tokensUsed: Number;
	name: string;
	display_name: string;
	messages: Array<Message>;
}

export interface ConversationProperties {
	name: string;
	metadata: string;
}

export interface LongRunningOperation {
	id?: string;
	type: string;
	operation_id?: string;
	status: string;
	status_message?: string;
	last_updated?: Date;
	ttl: number;
	prompt_tokens: number;
	result?: Message;
	isCompleted: boolean;
}

export interface CompletionPrompt {
	id: string;
	type: string;
	sessionId: string;
	messageId: string;
	prompt: string;
}

export interface OrchestrationSettings {
	orchestrator?: string;
	endpoint_configuration?: { [key: string]: any } | null;
	model_parameters?: { [key: string]: any } | null;
}
export interface Agent {
	type: string;
	name: string;
	display_name: string;
	object_id: string;
	description: string;
	properties?: { [key: string]: string | null };
	long_running: boolean;
	orchestration_settings?: OrchestrationSettings;
	show_message_tokens?: boolean;
	show_message_rating?: boolean;
	show_view_prompt?: boolean;
	show_file_upload?: boolean;
}

export interface CompletionRequest {
	session_id?: string;
	user_prompt: string;
	agent_name?: string;
	settings?: OrchestrationSettings;
	attachments?: string[];
	metadata?: { [key: string]: any };
}

export interface Attachment {
	id: string;
	fileName: string;
	sessionId: string;
	contentType: string;
	source: string;
}

export interface ResourceProviderDeleteResult {
	deleted: boolean;
	reason?: string;
}

export interface ResourceProviderDeleteResults {
	[key: string]: ResourceProviderDeleteResult;
}

export interface UserProfile {
	id: string;
	type: string;
	upn: string;
	flags: Record<string, boolean>;
}

export interface FileStoreConnector {
	name: string;
	category: string;
	subcategory: string;
	url: string;
}

export interface CoreConfiguration {
	maxUploadsPerMessage: number;
	fileStoreConnectors?: FileStoreConnector[];
	completionResponsePollingIntervalSeconds: number;
}

export interface OneDriveWorkSchool {
	id: string;
	driveId?: string;
	objectId?: string;
	name?: string;
	mimeType?: string;
	access_token?: string;
}

export interface RateLimitError {
	quota_name: string;
	quota_context: string;
	quota_exceeded: boolean;
	time_until_retry_seconds: number;
}

export interface MessageResponse {
	status: string;
	result?: Message;
	operation_id?: string;
	status_message?: string;
}

export interface ResourceBase {
    type: string;
    name: string;
    object_id: string;
    display_name?: string | null;
    description?: string | null;
    cost_center?: string | null;
    created_on?: string;
    updated_on?: string;
    created_by?: string | null;
    updated_by?: string | null;
    deleted?: boolean;
    expiration_date?: string | null;
}


export interface AIModel extends ResourceBase {
	version?: string;
	deployment_name?: string;
	model_parameters?: Record<string, any>;
	endpoint_object_id?: string;
	properties?: Record<string, any>;
}

export interface KnowledgeManagementAgent {
	AGENT_NAME: string;
	AGENT_DISPLAY_NAME: string;
	AGENT_EXPIRATION_DATE: string;
	AGENT_DESCRIPTION: string;
	AGENT_WELCOME_MESSAGE: string;
}

export interface AgentBase {
  type: string;
  name: string;
  object_id: string;
  display_name: string;
  description: string;
  cost_center: string;
  inline_context: boolean;
  sessions_enabled: boolean;
  text_rewrite_settings: {
    user_prompt_rewrite_enabled: boolean;
    user_prompt_rewrite_settings: {
      user_prompt_rewrite_ai_model_object_id: string | null;
      user_prompt_rewrite_prompt_object_id: string | null;
      user_prompts_window_size: number;
    };
  };
  cache_settings: {
    semantic_cache_enabled: boolean;
    semantic_cache_settings: {
      embedding_ai_model_object_id: string | null;
      embedding_dimensions: number;
      minimum_similarity_threshold: number;
    };
  };
  conversation_history_settings: {
    enabled: boolean;
    max_history: number;
    history_content_artifact_types: string[] | null;
  };
  gatekeeper_settings: {
    use_system_setting: boolean;
    options: any[];
  };
  workflow: {
    type: string;
    name: string;
    package_name: string;
    class_name: string;
    workflow_host: string;
    resource_object_ids: {
      [objectId: string]: {
        object_id: string;
        properties: Record<string, any>;
      };
    };
    properties: Record<string, any> | null;
  };
  tools: Array<{
    name: string;
    description: string;
    category: string;
    package_name: string;
    class_name: string;
    resource_object_ids: {
      [objectId: string]: {
        object_id: string;
        properties: Record<string, any>;
      };
    };
    properties: Record<string, any>;
  }>;
  virtual_security_group_id: string;
  show_message_tokens: boolean;
  show_message_rating: boolean;
  show_view_prompt: boolean;
  show_file_upload: boolean;
  properties: {
    welcome_message: string;
    [key: string]: any;
  };
  created_on: string;
  updated_on: string;
  created_by: string | null;
  updated_by: string;
  deleted: boolean;
  expiration_date: string;
}

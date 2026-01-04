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

	/**
	 * Additional properties associated with the resource.
	 */
	properties: Record<string, unknown>;
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
	id?: string; // Made optional to allow temporary messages
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
	operation_id?: string; // For long-running operations
	status?: string; // For long-running operations
	status_message?: string; // For long-running operations
	renderId?: number; // For temporary messages
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
	metadata: string;
	messages: Array<Message>;
	is_temp?: boolean; // For temporary sessions
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

// --- Agent Workflow Types ---
export interface ResourceObjectId {
	object_id: string;
	properties: {
		object_role?: string;
		[key: string]: any;
	};
}

export interface AgentWorkflow {
	type: string;
	name: string;
	package_name: string;
	class_name: string;
	workflow_host: string;
	resource_object_ids: {
		[objectId: string]: ResourceObjectId;
	};
	properties?: Record<string, any>;
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
	agents: string[];
}

export interface UserProfileUpdateRequest {
	agent_object_id: string;
}

export interface AgentOption {
	object_id?: string;
	name: string;
	display_name?: string | null;
	label: string;
	value?: string;
	type?: string | null;
	description?: string | null;
	enabled: boolean;
	isReadonly?: boolean;
	isFeatured?: boolean;
	isPinnedFeatured?: boolean;
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


export interface ResourceName {
	type: string | null;
	name: string;
}

export interface ResourceBase extends ResourceName {
	object_id?: string;
	display_name?: string | null;
	description?: string | null;
	cost_center?: string | null;
	properties?: Record<string, string>;
	created_on?: string;
	updated_on?: string;
	created_by?: string | null;
	updated_by?: string | null;
	deleted?: boolean;
	expiration_date?: string | null;
	/**
	 * List of authorizable actions that can be inherited by child resources.
	 * This property is automatically preserved when loading and saving resources.
	 */
	inheritable_authorizable_actions?: string[];
}

export interface PromptBase extends ResourceBase {
	type: string | null;
	category?: string | null;
}


export interface AIModel extends ResourceBase {
	version?: string;
	deployment_name?: string;
	model_parameters?: Record<string, any>;
	endpoint_object_id?: string;
	properties?: Record<string, any>;
}

export interface AgentBase extends ResourceBase {
	// Inherited from ResourceBase:
	// type, name, object_id, display_name, description, cost_center, properties,
	// created_on, updated_on, created_by, updated_by, deleted, expiration_date

	inline_context: boolean;
	sessions_enabled: boolean;
	long_running: boolean;
	orchestration_settings?: OrchestrationSettings;
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
	realtime_speech_settings?: RealtimeSpeechSettings | null;
	conversation_history_settings: {
		enabled: boolean;
		max_history: number;
		history_content_artifact_types: string[] | null;
	};
	gatekeeper_settings: {
		use_system_setting: boolean;
		options: any[];
	};
	workflow?: AgentWorkflow;
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
	isReadonly: boolean;
	/**
	 * The primary owner's security principal identifier for this agent.
	 * May be null/undefined if not set.
	 */
	owner_user_id?: string | null;
}

export interface RealtimeSpeechSettings {
	/**
	 * Whether realtime speech is enabled for this agent.
	 */
	enabled: boolean;
	
	/**
	 * Object ID of the realtime speech AI model.
	 * This follows the same pattern as embedding_ai_model_object_id in AgentSemanticCacheSettings.
	 */
	realtime_speech_ai_model_object_id: string;
	
	/**
	 * Stop words that terminate the realtime session.
	 */
	stop_words: string[];
	
	/**
	 * Maximum session duration in seconds (0 = unlimited).
	 */
	max_session_duration_seconds: number;
	
	/**
	 * Whether to show transcriptions in the chat thread.
	 */
	show_transcriptions: boolean;
	
	/**
	 * Whether to include conversation history in session context.
	 */
	include_conversation_history: boolean;
}

export interface RealtimeSpeechConfiguration {
	enabled: boolean;
	stop_words: string[];
	websocket_url?: string;
	voice?: string;
}


// --- Agent Name Availability Check Types ---
export interface ResourceName {
	type: string | null;
	name: string;
}

export interface ResourceNameCheckResult {
	type: string | null;
	name: string;
	status: 'Allowed' | 'NotAllowed';
	message: string | null;
	exists: boolean;
	deleted: boolean;
}

export interface AgentCreationFromTemplateRequest {
	AGENT_NAME: string;
	AGENT_DISPLAY_NAME: string;
	AGENT_EXPIRATION_DATE: string;
	AGENT_DESCRIPTION: string;
	AGENT_WELCOME_MESSAGE: string;
}

// --- MultipartPrompt Type ---
export interface MultipartPrompt extends PromptBase {
	prefix?: string | null;
	suffix?: string | null;
}

// --- Role Assignment Types ---
export interface RoleDefinition extends ResourceBase {
	assignable_scopes: string[];
	permissions: RoleDefinitionPermissions[];
}

export interface RoleDefinitionPermissions {
	actions: string[];
	not_actions: string[];
	data_actions: string[];
	not_data_actions: string[];
}

export interface RoleAssignment extends ResourceBase {
	role_definition_id: string;
	principal_id: string;
	principal_type: 'User' | 'Group' | 'ServicePrincipal' | 'ManagedIdentity';
	scope: string;
	scope_name?: string;
	role_definition?: RoleDefinition;
	allowed_actions?: string[];
}

export interface SecurityPrincipal extends ResourceBase {
	id: string;
	email?: string;
}

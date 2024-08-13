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

export interface Citation {
	id: string;
	title: string;
	filepath: string;
}

export interface ResourceProviderUpsertResult {
	objectId: string;
	resourceExists: boolean;
}

export interface Message {
	id: string;
	type: string;
	sessionId: string;
	timeStamp: string;
	sender: 'User' | 'Assistant';
	senderDisplayName: string | null;
	tokens: number;
	text: string;
	rating: boolean | null;
	vector: Array<Number>;
	completionPromptId: string | null;
	citations: Array<Citation>;
	content: Array<MessageContent>;
	attachments: Array<string>;
	attachmentDetails: Array<AttachmentDetail>;
}

export interface MessageContent {
	type: string;
	fileName: string;
	value: string;
}

export interface AttachmentDetail {
	objectId: string;
	displayName: string;
	contentType: string;
}

export interface Session {
	id: string;
	type: string;
	sessionId: string;
	tokensUsed: Number;
	name: string;
	messages: Array<Message>;
}

export interface ChatSessionProperties {
	name: string;
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
	object_id: string;
	description: string;
	long_running: boolean;
	orchestration_settings?: OrchestrationSettings;
}

export interface CompletionRequest {
	session_id?: string;
	user_prompt: string;
	agent_name?: string;
	settings?: OrchestrationSettings;
	attachments?: string[];
}

export interface Attachment {
	id: string;
	fileName: string;
	sessionId: string;
}

export interface ResourceProviderDeleteResult {
    deleted: boolean;
  	reason?: string;
}

export interface ResourceProviderDeleteResults {
	[key: string]: ResourceProviderDeleteResult;
}

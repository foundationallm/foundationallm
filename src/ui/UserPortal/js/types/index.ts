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
}

export interface Session {
	id: string;
	type: string;
	sessionId: string;
	tokensUsed: Number;
	name: string;
	messages: Array<Message>;
}

export interface CompletionPrompt {
	id: string;
	type: string;
	sessionId: string;
	messageId: string;
	prompt: string;
}

export interface Agent {
	name: string;
	private: boolean;
}

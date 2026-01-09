import { ref, onUnmounted, getCurrentInstance } from 'vue';
import api from '@/js/api';
import { AudioHandler } from '@/js/audioHandler';

// Adapted from VoiceAgentClient in realtime-speech-prototype/static/app.js

interface UseRealtimeSpeechOptions {
	agentName: string;
	sessionId: string;
	onTranscription?: (text: string, sender: 'User' | 'AI') => void;
	onStatusChange?: (status: 'connecting' | 'connected' | 'disconnected' | 'error') => void;
}

export function useRealtimeSpeech(options: UseRealtimeSpeechOptions) {
	const isActive = ref(false);
	const isConnecting = ref(false);
	const isSupported = ref(typeof AudioContext !== 'undefined' && 'mediaDevices' in navigator);
	const userAudioLevel = ref(0);
	const aiAudioLevel = ref(0);
	const error = ref<string | null>(null);

	let websocket: WebSocket | null = null;
	let audioHandler: AudioHandler | null = null;
	let recordingActive = false;
	let greetingComplete = false;

	async function connect() {
		if (isActive.value || isConnecting.value) return;

		try {
			isConnecting.value = true;
			error.value = null;
			options.onStatusChange?.('connecting');

			// Initialize audio handler
			audioHandler = new AudioHandler();
			await audioHandler.initialize();

			// Get WebSocket URL and connect
			const bearerToken = await api.getBearerToken();
			const apiUrl = api.getApiUrl();
			if (!apiUrl) {
				throw new Error('API URL not configured');
			}
			const wsProtocol = apiUrl.startsWith('https') ? 'wss' : 'ws';
			const wsHost = apiUrl.replace(/^https?:\/\//, '');
			const instanceId = api.instanceId;
			if (!instanceId) {
				throw new Error('Instance ID not configured');
			}
			const wsUrl = `${wsProtocol}://${wsHost}/instances/${instanceId}/sessions/${options.sessionId}/realtime-speech?agentName=${options.agentName}&access_token=${bearerToken}`;

			websocket = new WebSocket(wsUrl);

			websocket.onopen = async () => {
				isConnecting.value = false;
				isActive.value = true;
				options.onStatusChange?.('connected');

				// Start recording and streaming audio
				await startAudioCapture();
			};

			websocket.onmessage = handleWebSocketMessage;

			websocket.onerror = (event) => {
				console.error('WebSocket error:', event);
				error.value = 'Connection error';
				disconnect();
			};

			websocket.onclose = () => {
				isActive.value = false;
				options.onStatusChange?.('disconnected');
			};

		} catch (err) {
			console.error('Failed to connect:', err);
			error.value = err instanceof Error ? err.message : 'Connection failed';
			isConnecting.value = false;
			options.onStatusChange?.('error');
		}
	}

	async function startAudioCapture() {
		if (!audioHandler || !websocket) return;

		recordingActive = true;
		greetingComplete = false;
		await audioHandler.startRecording((chunk: Uint8Array) => {
			if (!recordingActive || websocket?.readyState !== WebSocket.OPEN) return;
			if (!greetingComplete) return; // Don't send audio until greeting is complete

			// Calculate audio level for visualization
			const int16Data = new Int16Array(chunk.buffer);
			let sum = 0;
			for (let i = 0; i < int16Data.length; i++) {
				const normalized = int16Data[i] / 32768;
				sum += normalized * normalized;
			}
			userAudioLevel.value = Math.sqrt(sum / int16Data.length);

			// Send audio as base64-encoded PCM16
			const base64Audio = btoa(String.fromCharCode(...chunk));
			websocket.send(JSON.stringify({
				type: 'input_audio_buffer.append',
				audio: base64Audio,
			}));
		});
	}

	function handleWebSocketMessage(event: MessageEvent) {
		try {
			const message = JSON.parse(event.data);

			switch (message.type) {
				case 'session.created':
					console.log('Realtime session created');
					greetingComplete = true;
					break;

				case 'session.updated':
					greetingComplete = true;
					break;

				case 'input_audio_buffer.speech_started':
					// User started speaking - stop AI playback
					audioHandler?.clearPlayback();
					break;

			case 'response.audio.delta':
				// Receive AI audio chunk
				if (!audioHandler) break;
				const audioData = Uint8Array.from(
					atob(message.delta),
					(c) => c.charCodeAt(0)
				);
				
				// Start playback on first chunk
				if (!audioHandler.isPlaying()) {
					audioHandler.startStreamingPlayback();
				}
				
				audioHandler.playChunk(audioData);

				// Calculate AI audio level for visualization
				const int16Data = new Int16Array(audioData.buffer);
				let sum = 0;
				for (let i = 0; i < int16Data.length; i++) {
					const normalized = int16Data[i] / 32768;
					sum += normalized * normalized;
				}
				aiAudioLevel.value = Math.sqrt(sum / int16Data.length);
				break;

				case 'conversation.item.input_audio_transcription.completed':
					// User speech transcription completed
					if (message.transcript) {
						options.onTranscription?.(message.transcript, 'User');
					}
					break;

			case 'response.audio_transcript.delta':
				// AI response transcription (streaming)
				// Could be used for real-time display
				break;

			case 'response.audio_transcript.done':
				// AI response transcription completed
				if (message.transcript) {
					options.onTranscription?.(message.transcript, 'AI');
				}
				break;

				case 'response.done':
					// Response completed
					aiAudioLevel.value = 0;
					break;

				case 'error':
					console.error('Realtime API error:', message.error);
					error.value = message.error?.message || 'API error';
					break;
			}
		} catch (err) {
			console.error('Failed to parse message:', err);
		}
	}

	async function disconnect() {
		recordingActive = false;
		greetingComplete = false;

		if (audioHandler) {
			audioHandler.stopRecording();
			audioHandler.stopStreamingPlayback();
			await audioHandler.close();
			audioHandler = null;
		}

		if (websocket) {
			websocket.close();
			websocket = null;
		}

		isActive.value = false;
		isConnecting.value = false;
		userAudioLevel.value = 0;
		aiAudioLevel.value = 0;
	}

	// Only register onUnmounted if there's an active component instance
	// This prevents errors when the composable is used in mixed Options/Composition API components
	const instance = getCurrentInstance();
	if (instance) {
		onUnmounted(() => {
			disconnect();
		});
	}

	return {
		isActive,
		isConnecting,
		isSupported,
		userAudioLevel,
		aiAudioLevel,
		error,
		connect,
		disconnect,
	};
}

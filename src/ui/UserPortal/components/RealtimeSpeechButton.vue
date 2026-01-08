<template>
	<div class="realtime-speech-container">
		<!-- Speech Button -->
		<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
			<Button
				:class="['realtime-speech-button', { active: isActive }]"
				:icon="buttonIcon"
				:disabled="!isSupported || isConnecting || isCurrentAgentExpired"
				:loading="isConnecting"
				aria-label="Toggle voice conversation"
				style="height: 100%"
				@click="toggleRealtimeSpeech"
			/>
			<template #popper>
				<div role="tooltip">
					{{ tooltipText }}
				</div>
			</template>
		</VTooltip>
	</div>
</template>

<script lang="ts">
import { useRealtimeSpeech } from '@/composables/useRealtimeSpeech';
import { isAgentExpired } from '@/js/helpers';
import { ref, watch, onUnmounted, type Ref } from 'vue';

export default {
	name: 'RealtimeSpeechButton',

	props: {
		agentName: {
			type: String,
			required: true,
		},
		sessionId: {
			type: String,
			required: true,
		},
	},

	emits: ['transcription', 'status-change', 'audio-level'],

	data() {
		return {
			isMobile: window.screen.width < 950,
			// Local state that we'll sync from the composable
			isActive: false,
			isConnecting: false,
			userAudioLevel: 0,
			aiAudioLevel: 0,
			// Store composable controls (not reactive state)
			speechControls: null as {
				connect: () => Promise<void>;
				disconnect: () => Promise<void>;
				isActive: Ref<boolean>;
				isConnecting: Ref<boolean>;
				userAudioLevel: Ref<number>;
				aiAudioLevel: Ref<number>;
			} | null,
			// Watch cleanup functions
			watchCleanups: [] as (() => void)[],
		};
	},

	computed: {
		isCurrentAgentExpired() {
			const currentAgent = this.$appStore.lastSelectedAgent?.resource;
			return currentAgent ? isAgentExpired(currentAgent) : false;
		},

		isSupported() {
			return typeof AudioContext !== 'undefined' && 'mediaDevices' in navigator;
		},

		buttonIcon() {
			if (this.isActive) {
				return 'pi pi-stop-circle';
			}
			return 'pi pi-microphone';
		},

		tooltipText() {
			if (!this.isSupported) {
				return 'Voice conversations are not supported in this browser';
			}
			if (this.isConnecting) {
				return 'Connecting...';
			}
			if (this.isActive) {
				return 'Click or say "stop" to end voice conversation';
			}
			return 'Start voice conversation';
		},
	},

	watch: {
		userAudioLevel(newVal) {
			this.$emit('audio-level', { 
				userLevel: newVal, 
				aiLevel: this.aiAudioLevel,
				isActive: this.isActive 
			});
		},
		aiAudioLevel(newVal) {
			this.$emit('audio-level', { 
				userLevel: this.userAudioLevel, 
				aiLevel: newVal,
				isActive: this.isActive 
			});
		},
		isActive(newVal) {
			this.$emit('audio-level', { 
				userLevel: this.userAudioLevel, 
				aiLevel: this.aiAudioLevel,
				isActive: newVal 
			});
		},
	},

	beforeUnmount() {
		// Clean up watchers
		this.watchCleanups.forEach(cleanup => cleanup());
		this.watchCleanups = [];
	},

	methods: {
		async toggleRealtimeSpeech() {
			if (this.isActive) {
				await this.speechControls?.disconnect();
			} else {
				// Ensure we have a real session before starting voice conversation
				// This handles the case where user starts voice immediately on a new conversation
				if (this.$appStore.currentSession?.is_temp) {
					console.log('Creating real session before starting voice conversation');
					const newSession = await this.$appStore.addSession(this.$appStore.getDefaultChatSessionProperties());
					this.$appStore.changeSession(newSession);
					// Wait for the session change to propagate
					await this.$nextTick();
				}

				// Initialize the composable with the current session ID
				this.initRealtimeSpeech();
				await this.speechControls?.connect();
			}
		},

		initRealtimeSpeech() {
			// Clean up previous watchers if any
			this.watchCleanups.forEach(cleanup => cleanup());
			this.watchCleanups = [];

			// Get the current session ID from the store (which will be the real session ID now)
			const currentSessionId = this.$appStore.currentSession?.sessionId;
			if (!currentSessionId || currentSessionId === 'temp') {
				console.error('Cannot initialize realtime speech without a valid session ID');
				return;
			}

			console.log('Initializing realtime speech with session:', currentSessionId);

			const composable = useRealtimeSpeech({
				agentName: this.agentName,
				sessionId: currentSessionId,
				onTranscription: (text, sender) => {
					this.$emit('transcription', { text, sender });
				},
				onStatusChange: (status) => {
					this.$emit('status-change', status);
				},
			});

			// Store the controls
			this.speechControls = {
				connect: composable.connect,
				disconnect: composable.disconnect,
				isActive: composable.isActive,
				isConnecting: composable.isConnecting,
				userAudioLevel: composable.userAudioLevel,
				aiAudioLevel: composable.aiAudioLevel,
			};

			// Set up watchers to sync composable refs to local state
			const stopWatchActive = watch(composable.isActive, (newVal) => {
				this.isActive = newVal;
			}, { immediate: true });

			const stopWatchConnecting = watch(composable.isConnecting, (newVal) => {
				this.isConnecting = newVal;
			}, { immediate: true });

			const stopWatchUserAudio = watch(composable.userAudioLevel, (newVal) => {
				this.userAudioLevel = newVal;
			}, { immediate: true });

			const stopWatchAiAudio = watch(composable.aiAudioLevel, (newVal) => {
				this.aiAudioLevel = newVal;
			}, { immediate: true });

			this.watchCleanups = [stopWatchActive, stopWatchConnecting, stopWatchUserAudio, stopWatchAiAudio];
		},
	},
};
</script>

<style lang="scss" scoped>
.realtime-speech-container {
	display: flex;
	align-items: stretch;
	height: 100%;
}

.realtime-speech-button {
	width: 48px;
	min-width: 48px;
	border-radius: 0;
	background: #f5f5f5;
	border: 2px solid #e1e1e1;
	border-left: none;
	color: #666;
	transition: all 0.2s ease;
	display: flex;
	align-items: center;
	justify-content: center;
	cursor: pointer;

	:deep(.pi) {
		font-size: 1.1rem;
	}

	&:hover:not(:disabled) {
		background: #eee;
		color: #333;
	}

	&.active {
		background: linear-gradient(135deg, #ef5350 0%, #e53935 100%);
		border-color: #e53935;
		color: white;

		:deep(.pi) {
			font-size: 1rem;
		}
	}

	&:disabled {
		opacity: 0.4;
		cursor: not-allowed;
	}
}
</style>

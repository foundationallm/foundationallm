<template>
	<div class="realtime-speech-container">
		<!-- Waveform Visualizer (shown when active) -->
		<div v-if="isActive" class="waveform-container">
			<canvas ref="userWaveformCanvas" class="waveform user-waveform"></canvas>
			<canvas ref="aiWaveformCanvas" class="waveform ai-waveform"></canvas>
			<div class="waveform-labels">
				<span class="user-label">You</span>
				<span class="ai-label">AI</span>
			</div>
		</div>

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

	emits: ['transcription', 'status-change'],

	data() {
		return {
			isMobile: window.screen.width < 950,
		};
	},

	setup(props, { emit }) {
		const {
			isActive,
			isConnecting,
			isSupported,
			userAudioLevel,
			aiAudioLevel,
			connect,
			disconnect,
			error,
		} = useRealtimeSpeech({
			agentName: props.agentName,
			sessionId: props.sessionId,
			onTranscription: (text, sender) => {
				emit('transcription', { text, sender });
			},
			onStatusChange: (status) => {
				emit('status-change', status);
			},
		});

		return {
			isActive,
			isConnecting,
			isSupported,
			userAudioLevel,
			aiAudioLevel,
			connect,
			disconnect,
			error,
		};
	},

	computed: {
		isCurrentAgentExpired() {
			const currentAgent = this.$appStore.lastSelectedAgent?.resource;
			return currentAgent ? isAgentExpired(currentAgent) : false;
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
		userAudioLevel: 'drawUserWaveform',
		aiAudioLevel: 'drawAiWaveform',
	},

	methods: {
		async toggleRealtimeSpeech() {
			if (this.isActive) {
				await this.disconnect();
			} else {
				await this.connect();
			}
		},

		drawUserWaveform() {
			this.drawWaveform(this.$refs.userWaveformCanvas, this.userAudioLevel, '#4CAF50');
		},

		drawAiWaveform() {
			this.drawWaveform(this.$refs.aiWaveformCanvas, this.aiAudioLevel, '#2196F3');
		},

		drawWaveform(canvas: HTMLCanvasElement | undefined, level: number, color: string) {
			if (!canvas) return;
			
			const ctx = canvas.getContext('2d');
			if (!ctx) return;
			const width = canvas.width;
			const height = canvas.height;
			
			ctx.clearRect(0, 0, width, height);
			ctx.fillStyle = color;
			
			// Draw waveform bars
			const barCount = 20;
			const barWidth = width / barCount - 2;
			
			for (let i = 0; i < barCount; i++) {
				const barHeight = Math.random() * level * height;
				const x = i * (barWidth + 2);
				const y = (height - barHeight) / 2;
				ctx.fillRect(x, y, barWidth, barHeight);
			}
		},
	},

	mounted() {
		// Initialize canvas dimensions
		if (this.$refs.userWaveformCanvas) {
			this.$refs.userWaveformCanvas.width = 100;
			this.$refs.userWaveformCanvas.height = 40;
		}
		if (this.$refs.aiWaveformCanvas) {
			this.$refs.aiWaveformCanvas.width = 100;
			this.$refs.aiWaveformCanvas.height = 40;
		}
	},
};
</script>

<style lang="scss" scoped>
.realtime-speech-container {
	display: flex;
	flex-direction: column;
	align-items: center;
	gap: 8px;
}

.waveform-container {
	display: flex;
	gap: 16px;
	padding: 8px 16px;
	background: rgba(0, 0, 0, 0.05);
	border-radius: 8px;
	margin-bottom: 8px;
}

.waveform {
	width: 100px;
	height: 40px;
	border-radius: 4px;
}

.user-waveform {
	background: rgba(76, 175, 80, 0.1);
}

.ai-waveform {
	background: rgba(33, 150, 243, 0.1);
}

.waveform-labels {
	display: flex;
	gap: 16px;
	font-size: 0.75rem;
	color: #666;
}

.realtime-speech-button {
	width: 48px;
	height: 48px;
	border-radius: 50%;
	background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
	border: none;
	color: white;
	transition: all 0.3s ease;

	&:hover:not(:disabled) {
		transform: scale(1.1);
		box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
	}

	&.active {
		background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
		animation: pulse 2s infinite;
	}

	&:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}
}

@keyframes pulse {
	0%, 100% {
		box-shadow: 0 0 0 0 rgba(245, 87, 108, 0.4);
	}
	50% {
		box-shadow: 0 0 0 10px rgba(245, 87, 108, 0);
	}
}
</style>

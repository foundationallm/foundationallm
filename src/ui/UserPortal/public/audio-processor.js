/**
 * AudioWorklet processor for recording audio at 24kHz.
 * Converts Float32 samples to format suitable for GPT-Realtime API.
 */
class AudioRecorderProcessor extends AudioWorkletProcessor {
	constructor() {
		super();
		this.isRecording = false;
		this.port.onmessage = (event) => {
			if (event.data.command === 'START_RECORDING') {
				this.isRecording = true;
			} else if (event.data.command === 'STOP_RECORDING') {
				this.isRecording = false;
			}
		};
	}

	process(inputs, outputs, parameters) {
		const input = inputs[0];
		if (this.isRecording && input.length > 0) {
			const audioData = input[0];
			// Send audio data to main thread
			this.port.postMessage({
				eventType: 'audio',
				audioData: audioData,
			});
		}
		return true;
	}
}

registerProcessor('audio-recorder-processor', AudioRecorderProcessor);

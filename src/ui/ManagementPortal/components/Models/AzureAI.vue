<template>
	<div class="span-2">
		<div v-for="field in fields" :key="field.name">
			<div class="mb-2">
				<span>{{ field.title }}</span>
				<i v-tooltip="field.description" class="pi pi-info-circle" style="margin-left: 8px"></i>
			</div>
			<InputText
				v-model="model[field.name]"
				class="w-100 mb-4"
				type="number"
			/>
		</div>
	</div>
</template>

<script lang="ts">
const AZURE_AI_FIELDS = [
	{
		field_name: 'model',
		title: 'Model',
		type: 'string',
		required: false,
		description: `The model name. Ignored if the endpoint serves only one model.`,
	},
	{
		field_name: 'messages',
		title: 'Messages',
		type: 'ChatCompletionRequestMessage',
		required: true,
		description: `A list of messages comprising the conversation so far. Returns a 422 error if any of the messages cannot be understood by the model.`,
	},
	{
		field_name: 'frequency_penalty',
		title: 'Frequency Penalty',
		type: 'number',
		required: false,
		description: `Reduces the chance of word repetition by penalizing repeated words. Higher values decrease repetition likelihood. Returns a 422 error if unsupported by the model.`,
	},
	{
		field_name: 'max_tokens',
		title: 'Max Tokens',
		type: 'integer',
		required: false,
		description: `The maximum number of tokens that can be generated in the chat completion. Total input and output token length is limited by the model's context length. Passing null uses the model’s max context length.`,
	},
	{
		field_name: 'presence_penalty',
		title: 'Presence Penalty',
		type: 'number',
		required: false,
		description: `Reduces repetition of the same topics by penalizing tokens that exist in the completion already, even if only once. Returns a 422 error if unsupported by the model.`,
	},
	{
		field_name: 'response_format',
		title: 'Response Format',
		type: 'ChatCompletionResponseFormat',
		required: false,
		description: `The format in which the model’s response is returned.`,
	},
	{
		field_name: 'seed',
		title: 'Seed',
		type: 'integer',
		required: false,
		description: `Attempts deterministic sampling, meaning repeated requests with the same seed and parameters should yield the same result. Determinism is not guaranteed; use the system_fingerprint response parameter to monitor backend changes.`,
	},
	{
		field_name: 'stop',
		title: 'Stop',
		type: 'string | array',
		required: false,
		description: `Sequences where the API will stop generating further tokens.`,
	},
	// {
	// 	field_name: 'stream',
	// 	title: 'Stream',
	// 	type: 'boolean',
	// 	required: false,
	// 	description: `If set, sends partial message deltas as data-only server-sent events as they become available. Stream terminates with a data: [DONE] message.`,
	// },
	{
		field_name: 'temperature',
		title: 'Temperature',
		type: 'number',
		required: false,
		description: `Controls randomness in the model's output. Returns a 422 error if unsupported by the model.`,
	},
	{
		field_name: 'tool_choice',
		title: 'Tool Choice',
		type: 'ChatCompletionToolChoiceOption',
		required: false,
		description: `Specifies which function (if any) the model calls. none (default if no functions are present) means the model generates a message only. auto (default if functions are present) lets the model choose between generating a message or calling a function. Specify a function with {"type": "function", "function": {"name": "my_function"}} to force the model to call it. Returns a 422 error if unsupported by the model.`,
	},
	{
		field_name: 'tools',
		title: 'Tools',
		type: 'ChatCompletionTool[]',
		required: false,
		description: `A list of tools (currently only functions) the model may call. Use this to specify functions for the model to generate JSON inputs for. Returns a 422 error if unsupported by the model.`,
	},
	{
		field_name: 'top_p',
		title: 'Top P',
		type: 'number',
		required: false,
		description: `Nucleus sampling threshold. Limits tokens to those within top_p probability mass, e.g., 0.1 limits tokens to the top 10% probability mass. We generally recommend adjusting either this or temperature but not both.`,
	}
];

export default {
	data() {
		return {
			fields: AZURE_AI_FIELDS,
			model: {},
		};
	},

	created() {
		this.fillModelFields();
	},

	methods: {
		fillModelFields() {
			this.fields.forEach((field) => {
				this.model[field.name] = field.default || '';
			});
		},
	},
};
</script>

<style lang="scss">
	
</style>

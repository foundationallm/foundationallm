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
const OPEN_AI_FIELDS = [
	// {
	// 	name: 'prompt',
	// 	title: 'Prompt',
	// 	type: 'string | array of strings',
	// 	required: false,
	// 	description: `The prompt(s) to generate completions for, either as a string or array of strings. If omitted, the model generates as if starting a new document. Maximum allowed size is 2048.`,
	// },
	{
		name: 'max_tokens',
		title: 'Max Tokens',
		type: 'integer',
		required: false,
		default: 16,
		description: `The combined token count of prompt and max_tokens cannot exceed the model's context length (typically 2048 tokens, 4096 for newer models). Minimum of 0.`,
	},
	{
		name: 'temperature',
		title: 'Temperature',
		type: 'number',
		required: false,
		default: 1,
		description: `Sampling temperature for creativity. Higher values increase model risk-taking; 0 is deterministic. We recommend adjusting this or top_p, not both.`,
	},
	{
		name: 'top_p',
		title: 'Top P',
		type: 'number',
		required: false,
		default: 1,
		description: `Nucleus sampling probability threshold. Model considers tokens within top_p probability mass. We recommend adjusting this or temperature, not both.`,
	},
	{
		name: 'logit_bias',
		title: 'Logit Bias',
		type: 'object',
		required: false,
		description: `Modifies likelihood of specified tokens in completion. Maps tokens to bias values from -100 to 100. For example, {"50256":-100} prevents <|endoftext|> from being generated.`,
	},
	// {
	// 	name: 'user',
	// 	title: 'User',
	// 	type: 'string',
	// 	required: false,
	// 	description: `A unique identifier for the end-user to assist with monitoring and abuse detection.`,
	// },
	{
		name: 'n',
		title: 'N',
		type: 'integer',
		required: false,
		default: 1,
		description: `Number of completions to generate per prompt. Min 1, max 128. Higher values consume more tokens quickly.`,
	},
	// {
	// 	name: 'stream',
	// 	title: 'Stream',
	// 	type: 'boolean',
	// 	required: false,
	// 	default: false,
	// 	description: `If true, streams partial progress as tokens are generated, ending with a data: [DONE] message.`,
	// },
	{
		name: 'logprobs',
		title: 'Logprobs',
		type: 'integer',
		required: false,
		description: `Returns log probabilities for up to the specified number of likely tokens. If set to 5, it returns 5 most likely tokens.`,
	},
	{
		name: 'suffix',
		title: 'Suffix',
		type: 'string',
		required: false,
		description: `A suffix appended after the generated text completion.`,
	},
	{
		name: 'echo',
		title: 'Echo',
		type: 'boolean',
		required: false,
		default: false,
		description: `If true, returns the prompt along with the completion.`,
	},
	{
		name: 'stop',
		title: 'Stop',
		type: 'string | array of strings',
		required: false,
		description: `Up to 4 sequences where generation will stop. The returned text does not include the stop sequence.`,
	},
	{
		name: 'completion_config',
		title: 'Completion Config',
		type: 'string',
		required: false,
		description: `A configuration string for additional model parameters.`,
	},
	{
		name: 'presence_penalty',
		title: 'Presence Penalty',
		type: 'number',
		required: false,
		default: 0,
		description: `Ranges from -2.0 to 2.0. Positive values penalize tokens based on appearance, encouraging new topics.`,
	},
	{
		name: 'frequency_penalty',
		title: 'Frequency Penalty',
		type: 'number',
		required: false,
		default: 0,
		description: `Ranges from -2.0 to 2.0. Positive values penalize frequent tokens, discouraging repetition.`,
	},
	{
		name: 'best_of',
		title: 'Best Of',
		type: 'integer',
		required: false,
		description: `Generates best_of completions server-side and returns the best based on token log probability. Max value of 128. Can’t be used with streaming.`,
	}
];

export default {
	data() {
		return {
			fields: OPEN_AI_FIELDS,
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

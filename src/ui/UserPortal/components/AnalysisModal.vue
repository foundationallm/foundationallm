<template>
	<Dialog
		:visible="visible"
		:header="header"
		:modal="true"
		:closable="true"
		:style="{ width: '95vw' }"
		@update:visible="$emit('update:visible', $event)"
	>
		<template v-if="analysisResults && analysisResults.length > 0" #default>
			<div id="analysis-content" tabindex="0">
				<div v-for="(analysis, index) in analysisResults" :key="index" class="analysis-block">
					<h4>Tool: {{ analysis.tool_name }}</h4>
					<p><strong>Category:</strong> {{ analysis.agent_capability_category }}</p>

					<CodeBlockHeader language="python" :codecontent="encodeURIComponent(analysis.tool_input)">
						<!-- eslint-disable-next-line vue/no-v-html -->
						<pre v-html="renderCodeBlock(analysis.tool_input)" class="mt-0" />
					</CodeBlockHeader>

					<p v-if="analysis.tool_output"><strong>Output:</strong> {{ analysis.tool_output }}</p>
				</div>
			</div>
		</template>

		<template v-if="!analysisResults || analysisResults.length === 0">
			<p>No analysis results available.</p>
		</template>
	</Dialog>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { marked } from 'marked';
import type { AnalysisResult } from '@/js/types';

import hljs from 'highlight.js';
import 'highlight.js/styles/github-dark-dimmed.css';

const markedRenderer = new marked.Renderer();

markedRenderer.code = (code) => {
	const language = code.lang;
	const sourceCode = code.text || code;
	const validLanguage = !!(language && hljs.getLanguage(language));
	const highlighted = validLanguage
		? hljs.highlight(sourceCode, { language })
		: hljs.highlightAuto(sourceCode);
	const languageClass = validLanguage ? `hljs language-${language}` : 'hljs';
	const encodedCode = encodeURIComponent(sourceCode);
	return `<code class="${languageClass}" data-code="${encodedCode}" data-language="${highlighted.language}">${highlighted.value}</code>`;
};

marked.use({ renderer: markedRenderer });

export default {
	name: 'AnalysisModal',

	props: {
		visible: {
			type: Boolean,
			required: true,
		},

		analysisResults: {
			type: Array as PropType<Array<AnalysisResult>>,
			required: true,
		},

		header: {
			type: String,
			default: 'Analysis',
		},
	},

	emits: ['update:visible'],

	methods: {
		renderCodeBlock(code: string) {
			return marked(`\`\`\`python\n${code}\n\`\`\``);
		},
	},
};
</script>

<style lang="scss" scoped>
.analysis-block {
	margin-bottom: 20px;
}

p {
	margin: 10px 0;
}

pre {
	margin-top: 0;
}
</style>

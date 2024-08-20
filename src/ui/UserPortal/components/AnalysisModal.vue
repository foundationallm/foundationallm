<template>
	<Dialog
		:visible="visible"
		:header="header"
		:modal="true"
		:closable="true"
		:style="{ width: '50vw' }"
		@update:visible="$emit('update:visible', $event)"
	>
<template v-if="analysisResults && analysisResults.length > 0" v-slot:default>
			<div v-for="(analysis, index) in analysisResults" :key="index" class="analysis-block">
				<h4>Tool: {{ analysis.tool_name }}</h4>
				<p><strong>Category:</strong> {{ analysis.agent_capability_category }}</p>
				<CodeBlockHeader language="python" :codecontent="encodeURIComponent(analysis.tool_input)">
                    <pre v-html="renderCodeBlock(analysis.tool_input)"></pre>
                </CodeBlockHeader>
				<p v-if="analysis.tool_output"><strong>Output:</strong> {{ analysis.tool_output }}</p>
			</div>
		</template>
		<template v-if="!analysisResults || analysisResults.length === 0">
			<p>No analysis results available.</p>
		</template>
	</Dialog>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import type { PropType } from 'vue';
import { marked } from 'marked';
import type { AnalysisResult } from '@/js/types';
import CodeBlockHeader from '@/components/CodeBlockHeader.vue';

export default defineComponent({
	name: 'AnalysisModal',
    components: {
        CodeBlockHeader
    },
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
	methods: {
		renderCodeBlock(code: string) {
            return marked(`\`\`\`python\n${code}\n\`\`\``);
		},
	},
});
</script>

<style scoped>
.analysis-block {
	margin-bottom: 20px;
}

p {
	margin: 10px 0;
}

.header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	color: var(--secondary-button-text);
	background: var(--secondary-color);
	padding-left: 8px;
}

.copy-button {
	color: var(--secondary-button-text) !important;
}
</style>

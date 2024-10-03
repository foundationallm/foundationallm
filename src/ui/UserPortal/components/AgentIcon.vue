<template>
	<img
		v-if="resolvedSrc !== ''"
		v-tooltip.bottom="tooltip"
		:alt="alt"
		class="avatar"
		:src="resolvedSrc"
	/>
</template>

<script lang="ts">
import { defineComponent, computed } from 'vue';

export default defineComponent({
	name: 'AgentIcon',
	props: {
		src: {
			type: String,
			required: true,
		},
		alt: {
			type: String,
			default: 'Agent icon',
		},
		tooltip: {
			type: String,
			default: '',
		},
	},
	setup(props) {
		const resolvedSrc = computed(() => {
			if (props.src.startsWith('~')) {
				// Handle relative path dynamically
				return new URL(`../assets/${props.src.slice(9)}`, import.meta.url).href;
			}
			return props.src;
		});

		const tooltipDirective = computed(() => {
			return props.tooltip !== '' ? { 'v-tooltip': { value: props.tooltip, bottom: true } } : {};
		});

		return {
			resolvedSrc,
			tooltipDirective,
		};
	},
});
</script>

<style scoped>
.avatar {
	/* Add any specific styling for the avatar here */
	width: 40px;
	height: 40px;
	border-radius: 50%;
}
</style>

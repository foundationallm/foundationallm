<template>
	<Image
        class="message-content__image"
		:src="value.filepath"
		:alt="value.metadata?.revised_tool_input"
        :width="isMobile ? '100%' : '75%'"
        preview
        @load="loading = false"
        @error="
            loading = false;
            error = true;
        "
    />
</template>

<script>
export default {
	props: {
		value: {
			type: Object,
			required: false,
			default: null,
		},
	},

	data() {
		return {
			loading: false,
			error: false,
            isMobile: window.screen.width < 950,
		};
	},

};
</script>

<style lang="scss" scoped>
.message-content__image {
	margin-top: 5px;
	margin-bottom: 5px;
}

img {
	max-width: 100%;
	height: auto;
	border-radius: 8px;
}
</style>

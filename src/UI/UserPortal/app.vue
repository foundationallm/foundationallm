<template>
	<NuxtPage :style="style" />
</template>

<script lang="ts">
export default {
	computed: {
		style() {
			return {
				'--primary-bg': this.$config.public.BRANDING_BACKGROUND_COLOR,
				'--primary-color': this.$config.public.BRANDING_PRIMARY_COLOR,
				'--secondary-color': this.$config.public.BRANDING_SECONDARY_COLOR,
				'--accent-color': this.$config.public.BRANDING_ACCENT_COLOR,
				'--primary-text': this.$config.public.BRANDING_PRIMARY_TEXT_COLOR,
				'--secondary-text': this.$config.public.BRANDING_SECONDARY_TEXT_COLOR,
			};
		},
	},
};
</script>

<style lang="scss">
html,
body,
#__nuxt,
#__layout {
	height: 100%;
	margin: 0;
	font-family: 'Poppins', sans-serif;
}
</style>

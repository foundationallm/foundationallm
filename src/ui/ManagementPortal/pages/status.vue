<template>
	<div>Status: {{ status }}</div>
</template>

<script>
export default defineNuxtComponent({
	asyncData() {
		return {
			status: 'OK',
		};
	},
});
</script>

<template>
	<CreateModel :edit-id="$route.params.modelOrEndpointName" />
</template>

<script lang="ts">
import CreateModel from '../create.vue';

export default {
	components: {
		CreateModel,
	},
};
</script>

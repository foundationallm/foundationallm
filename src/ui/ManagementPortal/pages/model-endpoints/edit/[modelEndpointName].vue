<template>
	<CreateModelEndpoint :edit-id="$route.params.modelEndpointName" />
</template>

<script lang="ts">
import CreateModelEndpoint from '../create.vue';

export default {
	components: {
		CreateModelEndpoint,
	},
};
</script>

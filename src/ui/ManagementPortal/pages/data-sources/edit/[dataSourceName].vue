<template>
	<CreateModelOrEndpoint :edit-id="$route.params.modelOrEndpointName" />
</template>

<script lang="ts">
import CreateModelOrEndpoint from '../create.vue';

export default {
	name: 'EditModelOrEndpoint',

	components: {
		CreateModelOrEndpoint,
	},
};
</script>

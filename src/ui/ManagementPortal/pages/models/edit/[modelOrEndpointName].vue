<template>
	<Create :edit-id="$route.params.modelOrEndpointName" />
</template>

<script lang="ts">
import Create from '../create.vue';

export default {
	components: {
		Create,
	},
};
</script>

<template>
	<CreatePipeline :edit-id="$route.params.pipelineName" />
</template>

<script lang="ts">
import CreatePipeline from '../create.vue';

export default {
	name: 'EditPipeline',
	components: {
		CreatePipeline,
	},
};
</script>

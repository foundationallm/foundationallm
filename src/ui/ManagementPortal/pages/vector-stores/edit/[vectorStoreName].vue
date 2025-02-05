<template>
	<CreateVectorStore :edit-id="$route.params.vectorStoreName" />
</template>

<script lang="ts">
import CreateVectorStore from '../create.vue';

export default {
	name: 'EditVectorStore',

	components: {
		CreateVectorStore,
	},
};
</script>

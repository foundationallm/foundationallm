<template>
	<CreateAgent :edit-agent="$route.params.agentName" />
</template>

<script lang="ts">
import CreateAgent from '../create.vue';

export default {
	components: {
		CreateAgent,
	},
};
</script>

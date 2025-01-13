<template>
	<CreateIndexingProfile :edit-id="$route.params.indexingProfileName" />
</template>

<script lang="ts">
import CreateIndexingProfile from '../create.vue';

export default {
	name: 'EditIndexingProfile',

	components: {
		CreateIndexingProfile,
	},
};
</script>

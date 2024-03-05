<template>
	<CreateDataSource :editDataSource="$route.params.dataSourceName" />
</template>

<script lang="ts">
import CreateDataSource from '../create.vue';

export default {
	components: {
		CreateDataSource,
	},
};
</script>
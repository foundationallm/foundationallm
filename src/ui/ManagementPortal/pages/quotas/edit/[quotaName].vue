<template>
	<CreateQuota :editId="quotaName" />
</template>

<script lang="ts">
import CreateQuota from '../create.vue';

export default {
	name: 'EditQuota',

	components: {
		CreateQuota,
	},

	computed: {
		quotaName() {
			return this.$route.params.quotaName as string;
		},
	},
};
</script>

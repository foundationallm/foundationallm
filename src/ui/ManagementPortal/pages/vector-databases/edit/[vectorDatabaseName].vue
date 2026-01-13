<script setup lang="ts">
import CreateVectorDatabase from '../create.vue';

const route = useRoute();
const vectorDatabaseName = route.params.vectorDatabaseName;
</script>

<template>
	<CreateVectorDatabase :edit-id="vectorDatabaseName" />
</template>

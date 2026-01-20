<script setup lang="ts">
import CreateKnowledgeSource from '../create.vue';

const route = useRoute();
const knowledgeSourceName = route.params.knowledgeSourceName;
</script>

<template>
	<CreateKnowledgeSource :edit-id="knowledgeSourceName" />
</template>

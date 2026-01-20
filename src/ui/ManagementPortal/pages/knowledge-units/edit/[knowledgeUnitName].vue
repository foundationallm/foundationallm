<script setup lang="ts">
import CreateKnowledgeUnit from '../create.vue';

const route = useRoute();
const knowledgeUnitName = route.params.knowledgeUnitName;
</script>

<template>
	<CreateKnowledgeUnit :edit-id="knowledgeUnitName" />
</template>

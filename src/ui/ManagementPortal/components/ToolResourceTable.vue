<template>
	<DataTable
		:value="Object.keys(resources).map((key) => ({ key, value: resources[key] }))"
		:expanded-rows="expandedRows"
		data-key="key"
		striped-rows
		scrollable
		table-style="max-width: 100%"
		size="small"
	>
		<template #empty>No tool resources added.</template>

		<Column
			expander
			header="Expand Properties"
			header-style="width:12rem"
			:pt="{
				headerCell: {
					style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
				},
				sortIcon: { style: { color: 'var(--primary-text)' } },
			}"
		/>

		<!-- Tool resource name -->
		<Column
			field="name"
			header="Name"
			sortable
			:pt="{
				headerCell: {
					style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
				},
				sortIcon: { style: { color: 'var(--primary-text)' } },
			}"
		>
			<template #body="{ data }">
				{{ getResourceNameFromId(data.value.object_id) }}
			</template>
		</Column>

		<!-- Tool resource type -->
		<Column
			field="type"
			header="Type"
			sortable
			:pt="{
				headerCell: {
					style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
				},
				sortIcon: { style: { color: 'var(--primary-text)' } },
			}"
		>
			<template #body="{ data }">
				{{ getResourceTypeFromId(data.value.object_id) }}
			</template>
		</Column>

		<!-- Edit tool resource -->
		<!-- <Column
			header="Edit"
			header-style="width:6rem"
			style="text-align: center"
			:pt="{
				headerCell: {
					style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
				},
				headerContent: { style: { justifyContent: 'center' } },
			}"
		>
			<template #body="{ data }">
				<Button link @click="toolResourceToEdit = data.value">
					<i class="pi pi-cog" style="font-size: 1.2rem"></i>
				</Button>

				<CreateResourceObjectDialog
					v-if="toolResourceToEdit?.object_id === data.value.object_id"
					:model-value="toolResourceToEdit"
					:visible="toolResourceToEdit?.object_id === data.value.object_id"
					@update:visible="toolResourceToEdit = null"
					@update:modelValue="handleEditResourceObject(data.value)"
				/>
			</template>
		</Column> -->

		<!-- Delete tool resource -->
		<Column
			header="Delete"
			header-style="width:6rem"
			style="text-align: center"
			:pt="{
				headerCell: {
					style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
				},
				headerContent: { style: { justifyContent: 'center' } },
			}"
		>
			<template #body="{ data }">
				<Button link @click="handleDelete(data.value)">
					<i class="pi pi-trash" style="font-size: 1.2rem"></i>
				</Button>
			</template>
		</Column>

		<template #expansion="{ data }">
			<div style="padding-left: 16px">
				<div class="mb-2 font-weight-bold">
					{{ getResourceNameFromId(data.value.object_id) }} properties:
				</div>
				<PropertyBuilder v-model="resources[data.value.object_id].properties" />
			</div>
		</template>
	</DataTable>
</template>

<script lang="ts">
export default {
	emits: ['delete'],

	props: {
		resources: {
			type: Object,
			required: true,
		},
	},

	data() {
		return {
			expandedRows: {},
		};
	},

	methods: {
		getResourceNameFromId(resourceId: string): string {
			const parts = resourceId.split('/').filter(Boolean);
			return parts.slice(-1)[0];
		},

		getResourceTypeFromId(resourceId: string): string {
			const parts = resourceId.split('/').filter(Boolean);
			const type = parts.slice(-2)[0];

			if (type === 'prompts') {
				return 'Prompt';
			} else if (type === 'aiModels') {
				return 'AI Model';
			} else if (type === 'textEmbeddingProfiles') {
				return 'Embedding Profile';
			} else if (type === 'indexingProfiles') {
				return 'Vector Store';
			} else if (type === 'apiEndpointConfigurations') {
				return 'API Endpoint';
			}

			return type;
		},

		handleDelete(resourceToDelete: any): void {
			this.$emit('delete', resourceToDelete);
		},
	},
};
</script>

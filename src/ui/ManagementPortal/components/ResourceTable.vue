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
		<template #empty>No resources added.</template>

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

		<!-- Resource name -->
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

		<!-- Resource type -->
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

		<!-- Edit resource -->
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
				<Button link @click="resourceToEdit = data.value">
					<i class="pi pi-cog" style="font-size: 1.2rem"></i>
				</Button>

				<CreateResourceObjectDialog
					v-if="resourceToEdit?.object_id === data.value.object_id"
					:model-value="resourceToEdit"
					:visible="resourceToEdit?.object_id === data.value.object_id"
					@update:visible="resourceToEdit = null"
					@update:model-value="handleEditResourceObject(data.value)"
				/>
			</template>
		</Column> -->

		<!-- Delete resource -->
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
				<div class="mb-2 font-bold">
					{{ getResourceNameFromId(data.value.object_id) }} properties:
				</div>
				<PropertyBuilder v-model="resources[data.value.object_id].properties" />
			</div>
		</template>
	</DataTable>
</template>

<script lang="ts">
export default {
	props: {
		resources: {
			type: Object,
			required: true,
		},
	},

	emits: ['delete'],

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

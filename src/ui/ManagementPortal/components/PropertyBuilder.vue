<template>
	<div class="flex flex-col gap-4">
		<DataTable
			:value="Object.keys(properties).map((key) => ({ key, value: properties[key] }))"
			striped-rows
			scrollable
			table-style="max-width: 100%"
			size="small"
		>
			<template #empty>No properties added.</template>

			<!-- Property name -->
			<Column
				field="name"
				header="Property Name"
				sortable
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			>
				<template #body="{ data }">
					{{ data.key }}
				</template>
			</Column>

			<!-- Property value -->
			<Column
				field="value"
				header="Property Value"
				sortable
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			>
				<template #body="{ data }">
					{{ JSON.stringify(data.value) }}
				</template>
			</Column>

			<!-- Edit property -->
			<Column
				header="Edit"
				header-style="width:6rem"
				class="text-center"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					headerContent: { style: { justifyContent: 'center' } },
				}"
			>
				<template #body="{ data }">
					<Button link @click="handleEditProperty(data.key)">
						<i class="pi pi-cog" style="font-size: 1.2rem"></i>
					</Button>
				</template>
			</Column>

			<!-- Delete property -->
			<Column
				header="Delete"
				header-style="width:6rem"
				class="text-center"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					headerContent: { style: { justifyContent: 'center' } },
				}"
			>
				<template #body="{ data }">
					<Button link @click="handleDeleteProperty(data.key)">
						<i class="pi pi-trash" style="font-size: 1.2rem"></i>
					</Button>
				</template>
			</Column>
		</DataTable>

		<div class="flex gap-4">
			<PropertyDialog
				v-if="showCreateOrEditPropertyDialog"
				v-model="propertyToEdit"
				:title="propertyToEdit ? 'Edit Property' : 'Create Property'"
				:visible="showCreateOrEditPropertyDialog"
				@update:model-value="handleAddProperty($event)"
				@update:visible="handleClosePropertyDialog"
			/>

			<!-- Add property -->
			<div class="w-full flex justify-end mt-4">
				<Button
					label="Add Property"
					severity="primary"
					class="text-nowrap"
					@click="showCreateOrEditPropertyDialog = true"
				/>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
export default {
	props: {
		modelValue: {
			type: Object,
			required: false,
			default: () => ({}),
		},
	},

	emits: ['update:modelValue'],

	data() {
		return {
			showCreateOrEditPropertyDialog: false,
			propertyToEdit: null,

			properties: {},
		};
	},

	watch: {
		modelValue: {
			immediate: true,
			deep: true,
			handler() {
				if (JSON.stringify(this.modelValue) === JSON.stringify(this.properties)) return;
				this.properties = this.modelValue;
			},
		},
	},

	methods: {
		handleEditProperty(propertyKey) {
			this.propertyToEdit = { key: propertyKey, value: this.properties[propertyKey] };
			this.showCreateOrEditPropertyDialog = true;
		},

		handleDeleteProperty(propertyKey) {
			delete this.properties[propertyKey];
			this.$emit('update:modelValue', this.properties);
		},

		handleClosePropertyDialog() {
			this.showCreateOrEditPropertyDialog = false;
			this.propertyToEdit = null;
		},

		handleAddProperty(propertyObject) {
			const errors = [];

			// if (!this.propertyKey) {
			// 	errors.push('Please input a property name.');
			// }

			// if (!this.propertyValue) {
			// 	errors.push('Please input a property value.');
			// }

			// If we are not editing a property, prevent overwriting an existing one
			if (!this.propertyToEdit && this.properties[propertyObject.key] !== undefined) {
				errors.push('This property name already exists.');
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.properties[propertyObject.key] = propertyObject.value;
			this.showCreateOrEditPropertyDialog = false;
			this.propertyToEdit = null;
			this.$emit('update:modelValue', this.properties);
		},
	},
};
</script>

<style lang="scss"></style>

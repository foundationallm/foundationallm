<template>
	<div class="d-flex flex-column gap-4">
		<div
			v-for="(propertyValue, propertyKey) in properties"
			:key="propertyKey"
			class="d-flex justify-content-between gap-4"
		>
			<div class="d-flex flex-1 gap-4">
				<!-- Property name -->
				<InputText :value="propertyKey" type="text" placeholder="Property Name" disabled />

				<span class="d-flex align-center">=</span>

				<!-- Property value -->
				<InputText
					:value="propertyValue.value"
					type="text"
					placeholder="Property Value"
					disabled
				/>
			</div>

			<!-- Edit property -->
			<Button link @click="handleEditProperty(propertyKey)">
				<i class="pi pi-cog" style="font-size: 1.2rem"></i>
			</Button>

			<!-- Delete property -->
			<Button link @click="handleDeleteProperty(propertyKey)">
				<i class="pi pi-trash" style="font-size: 1.2rem"></i>
			</Button>
		</div>

		<div class="d-flex gap-4">
			<Dialog
				v-if="showCreateOrEditPropertyDialog"
				:visible="showCreateOrEditPropertyDialog"
				modal
				header="Add Authentication Parameter"
				:style="{ minWidth: '50%' }"
				:closable="false"
			>
				<!-- Property name -->
				<div class="mb-1">Parameter Key:</div>
				<InputText
					v-model="parameterToEdit.key"
					type="text"
					placeholder="Enter parameter key"
					@input="handleParameterKeyInput"
				/>

				<!-- Property type -->
				<div class="mb-1 mt-4">Is the parameter secret?</div>
				<ToggleButton
					v-model="parameterToEdit.secret"
					onIcon="pi pi-lock" 
    			offIcon="pi pi-lock-open"
    			class="w-36"
    			aria-label="Do you confirm"
    		/>
				<!-- <Dropdown
					v-model="parameterToEdit.type"
					:options="propertyTypeOptions"
					option-label="label"
					option-value="value"
					placeholder="--Select--"
				/> -->

				<!-- Property Value -->
				<div class="mb-1 mt-4">Parameter Value:</div>

				<!-- String -->
				<SecretKeyInput
					v-if="parameterToEdit.secret"
					v-model="parameterToEdit.value"
					placeholder="Enter parameter secret value"
					aria-labelledby="aria-api-key"
				/>

				<!-- Number -->
				<InputText v-else v-model="parameterToEdit.value" placeholder="Enter parameter value" />

				<template #footer>
					<!-- Save -->
					<Button severity="primary" label="Save" @click="handleAddProperty" />

					<!-- Cancel -->
					<Button class="ml-2" label="Close" text @click="handleClose" />
				</template>
			</Dialog>

			<!-- Add property -->
			<div class="d-flex w-100 justify-content-end">
				<Button
					label="Add Authentication Parameter"
					severity="primary"
					style="word-wrap: none"
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

	data() {
		return {
			showCreateOrEditPropertyDialog: false,
			propertyToEdit: null,

			properties: {},

			parameterToEdit: {
				key: '',
				value: '',
				type: 'public',
			},

			// propertyType: 'public',
			propertyTypeOptions: [
				{
					label: 'Public',
					value: 'public',
				},
				{
					label: 'Secret',
					value: 'secret',
				},
			],
		};
	},

	watch: {
		modelValue: {
			immediate: true,
			deep: true,
			handler() {
				this.properties = this.modelValue;
			},
		},
	},

	methods: {
		handleParameterKeyInput(event) {
			// Remove spaces and any characters that are not letters, digits, dashes, or underscores.
			let sanitizedValue = event.target.value.replace(/\s/g, '').replace(/[^a-zA-Z0-9-_]/g, '');

			// Ensure the first character is a letter.
			while (sanitizedValue.length > 0 && !/^[a-zA-Z]/.test(sanitizedValue.charAt(0))) {
				sanitizedValue = sanitizedValue.substring(1);
			}

			event.target.value = sanitizedValue;

			this.apiEndpoint.name = sanitizedValue;

			// Check if the name is available if we are creating a new data source.
			if (!this.editId) {
				this.debouncedCheckName();
			}
		},

		handleClose() {
			this.showCreateOrEditPropertyDialog = false;
		},

		handleEditProperty(propertyKey) {
			this.parameterToEdit = {
				currentKey: propertyKey,
				key: propertyKey,
				...this.properties[propertyKey],
			},
			this.showCreateOrEditPropertyDialog = true;
		},

		handleDeleteProperty(propertyKey) {
			delete this.properties[propertyKey];
			this.$emit('update:modelValue', this.properties);
		},

		handleAddProperty() {
			const errors = [];

			if (!this.parameterToEdit.key) {
				errors.push('The parameter requires a key name.');
			}

			if (!this.parameterToEdit.value) {
				errors.push('The parameter requires a value.');
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.properties[this.parameterToEdit.currentKey] = {
				secret: this.parameterToEdit.secret,
				value: this.parameterToEdit.value,
			};
			this.showCreateOrEditPropertyDialog = false;
			this.parameterToEdit = {
				currentKey: '',
				key: '',
				value: '',
				secret: false,
			};
			this.$emit('update:modelValue', this.properties);
		},
	},
};
</script>

<style lang="scss"></style>

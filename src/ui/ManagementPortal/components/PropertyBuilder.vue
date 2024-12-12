<template>
	<div class="span-2 d-flex flex-column" style="gap: 16px">
		<div v-for="(propertyValue, propertyKey) in properties" :key="propertyKey" class="d-flex justify-content-between" style="gap: 16px">

			<div class="d-flex flex-1" style="gap: 16px">
				<!-- Property name -->
				<InputText
					:value="propertyKey"
					type="text"
					placeholder="Property Name"
					disabled
				/>

				<span class="d-flex align-center">=</span>

				<!-- Property value -->
				<InputText
					:value="propertyValue"
					type="text"
					placeholder="Property Value"
					disabled
				/>
			</div>

			<!-- Delete property -->
			<Button link @click="handleDeleteProperty(propertyKey)">
				<i class="pi pi-trash" style="font-size: 1.2rem"></i>
			</Button>
		</div>
	</div>

	<div class="span-2 d-flex mt-4" style="gap: 16px">
		<div class="d-flex flex-1" style="gap: 16px">
			<!-- Property name -->
			<InputText
				v-model="propertyName"
				type="text"
				placeholder="Property Name"
			/>

			<!-- Property value -->
			<InputText
				v-model="propertyValue"
				type="text"
				placeholder="Property Value"
			/>
		</div>

		<!-- Add property -->
		<Button
			label="Add Property"
			severity="primary"
			style="word-wrap: none"
			@click="handleAddProperty"
		/>
	</div>

</template>

<script lang="ts">
export default {
	props: {
		modelValue: {
			type: Object,
			required: true,
		},
	},

	data() {
		return {
			properties: {},
			propertyName: '' as string,
			propertyValue: '' as string,
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
		handleDeleteProperty(propertyName) {
			delete this.properties[propertyName];
			this.$emit('update:modelValue', this.properties);
		},

		handleAddProperty() {
			const errors = [];

			if (!this.propertyName) {
				errors.push('Please input a property name.');
			}

			if (!this.propertyValue) {
				errors.push('Please input a property value.');
			}

			if (this.properties[this.propertyName]) {
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

			this.properties[this.propertyName] = this.propertyValue;
			this.propertyName = '';
			this.propertyValue = '';
			this.$emit('update:modelValue', this.properties);
		},
	},
}
</script>

<style lang="scss">
	
</style>
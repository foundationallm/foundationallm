<template>
	<div class="flex">
		<Button
			type="button"
			icon="pi pi-filter-slash"
			label="Clear"
			outlined
			@click="handleClearFilter"
		/>
		<IconField>
			<InputIcon>
				<i class="pi pi-search" />
			</InputIcon>
			<InputText v-model="filters['global'].value" :placeholder="placeholder" />
		</IconField>
	</div>
</template>

<script lang="ts">
// Define filter match modes
const FilterMatchMode = {
	STARTS_WITH: 'startsWith',
	CONTAINS: 'contains',
	EQUALS: 'equals',
	IN: 'in',
	LESS_THAN: 'lt',
	GREATER_THAN: 'gt',
	LESS_THAN_OR_EQUAL: 'lte',
	GREATER_THAN_OR_EQUAL: 'gte',
	AFTER: 'after',
	BEFORE: 'before',
	DATE_IS: 'dateIs',
	DATE_IS_NOT: 'dateIsNot',
	DATE_BEFORE: 'dateBefore',
	DATE_AFTER: 'dateAfter',
	CUSTOM: 'custom',
};

// Define filter operators
// const FilterOperator = {
// 	AND: 'and',
// 	OR: 'or',
// };

export default {
	props: {
		placeholder: {
			type: String,
			required: false,
			default: 'Search table'
		},

		modelValue: {
			type: Object,
			required: true,
		},
	},

	computed: {
		filters: {
			get() {
				return this.modelValue;
			},
			set(value: any) {
				this.$emit('update:modelValue', value);
			},
		},
	},

	methods: {
		handleClearFilter() {
			this.filters['global'].value = null;
		},
	},
}
</script>
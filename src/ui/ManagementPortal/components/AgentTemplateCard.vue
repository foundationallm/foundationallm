<template>
	<div class="template-card" role="region">
		<div class="template-card__header" @click="toggleExpanded">
			<div class="template-card__header-content">
				<h2>{{ template.display_name || template.name }}</h2>
				<span class="template-card__version">v{{ template.version }}</span>
			</div>
			<i :class="['pi', expanded ? 'pi-chevron-up' : 'pi-chevron-down']"></i>
		</div>
		<p class="template-card__updated">
			<strong>Last Updated:</strong> {{ formattedUpdatedOn }}
		</p>
		
		<div v-if="expanded" class="template-card__details">
			<div class="template-detail">
				<p v-if="template.description"><strong>Description:</strong> {{ template.description }}</p>
				<p><strong>Name:</strong> {{ template.name }}</p>
			</div>
		</div>
	</div>
</template>

<script>
export default {
	props: {
		template: {
			type: Object,
			required: true,
		},
	},

	data() {
		return {
			expanded: false,
		};
	},

	computed: {
		formattedUpdatedOn() {
			if (!this.template.updated_on) return 'N/A';
			const date = new Date(this.template.updated_on);
			return date.toLocaleString();
		},
	},

	methods: {
		toggleExpanded() {
			this.expanded = !this.expanded;
		},
	},
};
</script>

<style scoped>
.template-card {
	width: 100%;
	max-width: 800px;
	border: 1px solid #ddd;
	border-radius: 8px;
	padding: 1rem 1.5rem;
	box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
	background-color: #fff;
	transition: box-shadow 0.3s ease;
}

.template-card:hover {
	box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
}

.template-card__header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	cursor: pointer;
	user-select: none;
}

.template-card__header:hover {
	opacity: 0.8;
}

.template-card__header-content {
	display: flex;
	align-items: center;
	gap: 1rem;
}

.template-card__header i {
	color: #666;
	font-size: 0.875rem;
}

.template-card h2 {
	margin: 0;
	font-size: 0.95rem;
	font-weight: 500;
	color: #444;
}

.template-card__version {
	font-size: 0.8rem;
	color: #666;
	background-color: #f0f0f0;
	padding: 0.125rem 0.5rem;
	border-radius: 4px;
}

.template-card__updated {
	word-break: break-all;
	font-size: 0.8rem;
	margin: 0.5rem 0 0 0;
	color: #666;
}

.template-card__details {
	margin-top: 1rem;
	padding-top: 1rem;
	border-top: 1px solid #eee;
}

.template-card .template-detail p {
	margin: 0.25rem 0;
	font-size: 0.875rem;
}

.template-card .template-detail p strong {
	color: #555;
}
</style>

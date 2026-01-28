<template>
	<div class="plugin-card" role="region">
		<div class="plugin-card__header" @click="toggleExpanded">
			<div class="plugin-card__header-content">
				<h2>{{ plugin.display_name || plugin.name }}</h2>
				<span class="plugin-card__version">v{{ plugin.package_version }}</span>
			</div>
			<i :class="['pi', expanded ? 'pi-chevron-up' : 'pi-chevron-down']"></i>
		</div>
		<p class="plugin-card__updated">
			<strong>Last Updated:</strong> {{ formattedUpdatedOn }}
		</p>
		
		<div v-if="expanded" class="plugin-card__details">
			<div class="plugin-detail">
				<p v-if="plugin.description"><strong>Description:</strong> {{ plugin.description }}</p>
				<p><strong>Platform:</strong> {{ plugin.package_platform }}</p>
				<p><strong>Name:</strong> {{ plugin.name }}</p>
			</div>
		</div>
	</div>
</template>

<script>
export default {
	props: {
		plugin: {
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
			if (!this.plugin.updated_on) return 'N/A';
			const date = new Date(this.plugin.updated_on);
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
.plugin-card {
	width: 100%;
	max-width: 800px;
	border: 1px solid #ddd;
	border-radius: 8px;
	padding: 1rem 1.5rem;
	box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
	background-color: #fff;
	transition: box-shadow 0.3s ease;
}

.plugin-card:hover {
	box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
}

.plugin-card__header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	cursor: pointer;
	user-select: none;
}

.plugin-card__header:hover {
	opacity: 0.8;
}

.plugin-card__header-content {
	display: flex;
	align-items: center;
	gap: 1rem;
}

.plugin-card__header i {
	color: #666;
	font-size: 0.875rem;
}

.plugin-card h2 {
	margin: 0;
	font-size: 0.95rem;
	font-weight: 500;
	color: #444;
}

.plugin-card__version {
	font-size: 0.8rem;
	color: #666;
	background-color: #f0f0f0;
	padding: 0.125rem 0.5rem;
	border-radius: 4px;
}

.plugin-card__updated {
	word-break: break-all;
	font-size: 0.8rem;
	margin: 0.5rem 0 0 0;
	color: #666;
}

.plugin-card__details {
	margin-top: 1rem;
	padding-top: 1rem;
	border-top: 1px solid #eee;
}

.plugin-card .plugin-detail p {
	margin: 0.25rem 0;
	font-size: 0.875rem;
}

.plugin-card .plugin-detail p strong {
	color: #555;
}
</style>

<template>
	<div class="api-status-card" role="region">
		<div class="api-status-card__header" @click="toggleExpanded">
			<div class="api-status-card__header-content">
				<h2>{{ apiName }}</h2>
				<span v-if="!loading && apiStatus" class="api-status-card__version">v{{ apiStatus.version }}</span>
			</div>
			<i :class="['pi', expanded ? 'pi-chevron-up' : 'pi-chevron-down']"></i>
		</div>
		<p class="api-status-card__url">
			<strong>URL:</strong> {{ apiUrl }}
			<button 
				class="copy-button" 
				@click.stop="copyToClipboard(apiUrl)"
				title="Copy to clipboard"
			>
				<i class="pi pi-copy"></i>
			</button>
		</p>
		
		<div v-if="expanded" class="api-status-card__details">
			<div v-if="loading" class="loading" aria-live="polite">Loading...</div>
			<div v-else-if="error" class="error" aria-live="assertive">{{ error }}</div>
			<div v-else-if="apiStatus">
				<div class="api-detail">
					<p><strong>Description:</strong> {{ description }}</p>
					<p><strong>Instance:</strong> {{ apiStatus.instance_name }}</p>
					<p><strong>Status:</strong> {{ apiStatus.status }}</p>
					<p v-if="apiStatus.message"><strong>Message:</strong> {{ apiStatus.message }}</p>
				</div>
				<div v-if="apiStatus.subordinate_services" class="subordinate-services">
					<h3>Subordinate Services</h3>
					<ul>
						<li v-for="service in apiStatus.subordinate_services" :key="service.name">
							<p><strong>Name:</strong> {{ service.name }}</p>
							<p><strong>Instance:</strong> {{ service.instance_name }}</p>
							<p><strong>Version:</strong> {{ service.version }}</p>
							<p><strong>Status:</strong> {{ service.status }}</p>
							<p v-if="service.message"><strong>Message:</strong> {{ service.message }}</p>
						</li>
					</ul>
				</div>
			</div>
			<div v-else>This service does not contain a status endpoint.</div>
		</div>
	</div>
</template>

<script>
export default {
	props: {
		apiName: {
			type: String,
			required: true,
		},
		apiUrl: {
			type: String,
			required: true,
		},
		statusEndpoint: {
			type: [String, null],
			required: false,
			default: '',
		},
		description: {
			type: String,
			required: false,
			default: '',
		},
	},

	data() {
		return {
			apiStatus: null,
			loading: true,
			error: null,
			expanded: false,
		};
	},

	async mounted() {
		await this.fetchApiStatus();
	},

	methods: {
		toggleExpanded() {
			this.expanded = !this.expanded;
		},

		async copyToClipboard(text) {
			try {
				await navigator.clipboard.writeText(text);
			} catch (err) {
				console.error('Failed to copy to clipboard:', err);
			}
		},

		async fetchApiStatus() {
			this.loading = true;
			let fullUrl = this.apiUrl;
			try {
				if (this.statusEndpoint) {
					fullUrl =
						this.apiUrl +
						(this.statusEndpoint ? `/${this.statusEndpoint.replace(/^\/+/, '')}` : '');
					const response = await $fetch(`/api/api-status?url=${encodeURIComponent(fullUrl)}`);

					// const response = await $fetch(fullUrl);
					if (response.error) {
						this.error = response.error;
					} else {
						this.apiStatus = response;
					}
				}
			} catch (error) {
				console.error('Error fetching API status:', error);
				this.error = `Error fetching API status from ${fullUrl}`;
			} finally {
				this.loading = false;
			}
		},
	},
};
</script>

<style scoped>
.api-status-card {
	width: 100%;
	max-width: 800px;
	border: 1px solid #ddd;
	border-radius: 8px;
	padding: 1rem 1.5rem;
	box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
	background-color: #fff;
	transition: box-shadow 0.3s ease;
}

.api-status-card:hover {
	box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
}

.api-status-card__header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	cursor: pointer;
	user-select: none;
}

.api-status-card__header:hover {
	opacity: 0.8;
}

.api-status-card__header-content {
	display: flex;
	align-items: center;
	gap: 1rem;
}

.api-status-card__header i {
	color: #666;
	font-size: 0.875rem;
}

.api-status-card h2 {
	margin: 0;
	font-size: 0.95rem;
	font-weight: 500;
	color: #444;
}

.api-status-card__version {
	font-size: 0.8rem;
	color: #666;
	background-color: #f0f0f0;
	padding: 0.125rem 0.5rem;
	border-radius: 4px;
}

.api-status-card__url {
	word-break: break-all;
	font-size: 0.8rem;
	margin: 0.5rem 0 0 0;
	color: #666;
	display: flex;
	align-items: center;
	gap: 0.5rem;
	flex-wrap: wrap;
}

.copy-button {
	background: none;
	border: none;
	padding: 0.25rem;
	cursor: pointer;
	color: #666;
	border-radius: 4px;
	transition: color 0.2s, background-color 0.2s;
	flex-shrink: 0;
}

.copy-button:hover {
	color: #333;
	background-color: #f0f0f0;
}

.copy-button i {
	font-size: 0.875rem;
}

.api-status-card__details {
	margin-top: 1rem;
	padding-top: 1rem;
	border-top: 1px solid #eee;
}

.api-status-card .api-detail p,
.api-status-card .subordinate-services p {
	margin: 0.25rem 0;
	font-size: 0.875rem;
}

.api-status-card .api-detail p strong,
.api-status-card .subordinate-services p strong {
	color: #555;
}

.api-status-card .loading,
.api-status-card .error {
	color: #672525;
	font-weight: normal;
	font-size: 0.875rem;
}

.api-status-card .status.ready {
	color: #4caf50;
}

.api-status-card .status.error {
	color: #f44336;
}

.api-status-card .subordinate-services {
	margin-top: 0.75rem;
}

.api-status-card .subordinate-services h3 {
	margin-bottom: 0.5rem;
	font-size: 1rem;
	color: #555;
}

.api-status-card .subordinate-services ul {
	padding-left: 1.25rem;
}

.api-status-card .subordinate-services ul li {
	margin-bottom: 0.5rem;
	list-style-type: disc;
}

@media (max-width: 600px) {
	.api-status-card h2 {
		font-size: 1rem;
	}

	.error {
		word-break: break-all;
	}
}
</style>

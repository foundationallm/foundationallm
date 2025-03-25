<template>
	<div v-if="content && !content.skip" class="message-content">
		<!-- Image -->
		<div v-if="content.type === 'image_file'">
			<template v-if="loading || (!error && !content.blobUrl)">
				<div class="loading-content-container">
					<i class="pi pi-image loading-content-icon" style="font-size: 2rem"></i>
					<i class="pi pi-spin pi-spinner loading-content-icon" style="font-size: 1rem"></i>
					<span class="loading-content-text">Loading image...</span>
				</div>
			</template>
			<Image
				v-if="content.blobUrl"
				:src="content.blobUrl"
				:alt="content.fileName"
				width="45%"
				preview
				@load="loading = false"
				@error="
					loading = false;
					error = true;
				"
			/>
			<div v-if="error" class="loading-content-error">
				<i class="pi pi-times-circle loading-content-error-icon" style="font-size: 2rem"></i>
				<span class="loading-content-error-text">Could not load image</span>
			</div>
		</div>

		<!-- HTML -->
		<div v-else-if="content.type === 'html'">
			<template v-if="loading || !content.blobUrl">
				<div class="loading-content-container">
					<i class="pi pi-chart-line loading-content-icon" style="font-size: 2rem"></i>
					<i class="pi pi-spin pi-spinner loading-content-icon" style="font-size: 1rem"></i>
					<span class="loading-content-text">Loading visualization...</span>
				</div>
			</template>
			<iframe v-if="content.blobUrl" :src="content.blobUrl" frameborder="0"></iframe>
		</div>

		<!-- File -->
		<div v-else-if="content.type === 'file_path'">
			<i :class="$getFileIconClass(content.fileName, true)" class="attachment-icon"></i>
			<a
				:download="content.fileName ?? content.blobUrl ?? content.origValue"
				href="#"
				target="_blank"
				@click.prevent="handleFileDownload(content)"
			>{{ content.fileName ?? content.blobUrl ?? content.origValue }}
			</a>
		</div>
	</div>
</template>

<script>
import api from '@/js/api';
import { fetchBlobUrl } from '@/js/fileService';
import { useAppStore } from '@/stores/appStore';

export default {
	props: {
		value: {
			type: Object,
			required: false,
			default: null,
		},
	},

	data() {
		return {
			content: null,
			loading: false,
			error: false,
		};
	},

	watch: {
		value: {
			immediate: true,
			handler() {
				this.loadFile();
			},
		},
	},

	methods: {
		async loadFile() {
			this.content = this.value;

			// File is still generating
			if (!this.content.origValue) {
				this.loading = false;
				return;
			}

			if (['image_file', 'html', 'file_path'].includes(this.content.type)) {
				this.loading = true;
				this.content.fileName = this.content.fileName?.split('/').pop();
				try {
					if (this.content.type !== 'file_path') {
						const response = await api.fetchDirect(this.content.origValue);
						if (this.content.type === 'html') {
							const blob = new Blob([response], { type: 'text/html' });
							this.content.blobUrl = URL.createObjectURL(blob);
						} else if (this.content.type === 'image_file') {
							this.content.blobUrl = URL.createObjectURL(response);
							// Update the blob URL in the Pinia store
							const appStore = useAppStore();
							const messageIndex = appStore.currentMessages.findIndex(message => 
								message.content?.some(content => 
									content.type === 'image_file' && content.origValue === this.content.origValue
								)
							);
							
							if (messageIndex !== -1) {
								const contentIndex = appStore.currentMessages[messageIndex].content.findIndex(content =>
									content.type === 'image_file' && content.origValue === this.content.origValue
								);
								
								if (contentIndex !== -1) {
									appStore.currentMessages[messageIndex].content[contentIndex].blobUrl = this.content.blobUrl;
								}
							}
						}
					}
				} catch (error) {
					console.error(`Failed to fetch content from ${this.content.origValue}`, error);
					this.error = true;
				}
				this.loading = false;
			}
		},

		// handleFileDownloadLinkClick(event) {
		// 	const link = event.target.closest('a.file-download-link');
		// 	if (link && link.dataset.href) {
		// 		event.preventDefault();
		// 		this.fetchBlobUrlFromHref(link.dataset.href);
		// 	}
		// },

		// async fetchBlobUrlFromHref(href) {
		// 	const content = {
		// 		value: href,
		// 		blobUrl: null,
		// 		fileName: href.split('/').pop(),
		// 	};

		// 	await this.fetchBlobUrl(content);
		// },

		async handleFileDownload(content) {
			await fetchBlobUrl(content);
		},
	},
};
</script>

<style lang="scss" scoped>
.message-content {
	margin-top: 5px;
	margin-bottom: 5px;
}

img {
	max-width: 100%;
	height: auto;
	border-radius: 8px;
}

iframe {
	width: 100%;
	height: 600px;
	border-radius: 8px;
}

.loading-content-container {
	display: flex;
	align-items: center;

	.loading-content-icon {
		margin-right: 8px;
		vertical-align: middle;
		line-height: 1;
	}

	.loading-content-text {
		font-size: 0.75rem;
		font-style: italic;
		line-height: 1.5;
	}
}

.loading-content-error {
	display: flex;
	align-items: center;
	width: 200px;
	padding: 8px 12px;
	border-radius: 0.75rem;
	border-color: rgb(182, 2, 2);
	color: rgb(182, 2, 2);
	box-shadow: 0 1px 3px rgba(182, 2, 2, 0.664);

	.loading-content-error-icon {
		margin-right: 8px;
		vertical-align: middle;
		line-height: 1;
	}

	.loading-content-error-text {
		font-size: 0.85rem;
		font-style: italic;
		line-height: 1.5;
	}
}
</style>

import { useAppStore } from '@/stores/appStore';
import api from '@/js/api';
import type { MessageContent } from '@/js/types';

export async function fetchBlobUrl(content: MessageContent) {
	const appStore = useAppStore();
	if (!content.blobUrl) {
		content.loading = true;
		try {
			const response = await api.fetchDirect(content.origValue);
			const blob = new Blob([response], { type: response.type });
			content.blobUrl = URL.createObjectURL(blob);
		} catch (error) {
			console.error(`Failed to fetch content from ${content.origValue}`, error);
			appStore.addToast({
				severity: 'error',
				summary: 'Error downloading file',
				detail: `Failed to download "${content.fileName}".`,
				life: 5000
			});
			content.error = true;
		} finally {
			content.loading = false;
		}
	}

	if (content.blobUrl) {
		const link = document.createElement('a');
		link.href = content.blobUrl;
		link.download = content.fileName;
		document.body.appendChild(link);
		link.click();
		document.body.removeChild(link);
	}
}

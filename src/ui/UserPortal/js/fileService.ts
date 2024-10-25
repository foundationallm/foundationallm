import api from '@/js/api';
import type { MessageContent } from '@/js/types';
import { useAppStore } from '../stores/appStore';

export async function fetchBlobUrl(content: MessageContent, toast: any) {
	const appStore = useAppStore();
	if (!content.blobUrl) {
		content.loading = true;
		try {
			const response = await api.fetchDirect(content.value);
			const blob = new Blob([response], { type: response.type });
			content.blobUrl = URL.createObjectURL(blob);
		} catch (error) {
			console.error(`Failed to fetch content from ${content.value}`, error);
			toast.add({
				severity: 'error',
				summary: 'Error downloading file',
				detail: `Failed to download "${content.fileName}". ${error.message || error}`,
				life: appStore.autoHideToasts ? 5000 : null,
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

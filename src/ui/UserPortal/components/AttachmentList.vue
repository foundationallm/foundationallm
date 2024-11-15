<template>
	<div v-if="attachments && attachments.length > 0" class="attachments">
		<div v-for="attachment in attachments" :key="attachment.objectId" class="attachment-item">
			<i :class="$getFileIconClass(attachment.displayName, false)" class="attachment-icon"></i>
			<span class="attachment-name">{{ attachment.displayName }}</span>
		</div>
	</div>
	<div v-else-if="attachmentIds && attachmentIds.length > 0" class="attachments">
		<div v-for="attachmentId in attachmentIds" :key="attachmentId" class="attachment-item">
			<i class="pi pi-exclamation-triangle attachment-icon"></i>
			<span class="no-attachment-name">File no longer available</span>
		</div>
	</div>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { AttachmentDetail } from '@/js/types';

export default defineComponent({
	name: 'AttachmentList',
	props: {
		attachments: {
			type: Array as PropType<Array<AttachmentDetail>>,
			required: true,
		},
		attachmentIds: {
			type: Array as PropType<Array<string>>,
			required: false,
			default: () => [],
		},
	},
});
</script>

<style scoped>
.attachments {
	display: flex;
	flex-wrap: wrap;
	gap: 10px;
	margin-top: 10px;
}

.attachment-item {
	display: flex;
	align-items: center;
	padding: 8px 12px;
	border-radius: 0.75rem;
	background-color: var(--accent-color);
	color: var(--accent-text);
	box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.attachment-icon {
	width: 24px;
	margin-right: 8px;
	vertical-align: middle;
	line-height: 1;
}

.attachment-name {
	font-size: 14px;
	line-height: 1.5;
}

.no-attachment-name {
	font-size: 14px;
	line-height: 1.5;
	font-style: italic;
}
</style>

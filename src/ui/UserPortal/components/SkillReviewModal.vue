<template>
	<Dialog
		v-model:visible="visible"
		:header="modalTitle"
		modal
		:style="{ minWidth: '80%', maxWidth: '90%' }"
		:closable="true"
	>
		<div v-if="skillData" class="skill-review-modal">
			<!-- Skill Info -->
			<div class="skill-info mb-4">
				<div class="flex align-items-center gap-2 mb-2">
					<i :class="iconClass" style="font-size: 1.5rem; color: var(--primary-color)"></i>
					<h3 class="m-0">{{ skillData.name || 'Unknown Skill' }}</h3>
				</div>
				<div v-if="skillData.description" class="text-color-secondary mb-3">
					{{ skillData.description }}
				</div>
				<div v-if="isSkillUsed && skillData.execution_count !== undefined" class="flex gap-4 text-sm">
					<span><strong>Used:</strong> {{ skillData.execution_count }} times</span>
					<span v-if="skillData.success_rate !== undefined">
						<strong>Success Rate:</strong> {{ (skillData.success_rate * 100).toFixed(1) }}%
					</span>
					<span v-if="skillData.status">
						<strong>Status:</strong> {{ skillData.status }}
					</span>
				</div>
			</div>

			<!-- Code Viewer -->
			<div class="code-viewer mb-4">
				<label class="font-semibold mb-2 block">Python Code:</label>
				<div class="code-container">
					<pre><code class="language-python">{{ skillData.code || 'No code available' }}</code></pre>
				</div>
			</div>

			<!-- Parameters (if available) -->
			<div v-if="skillData.parameters && skillData.parameters.length > 0" class="parameters mb-4">
				<label class="font-semibold mb-2 block">Parameters:</label>
				<ul class="list-none p-0 m-0">
					<li v-for="param in skillData.parameters" :key="param.name" class="mb-2 p-2 border-round" style="background-color: var(--surface-ground);">
						<strong>{{ param.name }}</strong> ({{ param.type }})
						<span v-if="param.description" class="text-color-secondary"> - {{ param.description }}</span>
						<span v-if="param.required === false" class="text-color-secondary"> (optional)</span>
					</li>
				</ul>
			</div>
		</div>

		<div v-else-if="loading" class="text-center p-4">
			<i class="pi pi-spin pi-spinner" style="font-size: 2rem"></i>
			<p>Loading skill details...</p>
		</div>

		<div v-else-if="error" class="text-center p-4">
			<i class="pi pi-exclamation-triangle" style="font-size: 2rem; color: var(--red-500)"></i>
			<p class="text-red-500">{{ error }}</p>
		</div>

		<template #footer>
			<Button
				v-if="isSkillSaved"
				severity="success"
				label="Approve"
				icon="pi pi-check"
				:loading="processing"
				@click="handleApprove"
			/>
			<Button
				v-if="isSkillSaved"
				severity="danger"
				label="Reject"
				icon="pi pi-times"
				:loading="processing"
				class="ml-2"
				@click="handleReject"
			/>
			<Button
				v-if="isSkillUsed"
				severity="success"
				label="Keep Skill"
				icon="pi pi-check"
				:loading="processing"
				@click="handleKeep"
			/>
			<Button
				v-if="isSkillUsed"
				severity="danger"
				label="Remove Skill"
				icon="pi pi-trash"
				:loading="processing"
				class="ml-2"
				@click="handleRemove"
			/>
			<Button
				label="Close"
				text
				class="ml-2"
				:disabled="processing"
				@click="handleClose"
			/>
		</template>
	</Dialog>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { ContentArtifact } from '@/js/types';
import Button from 'primevue/button';
import Dialog from 'primevue/dialog';
import api from '@/js/api';

export default defineComponent({
	name: 'SkillReviewModal',
	components: {
		Button,
		Dialog,
	},
	props: {
		artifact: {
			type: Object as PropType<ContentArtifact | null>,
			required: false,
			default: null,
		},
		visible: {
			type: Boolean,
			required: false,
			default: false,
		},
	},
	emits: ['update:visible', 'skill-approved', 'skill-rejected', 'skill-kept', 'skill-removed'],
	data() {
		return {
			skillData: null as any,
			loading: false,
			error: null as string | null,
			processing: false,
		};
	},
	computed: {
		isSkillSaved(): boolean {
			return this.artifact?.type === 'skill_saved';
		},
		isSkillUsed(): boolean {
			return this.artifact?.type === 'skill_used';
		},
		modalTitle(): string {
			if (this.isSkillSaved) {
				return `🔧 Skill Saved: ${this.artifact?.metadata?.skill_name || 'Unknown'}`;
			}
			return `⚡ Skill Used: ${this.artifact?.metadata?.skill_name || 'Unknown'}`;
		},
		iconClass(): string {
			return this.isSkillSaved ? 'pi pi-wrench' : 'pi pi-bolt';
		},
	},
	watch: {
		artifact: {
			immediate: true,
			async handler(newArtifact) {
				if (newArtifact && this.visible) {
					await this.loadSkillDetails();
				}
			},
		},
		visible: {
			immediate: true,
			async handler(newVisible) {
				if (newVisible && this.artifact) {
					await this.loadSkillDetails();
				} else {
					// Reset when closed
					this.skillData = null;
					this.error = null;
				}
			},
		},
	},
	methods: {
		async loadSkillDetails() {
			if (!this.artifact) return;

			this.loading = true;
			this.error = null;

			try {
				// Try to get skill ID from filepath or metadata
				const skillId = this.artifact.filepath || (this.artifact.metadata?.skill_id as string);

				if (!skillId) {
					// If no skill ID, use metadata from artifact
					this.skillData = {
						name: (this.artifact.metadata?.skill_name as string) || 'Unknown',
						description: (this.artifact.metadata?.skill_description as string) || '',
						code: (this.artifact.metadata?.skill_code as string) || '',
						status: (this.artifact.metadata?.skill_status as string) || 'Active',
						execution_count: (this.artifact.metadata?.execution_count as number) || 0,
						success_rate: (this.artifact.metadata?.success_rate as number) || 1.0,
						parameters: (this.artifact.metadata?.parameters as any[]) || [],
					};
				} else {
					// Fetch skill details from API
					const skill = await api.getSkill(skillId);
					this.skillData = skill;
				}
			} catch (err: any) {
				console.error('Error loading skill details:', err);
				this.error = err.message || 'Failed to load skill details';
				// Fallback to metadata if API call fails
				if (this.artifact.metadata) {
					this.skillData = {
						name: (this.artifact.metadata.skill_name as string) || 'Unknown',
						description: (this.artifact.metadata.skill_description as string) || '',
						code: (this.artifact.metadata.skill_code as string) || '',
						status: (this.artifact.metadata.skill_status as string) || 'Active',
						execution_count: (this.artifact.metadata.execution_count as number) || 0,
						success_rate: (this.artifact.metadata.success_rate as number) || 1.0,
						parameters: (this.artifact.metadata.parameters as any[]) || [],
					};
				}
			} finally {
				this.loading = false;
			}
		},

		async handleApprove() {
			if (!this.artifact) return;

			this.processing = true;
			try {
				const skillId = this.artifact.filepath || (this.artifact.metadata?.skill_id as string);
				if (skillId) {
					await api.approveSkill(skillId);
				}
				(this.$appStore as any).addToast({
					severity: 'success',
					life: 3000,
					detail: 'Skill approved and ready to use',
				});
				this.$emit('skill-approved', this.artifact);
				this.$emit('update:visible', false);
			} catch (err: any) {
				console.error('Error approving skill:', err);
				(this.$appStore as any).addToast({
					severity: 'error',
					life: 5000,
					detail: err.message || 'Failed to approve skill',
				});
			} finally {
				this.processing = false;
			}
		},

		async handleReject() {
			if (!this.artifact) return;

			this.processing = true;
			try {
				const skillId = this.artifact.filepath || (this.artifact.metadata?.skill_id as string);
				if (skillId) {
					await api.deleteSkill(skillId);
				}
				(this.$appStore as any).addToast({
					severity: 'success',
					life: 3000,
					detail: 'Skill rejected and removed',
				});
				this.$emit('skill-rejected', this.artifact);
				this.$emit('update:visible', false);
			} catch (err: any) {
				console.error('Error rejecting skill:', err);
				(this.$appStore as any).addToast({
					severity: 'error',
					life: 5000,
					detail: err.message || 'Failed to reject skill',
				});
			} finally {
				this.processing = false;
			}
		},

		async handleKeep() {
			if (!this.artifact) return;

			this.processing = true;
			try {
				// Keep skill - no API call needed, just acknowledge
				(this.$appStore as any).addToast({
					severity: 'success',
					life: 3000,
					detail: 'Skill will continue to be used',
				});
				this.$emit('skill-kept', this.artifact);
				this.$emit('update:visible', false);
			} catch (err: any) {
				console.error('Error keeping skill:', err);
				(this.$appStore as any).addToast({
					severity: 'error',
					life: 5000,
					detail: err.message || 'Failed to keep skill',
				});
			} finally {
				this.processing = false;
			}
		},

		async handleRemove() {
			if (!this.artifact) return;

			this.processing = true;
			try {
				const skillId = this.artifact.filepath || (this.artifact.metadata?.skill_id as string);
				if (skillId) {
					await api.deleteSkill(skillId);
				}
				(this.$appStore as any).addToast({
					severity: 'success',
					life: 3000,
					detail: 'Skill removed - agent will generate new code next time',
				});
				this.$emit('skill-removed', this.artifact);
				this.$emit('update:visible', false);
			} catch (err: any) {
				console.error('Error removing skill:', err);
				(this.$appStore as any).addToast({
					severity: 'error',
					life: 5000,
					detail: err.message || 'Failed to remove skill',
				});
			} finally {
				this.processing = false;
			}
		},

		handleClose() {
			this.$emit('update:visible', false);
		},
	},
});
</script>

<style lang="scss" scoped>
.skill-review-modal {
	.code-viewer {
		.code-container {
			background-color: var(--surface-ground);
			border: 1px solid var(--surface-border);
			border-radius: 4px;
			padding: 16px;
			overflow-x: auto;
			max-height: 400px;
			overflow-y: auto;

			pre {
				margin: 0;
				font-family: 'Courier New', monospace;
				font-size: 0.9rem;
				line-height: 1.5;
				white-space: pre-wrap;
				word-wrap: break-word;
			}

			code {
				color: var(--text-color);
			}
		}
	}

	.parameters {
		ul {
			list-style: none;
		}
	}
}
</style>

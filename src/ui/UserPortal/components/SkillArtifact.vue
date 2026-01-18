<template>
	<div class="skill-artifact" :class="artifactClass">
		<div class="skill-artifact__header" @click="handleViewSkill">
			<i :class="iconClass" class="skill-artifact__icon"></i>
			<span class="skill-artifact__title">{{ artifactTitle }}</span>
			<Button
				class="skill-artifact__view-button"
				size="small"
				text
				label="View Skill"
				icon="pi pi-eye"
				@click.stop="handleViewSkill"
			/>
		</div>
		<div v-if="artifact.metadata?.skill_description" class="skill-artifact__description">
			{{ artifact.metadata.skill_description }}
		</div>
	</div>
</template>

<script lang="ts">
import { defineComponent, PropType } from 'vue';
import type { ContentArtifact } from '@/js/types';
import Button from 'primevue/button';

export default defineComponent({
	name: 'SkillArtifact',
	components: {
		Button,
	},
	props: {
		artifact: {
			type: Object as PropType<ContentArtifact>,
			required: true,
		},
	},
	emits: ['view-skill'],
	computed: {
		isSkillSaved(): boolean {
			return this.artifact.type === 'skill_saved';
		},
		isSkillUsed(): boolean {
			return this.artifact.type === 'skill_used';
		},
		artifactTitle(): string {
			if (this.artifact.title) {
				return this.artifact.title;
			}
			const skillName = this.artifact.metadata?.skill_name as string || 'Unknown Skill';
			return this.isSkillSaved ? `🔧 Skill Saved: ${skillName}` : `⚡ Skill Used: ${skillName}`;
		},
		iconClass(): string {
			return this.isSkillSaved ? 'pi pi-wrench' : 'pi pi-bolt';
		},
		artifactClass(): string {
			return {
				'skill-artifact--saved': this.isSkillSaved,
				'skill-artifact--used': this.isSkillUsed,
			};
		},
	},
	methods: {
		handleViewSkill() {
			this.$emit('view-skill', this.artifact);
		},
	},
});
</script>

<style lang="scss" scoped>
.skill-artifact {
	border: 1px solid var(--surface-border);
	border-radius: 8px;
	padding: 12px;
	margin: 8px 0;
	background-color: var(--surface-card);
	cursor: pointer;
	transition: all 0.2s ease;

	&:hover {
		background-color: var(--surface-hover);
		border-color: var(--primary-color);
	}

	&--saved {
		border-left: 4px solid var(--blue-500);
	}

	&--used {
		border-left: 4px solid var(--orange-500);
	}

	&__header {
		display: flex;
		align-items: center;
		gap: 8px;
		font-weight: 600;
	}

	&__icon {
		font-size: 1.2rem;
		color: var(--primary-color);
	}

	&__title {
		flex: 1;
		font-size: 0.95rem;
	}

	&__view-button {
		font-size: 0.85rem;
	}

	&__description {
		margin-top: 8px;
		font-size: 0.85rem;
		color: var(--text-color-secondary);
		line-height: 1.4;
	}
}
</style>

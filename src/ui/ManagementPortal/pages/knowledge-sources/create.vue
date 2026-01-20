<template>
	<main id="main-content">
		<div style="display: flex">
			<!-- Title -->
			<div style="flex: 1">
				<h2 class="page-header">
					{{ editId ? 'Edit Knowledge Source' : 'Create Knowledge Source' }}
				</h2>
				<div class="page-subheader">
					{{
						editId
							? 'Edit your knowledge source below.'
							: 'Complete the settings below to create a new knowledge source.'
					}}
				</div>
			</div>

			<!-- Edit access control -->
			<AccessControl
				v-if="editId"
				:scopes="[
					{
						label: 'Knowledge Source',
						value: `providers/FoundationaLLM.Context/knowledgeSources/${editId}`,
					},
				]"
			/>
		</div>

		<div class="steps">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay" role="status" aria-live="polite" aria-label="Loading knowledge source form">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Name -->
			<div class="col-span-2">
				<div id="aria-knowledge-source-name" class="step-header !mb-2">Knowledge Source name:</div>
				<div id="aria-knowledge-source-name-desc" class="mb-2">
					{{ editId ? 'The name cannot be changed after creation.' : 'No special characters or spaces, use letters and numbers with dashes and underscores only.' }}
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="knowledgeSource.name"
						:disabled="!!editId"
						type="text"
						class="w-full"
						placeholder="Enter knowledge source name"
						aria-labelledby="aria-knowledge-source-name aria-knowledge-source-name-desc"
						@input="handleNameInput"
					/>
					<span
						v-if="!editId && nameValidationStatus"
						class="ml-2"
						:title="validationMessage"
						style="font-size: 1.2rem"
						:aria-label="validationMessage"
					>
						<i v-if="nameValidationStatus === 'valid'" class="pi pi-check" style="color: green"></i>
						<i v-else-if="nameValidationStatus === 'invalid'" class="pi pi-times" style="color: red"></i>
					</span>
				</div>
			</div>

			<!-- Description -->
			<div class="col-span-2">
				<div class="step-header !mb-2">Description:</div>
				<div id="aria-description" class="mb-2">
					Provide a description to help others understand the knowledge source's purpose.
				</div>
				<InputText
					v-model="knowledgeSource.description"
					type="text"
					class="w-full"
					placeholder="Enter description"
					aria-labelledby="aria-description"
				/>
			</div>

			<!-- Knowledge Units -->
			<div class="col-span-2">
				<div class="step-header !mb-2">Knowledge Units:</div>
				<div id="aria-knowledge-units" class="mb-2">
					Select the knowledge units associated with this knowledge source.
				</div>
				<MultiSelect
					v-model="knowledgeSource.knowledge_unit_object_ids"
					:options="knowledgeUnitOptions"
					option-label="name"
					option-value="object_id"
					placeholder="Select knowledge units"
					class="w-full"
					aria-labelledby="aria-knowledge-units"
					display="chip"
					filter
					:maxSelectedLabels="5"
				/>
			</div>
		</div>

		<!-- Buttons -->
		<div class="flex justify-end gap-4 mt-6">
			<!-- Create/Save knowledge source -->
			<Button
				:label="editId ? 'Save Changes' : 'Create Knowledge Source'"
				severity="primary"
				@click="handleSave"
			/>

			<!-- Cancel -->
			<Button label="Cancel" severity="secondary" @click="handleCancel" />
		</div>
	</main>
</template>

<script lang="ts">
import api from '@/js/api';
import { debounce } from 'lodash';

export default {
	name: 'CreateKnowledgeSource',

	props: {
		editId: {
			type: String,
			default: null,
		},
	},

	data() {
		return {
			knowledgeSource: {
				name: '',
				description: '',
				type: 'knowledge-source',
				object_id: '',
				knowledge_unit_object_ids: [] as string[],
			},
			knowledgeUnitOptions: [] as { name: string; object_id: string }[],
			loading: false,
			loadingStatusText: 'Loading...',
			nameValidationStatus: null as 'valid' | 'invalid' | null,
			validationMessage: null as string | null,
			debouncedCheckName: null as any,
		};
	},

	async created() {
		if (this.editId) {
			await this.loadKnowledgeSource(this.editId);
		} else {
			await this.loadKnowledgeUnits();
		}

		this.debouncedCheckName = debounce(this.checkName, 500);
	},

	methods: {
		async checkName() {
			try {
				const response = await api.checkKnowledgeSourceName(this.knowledgeSource.name);

				// Handle response based on the status
				if (response.status === 'Allowed') {
					// Name is available
					this.nameValidationStatus = 'valid';
					this.validationMessage = null;
				} else if (response.status === 'Denied') {
					// Name is taken
					this.nameValidationStatus = 'invalid';
					this.validationMessage = response.error_message;
				}
			} catch (error) {
				console.error('Error checking knowledge source name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the knowledge source name. Please try again.';
			}
		},

		handleNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.knowledgeSource.name = sanitizedValue;

			// Check if the name is available if we are creating a new knowledge source.
			if (!this.editId) {
				this.debouncedCheckName();
			}
		},

		async loadKnowledgeUnits() {
			this.loading = true;
			this.loadingStatusText = 'Loading knowledge units...';
			try {
				const knowledgeUnitsResult = await api.getKnowledgeUnits();
				if (knowledgeUnitsResult && Array.isArray(knowledgeUnitsResult)) {
					this.knowledgeUnitOptions = knowledgeUnitsResult
						.map((item: any) => ({
							name: item.resource?.name || item.name || 'Unknown',
							object_id: item.resource?.object_id || item.object_id || '',
						}))
						.sort((a, b) => a.name.localeCompare(b.name));
				}
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async loadKnowledgeSource(knowledgeSourceName: string) {
			this.loading = true;
			this.loadingStatusText = 'Loading knowledge source...';
			try {
				// Load knowledge units and knowledge source in parallel
				const [knowledgeUnitsResult, knowledgeSourceResult] = await Promise.all([
					api.getKnowledgeUnits(),
					api.getKnowledgeSource(knowledgeSourceName),
				]);

				// Process knowledge unit options
				if (knowledgeUnitsResult && Array.isArray(knowledgeUnitsResult)) {
					this.knowledgeUnitOptions = knowledgeUnitsResult
						.map((item: any) => ({
							name: item.resource?.name || item.name || 'Unknown',
							object_id: item.resource?.object_id || item.object_id || '',
						}))
						.sort((a, b) => a.name.localeCompare(b.name));
				}

				// Process knowledge source
				if (knowledgeSourceResult && knowledgeSourceResult.length > 0) {
					this.knowledgeSource = {
						...this.knowledgeSource,
						...knowledgeSourceResult[0].resource,
					};
				}
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async handleSave() {
			// Validate required fields
			if (!this.knowledgeSource.name) {
				this.$toast.add({
					severity: 'warn',
					detail: 'Knowledge source name is required.',
					life: 3000,
				});
				return;
			}

			if (!this.editId && this.nameValidationStatus === 'invalid') {
				this.$toast.add({
					severity: 'warn',
					detail: this.validationMessage || 'The knowledge source name is not available.',
					life: 3000,
				});
				return;
			}

			if (!this.knowledgeSource.knowledge_unit_object_ids || this.knowledgeSource.knowledge_unit_object_ids.length === 0) {
				this.$toast.add({
					severity: 'warn',
					detail: 'At least one knowledge unit is required.',
					life: 3000,
				});
				return;
			}

			this.loading = true;
			this.loadingStatusText = this.editId ? 'Updating knowledge source...' : 'Creating knowledge source...';
			
			try {
				await api.createOrUpdateKnowledgeSource(this.knowledgeSource.name, this.knowledgeSource);
				this.$toast.add({
					severity: 'success',
					detail: this.editId ? 'Knowledge source updated successfully.' : 'Knowledge source created successfully.',
					life: 3000,
				});
				this.$router.push('/knowledge-sources');
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		handleCancel() {
			this.$router.push('/knowledge-sources');
		},
	},
};
</script>

<style lang="scss" scoped>
.steps {
	display: grid;
	grid-template-columns: 1fr 1fr;
	gap: 1rem;
	margin-top: 2rem;
	position: relative;
}

.steps__loading-overlay {
	position: absolute;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	display: flex;
	flex-direction: column;
	justify-content: center;
	align-items: center;
	gap: 16px;
	z-index: 10;
	background-color: rgba(255, 255, 255, 0.9);
	pointer-events: none;
}

.input-wrapper {
	position: relative;
	display: flex;
	align-items: center;
}

.col-span-2 {
	grid-column: span 2;
}
</style>

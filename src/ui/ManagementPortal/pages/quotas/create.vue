<template>
	<div>
		<!-- Header -->
		<h2 class="page-header">{{ editId ? 'Edit Quota' : 'Create Quota' }}</h2>
		<div class="page-subheader">
			{{
				editId
					? 'Edit your quota settings below.'
					: 'Complete the settings below to configure a new rate limit quota.'
			}}
		</div>

		<div class="quota-editor" :class="{ 'quota-editor--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="quota-editor__loading-overlay" role="status" aria-live="polite" aria-label="Loading quota form">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<div class="quota-editor__grid">
				<!-- Left: Form -->
				<div class="quota-editor__form">
					<!-- Basics -->
					<section class="section-card">
						<div class="section-card__header">
							<div class="section-card__title">Basics</div>
							<div class="section-card__subtitle">Name + description for admins.</div>
						</div>

						<div class="field">
							<label class="field__label">Name</label>

							<template v-if="editId">
								<div class="readonly-row">
									<code class="readonly-row__value">{{ quota.name }}</code>
									<Button
										icon="pi pi-copy"
										severity="secondary"
										text
										aria-label="Copy quota name"
										@click="copyToClipboard(quota.name)"
									/>
								</div>
								<div class="field__hint">Quota name can’t be changed after creation.</div>
							</template>

							<template v-else>
								<div id="aria-name-desc" class="field__hint">
									Letters/numbers with dashes and underscores only.
								</div>
								<div class="input-wrapper">
									<InputText
										v-model="quota.name"
										type="text"
										class="w-full"
										placeholder="e.g. coreapi-completions-per-user"
										aria-labelledby="aria-name aria-name-desc"
										@input="handleNameInput"
										@blur="touch('name')"
									/>
									<span
										v-if="nameValidationStatus === 'valid'"
										class="icon valid"
										title="Name is available"
									>
										✔️
									</span>
									<span
										v-else-if="nameValidationStatus === 'invalid'"
										:title="validationMessage"
										class="icon invalid"
									>
										❌
									</span>
								</div>
								<div v-if="shouldShowError('name')" class="field__error">{{ validationErrors.name }}</div>
							</template>
						</div>

						<div class="field">
							<label class="field__label">Description (optional)</label>
							<Textarea
								v-model="quota.description"
								class="w-full"
								rows="3"
								placeholder="Optional notes about the intent of this quota"
							/>
						</div>
					</section>

					<!-- Scope -->
					<section class="section-card">
						<div class="section-card__header">
							<div class="section-card__title">Scope</div>
							<div class="section-card__subtitle">What requests does this apply to?</div>
						</div>

						<div class="field">
							<label class="field__label">Quota type</label>
							<div class="field__hint">
								<strong>Raw Request Rate Limit</strong>: all API requests to a controller. <br>
								<strong>Agent Request Rate Limit</strong>: completion requests to a specific agent (optional).
							</div>
							<Dropdown
								v-model="quota.quota_type"
								:options="quotaTypeOptions"
								option-label="label"
								option-value="value"
								placeholder="Select quota type"
								class="w-full"
								@change="onTypeChange"
								@blur="touch('quota_type')"
							/>
							<div v-if="shouldShowError('quota_type')" class="field__error">{{ validationErrors.quota_type }}</div>
						</div>

						<div class="field">
							<label class="field__label">Context</label>
							<div class="field__hint">
								Pick the API + controller. For agent quotas you can optionally target one agent.
							</div>

							<div class="context-builder">
								<div class="context-field">
									<label>API</label>
									<Dropdown
										v-model="contextApi"
										:options="apiOptions"
										placeholder="Select API"
										class="w-full"
										@change="buildContext"
										@blur="touch('context')"
									/>
								</div>
								<div class="context-field">
									<label>Controller</label>
									<Dropdown
										v-model="contextController"
										:options="controllerOptions"
										placeholder="Select Controller"
										class="w-full"
										@change="buildContext"
										@blur="touch('context')"
									/>
								</div>
								<div v-if="quota.quota_type === 'AgentRequestRateLimit'" class="context-field">
									<label>Agent (optional)</label>
									<InputText
										v-model="contextAgent"
										type="text"
										class="w-full"
										placeholder="Leave blank for all agents"
										@input="buildContext"
									/>
								</div>
							</div>

							<div class="chip-row">
								<Chip :label="`API: ${contextApi || '—'}`" />
								<Chip :label="`Controller: ${contextController || '—'}`" />
								<Chip
									v-if="quota.quota_type === 'AgentRequestRateLimit'"
									:label="`Agent: ${contextAgent || 'Any'}`"
								/>
							</div>

							<div v-if="shouldShowError('context')" class="field__error">{{ validationErrors.context }}</div>

							<details class="details">
								<summary>Raw context value</summary>
								<div class="details__body">
									<code>{{ quota.context || '(not set)' }}</code>
								</div>
							</details>
						</div>
					</section>

					<!-- Limit -->
					<section class="section-card">
						<div class="section-card__header">
							<div class="section-card__title">Limit</div>
							<div class="section-card__subtitle">How many requests are allowed?</div>
						</div>

						<div class="field">
							<label class="field__label">Quick presets (optional)</label>
							<Dropdown
								v-model="selectedRatePreset"
								:options="ratePresetOptions"
								option-label="label"
								option-value="value"
								placeholder="Choose a preset…"
								class="w-full"
								@change="applyRatePreset"
							/>
							<div class="field__hint">Presets just fill the numbers; you can still edit them.</div>
						</div>

						<div class="field">
							<label class="field__label">Rate limit</label>
							<div class="rate-row">
								<div class="rate-row__cell">
									<label class="rate-row__subLabel">Requests</label>
									<InputNumber
										v-model="quota.metric_limit"
										:min="1"
										:max="10000"
										class="w-full"
										placeholder="e.g., 3"
										@blur="touch('metric_limit')"
									/>
								</div>
								<div class="rate-row__cell">
									<label class="rate-row__subLabel">Per</label>
									<Dropdown
										v-model="quota.metric_window_seconds"
										:options="windowOptions"
										option-label="label"
										option-value="value"
										placeholder="Select time window"
										class="w-full"
										@blur="touch('metric_window_seconds')"
									/>
								</div>
							</div>

							<div class="field__hint">
								{{ effectiveRateHint }}
							</div>
							<div v-if="shouldShowError('metric_limit')" class="field__error">{{ validationErrors.metric_limit }}</div>
							<div v-if="shouldShowError('metric_window_seconds')" class="field__error">{{ validationErrors.metric_window_seconds }}</div>
						</div>

						<div class="field">
							<label class="field__label">Partition</label>
							<div class="field__hint">
								<strong>None</strong>: global across all users. <strong>UPN</strong>/<strong>User ID</strong>: per-user.
							</div>
							<Dropdown
								v-model="quota.metric_partition"
								:options="partitionOptions"
								option-label="label"
								option-value="value"
								placeholder="Select partition type"
								class="w-full"
								@blur="touch('metric_partition')"
							/>
							<div v-if="shouldShowError('metric_partition')" class="field__error">{{ validationErrors.metric_partition }}</div>
						</div>
					</section>

					<!-- Advanced -->
					<section class="section-card">
						<details class="details details--always-open-on-desktop">
							<summary>Advanced</summary>
							<div class="details__body">
								<div class="field">
									<label class="field__label">Lockout duration (seconds)</label>
									<div class="field__hint">How long to block requests after the quota is exceeded.</div>
									<InputNumber
										v-model="quota.lockout_duration_seconds"
										:min="0"
										:max="3600"
										class="w-full"
										placeholder="e.g., 60"
										@blur="touch('lockout_duration_seconds')"
									/>
									<div v-if="shouldShowError('lockout_duration_seconds')" class="field__error">
										{{ validationErrors.lockout_duration_seconds }}
									</div>
								</div>

								<div class="field">
									<label class="field__label">Distributed enforcement</label>
									<div class="field__hint">
										When enabled, quota enforcement is synchronized across multiple API instances.
									</div>
									<div class="flex align-items-center gap-2">
										<InputSwitch v-model="quota.distributed_enforcement" />
										<span>{{ quota.distributed_enforcement ? 'Enabled' : 'Disabled' }}</span>
									</div>
								</div>
							</div>
						</details>
					</section>

					<!-- Actions -->
					<div class="actions-row">
						<Button
							:label="editId ? 'Save Changes' : 'Create Quota'"
							severity="primary"
							:disabled="!canSave || loading"
							@click="handleCreate"
						/>
						<Button label="Cancel" severity="secondary" :disabled="loading" @click="handleCancel" />
					</div>
				</div>

				<!-- Right: Summary -->
				<aside class="quota-editor__summary">
					<div class="summary-card">
						<div class="summary-card__title">Summary</div>

						<div class="summary-item">
							<div class="summary-item__label">Name</div>
							<div class="summary-item__value">
								<code>{{ quota.name || '—' }}</code>
							</div>
						</div>

						<div class="summary-item">
							<div class="summary-item__label">Type</div>
							<div class="summary-item__value">
								<Tag
									:severity="quota.quota_type === 'RawRequestRateLimit' ? 'info' : 'warning'"
									:pt="{
										root: {
											style: {
												backgroundColor: 'var(--primary-color)',
												color: 'var(--primary-text)',
												border: 'none',
											},
										},
									}"
								>
									{{ quota.quota_type || '—' }}
								</Tag>
							</div>
						</div>

						<div class="summary-item">
							<div class="summary-item__label">Applies to</div>
							<div class="summary-item__value">
								<div>{{ contextApi || '—' }} / {{ contextController || '—' }}</div>
								<div v-if="quota.quota_type === 'AgentRequestRateLimit'">
									Agent: <strong>{{ contextAgent || 'Any' }}</strong>
								</div>
							</div>
						</div>

						<div class="summary-item">
							<div class="summary-item__label">Partition</div>
							<div class="summary-item__value">{{ quota.metric_partition || '—' }}</div>
						</div>

						<div class="summary-item">
							<div class="summary-item__label">Rate</div>
							<div class="summary-item__value">
								<div><strong>{{ quota.metric_limit }}</strong> / {{ quota.metric_window_seconds }}s</div>
								<div class="summary-item__hint">{{ effectiveRateHint }}</div>
							</div>
						</div>

						<div class="summary-item">
							<div class="summary-item__label">Lockout</div>
							<div class="summary-item__value">{{ quota.lockout_duration_seconds }}s</div>
						</div>

						<div class="summary-item">
							<div class="summary-item__label">Distributed</div>
							<div class="summary-item__value">{{ quota.distributed_enforcement ? 'Enabled' : 'Disabled' }}</div>
						</div>

						<div v-if="Object.keys(validationErrors).length > 0" class="summary-errors">
							<div class="summary-errors__title">Fix these to save</div>
							<ul class="summary-errors__list">
								<li v-for="(msg, key) in validationErrors" :key="key">{{ msg }}</li>
							</ul>
						</div>

						<div class="summary-note">
							Note: quota enforcement updates take effect in the Core API after it restarts.
						</div>
					</div>
				</aside>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { debounce } from 'lodash';
import { useConfirmationStore } from '@/stores/confirmationStore';
import api from '@/js/api';
import type { QuotaDefinition, QuotaType, QuotaMetricPartitionType } from '@/js/types';

export default {
	name: 'CreateQuota',

	props: {
		editId: {
			type: [Boolean, String] as PropType<false | string>,
			required: false,
			default: false,
		},
	},

	data() {
		return {
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			nameValidationStatus: null as string | null,
			validationMessage: '' as string,
			saveAttempted: false as boolean,
			touched: {} as Record<string, boolean>,

			contextApi: 'CoreAPI' as string,
			contextController: '' as string,
			contextAgent: '' as string,

			quota: {
				name: '' as string,
				description: '' as string,
				context: '' as string,
				quota_type: 'RawRequestRateLimit' as QuotaType,
				metric_partition: 'UserPrincipalName' as QuotaMetricPartitionType,
				metric_limit: 100 as number,
				metric_window_seconds: 60 as number,
				lockout_duration_seconds: 60 as number,
				distributed_enforcement: false as boolean,
			} as QuotaDefinition,

			quotaTypeOptions: [
				{ label: 'Raw Request Rate Limit', value: 'RawRequestRateLimit' },
				{ label: 'Agent Request Rate Limit', value: 'AgentRequestRateLimit' },
			],

			partitionOptions: [
				{ label: 'None (Global)', value: 'None' },
				{ label: 'User Principal Name (UPN)', value: 'UserPrincipalName' },
				{ label: 'User Identifier', value: 'UserIdentifier' },
			],

			apiOptions: ['CoreAPI'],

			controllerOptions: [
				'Completions',
				'CompletionsStatus',
				'Sessions',
				'Files',
				'Branding',
				'Configuration',
				'UserProfiles',
				'Status',
				'OneDriveWorkSchool',
			],

			windowOptions: [
				{ label: '20 seconds', value: 20 },
				{ label: '40 seconds', value: 40 },
				{ label: '1 minute (60s)', value: 60 },
				{ label: '2 minutes (120s)', value: 120 },
				{ label: '5 minutes (300s)', value: 300 },
				{ label: '10 minutes (600s)', value: 600 },
				{ label: '30 minutes (1800s)', value: 1800 },
				{ label: '1 hour (3600s)', value: 3600 },
			],

			selectedRatePreset: null as null | string,
			ratePresetOptions: [
				{ label: '1 req / 20s (≈1 per 20s)', value: '1_20' },
				{ label: '3 req / 60s (≈1 per 20s)', value: '3_60' },
				{ label: '6 req / 60s (≈2 per 20s)', value: '6_60' },
				{ label: '9 req / 60s (≈3 per 20s)', value: '9_60' },
				{ label: '30 req / 60s (≈10 per 20s)', value: '30_60' },
			],

			debouncedCheckName: null as any,
		};
	},

	computed: {
		validationErrors(): Record<string, string> {
			const errors: Record<string, string> = {};

			// Name
			if (!this.editId) {
				if (!this.quota.name) {
					errors.name = 'Name is required.';
				} else if (this.nameValidationStatus === 'invalid') {
					errors.name = this.validationMessage || 'This name is not available.';
				}
			}

			// Quota type
			if (!this.quota.quota_type) {
				errors.quota_type = 'Please select a quota type.';
			}

			// Context (requires controller; API defaults to CoreAPI)
			if (!this.contextController) {
				errors.context = 'Please select a Controller for the context.';
			}

			// Metric partition
			if (!this.quota.metric_partition) {
				errors.metric_partition = 'Please select a partition type.';
			}

			// Limit + window
			if (!this.quota.metric_limit || this.quota.metric_limit < 1) {
				errors.metric_limit = 'Metric limit must be at least 1.';
			}

			if (!this.quota.metric_window_seconds || this.quota.metric_window_seconds < 20) {
				errors.metric_window_seconds = 'Time window must be at least 20 seconds.';
			} else if (this.quota.metric_window_seconds % 20 !== 0) {
				errors.metric_window_seconds = 'Time window must be a multiple of 20 seconds.';
			}

			// Lockout
			if (this.quota.lockout_duration_seconds < 0) {
				errors.lockout_duration_seconds = 'Lockout duration cannot be negative.';
			}

			return errors;
		},

		canSave(): boolean {
			return Object.keys(this.validationErrors).length === 0;
		},

		effectiveRateHint(): string {
			const windowSeconds = this.quota.metric_window_seconds;
			const limit = this.quota.metric_limit;
			const smoothingWindowSeconds = 20;

			if (!windowSeconds || windowSeconds < 20 || windowSeconds % 20 !== 0 || !limit) {
				return 'Tip: the time window must be a multiple of 20 seconds.';
			}

			const buckets = windowSeconds / smoothingWindowSeconds;
			const approxPerBucket = limit / buckets;
			const approxPer20s = Math.max(0, approxPerBucket);

			// Keep wording simple and aligned to observed behavior.
			if (buckets === 1) {
				return `Effective rate (20s smoothing): ~${approxPer20s.toFixed(2)} requests per 20 seconds.`;
			}

			return `Effective rate (20s smoothing): ~${approxPer20s.toFixed(2)} requests per 20 seconds (because ${limit} / ${windowSeconds}s is averaged over ${buckets} × 20s buckets).`;
		},
	},

	async created() {
		this.debouncedCheckName = debounce(this.checkName, 500);

		if (this.editId) {
			this.loading = true;
			this.loadingStatusText = `Retrieving quota "${this.editId}"...`;
			try {
				const result = await api.getQuota(this.editId);
				if (result && result.length > 0) {
					this.quota = result[0].resource;
					this.parseContext();
				}
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		} else {
			// Ensure the context string stays in sync with defaults on first render.
			this.buildContext();
		}
	},

	methods: {
		touch(fieldName: string) {
			this.touched[fieldName] = true;
		},

		shouldShowError(fieldName: string) {
			return Boolean(this.validationErrors[fieldName]) && (this.saveAttempted || this.touched[fieldName]);
		},

		async copyToClipboard(text: string) {
			try {
				await navigator.clipboard.writeText(text);
				this.$toast.add({
					severity: 'success',
					detail: 'Copied to clipboard.',
					life: 1500,
				});
			} catch {
				this.$toast.add({
					severity: 'error',
					detail: 'Could not copy to clipboard.',
					life: 2500,
				});
			}
		},

		parseContext() {
			if (this.quota.context) {
				const parts = this.quota.context.split(':');
				this.contextApi = parts[0] || 'CoreAPI';
				this.contextController = parts[1] || '';
				this.contextAgent = parts[2] || '';
			}
		},

		buildContext() {
			const parts = [this.contextApi, this.contextController];
			if (this.quota.quota_type === 'AgentRequestRateLimit' && this.contextAgent) {
				parts.push(this.contextAgent);
			}
			this.quota.context = parts.filter(p => p).join(':');
		},

		onTypeChange() {
			if (this.quota.quota_type === 'RawRequestRateLimit') {
				this.contextAgent = '';
			}
			this.buildContext();
		},

		applyRatePreset() {
			if (!this.selectedRatePreset) return;
			const [limit, windowSeconds] = this.selectedRatePreset.split('_').map(v => Number(v));
			if (!Number.isFinite(limit) || !Number.isFinite(windowSeconds)) return;

			this.quota.metric_limit = limit;
			this.quota.metric_window_seconds = windowSeconds;
		},

		async checkName() {
			if (!this.quota.name) {
				this.nameValidationStatus = null;
				return;
			}

			try {
				const response = await api.checkQuotaName(this.quota.name);
				if (response.status === 'Allowed') {
					this.nameValidationStatus = 'valid';
					this.validationMessage = '';
				} else if (response.status === 'Denied') {
					this.nameValidationStatus = 'invalid';
					this.validationMessage = response.message;
				}
			} catch (error) {
				console.error('Error checking quota name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the quota name. Please try again.';
			}
		},

		handleNameInput(event: any) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.quota.name = sanitizedValue;

			if (!this.editId) {
				this.debouncedCheckName();
			}
		},

		async handleCancel() {
			const confirmationStore = useConfirmationStore();
			const confirmed = await confirmationStore.confirmAsync({
				title: 'Cancel Quota Creation',
				message: 'Are you sure you want to cancel?',
				confirmText: 'Yes',
				cancelText: 'Cancel',
				confirmButtonSeverity: 'danger',
			});

			if (confirmed) {
				this.$router.push('/quotas');
			}
		},

		async handleCreate() {
			this.saveAttempted = true;
			this.buildContext();

			if (!this.canSave) {
				this.$toast.add({
					severity: 'error',
					detail: 'Please fix the highlighted fields before saving.',
					life: 3500,
				});
				return;
			}

			this.loading = true;
			try {
				this.loadingStatusText = 'Saving quota...';
				await api.createQuota(this.quota);
				this.$toast.add({
					severity: 'success',
					detail: `Quota "${this.quota.name}" was successfully saved.`,
					life: 5000,
				});

				if (!this.editId) {
					this.$router.push('/quotas');
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
	},
};
</script>

<style lang="scss" scoped>
.quota-editor {
	position: relative;
}

.quota-editor--loading {
	pointer-events: none;
}

.quota-editor__loading-overlay {
	position: fixed;
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

.quota-editor__grid {
	display: grid;
	grid-template-columns: minmax(0, 1fr) 360px;
	gap: 24px;
	align-items: start;
}

.quota-editor__summary {
	position: sticky;
	top: 16px;
}

.input-wrapper {
	position: relative;
	display: flex;
	align-items: center;
}

input {
	width: 100%;
	padding-right: 30px;
}

.icon {
	position: absolute;
	right: 10px;
	cursor: default;
}

.valid {
	color: green;
}

.invalid {
	color: red;
}

.section-card {
	border: 1px solid rgba(0, 0, 0, 0.08);
	border-radius: 10px;
	padding: 16px;
	background: #fff;
	margin-bottom: 16px;
}

.section-card__header {
	margin-bottom: 12px;
}

.section-card__title {
	font-weight: 700;
	font-size: 1.05rem;
}

.section-card__subtitle {
	color: rgba(0, 0, 0, 0.6);
	font-size: 0.9rem;
	margin-top: 2px;
}

.field {
	margin-top: 14px;
}

.field:first-child {
	margin-top: 0;
}

.field__label {
	display: block;
	font-weight: 600;
	margin-bottom: 6px;
}

.field__hint {
	color: rgba(0, 0, 0, 0.65);
	font-size: 0.9rem;
	margin-bottom: 8px;
}

.field__error {
	color: #b42318;
	font-size: 0.9rem;
	margin-top: 6px;
}

.context-builder {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
	gap: 16px;
}

.context-field {
	display: flex;
	flex-direction: column;
	gap: 8px;
}

.context-field label {
	font-weight: 500;
	font-size: 0.9rem;
}

.chip-row {
	display: flex;
	flex-wrap: wrap;
	gap: 8px;
	margin-top: 12px;
}

.details {
	margin-top: 12px;
}

.details summary {
	cursor: pointer;
	color: rgba(0, 0, 0, 0.7);
	font-weight: 600;
}

.details__body {
	margin-top: 8px;
}

.readonly-row {
	display: flex;
	align-items: center;
	gap: 8px;
}

.readonly-row__value {
	padding: 6px 8px;
	border-radius: 6px;
	background: #f5f5f5;
}

.rate-row {
	display: grid;
	grid-template-columns: 1fr 1fr;
	gap: 12px;
}

.rate-row__subLabel {
	display: block;
	font-size: 0.85rem;
	color: rgba(0, 0, 0, 0.65);
	margin-bottom: 6px;
}

.actions-row {
	display: flex;
	justify-content: flex-end;
	gap: 12px;
	margin-top: 8px;
}

.summary-card {
	border: 1px solid rgba(0, 0, 0, 0.08);
	border-radius: 10px;
	padding: 16px;
	background: #fff;
}

.summary-card__title {
	font-weight: 800;
	margin-bottom: 12px;
}

.summary-item {
	padding: 10px 0;
	border-top: 1px solid rgba(0, 0, 0, 0.06);
}

.summary-item:first-of-type {
	border-top: none;
}

.summary-item__label {
	font-size: 0.85rem;
	color: rgba(0, 0, 0, 0.6);
	margin-bottom: 4px;
}

.summary-item__value {
	font-size: 0.95rem;
}

.summary-item__hint {
	color: rgba(0, 0, 0, 0.65);
	font-size: 0.85rem;
	margin-top: 4px;
}

.summary-errors {
	margin-top: 12px;
	padding: 12px;
	border-radius: 10px;
	background: #fff5f5;
	border: 1px solid rgba(180, 35, 24, 0.25);
}

.summary-errors__title {
	font-weight: 700;
	color: #b42318;
	margin-bottom: 8px;
}

.summary-errors__list {
	margin: 0;
	padding-left: 18px;
	color: #b42318;
	font-size: 0.9rem;
}

.summary-note {
	margin-top: 12px;
	color: rgba(0, 0, 0, 0.6);
	font-size: 0.85rem;
}

code {
	background-color: #f0f0f0;
	padding: 2px 6px;
	border-radius: 4px;
	font-family: monospace;
}

@media (max-width: 1100px) {
	.quota-editor__grid {
		grid-template-columns: 1fr;
	}

	.quota-editor__summary {
		position: static;
	}
}
</style>

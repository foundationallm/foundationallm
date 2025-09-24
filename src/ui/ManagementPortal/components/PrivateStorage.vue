<template>
	<div>
		<!-- Loading overlay -->
		<template v-if="loading">
			<div class="grid__loading-overlay" role="status" aria-live="polite" aria-label="Loading private storage">
				<LoadingGrid />
				<div>{{ loadingStatusText }}</div>
			</div>
		</template>

		<!-- Trigger button -->
		<Button
			v-if="isButtonVisible"
			style="margin-right: 8px"
			@click="handleOpenPrivateStorageDialog"
		>
			<i class="pi pi-box" style="font-size: 1.2rem; margin-right: 8px"></i>
			Private Storage
		</Button>

		<!-- Private Storage dialog -->
		<Dialog
			v-model:visible="privateStorageDialogOpen"
			modal
			:header="'Private Storage'"
			:style="{ minWidth: '70%' }"
			@hide="handleClosePrivateStorage"
		>
			<template v-if="modalLoading">
				<div class="grid__loading-overlay" role="status" aria-live="polite" aria-label="Loading modal content">
					<LoadingGrid />
					<div>{{ loadingModalStatusText }}</div>
				</div>
			</template>
			<div class="card">
				<FileUpload
					ref="fileUpload"
					:auto="false"
					:multiple="true"
					custom-upload
					class="p-button-outlined"
					@upload="handleUpload($event)"
					@select="fileSelected"
				>
					<template #header="{ chooseCallback }">
						<div>
							<div>
								<Button @click="chooseCallback()">
									<i class="pi pi-file-plus" style="font-size: 1.2rem; margin-right: 8px"></i>
									Select file from Computer
								</Button>
								<Button
									icon="pi pi-upload"
									label="Upload"
									class="file-upload-container-button"
									style="margin-top: 0.5rem; margin-left: 2px; margin-right: 2px"
									:disabled="agentFiles.localFiles.length === 0"
									@click="handleUpload"
								/>
							</div>
						</div>
					</template>
					<template #content>
						<!-- File list -->
						<div class="file-upload-file-container">
							<div
								v-for="file of agentFiles.localFiles"
								:key="file.name + file.type + file.size"
								class="file-upload-file"
							>
								<div class="file-upload-file_info">
									<span style="font-weight: 600">{{ file.name }}</span>
								</div>
								<div class="file-info-right">
									<Badge v-if="!isMobile" value="Local Computer" />
									<Badge value="Pending" />
									<Button text aria-label="Remove file" @click="removeLocalFile(file.name)">
										<i class="pi pi-times"></i>
									</Button>
								</div>
							</div>
						</div>
						<span v-if="agentFiles.localFiles.length == 0">
							Drag and drop files to here to upload.
						</span>
					</template>
				</FileUpload>
			</div>
			<Divider />

			<!-- Files table -->
			<DataTable
				:value="agentFiles.uploadedFiles"
				paginator
				:rows="10"
				:rowsPerPageOptions="[5, 10, 20, 50]"
				:paginatorTemplate="'FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown'"
				:globalFilterFields="['display_name']"
				v-model:filters="filters"
				filterDisplay="menu"
				showGridlines
				stripedRows
				removableSort
				:sortMode="'single'"
				:sortField="sortField"
				:sortOrder="sortOrder"
				@sort="onSort"
				tableStyle="min-width: 50rem"
			>
				<template #header>
					<div class="filter-container">
						<Button
							type="button"
							icon="pi pi-filter-slash"
							label="Clear"
							outlined
							@click="clearFilter"
						/>
						<IconField>
							<InputIcon>
								<i class="pi pi-search" />
							</InputIcon>
							<InputText v-model="filters['global'].value" placeholder="Search filenames..." />
						</IconField>
					</div>
				</template>

				<template #empty>There are no private storage files uploaded for this agent.</template>

				<!-- Name -->
				<Column
					field="display_name"
					header="File Name"
					sortable
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #filter="{ filterModel }">
						<InputText v-model="filterModel.value" type="text" placeholder="Search by name" />
					</template>
				</Column>

				<!-- Tools -->
				<Column
					v-for="tool in tools"
					:key="tool"
					:header="tool"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="{ data }">
						<Checkbox
							v-if="fileToolAccess[data.object_id] !== undefined"
							v-model="fileToolAccess[data.object_id][toolNameToObjectId(tool)]"
							binary
							size="large"
						/>
					</template>
				</Column>

				<!-- Delete -->
				<Column
					header="Delete"
					header-style="width:6rem"
					style="text-align: center"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						headerContent: { style: { justifyContent: 'center' } },
					}"
				>
					<template #body="{ data }">
						<Button link @click="deletePrivateStorageFile(data.name, data.object_id)">
							<i class="pi pi-trash" style="font-size: 1.2rem"></i>
						</Button>
					</template>
				</Column>
			</DataTable>

			<!-- Footer -->
			<template #footer>
				<!-- Save -->
				<Button severity="primary" label="Save" @click="handleSaveFileToolAccess" />

				<!-- Cancel -->
				<Button label="Close" text @click="handleClosePrivateStorage" />
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import api from '@/js/api';

// Define filter match modes
const FilterMatchMode = {
	STARTS_WITH: 'startsWith',
	CONTAINS: 'contains',
	EQUALS: 'equals',
	IN: 'in',
	LESS_THAN: 'lt',
	GREATER_THAN: 'gt',
	LESS_THAN_OR_EQUAL: 'lte',
	GREATER_THAN_OR_EQUAL: 'gte',
	AFTER: 'after',
	BEFORE: 'before',
	DATE_IS: 'dateIs',
	DATE_IS_NOT: 'dateIsNot',
	DATE_BEFORE: 'dateBefore',
	DATE_AFTER: 'dateAfter',
	CUSTOM: 'custom',
};

// Define filter operators
const FilterOperator = {
	AND: 'and',
	OR: 'or',
};

export default {
	props: {
		agentName: {
			type: String,
			required: true,
		},

		tools: {
			type: Array as PropType<string[]>,
			required: false,
			default: () => ['OpenAIAssistantsCodeInterpreter', 'OpenAIAssistantsFileSearch'],
		},
	},

	data() {
		return {
			privateStorageDialogOpen: false,
			maxFiles: 1000,
			isMobile: window.screen.width < 950,
			loading: false as boolean,
			modalLoading: false as boolean,
			loadingStatusText: 'Retrieving private storage files...' as string,
			loadingModalStatusText: '' as string,
			agentFiles: {
				localFiles: [] as any[],
				uploadedFiles: [] as any[],
			},
			fileToolAccess: {} as {
				[key: string]: {
					[key: string]: boolean;
				};
			},
			filters: {
				global: { value: null, matchMode: FilterMatchMode.CONTAINS },
				display_name: {
					operator: FilterOperator.AND,
					constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
				},
			},
			sortField: 'display_name',
			sortOrder: 1,
		};
	},

	computed: {
		isButtonVisible: function () {
			return this.$appConfigStore.agentPrivateStoreFeatureFlag;
		},
	},

	methods: {
		handleClosePrivateStorage() {
			this.privateStorageDialogOpen = false;
			this.clearFilter();
		},

		browseFiles() {
			this.$refs.fileUpload.$el.querySelector('input[type="file"]').click();
		},

		alignOverlay() {
			if (this.$refs.menu.visible) {
				this.$nextTick(() => {
					this.$refs.menu.alignOverlay();
				});
			}
		},

		removeLocalFile(fileName) {
			const fileIndex = this.agentFiles.localFiles.findIndex((file) => file.name === fileName);
			if (fileIndex !== -1) {
				this.agentFiles.localFiles.splice(fileIndex, 1);
			}
		},

		validateUploadedFiles(files, currentFiles) {
			const allowedFileTypes = this.$appConfigStore.allowedUploadFileExtensions;
			const filteredFiles = [];

			files.forEach((file) => {
				const localFileAlreadyExists = currentFiles.localFiles.some(
					(existingFile) => existingFile.name === file.name && existingFile.size === file.size,
				);
				const uploadedFileAlreadyExists = currentFiles.uploadedFiles.some(
					(existingFile) => existingFile.name === file.name,
				);
				const fileAlreadyExists = localFileAlreadyExists || uploadedFileAlreadyExists;

				if (fileAlreadyExists) return;

				if (file.size > 536870912) {
					this.$toast.add({
						severity: 'error',
						summary: 'Error',
						detail: 'File size exceeds the limit of 512MB.',
						life: 5000,
					});
				} else if (allowedFileTypes && allowedFileTypes !== '') {
					const fileExtension = file.name.split('.').pop()?.toLowerCase();
					const isFileTypeAllowed = allowedFileTypes
						.split(',')
						.map((type) => type.trim().toLowerCase())
						.includes(fileExtension);

					if (!isFileTypeAllowed) {
						this.$toast.add({
							severity: 'error',
							summary: 'Error',
							detail: `File type not supported. File: ${file.name}`,
							life: 5000,
						});
					} else {
						filteredFiles.push(file);
					}
				} else {
					filteredFiles.push(file);
				}
			});

			if (
				currentFiles.localFiles.length + currentFiles.uploadedFiles.length + filteredFiles.length >
				this.maxFiles
			) {
				this.$toast.add({
					severity: 'error',
					summary: 'Error',
					detail: `You can only upload a maximum of ${this.maxFiles} ${
						this.maxFiles === 1 ? 'file' : 'files'
					} at a time.`,
					life: 5000,
				});
				filteredFiles.splice(
					this.maxFiles - (currentFiles.localFiles.length + currentFiles.uploadedFiles.length),
				);
			}

			return filteredFiles;
		},

		fileSelected(event: any) {
			const filteredFiles = this.validateUploadedFiles(event.files, this.agentFiles);
			this.agentFiles.localFiles = [...this.agentFiles.localFiles, ...filteredFiles];

			if (this.$refs.fileUpload) {
				this.$refs.fileUpload.clear();
			}
		},

		async handleOpenPrivateStorageDialog() {
			this.loading = true;
			this.clearFilter();
			await this.getPrivateAgentFiles();
			await this.getPrivateAgentFileToolAssociations();
			this.loading = false;
			this.privateStorageDialogOpen = true;
		},

		async deletePrivateStorageFile(fileName: string, fileObjectId: string) {
			if (!confirm('Are you sure you want to delete this file?')) {
				return;
			}

			this.loadingModalStatusText = 'Deleting file...';
			this.modalLoading = true;
			await api.deleteFileFromPrivateStorage(this.agentName, fileName);
			delete this.fileToolAccess[fileObjectId];
			await this.getPrivateAgentFiles();
			await this.handleSaveFileToolAccess();
			this.$toast.add({
				severity: 'success',
				summary: 'Success',
				detail: `File successfully deleted.`,
				life: 5000,
			});
			this.modalLoading = false;
		},

		async getPrivateAgentFiles() {
			this.agentFiles.localFiles = [];
			const files = (await api.getPrivateStorageFiles(this.agentName)).map((r) => r.resource);
			this.agentFiles.uploadedFiles = files;
		},

		toolNameToObjectId(toolName: string): string {
			const toolPrefix = `/instances/${this.$appConfigStore.instanceId}/providers/FoundationaLLM.Agent/tools`;
			return `${toolPrefix}/${toolName}`;
		},

		async getPrivateAgentFileToolAssociations() {
			const toolAssociations = await api.getPrivateStorageFileToolAssociations(this.agentName);

			// Create an empty object to store tool permissions
			this.fileToolAccess = {};

			toolAssociations.forEach((association) => {
				const fileId = association.resource.file_object_id;
				const associatedTools = association.resource.associated_resource_object_ids || {};

				// Initialize file entry if it does not exist
				if (!this.fileToolAccess[fileId]) {
					if (!this.fileToolAccess[fileId]) {
						this.fileToolAccess[fileId] = {};
					}

					this.tools.forEach((tool: string) => {
						this.fileToolAccess[fileId][this.toolNameToObjectId(tool)] =
							Object.prototype.hasOwnProperty.call(associatedTools, this.toolNameToObjectId(tool));
					});
				}
			});
		},

		async handleSaveFileToolAccess() {
			const payload = {
				agent_file_tool_associations: { ...this.fileToolAccess },
			};

			this.loadingModalStatusText = 'Saving tool file access permissions...';
			this.modalLoading = true;
			try {
				const response = await api.updateFileToolAssociations(this.agentName, payload);
				if (response.resource?.success) {
					this.$toast.add({
						severity: 'success',
						summary: 'Updated',
						detail: 'File tool association updated successfully.',
						life: 3000,
					});
				} else {
					this.getPrivateAgentFileToolAssociations();
					this.$toast.add({
						severity: 'error',
						summary: 'Update Failed',
						detail: 'Failed to update tool associations.',
						life: 5000,
					});
				}
			} catch (error) {
				console.error('Error updating tool associations:', error);
				this.$toast.add({
					severity: 'error',
					summary: 'Update Failed',
					detail: 'Failed to update tool associations.',
					life: 5000,
				});
			}
			this.modalLoading = false;
		},

		handleUpload() {
			this.loadingModalStatusText =
				this.agentFiles.localFiles.length === 1 ? 'Uploading file...' : 'Uploading files...';
			this.modalLoading = true;

			const totalFiles = this.agentFiles.localFiles.length;
			const combinedFiles = [...this.agentFiles.localFiles];

			combinedFiles.forEach((file) => {
				if (file instanceof File) {
					file.source = 'local';
				}
			});

			let filesUploaded = 0;
			let filesFailed = 0;

			combinedFiles.forEach(async (file) => {
				try {
					if (file.source === 'local') {
						const formData = new FormData();
						formData.append('file', file);

						await api.uploadToPrivateStorage(this.agentName, file.name, formData);
					}
					filesUploaded += 1;
				} catch (error) {
					filesFailed += 1;
					this.modalLoading = false;
					this.$toast.add({
						severity: 'error',
						summary: 'Error',
						detail: `File upload failed for "${file.name}". ${error.message || error.title || ''}`,
						life: 5000,
					});
				} finally {
					if (totalFiles === filesUploaded + filesFailed) {
						this.agentFiles.localFiles = [];
						if (filesUploaded > 0) {
							await this.getPrivateAgentFiles();
							await this.getPrivateAgentFileToolAssociations();
							this.modalLoading = false;
							this.$toast.add({
								severity: 'success',
								summary: 'Success',
								detail: `Successfully uploaded ${filesUploaded} file${totalFiles > 1 ? 's' : ''}.`,
								life: 5000,
							});
							this.$refs.fileupload = null;
						}
					}
				}
			});
		},

		onSort(event: any) {
			this.sortField = event.sortField;
			this.sortOrder = event.sortOrder;
		},

		clearFilter() {
			this.filters = {
				global: { value: null, matchMode: FilterMatchMode.CONTAINS },
				display_name: {
					operator: FilterOperator.AND,
					constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
				},
			};
		},
	},
};
</script>

<style lang="scss" scoped>
.file-upload-icon {
	width: 100%;
	text-align: center;
	font-size: 5rem;
	color: #000;
}

.file-upload-file-container {
	max-height: 50vh;
	overflow-y: auto;
}
.file-upload-file {
	border-color: rgb(226, 232, 240);
	border-radius: 6px;
	border-style: solid;
	border-width: 1px;
	display: flex;
	flex-direction: row;
	justify-content: space-between;
	padding: 0.5rem;
	width: 100%;
	align-items: center;
	margin-bottom: 0.5rem;
}

.file-upload-file_info {
	flex: 1;
	display: flex;
	flex-direction: row;
	align-items: center;
	gap: 10px;
	overflow: hidden;
	flex-shrink: 1;
	max-width: calc(100% - 50px);

	span {
		font-weight: 600;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: wrap;
		flex-shrink: 1;
		min-width: 0;
	}
}
@media only screen and (max-width: 405px) {
	.file-upload-file_info div {
		display: none;
	}
}

.p-fileupload-content {
	padding: 0px;
}

.p-fileupload-buttonbar {
	display: none;
}

.p-fileupload-content {
	border: none;
}

.file-info-right {
	display: flex;
	align-items: center;
	margin-left: 10px;
	gap: 0.5rem;
}

.filter-container {
	display: flex;
	align-items: center;
	gap: 0.5rem;
	width: 100%;
}

:deep(.p-datatable-header) {
	padding: 1rem;
	background: transparent;
	border: none;
}

:deep(.p-input-icon-left) {
	flex: 1;
}

:deep(.p-input-icon-left input) {
	width: 100%;
	padding-left: 2.5rem;
}
</style>

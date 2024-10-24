<template>
	<!-- Loading overlay -->
	<template v-if="loading">
		<div class="grid__loading-overlay">
			<LoadingGrid />
			<div>{{ loadingStatusText }}</div>
		</div>
	</template>

	<!-- Trigger button -->
	<div style="display: flex; align-items: center; margin-right: 8px">
		<Button @click="openPrivateStorageDialog">
			<i class="pi pi-box" style="font-size: 1.2rem; margin-right: 8px"></i>
			Private Storage
		</Button>
	</div>

	<!-- Private Storage dialog -->
	<Dialog
		v-model:visible="privateStorageDialogOpen"
		modal
		:header="'Private Storage'"
		:style="{ minWidth: '70%' }"
		@hide="handleClosePrivateStorage"
	>
		<template v-if="modalLoading">
			<div class="grid__loading-overlay">
				<LoadingGrid />
				<div>{{ loadingModalStatusText }}</div>
			</div>
		</template>
		<div class="card">
			<FileUpload customUpload ref="fileUpload" class="p-button-outlined" :auto="false"  :multiple="true" @upload="handleUpload($event)" @select="fileSelected">
				<template #header="{ chooseCallback }">
					<div>
						<div>
							<Button @click="chooseCallback()">
								<i class="pi pi-plus" style="font-size: 1.2rem; margin-right: 8px"></i>
								Select file from Computer
							</Button>
							<Button
								icon="pi pi-upload"
								label="Upload"
								class="file-upload-container-button"
								style="margin-top: 0.5rem; margin-left: 2px; margin-right: 2px"
								:disabled="isUploading || agentFiles.localFiles.length === 0"
								@click="handleUpload"
							/>
						</div>
					</div>
				</template>
				<template #content>
					<!-- Progress bar -->
					<div v-if="isUploading" style="padding: 60px 10px">
						<ProgressBar
							:value="uploadProgress"
							:show-value="false"
							style="display: flex; width: 95%; margin: 10px 2.5%"
						/>
						<p style="text-align: center">Uploading...</p>
					</div>

					<!-- File list -->
					<div v-else class="file-upload-file-container">
						<div
							v-for="file of agentFiles.localFiles"
							:key="file.name + file.type + file.size"
							class="file-upload-file"
						>
							<div class="file-upload-file_info">
								<span style="font-weight: 600">{{ file.name }}</span>
							</div>
							<div style="display: flex; align-items: center; margin-left: 10px; gap: 0.5rem">
								<Badge v-if="!isMobile" value="Local Computer" />
								<Badge value="Pending" />
								<Button
									icon="pi pi-times"
									text
									severity="danger"
									aria-label="Remove file"
									@click="removeLocalFile(file.name)"
								/>
							</div>
						</div>
					</div>
					<span v-if="agentFiles.localFiles.length == 0">Drag and drop files to here to upload.</span>
				</template>
			</FileUpload>
		</div>
		<Divider />
		<!-- Table -->
		<DataTable :value="agentFiles.uploadedFiles">
			<!-- Name -->
			<Column
				field="display_name"
				header="Name"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			></Column>

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
					<Button link @click="deletePrivateStorageFile(data.name)">
						<i class="pi pi-trash" style="font-size: 1.2rem"></i>
					</Button>
				</template>
			</Column>
			<template #empty>There are no private storage files uploaded for this agent.</template>
		</DataTable>
		<template #footer>
			<Button label="Close" text @click="handleClosePrivateStorage" />
		</template>
	</Dialog>
</template>

<script lang="ts">
import api from '@/js/api';

export default {
	props: {
		agentName: {
			type: String,
			required: true,
		},
	},
	data() {
		return {
			privateStorageDialogOpen: false,
			isUploading: false,
			uploadProgress: 0,
			maxFiles: 10,
			isMobile: window.screen.width < 950,
			loading: false as boolean,
			modalLoading: false as boolean,
			loadingStatusText: 'Retrieving private storage files...' as string,
			loadingModalStatusText: '' as string,
			fileToDelete: null as any,
			agentFiles: {
				localFiles: [] as any[],
				uploadedFiles: [] as any[],
			},
		};
	},

	mounted() {
		this.adjustTextareaHeight();
		window.addEventListener('resize', this.handleResize);
	},

	beforeUnmount() {
		window.removeEventListener('resize', this.handleResize);
	},

	methods: {
		handleClosePrivateStorage() {
			this.privateStorageDialogOpen = false;
		},

		adjustTextareaHeight() {
			this.$nextTick(() => {
				this.$refs.inputRef.style.height = 'auto';
				this.$refs.inputRef.style.height = this.$refs.inputRef.scrollHeight + 'px';
			});
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

		removeLocalFile(fileName){
			const fileIndex = this.agentFiles.localFiles.findIndex(file => file.name === fileName);
  
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
					(existingFile) => existingFile.name === file.name && existingFile.size === file.size,
				);
				const fileAlreadyExists =
					localFileAlreadyExists  || uploadedFileAlreadyExists;

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
				currentFiles.localFiles.length +
					currentFiles.uploadedFiles.length +
					filteredFiles.length >
				this.maxFiles
			) {
				this.$toast.add({
					severity: 'error',
					summary: 'Error',
					detail: `You can only upload a maximum of ${this.maxFiles} ${this.maxFiles === 1 ? 'file' : 'files'} at a time.`,
					life: 5000,
				});
				filteredFiles.splice(
					this.maxFiles -
						(currentFiles.localFiles.length +
							currentFiles.uploadedFiles.length),
				);
			}

			return filteredFiles;
		},

		fileSelected(event: any) {
			const filteredFiles = this.validateUploadedFiles(event.files, this.agentFiles); 
			console.log(filteredFiles, this.agentfiles)
			this.agentFiles.localFiles = [...this.agentFiles.localFiles, ...filteredFiles];

			if (this.$refs.fileUpload) {
				this.$refs.fileUpload.clear();
			}
		},

		async openPrivateStorageDialog(){
			this.loading = true;
			await this.getPrivateAgentFiles();
			this.loading = false;
			this.privateStorageDialogOpen = true
		},

		async deletePrivateStorageFile(fileName: string){
			this.loadingModalStatusText = "Deleting file...";
			this.modalLoading=true;
			await api.deleteFileFromPrivateStorage(this.agentName, fileName);
			await this.getPrivateAgentFiles();
			this.$toast.add({
				severity: 'success',
				summary: 'Success',
				detail: `File successfully deleted.`,
				life: 5000,
			});
			this.modalLoading=false;
		},

		async getPrivateAgentFiles(){
			this.agentFiles.localFiles = [];
			this.agentFiles.uploadedFiles = await api.getPrivateStorageFiles(this.agentName);
			console.log('files=',this.agentFiles)
		},

		async handleUpload() {
			this.isUploading = true;

			console.log(this.agentFiles)
			const totalFiles = this.agentFiles.localFiles.length;
			const combinedFiles = [...this.agentFiles.localFiles];

			combinedFiles.forEach((file) => {
				if (file instanceof File) {
					file.source = 'local';
				} 
			});

			let filesUploaded = 0;
			let filesFailed = 0;
			const filesProgress = [];

			combinedFiles.forEach(async (file, index) => {
				try {
					if (file.source === 'local') {
						const formData = new FormData();
						formData.append('file', file);

						const onProgress = (event) => {
							if (event.lengthComputable) {
								filesProgress[index] = (event.loaded / event.total) * 100;

								let totalUploadProgress = 0;
								filesProgress.forEach((fileProgress) => {
									totalUploadProgress += fileProgress / totalFiles;
								});

								this.uploadProgress = totalUploadProgress;
							}
						};
						await api.uploadToPrivateStorage(formData, this.agentName)
					}
					filesUploaded += 1;
					//this.agentFiles.uploadedFiles.push(file);
				} catch (error) {
					filesFailed += 1;
					this.$toast.add({
						severity: 'error',
						summary: 'Error',
						detail: `File upload failed for "${file.name}". ${error.message || error.title || ''}`,
						life: 5000,
					});
				} finally {
					if (totalFiles === filesUploaded + filesFailed) {
						this.isUploading = false;
						this.uploadProgress = 0;
						this.agentFiles.localFiles = [];
						if (filesUploaded > 0) {
							await this.getPrivateAgentFiles();
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

.p-button-icon{
	color: var(--primary-button-text)
}
</style>
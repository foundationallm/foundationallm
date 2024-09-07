<template>
	<div class="chat-input p-inputgroup">
		<div class="input-wrapper">
			<div class="tooltip-component">
				<VTooltip :auto-hide="false" :popper-triggers="['hover']">
					<i class="pi pi-info-circle" tabindex="0"></i>
					<template #popper> Use Shift+Enter to add a new line </template>
				</VTooltip>
			</div>
			<VTooltip :auto-hide="false" :popper-triggers="['hover']">
				<Button 
					type="button" 
					:badge="fileArrayFiltered.length.toString() || null"
					:aria-label="'Upload file (' + fileArrayFiltered.length.toString() + ' files attached)'" 
					icon="pi pi-paperclip" 
					class="file-upload-button secondary-button" 
					@click="toggle" 
					aria-haspopup="true" 
					aria-controls="overlay_menu" />
				<Menu ref="menu" id="overlay_menu" :model="items" :popup="true">
					<template #item="{ item, props }">
						<li class="menu-item" v-bind="props.action" v-show="item.visible">
							<a class="flex items-center">
								<span :class="item.icon"></span>
								<span class="labelPadding">{{ item.label }} </span>
								<Badge v-if="item.badge" class="ml-auto" :value="item.badge" />
							</a>
							<!-- Check if item has sub-items (submenu) -->
							<ul v-if="item.items && item.items.length">
								<li v-for="subItem in item.items" :key="subItem.label">
									<a v-bind="subItem.command">
										<span :class="subItem.icon"></span>
										{{ subItem.label }}
									</a>
								</li>
							</ul>
						</li>
					</template>
				</Menu>
				<template #popper>
					Attach files ({{
						fileArrayFiltered.length === 1 ? '1 file' : fileArrayFiltered.length + ' files'
					}})
				</template>
			</VTooltip>
			<Dialog
				v-model:visible="showFileUploadDialog"
				header="Upload File(s)"
				modal
				aria-label="File Upload Dialog"
				style="max-width: 98%"
			>
				<FileUpload
					ref="fileUpload"
					:multiple="true"
					:auto="false"
					:custom-upload="true"
					@uploader="handleUpload"
					@select="fileSelected"
				>
					<template #header="{ chooseCallback, uploadCallback, clearCallback, files }">
						<div>
							<div class="upload-files-header">
								<Button
									icon="pi pi-images"
									label="Choose"
									:disabled="uploadProgress !== 0"
									@click="chooseCallback()"
								></Button>
								<Button
									icon="pi pi-cloud-upload"
									label="Upload"
									:disabled="!files || files.length === 0"
									@click="uploadCallback()"
								></Button>
								<Button
									icon="pi pi-times"
									label="Cancel"
									:disabled="!files || files.length === 0"
									@click="clearCallback()"
								></Button>
							</div>
						</div>
					</template>

					<template #content="{ files, removeFileCallback }">
						<!-- Progress bar -->
						<div v-if="isUploading">
							<ProgressBar
								:value="uploadProgress"
								:show-value="false"
								style="display: flex; width: 95%; margin: 10px 2.5%"
							/>
							<p style="text-align: center">Uploading...</p>
						</div>

						<!-- File list -->
						<div v-else>
							<div
								v-for="(file, index) of files"
								:key="file.name + file.type + file.size"
								class="file-upload-file"
							>
								<div class="file-upload-file_info">
									<i class="pi pi-file" style="font-size: 2rem; margin-right: 1rem"></i>
									<span style="font-weight: 600">{{ file.name }}</span>
									<div>{{ formatSize(file.size) }}</div>
								</div>
								<div style="display: flex; align-items: center; margin-left: 10px">
									<Badge value="Pending" />
									<Button
										icon="pi pi-times"
										text
										severity="danger"
										@click="removeFileCallback(index)"
									/>
								</div>
							</div>
							<div v-for="file in fileArrayFiltered" :key="file.fileName" class="file-upload-file">
								<div class="file-upload-file_info">
									<i class="pi pi-file" style="font-size: 2rem; margin-right: 1rem"></i>
									<span style="font-weight: 600">{{ file.fileName }}</span>
								</div>
								<div style="display: flex; align-items: center; margin-left: 10px">
									<Badge value="Uploaded" severity="success" />
									<Button
										icon="pi pi-times"
										text
										severity="danger"
										aria-label="Remove attachment"
										@click="removeAttachment(file)"
									/>
								</div>
							</div>
							<div v-if="files.length === 0 && fileArrayFiltered.length === 0">
								<i class="pi pi-cloud-upload file-upload-icon" />
								<div>
									<p style="text-align: center">
										<span class="file-upload-empty-desktop">
											Drag and drop files here
											<br />
											or
											<br />
										</span>
										<a style="color: blue; cursor: pointer" @click="browseFiles">
											<span>Browse for files</span>
										</a>
									</p>
								</div>
							</div>
						</div>
					</template>
				</FileUpload>
				<ConfirmDialog></ConfirmDialog>
			</Dialog>
			<Mentionable
				:keys="['@']"
				:items="agents"
				offset="6"
				:limit="1000"
				insert-space
				class="mentionable"
				@keydown.enter.prevent
				@open="agentListOpen = true"
				@close="agentListOpen = false"
			>
				<textarea
					id="chat-input"
					ref="inputRef"
					v-model="text"
					class="input"
					:disabled="disabled"
					placeholder="What would you like to ask?"
					autofocus
					aria-label="Chat input"
					@keydown="handleKeydown"
				/>
				<template #no-result>
					<div class="dim">No result</div>
				</template>

				<template #item="{ item }">
					<div class="user">
						<span class="dim">
							{{ item.label }}
						</span>
					</div>
				</template>
			</Mentionable>
		</div>
		<Button
			:disabled="disabled"
			class="primary-button submit"
			icon="pi pi-send"
			label="Send"
			@click="handleSend"
		/>
	</div>
</template>

<script lang="ts">
import { Mentionable } from 'vue-mention';
import 'floating-vue/dist/style.css';

export default {
	name: 'ChatInput',

	components: {
		Mentionable,
	},

	props: {
		disabled: {
			type: Boolean,
			required: false,
			default: false,
		},
	},

	emits: ['send'],

	data() {
		return {
			text: '' as string,
			targetRef: null as HTMLElement | null,
			inputRef: null as HTMLElement | null,
			agents: [],
			agentListOpen: false,
			showFileUploadDialog: false,
			isUploading: false,
			uploadProgress: 0,
		};
	},

	computed: {
		fileArrayFiltered() {
			return this.$appStore.attachments.filter(
				(attachment) => attachment.sessionId === this.$appStore.currentSession.sessionId,
			);
		},

		items() {
			return [
				{
					label: 'Connect to Microsoft OneDrive (work/school)',
					icon: 'pi pi-sign-in',
					visible: !this.$appStore.oneDriveConnected,
					command: async() => {
						await this.connectOneDrive();
					}
				},
				{
					label: 'Microsoft OneDrive (work/school)',
					labelIcon: 'pi pi-sign-out',
					items: [
						{
							label: 'Upload from OneDrive',
							icon: 'pi pi-cloud-upload',
							visible: true,
							command: async () => {
							},
						},
						{
							label: 'Disconnect',
							icon: 'pi pi-sign-out',
							visible: true,
							command: async () => {
								await this.disconnectOneDrive();
							},
						},
					],
					visible: this.$appStore.oneDriveConnected,
					command: async() => {
						await this.uploadFromOneDrive();
					}
				},
				{
					separator: true
				},
				{
					label: 'Upload from computer',
					icon: 'pi pi-file-plus',
					visible: true,
					command: () => {
						this.showFileUploadDialog = true;
					}
				}
			];
		},

	},

	watch: {
		text: {
			handler() {
				this.adjustTextareaHeight();
			},
			immediate: true,
		},
		disabled: {
			handler(newValue) {
				if (!newValue) {
					this.$nextTick(() => {
						const textInput = this.$refs.inputRef as HTMLTextAreaElement;
						textInput.focus();
					});
				}
			},
			immediate: true,
		},
	},

	async created() {
		await this.$appStore.getAgents();

		this.agents = this.$appStore.agents.map((agent) => ({
			label: agent.name,
			value: agent.name,
		}));
	},

	mounted() {
		this.adjustTextareaHeight();
	},

	methods: {
		toggle(event: any) {
			this.$refs.menu.toggle(event);
		},
		handleKeydown(event: KeyboardEvent) {
			if (event.key === 'Enter' && !event.shiftKey && !this.agentListOpen) {
				event.preventDefault();
				this.handleSend();
			}
		},

		adjustTextareaHeight() {
			this.$nextTick(() => {
				this.$refs.inputRef.style.height = 'auto';
				this.$refs.inputRef.style.height = this.$refs.inputRef.scrollHeight + 'px';
			});
		},

		handleSend() {
			this.$emit('send', this.text);
			this.text = '';
		},

		handleUpload(event: any) {
			this.isUploading = true;

			const totalFiles = event.files.length;
			let filesUploaded = 0;
			let filesFailed = 0;
			const filesProgress = [];

			event.files.forEach(async (file: any, index) => {
				try {
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

					await this.$appStore.uploadAttachment(
						formData,
						this.$appStore.currentSession.sessionId,
						onProgress,
					);
					filesUploaded += 1;
				} catch (error) {
					filesFailed += 1;
					this.$toast.add({
						severity: 'error',
						summary: 'Error',
						detail: `File upload failed for "${file.name}". ${error.message ? error.message : error.title ? error.title : ''}`,
						life: 5000,
					});
				} finally {
					if (totalFiles === filesUploaded + filesFailed) {
						this.showFileUploadDialog = false;
						this.isUploading = false;
						this.uploadProgress = 0;
						if (filesUploaded > 0) {
							this.$toast.add({
								severity: 'success',
								summary: 'Success',
								detail: `Successfully uploaded ${filesUploaded} file${totalFiles > 1 ? 's' : ''}.`,
								life: 5000,
							});
						}
					}
				}
			});
		},

		toggleFileAttachmentOverlay(event: any) {
			this.$refs.fileAttachmentPanel.toggle(event);
		},

		async removeAttachment(file: any) {
			await this.$appStore.deleteAttachment(file);
		},

		browseFiles() {
			this.$refs.fileUpload.$el.querySelector('input[type="file"]').click();
		},

		formatSize(bytes) {
			const k = 1024;
			const dm = 3;
			const sizes = this.$primevue.config.locale.fileSizeTypes;

			if (bytes === 0) {
				return `0 ${sizes[0]}`;
			}

			const i = Math.floor(Math.log(bytes) / Math.log(k));
			const formattedSize = parseFloat((bytes / Math.pow(k, i)).toFixed(dm));

			return `${formattedSize} ${sizes[i]}`;
		},

		fileSelected(event: any) {
			event.files.forEach((file: any, index) => {
				if (file.size > 512000000) {
					this.$toast.add({
						severity: 'error',
						summary: 'Error',
						detail: 'File size exceeds the limit of 512MB.',
						life: 5000,
					});
					event.files.splice(index, 1);
				}
			});
		},
		
		async connectOneDrive(){
			//await this.$authStore.requestOneDriveConsent();
			this.$toast.add({
				severity: 'success',
				summary: 'Success',
				detail: `Your account is now connected to OneDrive.`,
				life: 5000,
			});
			await this.$appStore.oneDriveConnect();
		},

		async disconnectOneDrive(){
			this.$toast.add({
				severity: 'success',
				summary: 'Success',
				detail: `Your account is now disconnected from OneDrive.`,
				life: 5000,
			});
			await this.$appStore.oneDriveDisconnect();
		},

		async uploadFromOneDrive(){
			//
		}
	},
};
</script>

<style lang="scss" scoped>
.chat-input {
	display: flex;
	background-color: white;
	border-radius: 8px;
	width: 100%;
}

.primary-button {
	background-color: var(--primary-button-bg) !important;
	border-color: var(--primary-button-bg) !important;
	color: var(--primary-button-text) !important;
}

.secondary-button {
	background-color: var(--secondary-button-bg) !important;
	border-color: var(--secondary-button-bg) !important;
	color: var(--secondary-button-text) !important;
}

.pre-input {
	flex: 0 0 10%;
}

.chat-input .input-wrapper {
	display: flex;
	align-items: stretch;
	width: 100%;
}

.tooltip-component {
	height: 100%;
	margin-right: 0.5rem;
	display: flex;
	align-items: center;
}

.mentionable {
	width: 100%;
	height: auto;
	max-height: 128px;
	display: flex;
	flex-direction: column;
	flex: 1;
}

.input {
	width: 100%;
	height: 64px;
	max-height: 128px;
	overflow-y: scroll;
	border-radius: 0px;
	font-size: 1rem;
	color: #6c6c6c;
	padding: 1.05rem 0.75rem 0.5rem 0.75rem;
	border: 2px solid #e1e1e1;
	transition:
		background-color 0.3s,
		color 0.3s,
		border-color 0.3s,
		box-shadow 0.3s;
	resize: none;
}

.input:focus-visible {
	border-radius: 0px !important;
	outline: none;
}

.mention-item {
	padding: 4px 10px;
}

.mention-selected {
	background: rgb(192, 250, 153);
}

.input:focus {
	// height: 192px;
}

.context-menu {
	position: absolute;
	bottom: 100%;
}

.submit {
	flex: 0 0 10%;
	text-align: left;
	flex-basis: auto;
}

.file-upload-button {
	height: 100%;
}

.attached-files-container {
	padding-bottom: 1rem;
}

.attached-files {
	display: flex;
	flex-direction: row;
	align-items: center;
	justify-content: space-between;
	flex-wrap: nowrap;
}

.file-remove {
	margin-left: 1rem;
}

.p-fileupload-content {
	border-top-left-radius: 6px;
	border-top-right-radius: 6px;
}

.upload-files-header {
	width: 100%;
	max-width: 500px;
}

.upload-files-header button {
	margin-right: 0.5rem;
}

@media only screen and (max-width: 405px) {
	.upload-files-header button {
		padding: 0.1rem 0.25rem !important;
	}
}

@media only screen and (max-width: 620px) {
	.upload-files-header {
		width: auto !important;
	}

	.upload-files-header button {
		padding: 0.25rem 0.5rem;
		margin-right: 0.25rem !important;
		margin-bottom: 0.25rem !important;
	}

	.tooltip-component {
		display: none;
	}
}

@media only screen and (max-width: 950px) {
	.file-upload-empty-desktop {
		display: none;
	}
}
</style>

<style lang="scss">
@media only screen and (max-width: 545px) {
	.submit .p-button-label {
		display: none;
	}

	.submit .p-button-icon {
		margin: 0;
	}
}

.mention-item {
	padding: 4px 10px;
}

.mention-selected {
	background-color: #131833;
	color: #fff;
}

.file-upload-icon {
	width: 100%;
	text-align: center;
	font-size: 5rem;
	color: #000;
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
		max-width: 80%;
		min-width: 0;
	}
}

@media only screen and (max-width: 405px) {
	.file-upload-file_info div {
		display: none;
	}
}

.p-fileupload-content {
	padding: 30px 10px 10px 10px;
}

.labelPadding{
	padding-left: 10px;
}
</style>

<template>
    <header role="banner">
        <NavBarSettings />
    </header>

    <main id="main-content">
        <Toast position="top-center" />

        <div class="w-full max-w-[1430px] mx-auto px-4 py-7">
            <div class="csm-backto-chats-1">
                <nuxt-link to="/" class="backto-chats">
                    <i class="pi pi-angle-left relative top-[2px]"></i> Return to Chats
                </nuxt-link>
            </div>

            <div class="flex flex-wrap items-center -mx-4">
                <div class="w-full max-w-full md:max-w-[50%] px-4 mb-5 text-center md:text-left">
                    <h2 class="page-header text-3xl text-[#334581]">Create Agent</h2>
                </div>


            </div>

            <div class="mb-4">
                <TabView :activeIndex="activeTabIndex" @tab-change="onTabChange">
                    <TabPanel header="General">
                        <div class="px-4 py-8 mt-8 border border-solid border-gray-300">
                            <div class="w-full max-w-[1000px] mx-auto">

                                <div class="flex flex-wrap -mx-4">
                                    <div class="w-full max-w-full md:max-w-[50%] px-4 mb-6">
                                        <label for="agentDisplayName"
                                            class="block text-base text-[#898989] mb-2">Display Name <span
                                                class="text-[#ff0000]">*</span></label>
                                        <InputText type="text" class="w-full" name="agentDisplayName"
                                            id="agentDisplayName" required="true" maxlength="50" />
                                        <p class="text-xs text-[#898989]">(50 Characters)</p>
                                    </div>

                                    <div class="w-full max-w-full md:max-w-[50%] px-4 mb-6">
                                        <label for="agentExpirationDate" class="block text-base text-[#898989] mb-2">
                                            <VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']"
                                                class="inline-block relative top-[2px]">
                                                <i class="pi pi-info-circle text-[#5472d4]"></i>
                                                <template #popper>
                                                    <div role="tooltip" class="max-w-[250px]">Would you like to set an
                                                        expiration on this agent?</div>
                                                </template>
                                            </VTooltip>
                                            Expiration Date <span class="text-[#ff0000]">*</span>
                                        </label>
                                        <Calendar show-icon show-button-bar class="w-full" name="agentExpirationDate"
                                            id="agentExpirationDate" required="true" v-model="agentExpirationDate" :manualInput="false" dateFormat="yy-mm-dd" />
                                    </div>

                                    <div class="w-full max-w-full px-4 mb-6">
                                        <label for="agentDescription"
                                            class="block text-base text-[#898989] mb-2">Description</label>
                                        <Textarea class="w-full resize-none" name="agentDescription"
                                            id="agentDescription" aria-labelledby="aria-description" rows="5"
                                            maxlength="150" />
                                        <p class="text-xs text-[#898989]">(150 Characters)</p>
                                    </div>

                                    <div class="w-full max-w-full px-4 mb-6">
                                        <label for="agentWelcomeMessage" class="block text-base text-[#898989] mb-2">
                                            <VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']"
                                                class="inline-block relative top-[2px]">
                                                <i class="pi pi-info-circle text-[#5472d4]"></i>
                                                <template #popper>
                                                    <div role="tooltip" class="max-w-[250px]">Provide a message to
                                                        display when a user starts a new conversation with the agent. If
                                                        a message is not provided, the default welcome message will be
                                                        displayed.</div>
                                                </template>
                                            </VTooltip>
                                            Welcome Message
                                        </label>
                                        <Textarea class="w-full resize-none" name="agentWelcomeMessage"
                                            id="agentWelcomeMessage" aria-labelledby="aria-welcome-message-desc"
                                            rows="5" v-model="textCounter" @input="updateCharacterCount" />
                                        <p class="text-xs text-[#898989]">(<span class="charectersControl">{{
                                            characterCount }}</span>
                                            Characters)</p>
                                    </div>
                                </div>
                            </div>
                              <div class="w-full max-w-full md:max-w-[100%] px-4 mb-5 text-center md:text-right" v-if="activeTabIndex === 0">
                                <ul class="flex flex-wrap justify-center md:justify-end list-none p-0">
                                    <li class="mb-4 pr-3">
                                        <Button v-if="!isEditMode" label="Create" severity="primary" class="min-h-[45px] min-w-[125px]" @click="onCreateAgent" :loading="isCreating" :disabled="isCreating" />
                                        <Button v-else label="Save" severity="primary" class="min-h-[45px] min-w-[125px]" @click="onSaveAgent" />
                                    </li>
                                    <li class="mb-4">
                                        <Button label="Cancel" severity="secondary" class="min-h-[45px] min-w-[125px]" @click="onCancel" />
                                    </li>
                                 </ul>
                              </div>
                        </div>

                    </TabPanel>

                    <TabPanel header="AI Configuration" :disabled="!isEditMode">
                        <div class="px-4 py-8 mt-8 border border-solid border-gray-300">
                            <div class="w-full max-w-[1000px] mx-auto">
                                <div class="flex flex-wrap -mx-4">
                                    <div class="w-full max-w-full md:max-w-[50%] px-4">
                                        <div class="mb-6">
                                            <label for="chatModel" class="block text-base text-[#898989] mb-2">
                                                <VTooltip :auto-hide="isMobile"
                                                    :popper-triggers="isMobile ? [] : ['hover']"
                                                    class="inline-block relative top-[2px]">
                                                    <i class="pi pi-info-circle text-[#5472d4]"></i>
                                                    <template #popper>
                                                        <div role="tooltip" class="max-w-[250px]">Lorem ipsum dolor sit
                                                            amet consectetur adipisicing elit. Voluptate tenetur iure,
                                                            distinctio soluta nostrum corporis excepturi consectetur
                                                            vitae mollitia eum cumque corrupti necessitatibus? Nihil
                                                            vero, dolorem nesciunt perspiciatis voluptas amet!</div>
                                                    </template>
                                                </VTooltip>
                                                Chat Model <span class="text-[#ff0000]">*</span>
                                            </label>
                                            <Dropdown
                                                class="w-full"
                                                :options="aiModels"
                                                optionLabel="name"
                                                optionValue="object_id"
                                                v-model="selectedAIModel"
                                                placeholder="--Select--"
                                                aria-label="Select a chat model"
                                                :filter="true"
                                                :showClear="true"
                                                :virtualScrollerOptions="{ itemSize: 38 }"
                                                :itemTemplate="(model: ResourceBase) => model?.name || model?.display_name || model?.object_id"
                                            />
                                        </div>

                                        <div class="mb-6">
                                            <label for="systemPrompt" class="block text-base text-[#898989] mb-2">
                                                <VTooltip :auto-hide="isMobile"
                                                    :popper-triggers="isMobile ? [] : ['hover']"
                                                    class="inline-block relative top-[2px]">
                                                    <i class="pi pi-info-circle text-[#5472d4]"></i>
                                                    <template #popper>
                                                        <div role="tooltip" class="max-w-[250px]">You are an analytic
                                                            agent named Khalil that helps people find information about
                                                            FoundationaLLM. Provide concise answers that are polite and
                                                            professional.</div>
                                                    </template>
                                                </VTooltip>
                                                System Prompt
                                            </label>
                                            <Textarea
                                                class="w-full resize-none"
                                                name="systemPrompt"
                                                id="systemPrompt"
                                                aria-labelledby="aria-system-prompt"
                                                rows="5"
                                            />
                                        </div>
                                    </div>

                                    <div class="w-full max-w-full md:max-w-[50%] px-4 mb-6">
                                        <div class="flex flex-wrap items-center mt-8 max-w-[275px] m-auto">
                                            <div class="w-full max-w-[calc(100%-50px)] pr-4">
                                                <p class="block text-base text-[#898989] my-0">
                                                    <VTooltip :auto-hide="isMobile"
                                                        :popper-triggers="isMobile ? [] : ['hover']"
                                                        class="inline-block relative top-[2px]">
                                                        <i class="pi pi-info-circle text-[#5472d4]"></i>
                                                        <template #popper>
                                                            <div role="tooltip" class="max-w-[250px]">Lorem ipsum dolor
                                                                sit amet, consectetur adipisicing elit. Iusto quae
                                                                molestias quam numquam alias?</div>
                                                        </template>
                                                    </VTooltip>
                                                    Image Generation
                                                </p>
                                            </div>

                                            <div class="w-full max-w-[50px]">
                                                <InputSwitch
                                                    v-model="$appStore.autoHideToasts"
                                                    class="csm-input-switch-1"
                                                />
                                            </div>
                                        </div>

                                        <div class="flex flex-wrap items-center mt-8 max-w-[275px] m-auto">
                                            <div class="w-full max-w-[calc(100%-50px)] pr-4">
                                                <p class="block text-base text-[#898989] my-0">
                                                    <VTooltip :auto-hide="isMobile"
                                                        :popper-triggers="isMobile ? [] : ['hover']"
                                                        class="inline-block relative top-[2px]">
                                                        <i class="pi pi-info-circle text-[#5472d4]"></i>
                                                        <template #popper>
                                                            <div role="tooltip" class="max-w-[250px]">Lorem ipsum dolor
                                                                sit amet, consectetur adipisicing elit. Iusto quae
                                                                molestias quam numquam alias?</div>
                                                        </template>
                                                    </VTooltip>
                                                    User Portal File Upload
                                                </p>
                                            </div>

                                            <div class="w-full max-w-[50px]">
                                                <InputSwitch
                                                    v-model="$appStore.showToastLogs"
                                                    class="csm-input-switch-1"
                                                />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </TabPanel>

                    <TabPanel header="Data Sources" :disabled="!isEditMode">
                        <div class="px-4 py-8 mt-8 border border-solid border-gray-300">
                            <div class="w-full max-w-[1000px] mx-auto">
                                <div class="mb-6">
                                    <p class="block text-base text-[#898989] mb-6">
                                        <VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']"
                                            class="inline-block relative top-[2px]">
                                            <i class="pi pi-info-circle text-[#5472d4]"></i>
                                            <template #popper>
                                                <div role="tooltip" class="max-w-[250px]">Lorem ipsum dolor sit amet
                                                    consectetur adipisicing elit. Voluptate tenetur iure, distinctio
                                                    soluta nostrum corporis excepturi consectetur vitae mollitia eum
                                                    cumque corrupti necessitatibus? Nihil vero, dolorem nesciunt
                                                    perspiciatis voluptas amet!</div>
                                            </template>
                                        </VTooltip>
                                        Data Source(s)
                                    </p>

                                    <p class="block text-base text-[#898989] mb-3">Upload File(s)</p>

                                    <!-- File Upload Area -->
                                    <div class="custom-uploader" :class="{ 'drag-over': isDragOver }"
                                        @dragover.prevent="onDragOver" @dragleave.prevent="onDragLeave"
                                        @drop.prevent="onDrop" @click="triggerFileInput">
                                        <div class="upload-content">
                                            <i class="pi pi-upload text-3xl text-[#94a3b8] mb-3 block"></i>
                                            <p class="text-lg text-[#64748b] font-normal mb-2">Drop and drag or <span class="text-[#5472d4] underline">choose file</span> to
                                                upload</p>
                                            <p class="text-sm text-[#94a3b8] font-medium italic">(Only PDF)</p>
                                        </div>
                                        <input ref="fileInput" type="file" accept=".pdf"
                                            @change="onFileSelect" class="hidden">
                                    </div>

                                    <!-- Selected files preview list -->
                                    <div v-if="uploadedFiles.length > 0" class="mt-8">
                                        <table class="w-full text-left border-collapse">
                                            <thead>
                                                <tr>
                                                    <th class="mnt-b-bottom p-3 bg-[#5472d4] text-white">File name</th>
                                                    <th class="mnt-b-bottom p-3 bg-[#5472d4] text-white text-center">Size</th>
                                                    <th class="mnt-b-bottom p-3 bg-[#5472d4] text-white text-center">Actions</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr v-for="(file, idx) in uploadedFiles" :key="file.name + file.size">
                                                    <td class="mnt-b-bottom p-3">{{ file.name }}</td>
                                                    <td class="mnt-b-bottom p-3 text-center">{{ formatFileSize(file.size) }}</td>
                                                    <td class="mnt-b-bottom p-3 text-center">
                                                        <Button label="Remove" severity="secondary" @click="removeFile(idx)" class="min-h-[45px] min-w-[125px]"/>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                </div>

                                <div class="mt-10">
                                    <p class="block text-base text-[#898989] mb-3">Existing File(s)</p>

                                    <div v-if="filesLoading" class="text-sm text-[#64748b]">Loading files...</div>
                                    <div v-else-if="filesError" class="text-sm text-red-600">{{ filesError }}</div>
                                    <div v-else>
                                        <div v-if="agentFiles.length === 0" class="text-sm text-[#94a3b8] italic">No files found for the selected agent.</div>
                                        <table v-else class="w-full text-left border-collapse">
                                            <thead>
                                                <tr>
                                                    <th class="mnt-b-bottom p-3 bg-[#5472d4] text-white">File ID</th>
                                                    <th class="mnt-b-bottom p-3 bg-[#5472d4] text-white">Filename</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr v-for="f in agentFiles" :key="f.resource?.name">
                                                    <td class="mnt-b-bottom p-3">{{ f.resource?.name }}</td>
                                                    <td class="mnt-b-bottom p-3">{{ f.resource?.filename || '-' }}</td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </TabPanel>

                    <TabPanel header="Share" :disabled="!isEditMode"></TabPanel>
                </TabView>
            </div>
        </div>
    </main>
</template>

<script lang="ts">
import api from '@/js/api';
    import type { CreateAgentFromTemplateRequest, KnowledgeManagementAgent } from '@/js/types';
import type { ResourceBase } from '@/js/types/index';
    import { defineComponent } from 'vue';
    import NavBarSettings from '~/components/NavBarSettings.vue';

    export default defineComponent({
        name: 'CreateAgent',

        components: {
            NavBarSettings,
        },

        data() {
            return {
                isMobile: window.screen.width < 950,
                textCounter: '',
                characterCount: 0,
                isDragOver: false,
                uploadedFiles: [] as File[],
							activeTabIndex: 0,
							isEditMode: false,
							isCreating: false,
							createdAgent: null as KnowledgeManagementAgent | null,
							agentExpirationDate: null as Date | null,
							filesLoading: false as boolean,
							filesError: '' as string,
							agentFiles: [] as any[],
							aiModels: [] as ResourceBase[],
                selectedAIModel: null as string | null,
            };
        },

        mounted() {
            this.fetchAIModels();
        },

        methods: {
            async fetchAIModels() {
                try {
                    const wrappers = await api.getAIModels();

					this.aiModels = Array.isArray(wrappers)
						? wrappers.map((w: any) => w.resource)
						: [];
				} catch (e) {
					this.aiModels = [];
				}
			},
			updateCharacterCount() {
            this.characterCount = this.textCounter.length;
        },
        onTabChange(e: { index: number }) {
            this.activeTabIndex = e.index;
        },
        onCreateAgent() {
            if (this.isCreating) return;
            // Collect form data
            const displayName = (document.getElementById('agentDisplayName') as HTMLInputElement)?.value || '';
            const description = (document.getElementById('agentDescription') as HTMLTextAreaElement)?.value || '';
            const welcomeMessage = (document.getElementById('agentWelcomeMessage') as HTMLTextAreaElement)?.value || '';
            // AGENT_NAME: Use a derived value or ask user for a unique name
            const agentName = displayName.replace(/\s+/g, '') + Date.now();
            // Format date to yyyy-MM-ddT00:00:00+00:00
            let formattedDate = '';
            if (this.agentExpirationDate) {
                const d = new Date(this.agentExpirationDate);
                formattedDate = d.toISOString().split('T')[0] + 'T00:00:00+00:00';
            }
            const payload: CreateAgentFromTemplateRequest = {
                AGENT_NAME: agentName,
                AGENT_DISPLAY_NAME: displayName,
                AGENT_EXPIRATION_DATE: formattedDate,
                AGENT_DESCRIPTION: description,
                AGENT_WELCOME_MESSAGE: welcomeMessage,
            };
            this.isCreating = true;
            api.createAgentFromTemplate(payload)
                .then((res) => {
                    this.createdAgent = res.resource;
                    this.isEditMode = true;
                    this.activeTabIndex = 1;
                })
                .catch((err) => {
                    this.$toast.add({ severity: 'error', summary: 'Error', detail: err.message || 'Failed to create agent', life: 5000 });
                })
                .finally(() => {
                    this.isCreating = false;
                });
        },
        onSaveAgent() {
            // Save logic for edit mode (not implemented)
            this.$toast.add({ severity: 'success', summary: 'Saved', detail: 'Agent changes saved.', life: 3000 });
        },
        onCancel() {
            // Cancel logic (redirect or reset form)
            this.$router.push('/');
        },
        // ...existing file upload and utility methods...
        onDragOver(event: DragEvent) { this.isDragOver = true; },
        onDragLeave(event: DragEvent) { this.isDragOver = false; },
        onDrop(event: DragEvent) { this.isDragOver = false; const files = Array.from(event.dataTransfer?.files || []); this.handleFiles(files); },
        triggerFileInput() { const input = this.$refs.fileInput as HTMLInputElement;
                if (input) input.value = '';
                input?.click(); },
        onFileSelect(event: Event) { const target = event.target as HTMLInputElement; const files = Array.from(target.files || []); this.handleFiles(files); if (target) target.value = '';
            },
        handleFiles(files: File[]) {
            const allowedTypes = [ 'application/pdf', ];
            const validFiles = files.filter(file => {
                if (!allowedTypes.includes(file.type)) { console.warn(`File type not supported: ${file.name}`); return false; }
                if (file.size > 10 * 1024 * 1024) { console.warn(`File too large: ${file.name}`); return false; }
                return true;
            });
            this.uploadedFiles.push(...validFiles);
        },
			removeFile(index: number) {
				this.uploadedFiles.splice(index, 1);
			},

			formatFileSize(bytes: number): string {
				if (bytes === 0) return '0 Bytes';
				const k = 1024;
				const sizes = ['Bytes', 'KB', 'MB', 'GB'];
				const i = Math.floor(Math.log(bytes) / Math.log(k));
				return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
			},

async loadAgentFiles(agentName?: string) {
                this.filesError = '';
                this.filesLoading = true;
                try {
                    const resolvedAgentName = agentName;
                    if (!resolvedAgentName) {
                        this.agentFiles = [];
                        return;
                    }

                    const results = await api.getAgentPrivateFiles(resolvedAgentName);
                    this.agentFiles = Array.isArray(results) ? results : [];
                } catch (e: any) {
                    this.filesError = e?.message || 'Failed to load files.';
                    this.agentFiles = [];
                } finally {
                    this.filesLoading = false;
                }
            },
        },
    });
</script>

<style lang="scss">
    .csm-backto-chats-1 {
        margin-bottom: 30px;

        a {
            color: var(--primary-button-bg);
            text-decoration: none;
        }
    }

    .csm-input-switch-1 {
        &.p-inputswitch {
            .p-inputswitch-slider {
                border-radius: 50px;
                background: transparent;
                border: 3px solid #334581;

                &:before {
                    background: #334581;
                    border-radius: 50%;
                    left: 0.1rem;
                }
            }

            &.p-highlight {
                .p-inputswitch-slider {
                    background: #334581;

                    &:before {
                        background-color: #ffffff;
                        transform: translateX(1.2rem);
                    }
                }
            }
        }
    }

    .custom-uploader {
        border: 2px dashed #94a3b8;
        padding: 2rem;
        text-align: center;
        border-radius: 5px;
        cursor: pointer;
        transition: all 0.3s ease;

        &.drag-over {
            transform: scale(1.02);
        }

        .upload-content {
            pointer-events: none;
        }
    }

    .hidden {
        display: none;
    }

    .mnt-b-bottom{
        border-bottom: 1px solid #94a3b8;
    }
</style>

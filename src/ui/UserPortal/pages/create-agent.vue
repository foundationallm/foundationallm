<template>
    <header role="banner">
        <NavBarSettings />
    </header>

    <main id="main-content">
        <Toast position="top-center" />

        <div class="w-full max-w-[1430px] mx-auto px-4 py-7">
            <div class="csm-backto-chats-1">
                <nuxt-link to="/" class="backto-chats">
                    <i class="pi pi-angle-left relative top-[2px]"></i> {{ isEditMode ? 'Return to Chats' : 'Return to Chats' }}
                </nuxt-link>
            </div>

            <div class="flex flex-wrap items-center -mx-4">
                <div class="w-full max-w-full md:max-w-[50%] px-4 mb-5 text-center md:text-left">
                    <h2 class="page-header text-3xl text-[#334581]">{{ isEditMode ? 'Edit Agent' : 'Create Agent' }}</h2>
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
                                        <div class="relative">
                                            <InputText v-model="agentDisplayName" type="text" class="w-full pr-10"
                                                name="agentDisplayName" id="agentDisplayName" required="true"
                                                maxlength="50"
                                                @input="() => { onDisplayNameInput(); onAgentNameChange(); }" />
                                            <span v-if="displayNameStatus === 'loading'"
                                                class="absolute right-2 top-1/2 -translate-y-1/2">
                                                <i class="pi pi-spin pi-spinner text-blue-500"></i>
                                            </span>
                                            <span v-else-if="displayNameStatus === 'success'"
                                                class="absolute right-2 top-1/2 -translate-y-1/2">
                                                <i class="pi pi-check-circle text-green-500"></i>
                                            </span>
                                            <span v-else-if="displayNameStatus === 'error'"
                                                class="absolute right-2 top-1/2 -translate-y-1/2">
                                                <i class="pi pi-times-circle text-red-500"></i>
                                            </span>
                                        </div>
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
                                            id="agentExpirationDate" required="true" v-model="agentExpirationDate"
                                            :manualInput="false" dateFormat="yy-mm-dd" />
                                    </div>

                                    <div class="w-full max-w-full px-4 mb-6">
                                        <label for="agentDescription"
                                            class="block text-base text-[#898989] mb-2">Description</label>
                                        <Textarea class="w-full resize-none" name="agentDescription"
                                            id="agentDescription" aria-labelledby="aria-description" rows="5"
                                            maxlength="150" v-model="agentDescription" />
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

                                        <CustomQuillEditor
                                            v-model="welcomeMessage"
                                            :initial-content="JSON.parse(JSON.stringify(welcomeMessage))"
                                            class="w-full"
                                            placeholder="Enter agent welcome message"
                                            aria-labelledby="aria-welcome-message-desc"
                                            @content-update="updateAgentWelcomeMessage($event)"
                                            name="agentWelcomeMessage"
                                            id="agentWelcomeMessage"
                                        />

                                        <p class="text-xs text-[#898989]">(<span class="charectersControl">{{
                                            characterCount }}</span>
                                            Characters)</p>
                                    </div>
                                </div>
                            </div>
                            <div class="w-full max-w-full md:max-w-[100%] px-4 mb-5 text-center md:text-right"
                                v-if="activeTabIndex === 0">
                                <ul class="flex flex-wrap justify-center md:justify-end list-none p-0">
                                    <li class="mb-4 pr-3">
                                        <Button v-if="!isEditMode" label="Create" severity="primary"
                                            class="min-h-[45px] min-w-[125px]" @click="onCreateAgent"
                                            :loading="isCreating" :disabled="isCreating" />
                                        <Button v-else label="Save" severity="primary"
                                            class="min-h-[45px] min-w-[125px]" @click="onSaveAgent" />
                                    </li>
                                    <li class="mb-4">
                                        <Button label="Cancel" severity="secondary" class="min-h-[45px] min-w-[125px]"
                                            @click="onCancel" />
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
                                            <Dropdown class="w-full" :options="aiModels"
                                                :optionLabel="model => model.display_name || model.name"
                                                optionValue="object_id" v-model="selectedAIModel"
                                                placeholder="--Select--" aria-label="Select a chat model" :filter="true"
                                                :showClear="true" :virtualScrollerOptions="{ itemSize: 38 }"
                                                :itemTemplate="(model: ResourceBase) => model?.name || model?.display_name || model?.object_id" />
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
                                                v-model="systemPrompt"
                                                :readonly="!isEditMode"
                                            />
                                            <Button
                                                v-if="isEditMode"
                                                label="Save System Prompt"
                                                class="mt-2 min-h-[35px] min-w-[100px]"
                                                :loading="isSavingSystemPrompt"
                                                :disabled="isSavingSystemPrompt || !createdAgent"
                                                @click="onSaveSystemPrompt"
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
                                                <InputSwitch v-model="$appStore.autoHideToasts"
                                                    class="csm-input-switch-1" />
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
                                                <InputSwitch v-model="$appStore.showToastLogs"
                                                    class="csm-input-switch-1" />
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
                                            <p class="text-lg text-[#64748b] font-normal mb-2">Drop and drag or <span
                                                    class="text-[#5472d4] underline">choose file</span> to
                                                upload</p>
                                            <p class="text-sm text-[#94a3b8] font-medium italic">(Any file type)</p>
                                        </div>
                                        <input ref="fileInput" type="file" @change="onFileSelect"
                                            class="hidden">
                                    </div>

                                    <!-- Selected files preview list -->
                                    <div v-if="uploadedFiles.length > 0" class="mt-8">
                                        <div class="mb-4">
                                            <Button label="Upload Files" severity="primary" @click="uploadFiles"
                                                :loading="filesLoading" :disabled="filesLoading"
                                                class="min-h-[45px] min-w-[125px]" />
                                        </div>

                                        <table class="w-full text-left border-collapse">
                                            <thead>
                                                <tr>
                                                    <th class="mnt-b-bottom p-3 bg-[#5472d4] text-white">File name</th>
                                                    <th class="mnt-b-bottom p-3 bg-[#5472d4] text-white text-center">
                                                        Size</th>
                                                    <th class="mnt-b-bottom p-3 bg-[#5472d4] text-white text-center">
                                                        Actions</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr v-for="(file, idx) in uploadedFiles" :key="file.name + file.size">
                                                    <td class="mnt-b-bottom p-3">{{ file.name }}</td>
                                                    <td class="mnt-b-bottom p-3 text-center">{{
                                                        formatFileSize(file.size) }}</td>
                                                    <td class="mnt-b-bottom p-3 text-center">
                                                        <Button label="Remove" severity="secondary"
                                                            @click="removeFile(idx)"
                                                            class="min-h-[45px] min-w-[125px]" />
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                </div>

                                <div class="mt-10">
                                    <div class="flex justify-between items-center mb-3">
                                        <p class="block text-base text-[#898989]">Existing File(s)</p>
                                        <div class="flex items-center gap-2">
                                            <Button label="Load Files" severity="secondary" @click="loadAgentFiles"
                                                :loading="filesLoading" :disabled="filesLoading || !selectedAgentName"
                                                class="min-h-[35px] min-w-[100px]" />
                                        </div>
                                    </div>

                                    <div v-if="filesLoading" class="text-sm text-[#64748b] mt-10">Loading files...</div>
                                    <div v-else-if="filesError" class="text-sm text-red-600 mt-10">{{ filesError }}
                                    </div>
                                    <div v-else>
                                        <div v-if="agentFiles.length === 0" class="text-sm text-[#94a3b8] italic mt-10">
                                            No files found for
                                            the selected agent.</div>
                                        <table v-else class="w-full text-left border-collapse">
                                            <thead>
                                                <tr>
                                                    <th class="mnt-b-bottom p-3 bg-[#5472d4] text-white">Filename</th>
                                                    <th class="mnt-b-bottom p-3 bg-[#5472d4] text-white">File ID</th>
                                                    <th class="mnt-b-bottom p-3 bg-[#5472d4] text-white text-center">Actions</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr v-for="f in agentFiles" :key="f.resource?.name">
                                                    <td class="mnt-b-bottom p-3">{{ f.resource?.display_name ||
                                                        f.resource?.filename || '-'
                                                    }}</td>
                                                    <td class="mnt-b-bottom p-3">{{ f.resource?.name }}</td>
                                                    <td class="mnt-b-bottom p-3 text-center">
                                                        <Button
                                                            label="Delete"
                                                            severity="danger"
                                                            @click="deleteFile(f.resource?.name)"
                                                            class="min-h-[35px] min-w-[80px]"
                                                        />
                                                    </td>
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
import { debounce } from '@/js/helpers';
import type { AgentBase } from '@/js/types';
import type { AgentCreationFromTemplateRequest, ResourceBase } from '@/js/types/index';
import mime from 'mime';
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
                isSavingSystemPrompt: false,
                createdAgent: null as AgentBase | null,
                agentExpirationDate: null as Date | null,
                filesLoading: false as boolean,
                filesError: '' as string,
                agentFiles: [] as any[],
                agentDisplayName: '',
                displayNameStatus: '', // '', 'loading', 'success', 'error'
                displayNameDebouncedCheck: null as null | ((name: string) => void),
                aiModels: [] as ResourceBase[],
                selectedAIModel: null as string | null,
                systemPrompt: '',

                agentDescription: '',
                welcomeMessage: '',

                selectedAgentName: null as string | null,
                availableAgents: [] as any[],
                agentsLoaded: false as boolean,
                imageGenerationEnabled: false as boolean,
                userPortalFileUploadEnabled: false as boolean,
            };
        },

    mounted() {
        // Setup debounced check function
        this.displayNameDebouncedCheck = debounce(this.checkDisplayName, 500);
        this.fetchAIModels();
        this.loadAvailableAgents();

        // Check if we're in edit mode
        this.checkEditMode();
    },

    methods: {
        async checkEditMode() {
            // Check if we're in edit mode based on query parameters
            const query = this.$route.query;
            if (query.edit === 'true' && query.agentName) {
                this.isEditMode = true;
                this.selectedAgentName = query.agentName as string;

                // Load the agent data
                await this.loadAgentForEditing();
            }
        },

        async loadAgentForEditing() {
            if (!this.selectedAgentName) return;

            try {
                // Get the specific agent directly by name
                const agentResult = await api.getAgent(this.selectedAgentName);

                if (agentResult?.resource) {
                    this.createdAgent = agentResult.resource;

                    // Populate form fields
                    this.agentDisplayName = this.createdAgent.display_name || '';
                    this.agentDescription = this.createdAgent.description || '';

                    // Load welcome message from properties
                    if (this.createdAgent.properties?.welcome_message) {
                        this.welcomeMessage = this.createdAgent.properties.welcome_message;
                        this.characterCount = this.welcomeMessage.length;
                    }

                    // Load expiration date
                    if (this.createdAgent.expiration_date) {
                        this.agentExpirationDate = new Date(this.createdAgent.expiration_date);
                    }

                    // Load system prompt
                    try {
                        const prompt = await api.getAgentMainPrompt(this.createdAgent);
                        this.systemPrompt = prompt || '';
                    } catch (e) {
                        console.warn('Could not load system prompt:', e);
                    }

                    // Load current AI model
                    this.loadCurrentAIModel();

                    // Load agent files
                    await this.loadAgentFiles();

                    // Switch to first tab after loading
                    this.activeTabIndex = 0;
                } else {
                    throw new Error('Agent not found');
                }
            } catch (error: any) {
                this.$toast.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: error.message || 'Failed to load agent for editing',
                    life: 5000
                });
                // Redirect back if agent loading fails
                this.$router.push('/');
            }
        },

        loadCurrentAIModel() {
            if (!this.createdAgent?.workflow?.resource_object_ids) return;

            // Find the current main model from the workflow
            for (const [key, obj] of Object.entries(this.createdAgent.workflow.resource_object_ids)) {
                if (obj && obj.properties && obj.properties.object_role === 'main_model') {
                    this.selectedAIModel = obj.object_id;
                    break;
                }
            }
        },

        async onDisplayNameInput() {
            this.displayNameStatus = this.agentDisplayName ? 'loading' : '';
            if (this.displayNameDebouncedCheck) {
                this.displayNameDebouncedCheck(this.agentDisplayName);
            }
        },

        async checkDisplayName(name: string) {
            if (!name) {
                this.displayNameStatus = '';
                return;
            }

            if (!this.isValidDisplayName(name)) {
                this.displayNameStatus = 'error';
                return;
            }

            try {
                const res = await api.checkAgentNameAvailability(name);
                this.displayNameStatus = (res.status === 'Allowed' && !res.exists && !res.deleted) ? 'success' : 'error';
            } catch (e) {
                this.displayNameStatus = 'error';
            }
        },

        isValidDisplayName(name: string): boolean {
            if (!name || !name.trim()) {
                return false;
            }

            const trimmedName = name.trim();

            // Check length
            if (trimmedName.length < 2 || trimmedName.length > 50) {
                return false;
            }

            // Check for valid characters (allow letters, numbers, spaces, hyphens, and common punctuation)
            const validPattern = /^[a-zA-Z0-9\s\-.,!?()]+$/;
            if (!validPattern.test(trimmedName)) {
                return false;
            }

            // Check that it doesn't start or end with special characters
            if (/^[\s\-.,!?()]/.test(trimmedName) || /[\s\-.,!?()]$/.test(trimmedName)) {
                return false;
            }

            return true;
        },

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

        async loadAvailableAgents() {
            if (this.agentsLoaded) return;

            try {
                const agents = await api.getAgents();
                this.availableAgents = agents.map((a: any) => ({
                    name: a?.resource?.name,
                    displayName: a?.resource?.display_name
                }));
                this.agentsLoaded = true;
            } catch (err) {
                console.error('Error loading agents:', err);
                this.availableAgents = [];
            }
        },

        findAgentNameByName(agentName: string): string | null {
            if (!agentName.trim()) return null;

            const trimmedInput = agentName.trim().toLowerCase();

            let match = this.availableAgents.find(a =>
                a.name && a.name.trim() && a.name.toLowerCase() === trimmedInput
            );
            if (match && match.name) return match.name;

            match = this.availableAgents.find(a =>
                a.name && a.name.trim() && a.name === agentName.trim()
            );
            if (match && match.name) return match.name;

            match = this.availableAgents.find(a =>
                (a.displayName || '').toLowerCase().trim() === trimmedInput
            );
            if (match && match.name && match.name.trim()) return match.name;

            const hyphenated = agentName.trim().replace(/\s+/g, '-');
            match = this.availableAgents.find(a =>
                a.name && a.name.trim() && a.name.toLowerCase() === hyphenated.toLowerCase()
            );
            if (match && match.name) return match.name;

            return null;
        },

        updateAgentWelcomeMessage(newContent: string) {
			this.welcomeMessage = newContent;
            this.characterCount = this.welcomeMessage.length;
		},


        generateAgentName(displayName: string): string {
            if (!displayName || !displayName.trim()) {
                return 'agent';
            }

            let slug = displayName.trim();

            // Remove special characters and keep only alphanumeric, hyphens, and underscores (preserve case)
            slug = slug.replace(/[^a-zA-Z0-9\s_-]/g, '');

            // Replace multiple spaces with single hyphen
            slug = slug.replace(/\s+/g, '-');

            // Remove multiple consecutive hyphens
            slug = slug.replace(/-+/g, '-');

            // Remove leading and trailing hyphens
            slug = slug.replace(/^-+|-+$/g, '');

            // Ensure it's not empty
            if (!slug || slug.length === 0) {
                slug = 'agent';
            }

            // Ensure it starts with a letter (required by resource name pattern)
            if (slug.length > 0 && !/^[a-zA-Z]/.test(slug)) {
                slug = 'agent-' + slug;
            }

            return slug;
        },

        onTabChange(e: { index: number }) {
            this.activeTabIndex = e.index;
            if (e.index === 2 && this.selectedAgentName && this.agentFiles.length === 0) {
                this.loadAgentFiles();
            }

            // If switching to AI Configuration tab in edit mode, ensure we have the current model selected
            if (e.index === 1 && this.isEditMode && this.createdAgent && !this.selectedAIModel) {
                this.loadCurrentAIModel();
            }
        },
        async onCreateAgent() {
            if (this.isCreating || this.isEditMode) return;

            // Collect form data from v-model bindings
            const displayName = this.agentDisplayName || '';
            const description = this.agentDescription || '';
            const welcomeMessage = this.welcomeMessage || '';
            // AGENT_NAME: Create a proper slug from display name
            const agentName = this.generateAgentName(displayName);
            // Format date to yyyy-MM-ddT00:00:00+00:00
            let formattedDate = '';
            if (this.agentExpirationDate) {
                const d = new Date(this.agentExpirationDate);
                formattedDate = d.toISOString().split('T')[0] + 'T00:00:00+00:00';
            }
            const payload: AgentCreationFromTemplateRequest = {
                AGENT_NAME: agentName,
                AGENT_DISPLAY_NAME: displayName,
                AGENT_EXPIRATION_DATE: formattedDate,
                AGENT_DESCRIPTION: description,
                AGENT_WELCOME_MESSAGE: welcomeMessage,
            };
            this.isCreating = true;
            try {
                const res = await api.createAgentFromTemplate(payload);
                this.createdAgent = res.resource;
                this.selectedAgentName = res.resource?.name;
                this.isEditMode = true;
                this.activeTabIndex = 1;
                this.agentsLoaded = false;
                this.loadAvailableAgents();
                const prompt = await api.getAgentMainPrompt(res.resource);
                this.systemPrompt = prompt || '';
            } catch (err: any) {
                this.$toast.add({ severity: 'error', summary: 'Error', detail: err.message || 'Failed to create agent', life: 5000 });
            } finally {
                this.isCreating = false;
            }
        },

        async onSaveAgent() {
            if (!this.createdAgent) {
                this.$toast.add({ severity: 'error', summary: 'Error', detail: 'No agent to update.', life: 5000 });
                return;
            }

            // Update agent model with new values from v-model bindings
            const displayName = this.agentDisplayName || '';
            const description = this.agentDescription || '';
            const welcomeMessage = this.welcomeMessage || '';
            let formattedDate = '';
            if (this.agentExpirationDate) {
                const d = new Date(this.agentExpirationDate);
                formattedDate = d.toISOString().split('T')[0] + 'T00:00:00+00:00';
            }

            // Set values on the agent model
            this.createdAgent.display_name = displayName;
            this.createdAgent.description = description;
            if (!this.createdAgent.properties) this.createdAgent.properties = {};
            this.createdAgent.properties.welcome_message = welcomeMessage;
            this.createdAgent.expiration_date = formattedDate;

            try {
                // Update main model if changed
                let currentMainModelId = null;
                if (this.createdAgent.workflow && this.createdAgent.workflow.resource_object_ids) {
                    for (const [key, obj] of Object.entries(this.createdAgent.workflow.resource_object_ids)) {
                        if (obj && obj.properties && obj.properties.object_role === 'main_model') {
                            currentMainModelId = obj.object_id;
                            break;
                        }
                    }
                }

                const modelIdToUpdate = this.selectedAIModel || currentMainModelId;
                if (modelIdToUpdate && modelIdToUpdate !== currentMainModelId) {
                    const updatedAgent = await api.updateAgentMainModel(this.createdAgent, modelIdToUpdate);
                    this.createdAgent.object_id = updatedAgent.object_id;
                }

                // Update system prompt if changed
                if (this.systemPrompt !== '') {
                    await api.updateAgentMainPrompt(this.createdAgent, this.systemPrompt);
                }

                this.$toast.add({ severity: 'success', summary: 'Agent Updated', detail: 'Agent changes saved successfully.', life: 3000 });
            } catch (err: any) {
                this.$toast.add({ severity: 'error', summary: 'Error', detail: err.message || 'Failed to update agent', life: 5000 });
            }
        },

        async onSaveSystemPrompt() {
            if (!this.createdAgent) return;
            this.isSavingSystemPrompt = true;
            try {
                await api.updateAgentMainPrompt(this.createdAgent, this.systemPrompt);
                this.$toast.add({ severity: 'success', summary: 'System Prompt Updated', detail: 'The system prompt was updated successfully.', life: 3000 });
            } catch (err: any) {
                this.$toast.add({ severity: 'error', summary: 'Error', detail: err.message || 'Failed to update system prompt', life: 5000 });
            } finally {
                this.isSavingSystemPrompt = false;
            }
        },

        onCancel() {
            // Cancel logic (redirect or reset form)
            if (this.isEditMode) {
                // If editing, go back to previous page or reset form
                this.$router.push('/');
            } else {
                // If creating, just go back
                this.$router.push('/');
            }
        },

        // ...existing file upload and utility methods...
        onDragOver(event: DragEvent) {
            this.isDragOver = true;
        },

        onDragLeave(event: DragEvent) {
            this.isDragOver = false;
        },

        onDrop(event: DragEvent) {
            this.isDragOver = false;
            const files = Array.from(event.dataTransfer?.files || []);
            this.handleFiles(files);
        },

        triggerFileInput() {
            const input = this.$refs.fileInput as HTMLInputElement;
            if (input) input.value = '';
            input?.click();
        },

        onFileSelect(event: Event) {
            const target = event.target as HTMLInputElement;
            const files = Array.from(target.files || []);
            this.handleFiles(files);
            if (target) target.value = '';
        },

        handleFiles(files: File[]) {
            // Allow all file types, but keep the size restriction (10MB)
            const validFiles = files.filter(file => {
                if (file.size > 10 * 1024 * 1024) {
                    console.warn(`File too large: ${file.name}`);
                    return false;
                }
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

        async uploadFiles() {
            if (this.uploadedFiles.length === 0 || !this.selectedAgentName) {
                return;
            }

            let filesUploaded = 0;
            let filesFailed = 0;

            for (const file of this.uploadedFiles) {
                try {
                    let uploadFile = file;
                    if (file.name) {
                        const mimeType = mime.getType(file.name) || 'application/pdf';
                        uploadFile = new File([file], file.name, { type: mimeType });
                    }

                    const formData = new FormData();
                    formData.append('file', uploadFile);

                    await api.uploadAgentFile(this.selectedAgentName, file.name, formData);

                    filesUploaded++;

                } catch (error: any) {
                    filesFailed++;
                    console.error('Upload error:', error);
                }
            }

            if (filesUploaded > 0) {
                this.uploadedFiles = [];
                await this.loadAgentFiles();
            }
        },

        async loadAgentFiles() {
            if (!this.selectedAgentName) {
                this.agentFiles = [];
                return;
            }

            this.filesError = '';
            this.filesLoading = true;

            try {
                const results = await api.getAgentPrivateFiles(this.selectedAgentName);
                this.agentFiles = Array.isArray(results) ? results : [];
            } catch (e: any) {
                this.filesError = e?.message || 'Failed to load files.';
                this.agentFiles = [];
            } finally {
                this.filesLoading = false;
            }
        },

        onAgentNameChange() {
            const agentName = (this.agentDisplayName || '').trim();

            if (agentName) {
                // In edit mode, use the current agent name
                if (this.isEditMode && this.createdAgent?.name) {
                    this.selectedAgentName = this.createdAgent.name;
                } else {
                    // In create mode, try to find the agent by name
                    this.selectedAgentName = this.findAgentNameByName(agentName);
                }

                if (this.selectedAgentName) {
                    this.loadAgentFiles();
                } else {
                    this.agentFiles = [];
                }
            } else {
                this.selectedAgentName = null;
                this.agentFiles = [];
            }
        },

        async deleteFile(fileName: string) {
            if (!this.selectedAgentName) {
                return;
            }

            try {
                await api.deleteAgentFile(this.selectedAgentName, fileName);
                // Remove from both uploaded files and existing files lists
                this.uploadedFiles = this.uploadedFiles.filter(f => f.name !== fileName);
                this.agentFiles = this.agentFiles.filter(f => f.resource?.name !== fileName);
                this.$toast.add({ severity: 'success', summary: 'Success', detail: `File "${fileName}" deleted.`, life: 3000 });
            } catch (error: any) {
                this.$toast.add({ severity: 'error', summary: 'Error', detail: `Failed to delete file "${fileName}": ${error.message}`, life: 5000 });
                console.error('Delete error:', error);
            }
        }
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

.mnt-b-bottom {
    border-bottom: 1px solid #94a3b8;
}
</style>

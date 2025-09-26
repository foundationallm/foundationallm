<template>
    <!-- Access Denied Message for 403 Errors -->
    <div v-if="$appConfigStore.hasConfigurationAccessError" class="access-denied-overlay">
        <div class="access-denied-container">
            <div class="access-denied-icon">
                <i class="pi pi-ban" style="font-size: 4rem; color: #e74c3c;"></i>
            </div>
            <h2 class="access-denied-title">Access Denied</h2>
            <p class="access-denied-message">
                {{ $appConfigStore.configurationAccessErrorMessage }}
            </p>
        </div>
    </div>

    <!-- Normal Page Content -->
    <div v-else>
        <header role="banner">
            <NavBarSettings />
        </header>

        <main id="main-content">
        <Toast position="top-center" />

        <div class="w-full max-w-[1430px] mx-auto px-4 py-7">
            <div class="csm-backto-chats-1">
                <nuxt-link :to="returnToConversationsUrl" class="backto-chats">
                    <i class="pi pi-angle-left relative top-[2px]"></i> {{ returnToConversationsText }}
                </nuxt-link>
            </div>

            <div class="flex flex-wrap items-center -mx-4">
                <div class="w-full max-w-full md:max-w-[50%] px-4 mb-5 text-center md:text-left">
                    <h2 class="page-header text-3xl text-[#334581]">{{ isEditMode ? 'Edit Agent' : 'Create Agent' }}</h2>
                </div>

                <!-- Guidance shown until an agent is created -->
                <div
                    v-if="!isEditMode"
                    class="w-full max-w-[1000px] mx-auto px-4 mb-10 text-center text-base text-black"
                >
                    Please add the general information for your custom agent to create it. Upon creation you will be able to set the AI configuration, add data sources, and share your agent with others.
                </div>
            </div>

            <!-- Loading State for Edit Mode -->
            <div v-if="isEditMode && loadingAgentData" class="loading-container">
                <i class="pi pi-spin pi-spinner" style="font-size: 2rem"></i>
                <p>Loading agent data...</p>
            </div>

            <!-- Error State for Edit Mode -->
            <div v-else-if="isEditMode && agentLoadError" class="error-message">
                <i class="pi pi-exclamation-triangle" style="font-size: 2rem; color: #e74c3c;"></i>
                <p>{{ agentLoadError }}</p>
                <Button label="Retry" @click="loadAgentForEditing" />
            </div>

            <!-- Loading State for Create Mode -->
            <div v-else-if="!isEditMode && isCreating" class="loading-container">
                <i class="pi pi-spin pi-spinner" style="font-size: 2rem"></i>
                <p>Creating agent...</p>
            </div>

            <!-- Main Content -->
            <div v-else class="mb-4 cms-create-agent-tab-1">
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
                                            Expiration Date
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
                                            class="w-full max-w-[100%!important]"
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
                                                <InputSwitch v-model="(($appStore as any).autoHideToasts)"
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
                                                <InputSwitch v-model="(($appStore as any).showToastLogs)"
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
                                                        <Button icon="pi pi-times" severity="secondary"
                                                            text rounded
                                                            @click="removeFile(idx)"
                                                            class="min-h-[35px] min-w-[35px]"
                                                            aria-label="Remove file" />
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                </div>

                                <div class="mt-10">
                                    <div class="flex justify-between items-center mb-3">
                                        <p class="block text-base text-[#898989]">Existing File(s)</p>
                                    </div>

                                    <div v-if="filesLoading" class="loading-container">
                                        <i class="pi pi-spin pi-spinner" style="font-size: 2rem"></i>
                                        <p>Loading files...</p>
                                    </div>
                                    <div v-else-if="filesError" class="error-message">
                                        <i class="pi pi-exclamation-triangle" style="font-size: 2rem; color: #e74c3c;"></i>
                                        <p>{{ filesError }}</p>
                                        <Button label="Retry" @click="loadAgentFiles" />
                                    </div>
                                    <div v-else>
                                        <div v-if="agentFiles.length === 0" class="empty-state">
                                            <i class="pi pi-info-circle" style="font-size: 2rem; color: #6c757d;"></i>
                                            <p>No files found for the selected agent.</p>
                                        </div>
                                        <table v-else class="w-full text-left border-collapse">
                                            <thead>
                                                <tr>
                                                    <th class="mnt-b-bottom p-3 bg-[#5472d4] text-white">Filename</th>
                                                    <th class="mnt-b-bottom p-3 bg-[#5472d4] text-white text-center">Actions</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr v-for="f in agentFiles" :key="f.resource?.name">
                                                    <td class="mnt-b-bottom p-3">{{ f.resource?.display_name ||
                                                        f.resource?.filename || '-'
                                                    }}</td>
                                                    <td class="mnt-b-bottom p-3 text-center">
                                                        <Button
                                                            icon="pi pi-trash"
                                                            severity="danger"
                                                            text
                                                            rounded
                                                            @click="deleteFile(f.resource?.name)"
                                                            class="min-h-[35px] min-w-[35px]"
                                                            aria-label="Delete file"
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

                    <TabPanel header="Share" :disabled="!isEditMode">
                        <div class="px-4 py-8 mt-8 border border-solid border-gray-300">
                            <div class="w-full max-w-[1000px] mx-auto">
                                <div class="mb-6">
                                    <p class="block text-base text-[#898989] mb-6">
                                        <VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']"
                                            class="inline-block relative top-[2px]">
                                            <i class="pi pi-info-circle text-[#5472d4]"></i>
                                            <template #popper>
                                                <div role="tooltip" class="max-w-[250px]">View role assignments for this agent. Shows which users and groups have been granted specific roles to access and use this agent, including their names, email addresses, and assigned permissions.</div>
                                            </template>
                                        </VTooltip>
                                        Share with Individual Users
                                    </p>
                                </div>

                                <div class="flex items-center flex-wrap mb-6">
                                    <div class="w-full max-w-full md:max-w-[45%] lg:max-w-[35%] mb-4">
                                        <Button label="Add Role Assignment" severity="primary" class="min-h-[45px] min-w-[125px] w-full md:w-auto" @click="openAddRoleAssignmentModal"/>
                                    </div>

                                    <div class="w-full max-w-full md:max-w-[55%] lg:max-w-[65%] mb-4 relative">
                                        <div class="flex items-center">
                                            <label for="searchTabelInput1"
                                            class="block text-base text-[#898989] min-w-[70px]">Search</label>
                                            
                                            <InputText 
                                                type="text" 
                                                class="w-full" 
                                                name="searchTabelInput1" 
                                                id="searchTabelInput1" 
                                                v-model="globalFilter"
                                                placeholder="Search by principal name, email, role, or type..."
                                            />

                                            <Button aria-label="Search Users" severity="primary" icon="pi pi-search" class="min-h-[40px] min-w-[40px] w-auto absolute top-0 right-0 z-[2]"/>
                                        </div>
                                    </div>
                                </div>

                                <div class="mb-6">
                                    <div v-if="roleAssignmentsLoading" class="loading-container">
                                        <i class="pi pi-spin pi-spinner" style="font-size: 2rem"></i>
                                        <p>Loading role assignments...</p>
                                    </div>
                                    <div v-else-if="roleAssignmentsError" class="error-message">
                                        <i class="pi pi-exclamation-triangle" style="font-size: 2rem; color: #e74c3c;"></i>
                                        <p>{{ roleAssignmentsError }}</p>
                                        <Button label="Retry" @click="loadRoleAssignments" />
                                    </div>
                                    <div v-else>
                                        <div v-if="roleAssignments.length === 0" class="empty-state">
                                            <i class="pi pi-info-circle" style="font-size: 2rem; color: #6c757d;"></i>
                                            <p>No role assignments found for this agent.</p>
                                        </div>

                                        <div class="csm-table-area-1" v-else>
                                            <DataTable 
                                                :value="filteredRoleAssignments" 
                                                scrollable
                                                scrollHeight="400px"
                                                responsiveLayout="scroll"
                                                :sortOrder="1"
                                                class="p-datatable-sm csm-dataTable-1"
                                                tableStyle="min-width: 50rem"
                                            >
                                                <Column 
                                                    field="resource.principal_details.name" 
                                                    header="Principal Name" 
                                                    :sortable="true" 
                                                    style="min-width: 200px"
                                                >
                                                    <template #body="slotProps">
                                                        {{ slotProps.data.resource.principal_details?.name || slotProps.data.resource.principal_id || '-' }}
                                                    </template>
                                                </Column>

                                                <Column 
                                                    field="resource.principal_type" 
                                                    header="Principal Type" 
                                                    :sortable="true" 
                                                    style="min-width: 200px"
                                                >
                                                    <template #body="slotProps">
                                                        {{ slotProps.data.resource.principal_type || '-' }}
                                                    </template>
                                                </Column>

                                                <Column 
                                                    field="resource.principal_details.email" 
                                                    header="Principal Email" 
                                                    :sortable="false" 
                                                    style="min-width: 150px"
                                                >
                                                    <template #body="slotProps">
                                                        {{ slotProps.data.resource.principal_details?.email || '-' }}
                                                    </template>
                                                </Column>

                                                <Column 
                                                    field="resource.principal_details.onPremisesAccountName" 
                                                    header="On-premises account" 
                                                    :sortable="false" 
                                                    style="min-width: 150px"
                                                >
                                                    <template #body="slotProps">
                                                        {{ slotProps.data.resource.principal_details?.onPremisesAccountName || '-' }}
                                                    </template>
                                                </Column>

                                                <Column 
                                                    field="resource.role_definition.display_name" 
                                                    header="Role" 
                                                    :sortable="true" 
                                                    style="min-width: 180px"
                                                >
                                                    <template #body="slotProps">
                                                        {{ slotProps.data.resource.role_definition?.display_name || 
                                                           slotProps.data.resource.role_definition_id || '-' }}
                                                    </template>
                                                </Column>

                                                <Column 
                                                    field="resource.scope_name" 
                                                    header="Scope" 
                                                    :sortable="false" 
                                                    style="min-width: 120px"
                                                >
                                                    <template #body="slotProps">
                                                        {{ slotProps.data.resource.scope_name || 'Direct Assignment' }}
                                                    </template>
                                                </Column>
                                            </DataTable>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </TabPanel>
                </TabView>
            </div>
        </div>

        <Dialog 
            v-model:visible="showAddRoleAssignmentModal" 
            modal 
            class="csm-roleDialog-modal-1"
            header="Add New Role Assignment" 
            :style="{ width: '30rem' }" 
            :breakpoints="{ '1199px': '50vw', '575px': '90vw' }"
        >
            <!-- Add New Role Assignment Section -->
            <div class="mb-4">
                <div class="mb-2">
                    <!-- User Search -->
                    <div class="relative mb-5">
                        <label for="userSearch" class="block text-sm font-medium text-[#898989] mb-2">
                            Search User
                        </label>
                        <div class="relative">
                            <InputText 
                                id="userSearch"
                                v-model="newRoleAssignment.userSearch"
                                @input="onUserSearchInput"
                                placeholder="Start typing user name..."
                                class="w-full"
                            />
                            <div v-if="userSearchLoading" class="absolute right-2 top-1/2 -translate-y-1/2">
                                <i class="pi pi-spin pi-spinner text-blue-500"></i>
                            </div>
                        </div>
                        
                        <!-- User Search Results Dropdown -->
                        <div v-if="userSearchResults.length > 0 && newRoleAssignment.userSearch" 
                                class="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-md shadow-lg max-h-60 overflow-auto">
                            <div v-for="user in userSearchResults" 
                                    :key="user.id"
                                    @click="selectUser(user)"
                                    class="px-3 py-2 hover:bg-gray-100 cursor-pointer border-b border-gray-100 last:border-b-0">
                                <div class="font-medium text-[#334581] mb-3">{{ user.display_name || user.name }}</div>
                                <div class="text-sm text-[#898989] mb-3">Email: {{ user.email || 'No email' }}</div>
                                <div class="text-sm text-[#898989]">ID: {{ user.id || 'No id' }}</div>
                            </div>
                        </div>
                    </div>

                    <!-- Role Selection -->
                    <div class="relative mb-5">
                        <label for="roleSelect" class="block text-sm font-medium text-[#898989] mb-2">
                            Select Role
                        </label>
                        <Dropdown 
                            id="roleSelect"
                            v-model="newRoleAssignment.selectedRole"
                            :options="availableRoles"
                            optionLabel="display_name"
                            optionValue="object_id"
                            placeholder="Select a role..."
                            class="w-full"
                            :loading="rolesLoading"
                        />
                    </div>

                    <!-- Set Primary Owner Checkbox (only shown when Owner role is selected) -->
                    <div v-if="isOwnerRoleSelected" class="relative mb-5">
                        <div class="flex items-center">
                            <Checkbox 
                                v-model="newRoleAssignment.setAsPrimaryOwner"
                                :binary="true"
                                inputId="setPrimaryOwner"
                                class="mr-2"
                            />
                            <label for="setPrimaryOwner" class="text-sm font-medium text-[#898989] cursor-pointer">
                                Primary Owner and Contact
                            </label>
                        </div>
                    </div>
                </div>

                <!-- Selected User Display -->
                <div v-if="newRoleAssignment.selectedUser" class="mb-4 p-3 bg-blue-50 border border-blue-200 rounded-md">
                    <div class="flex items-center justify-between">
                        <div>
                            <span class="text-sm font-medium text-[#334581]">Selected User:</span>
                            <span class="ml-2 text-sm text-[#898989]">
                                {{ newRoleAssignment.selectedUser.display_name || newRoleAssignment.selectedUser.name }}
                                <span v-if="newRoleAssignment.selectedUser.email" class="ml-2 text-gray-500">
                                    ({{ newRoleAssignment.selectedUser.email }})
                                </span>
                            </span>
                        </div>
                        <Button 
                            icon="pi pi-times" 
                            severity="secondary" 
                            text 
                            @click="clearSelectedUser"
                            class="p-1"
                        />
                    </div>
                </div>
            </div>
            
            <template #footer>
                <div class="flex justify-between items-center w-full">
                    <div class="w-full max-w-[50%]">
                        <Button 
                            label="Cancel" 
                            class="w-full" 
                            @click="closeAddModal" 
                            text 
                        />
                    </div>

                    <div class="w-full max-w-[50%]">
                        <Button 
                            label="Add Role Assignment"
                            severity="primary w-full"
                            @click="addRoleAssignment"
                            :disabled="!canAddRoleAssignment || addingRoleAssignment"
                            :loading="addingRoleAssignment"
                            class="w-full"
                        />
                    </div>
                </div>
            </template>
        </Dialog>

        <!-- Delete file confirmation dialog -->
        <Dialog
            v-if="fileToDelete !== null"
            v-focustrap
            :visible="fileToDelete !== null"
            :closable="false"
            class="sidebar-dialog"
            modal
            header="Delete file"
            @keydown.esc="fileToDelete = null"
        >
            <div v-if="filesLoading" class="delete-dialog-content">
                <div role="status">
                    <i
                        class="pi pi-spin pi-spinner"
                        style="font-size: 2rem"
                        role="img"
                        aria-label="Loading"
                    ></i>
                </div>
            </div>
            <div v-else>
                <p>Do you want to delete the file "{{ fileToDelete }}" ?</p>
            </div>
            <template #footer>
                <Button
                    class="sidebar-dialog__button"
                    label="Cancel"
                    text
                    autofocus
                    :disabled="filesLoading"
                    @click="fileToDelete = null"
                />
                <Button
                    class="sidebar-dialog__button"
                    label="Delete"
                    severity="danger"
                    :disabled="filesLoading"
                    @click="confirmDelete"
                />
            </template>
        </Dialog>
    </main>
    </div>
</template>


<script lang="ts">
import api from '@/js/api';
import { debounce, isAgentReadonly } from '@/js/helpers';
import type { AgentBase } from '@/js/types';
import type { AgentCreationFromTemplateRequest, ResourceBase } from '@/js/types/index';
import mime from 'mime';
import { defineComponent } from 'vue';
import NavBarSettings from '~/components/NavBarSettings.vue';
import '@/styles/agents.scss';
import '@/styles/loading.scss';
import '@/styles/access-denied.scss';

import Column from 'primevue/column';
import DataTable from 'primevue/datatable';
import Dropdown from 'primevue/dropdown';
import Checkbox from 'primevue/checkbox';

// Constants for allowed roles
const ALLOWED_ROLE_NAMES = ['Reader', 'Contributor', 'Owner'];

export default defineComponent({
    name: 'CreateAgent',

    components: {
        NavBarSettings,
        DataTable,
        Column,
        Dropdown,
        Checkbox,
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
                fileToDelete: null as string | null,
                availableAgents: [] as any[],
                agentsLoaded: false as boolean,
                imageGenerationEnabled: false as boolean,
                userPortalFileUploadEnabled: false as boolean,

                // Track if page was opened from Settings -> Agents
                openedFromSettings: false as boolean,

                roleAssignments: [] as any[],
                roleAssignmentsLoading: false as boolean,
                roleAssignmentsError: '' as string,
                filters: {} as any,
                globalFilter: '' as string,

                // New role assignment form data
                newRoleAssignment: {
                    userSearch: '',
                    selectedRole: null as string | null,
                    selectedUser: null as any,
                    setAsPrimaryOwner: false as boolean,
                },
                userSearchLoading: false as boolean,
                userSearchResults: [] as any[],
                rolesLoading: false as boolean,
                availableRoles: [] as any[],
                addingRoleAssignment: false as boolean,
                showAddRoleAssignmentModal: false as boolean,
                
                // Loading states for edit mode
                loadingAgentData: false as boolean,
                agentLoadError: '' as string,
            };
        },

        computed: {
            filteredRoleAssignments() {
                if (!this.globalFilter) {
                    return this.roleAssignments;
                }
                
                const filter = this.globalFilter.toLowerCase();
                return this.roleAssignments.filter(assignment => {
                    const resource = assignment.resource || {};
                    
                    // Search in basic fields
                    const name = resource.name || '';
                    const displayName = resource.display_name || '';
                    
                    // Search in principal details
                    const principalName = resource.principal_details?.name || '';
                    const principalEmail = resource.principal_details?.email || '';
                    const principalType = resource.principal_type || '';
                    
                    // Search in role definition
                    const roleName = resource.role_definition?.display_name || '';
                    
                    return name.toLowerCase().includes(filter) || 
                           displayName.toLowerCase().includes(filter) ||
                           principalName.toLowerCase().includes(filter) ||
                           principalEmail.toLowerCase().includes(filter) ||
                           principalType.toLowerCase().includes(filter) ||
                           roleName.toLowerCase().includes(filter);
                });
            },

            canAddRoleAssignment() {
                // Add safety checks to prevent undefined access
                if (!this.newRoleAssignment) {
                    return false;
                }
                const hasUser = !!this.newRoleAssignment.selectedUser;
                const hasRole = !!this.newRoleAssignment.selectedRole;
                return hasUser && hasRole;
            },

            isOwnerRoleSelected() {
                if (!this.newRoleAssignment?.selectedRole || !this.availableRoles.length) {
                    return false;
                }
                
                // Find the selected role in available roles
                const selectedRole = this.availableRoles.find(role => role.object_id === this.newRoleAssignment.selectedRole);
                return selectedRole?.display_name === 'Owner';
            },

            returnToConversationsUrl() {
                // Check if we should return to manage-agents page
                const query = this.$route.query;
                if (query.returnTo === 'manage-agents') {
                    return '/manage-agents';
                }
                
                // If opened from Settings -> Agents, return to main page (Settings modal will remain open)
                // Otherwise, return to main page
                return '/';
            },

            returnToConversationsText() {
                // Check if we should return to manage-agents page
                const query = this.$route.query;
                if (query.returnTo === 'manage-agents') {
                    return 'Return to Manage Agents';
                }
                
                // Default text for other cases
                return 'Return to conversations';
            }
        },

    mounted() {
        // Setup debounced check function
        this.displayNameDebouncedCheck = debounce(this.checkDisplayName, 500);
        this.fetchAIModels();
        this.loadAvailableAgents();
        this.loadAvailableRoles(); // Load available roles on mount
        
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
                
                // Check if page was opened from Settings -> Agents
                // This is indicated by the presence of agentId parameter (which is set in ChatSidebar.vue)
                this.openedFromSettings = !!query.agentId;
                
                // Load the agent data
                await this.loadAgentForEditing();
            }
        },

        async loadAgentForEditing() {
            if (!this.selectedAgentName) return;
            
            this.loadingAgentData = true;
            this.agentLoadError = '';
            
            try {
                // Get the specific agent directly by name
                const agentResult = await api.getAgent(this.selectedAgentName);

                
                if (agentResult?.resource) {
                    this.createdAgent = {
                        ...agentResult.resource,
                        isReadonly: isAgentReadonly(agentResult?.roles || []),
                    };
                    
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
                this.agentLoadError = error.message || 'Failed to load agent for editing';
                this.$toast.add({ 
                    severity: 'error', 
                    summary: 'Error', 
                    detail: this.agentLoadError, 
                    life: 5000 
                });
            } finally {
                this.loadingAgentData = false;
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
            // Do not derive name and do not check name for an existing agent (edit mode)
            if (this.isEditMode) {
                this.displayNameStatus = '';
                return;
            }
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
                // Check the availability of the generated agent name, not the display name
                const generatedAgentName = this.generateAgentName(name);
                const res = await api.checkAgentNameAvailability(generatedAgentName);
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

        async validateAgentNameAvailability(): Promise<boolean> {
            const displayName = this.agentDisplayName || '';
            if (!displayName.trim()) {
                this.$toast.add({ 
                    severity: 'error', 
                    summary: 'Validation Error', 
                    detail: 'Display name is required.', 
                    life: 5000 
                });
                return false;
            }

            if (!this.isValidDisplayName(displayName)) {
                this.$toast.add({ 
                    severity: 'error', 
                    summary: 'Validation Error', 
                    detail: 'Display name format is invalid.', 
                    life: 5000 
                });
                return false;
            }

            const agentName = this.generateAgentName(displayName);
            try {
                const nameCheck = await api.checkAgentNameAvailability(agentName);
                if (nameCheck.status !== 'Allowed' || nameCheck.exists || nameCheck.deleted) {
                    this.$toast.add({ 
                        severity: 'error', 
                        summary: 'Name Not Available', 
                        detail: `Please choose a different name.`, 
                        life: 5000 
                    });
                    return false;
                }
                return true;
            } catch (err: any) {
                this.$toast.add({ 
                    severity: 'error', 
                    summary: 'Error', 
                    detail: 'Failed to check agent name availability. Please try again.', 
                    life: 5000 
                });
                return false;
            }
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

            // If switching to Share tab in edit mode, load role assignments
            if (e.index === 3 && this.isEditMode && this.selectedAgentName) {
                this.loadRoleAssignments();
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
            
            // Check if the generated agent name is available before creating
            if (!(await this.validateAgentNameAvailability())) {
                return;
            }
            
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
                this.loadCurrentAIModel();
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
            
            // Permission check
            if (this.createdAgent?.isReadonly) {
                this.$toast.add({ severity: 'error', summary: 'Permission Denied', detail: 'You have read-only access to this agent.', life: 5000 });
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

            // Do not generate or check agent name on update (edit mode)
            // Only create agent name on creation, not on update

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
                if (modelIdToUpdate) {
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

        onReturnToConversations() {
            // If opened from Settings -> Agents, ensure modal stays open
            if (this.openedFromSettings) {
                (this.$appStore as any).settingsModalVisible = true;
            }
            this.$router.push(this.returnToConversationsUrl);
        },

        onCancel() {
            // Cancel logic (redirect or reset form)
            // Use the same logic as Return to Chats button
            this.onReturnToConversations();
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
            let associationsFailed = 0;

            for (const file of this.uploadedFiles) {
                try {
                    let uploadFile = file;
                    if (file.name) {
                        const mimeType = mime.getType(file.name) || 'application/pdf';
                        uploadFile = new File([file], file.name, { type: mimeType });
                    }

                    const formData = new FormData();
                    formData.append('file', uploadFile);

                    // Upload the file
                    const uploadResult = await api.uploadAgentFile(this.selectedAgentName, file.name, formData);
                    
                    // Extract file ID from upload result
                    let fileId = null;
                    if (uploadResult?.resource?.name) {
                        fileId = uploadResult.resource.name;
                    } else if (uploadResult?.name) {
                        fileId = uploadResult.name;
                    } else {
                        // If we can't get the file ID from the response, try to extract it from the file name
                        // This is a fallback - ideally the API should return the file ID
                        fileId = file.name;
                    }

                    // Associate the file with Knowledge tool
                    if (fileId) {
                        try {
                            await api.associateFileWithKnowledgeTool(this.selectedAgentName, fileId);
                        } catch (associationError: any) {
                            console.error('File association error:', associationError);
                            associationsFailed++;
                            // Continue with the upload process even if association fails
                        }
                    }

                    filesUploaded++;

                } catch (error: any) {
                    filesFailed++;
                    console.error('Upload error:', error);
                }
            }

            if (filesUploaded > 0) {
                this.uploadedFiles = [];
                await this.loadAgentFiles();
                
                // Show appropriate success/error messages
                if (associationsFailed > 0) {
                    this.$toast.add({ 
                        severity: 'warn', 
                        summary: 'Files Uploaded', 
                        detail: `${filesUploaded} file(s) uploaded successfully, however ${associationsFailed} file(s) could not be associated with Knowledge tool.`, 
                        life: 5000 
                    });
                } else {
                    this.$toast.add({ 
                        severity: 'success', 
                        summary: 'Files Uploaded', 
                        detail: `${filesUploaded} file(s) uploaded and associated with Knowledge tool successfully.`, 
                        life: 3000 
                    });
                }
            }

            if (filesFailed > 0) {
                this.$toast.add({ 
                    severity: 'error', 
                    summary: 'Upload Failed', 
                    detail: `${filesFailed} file(s) failed to upload.`, 
                    life: 5000 
                });
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

        deleteFile(fileName: string) {
            this.fileToDelete = fileName;
        },

        async confirmDelete() {
            if (!this.selectedAgentName || !this.fileToDelete) {
                return;
            }

            try {
                await api.deleteAgentFile(this.selectedAgentName, this.fileToDelete);
                // Remove from both uploaded files and existing files lists
                this.uploadedFiles = this.uploadedFiles.filter(f => f.name !== this.fileToDelete);
                this.agentFiles = this.agentFiles.filter(f => f.resource?.name !== this.fileToDelete);
                this.$toast.add({ severity: 'success', summary: 'Success', detail: `File "${this.fileToDelete}" deleted.`, life: 3000 });
            } catch (error: any) {
                this.$toast.add({ severity: 'error', summary: 'Error', detail: `Failed to delete file "${this.fileToDelete}": ${error.message}`, life: 5000 });
                console.error('Delete error:', error);
            } finally {
                this.fileToDelete = null;
            }
        },

        async loadRoleAssignments() {
            if (!this.selectedAgentName) {
                this.roleAssignments = [];
                return;
            }

            this.roleAssignmentsError = '';
            this.roleAssignmentsLoading = true;

            try {
                const scope = api.getAgentScopeIdentifier(this.selectedAgentName);
                const assignments = await api.getRoleAssignments(scope);
                
                if (!Array.isArray(assignments) || assignments.length === 0) {
                    this.roleAssignments = [];
                    return;
                }

                // Extract principal IDs for resolution
                const principalIds = assignments.map(assignment => assignment.resource.principal_id).filter(Boolean);

                // Get security principals and role definitions in parallel
                const [principalsResult, roleDefinitionsResult] = await Promise.allSettled([
                    principalIds.length > 0 ? api.getSecurityPrincipals(principalIds) : Promise.resolve([]),
                    api.getRoleDefinitions()
                ]);

                // Extract results or fallback to empty arrays
                const principals = principalsResult.status === 'fulfilled' ? principalsResult.value : [];
                const roleDefinitions = roleDefinitionsResult.status === 'fulfilled' ? roleDefinitionsResult.value : [];

                // Log warnings for failed requests
                if (principalsResult.status === 'rejected') {
                    console.warn('Failed to load security principals:', principalsResult.reason);
                }
                if (roleDefinitionsResult.status === 'rejected') {
                    console.warn('Failed to load role definitions:', roleDefinitionsResult.reason);
                }

                // Map assignments with resolved data
                this.roleAssignments = assignments.map(assignment => {
                    const principal = principals.find(p => p.id === assignment.resource.principal_id);
                    const roleDefinition = roleDefinitions.find(r => r.object_id === assignment.resource.role_definition_id);
                    
                    return {
                        ...assignment,
                        resource: {
                            ...assignment.resource,
                            principal_details: principal,
                            role_definition: roleDefinition
                        }
                    };
                });
            } catch (e: any) {
                this.roleAssignmentsError = e?.message || 'Failed to load role assignments.';
                this.roleAssignments = [];
            } finally {
                this.roleAssignmentsLoading = false;
            }
        },

        async loadAvailableRoles() {
            this.rolesLoading = true;
            try {
                const roles = await api.getRoleDefinitions();
                const allRoles = Array.isArray(roles) ? roles : [];
                
                // Filter roles to only include the three allowed roles: Reader, Contributor, Owner
                this.availableRoles = allRoles.filter(role => 
                    role.display_name && ALLOWED_ROLE_NAMES.includes(role.display_name)
                );
            } catch (e) {
                this.availableRoles = [];
            } finally {
                this.rolesLoading = false;
            }
        },

        async onUserSearchInput() {
            if (!this.newRoleAssignment?.userSearch?.trim()) {
                this.userSearchResults = [];
                return;
            }

            this.userSearchLoading = true;
            try {
                const results = await api.filterSecurityPrincipalsByName(this.newRoleAssignment.userSearch);
                this.userSearchResults = Array.isArray(results) ? results : [];
            } catch (e) {
                this.userSearchResults = [];
            } finally {
                this.userSearchLoading = false;
            }
        },

        selectUser(user: any) {
            if (this.newRoleAssignment) {
                this.newRoleAssignment.selectedUser = user;
                this.newRoleAssignment.userSearch = '';
                this.userSearchResults = [];
            }
        },

        clearSelectedUser() {
            if (this.newRoleAssignment) {
                this.newRoleAssignment.selectedUser = null;
            }
        },

        async addRoleAssignment() {
            if (!this.canAddRoleAssignment || !this.selectedAgentName) return;

            this.addingRoleAssignment = true;
            try {
                // Generate a new GUID for the role assignment name
                const roleAssignmentId = this.generateGuid();
                
                // Get the agent scope dynamically from the API
                const scope = api.getAgentScope(this.selectedAgentName);
                
                // Use exact payload format as specified in requirements
                const payload = {
                    name: roleAssignmentId,
                    description: "",
                    principal_id: this.newRoleAssignment.selectedUser.id,
                    role_definition_id: this.newRoleAssignment.selectedRole,
                    type: api.getRoleAssignmentType(),
                    principal_type: api.getPrincipalType(this.newRoleAssignment.selectedUser),
                    scope: scope
                };
                
                await api.createRoleAssignment(payload);
                
                // If Owner role is selected and setAsPrimaryOwner is checked, make the set-owner API call
                if (this.isOwnerRoleSelected && this.newRoleAssignment.setAsPrimaryOwner) {
                    try {
                        await this.setAgentPrimaryOwner();
                    } catch (ownerError: any) {
                        // Log the error but don't fail the entire operation
                        console.error('Failed to set primary owner:', ownerError);
                        this.$toast.add({ 
                            severity: 'warn', 
                            summary: 'Role Assignment Added', 
                            detail: `Role assignment added successfully, but failed to set as primary owner: ${ownerError.message || 'Unknown error'}`, 
                            life: 5000 
                        });
                        return; // Exit early to avoid showing success message
                    }
                }
                
                this.$toast.add({ 
                    severity: 'success', 
                    summary: 'Role Assignment Added', 
                    detail: `Role assignment for ${this.newRoleAssignment.selectedUser.display_name || this.newRoleAssignment.selectedUser.name} added successfully.`, 
                    life: 3000 
                });
                
                // Refresh the list and reset form
                await this.loadRoleAssignments();
                this.newRoleAssignment.selectedUser = null;
                this.newRoleAssignment.selectedRole = null;
                this.newRoleAssignment.setAsPrimaryOwner = false;
            } catch (e: any) {
                let errorMessage = 'Failed to add role assignment';
                
                if (e.message?.includes('Access is not authorized') || e.message?.includes('403')) {
                    errorMessage = 'You do not have permission to create role assignments. Please contact your administrator or use the Management Portal.';
                } else if (e.message) {
                    errorMessage = e.message;
                }
                
                this.$toast.add({ 
                    severity: 'error', 
                    summary: 'Authorization Required', 
                    detail: errorMessage, 
                    life: 8000 
                });
            } finally {
                this.addingRoleAssignment = false;
            }
        },

        generateGuid(): string {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
                const r = Math.random() * 16 | 0;
                const v = c === 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        },

        async setAgentPrimaryOwner() {
            if (!this.selectedAgentName || !this.newRoleAssignment.selectedUser) {
                throw new Error('Agent name and selected user are required');
            }

            // Get instance ID from the app store or API
            const instanceId = (this.$appStore as any)?.instanceId || api.instanceId || 'default-instance';
            
            // Prepare the AgentOwnerUpdateRequest payload
            const payload = {
                owner_user_id: this.newRoleAssignment.selectedUser.id
            };

            // Make the API call to set the primary owner
            const response = await api.setAgentPrimaryOwner(instanceId, this.selectedAgentName, payload);
            
            // Check if the operation was successful
            if (!response?.IsSuccess) {
                throw new Error('Failed to set primary owner: API returned unsuccessful result');
            }

            return response;
        },

        getPrincipalDisplayName(assignment: any): string {
            if (!assignment) return '-';
            return assignment.resource?.principal_details?.name || 
                   assignment.resource?.principal_id || 'Unknown Principal';
        },

        getRoleDisplayName(assignment: any): string {
            if (!assignment) return '-';
            return assignment.resource?.role_definition?.display_name || 
                   assignment.resource?.role_definition_id || 'Unknown Role';
        },
        openAddRoleAssignmentModal() {
            this.showAddRoleAssignmentModal = true;
        },
        
        closeAddModal() {
            this.showAddRoleAssignmentModal = false;
        },
    },
});
</script>

<style lang="scss">
/* Access Denied Styles */
.access-denied-overlay {
	position: fixed;
	top: 0;
	left: 0;
	width: 100vw;
	height: 100vh;
	background-color: var(--primary-bg, #fff);
	display: flex;
	justify-content: center;
	align-items: center;
	z-index: 9999;
}

.access-denied-container {
	text-align: center;
	padding: 2rem;
	max-width: 500px;
	margin: 0 auto;
}

.access-denied-icon {
	margin-bottom: 1.5rem;
}

.access-denied-title {
	font-size: 2rem;
	font-weight: 600;
	color: var(--primary-color, #131833);
	margin-bottom: 1rem;
	margin-top: 0;
}

.access-denied-message {
	font-size: 1.1rem;
	color: var(--secondary-text, #666);
	line-height: 1.6;
	margin: 0;
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
.csm-roleDialog-modal-1{
    .p-dialog-content{
        overflow-y: unset;
    }
}

.cms-create-agent-tab-1{
    .p-tabview-header{
        &.p-disabled{
            opacity: 0;
        }
    }
}

.delete-dialog-content {
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 2rem;
}

.sidebar-dialog {
    max-width: 90vw;
}

@media only screen and (max-width: 950px) {
    .sidebar-dialog {
        width: 95vw;
    }
}

.p-button-text.sidebar-dialog__button:focus {
    box-shadow: 0 0 0 0.1rem var(--primary-button-bg);
}

.sidebar-dialog__button:focus {
    box-shadow: 0 0 0 0.1rem #000;
}
</style>

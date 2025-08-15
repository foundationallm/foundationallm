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

                <div class="w-full max-w-full md:max-w-[50%] px-4 mb-5 text-center md:text-right">
                    <ul class="flex flex-wrap justify-center md:justify-end list-none p-0">
                        <li class="mb-4 pr-3">
                            <!-- Create agent -->
                            <Button label="Create" severity="primary" class="min-h-[45px] min-w-[125px]" />
                        </li>

                        <li class="mb-4">
                            <!-- Cancel -->
                            <Button label="Cancel" severity="secondary" class="min-h-[45px] min-w-[125px]" />
                        </li>
                    </ul>
                </div>
            </div>

            <div class="mb-4">
                <TabView>
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
                                            id="agentExpirationDate" type="text" required="true" />
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
                                        <Textarea
                                            class="w-full resize-none"
                                            name="agentWelcomeMessage"
                                            id="agentWelcomeMessage" 
                                            aria-labelledby="aria-welcome-message-desc"
                                            rows="5"
                                            v-model="textCounter"
                                            @input="updateCharacterCount"
                                        />
                                        <p class="text-xs text-[#898989]">(<span class="charectersControl">{{ characterCount }}</span>
                                            Characters)</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </TabPanel>

                    <TabPanel header="AI Configuration" :disabled="true"></TabPanel>

                    <TabPanel header="Data Sources" :disabled="true"></TabPanel>

                    <TabPanel header="Share" :disabled="true"></TabPanel>
                </TabView>
            </div>
        </div>
    </main>
</template>

<script lang="ts">
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
            };
        },

        methods: {
            updateCharacterCount() {
                this.characterCount = this.textCounter.length;
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
</style>

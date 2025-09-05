<template>
    <header role="banner">
        <NavBarSettings />
    </header>

    <main id="main-content">

        <div class="w-full max-w-[1430px] mx-auto px-4 py-7">
            <div class="csm-backto-chats-1">
                <nuxt-link to="/" class="backto-chats">
                    <i class="pi pi-angle-left relative top-[2px]"></i> Return to Chats
                </nuxt-link>
            </div>

            <div class="flex flex-wrap items-center -mx-4">
                <div class="w-full max-w-full md:max-w-[50%] px-4 mb-5 text-center md:text-left">
                    <h2 class="page-header text-3xl text-[#334581]">Manage Agents</h2>
                </div>
            </div>

            <div class="flex flex-wrap items-center -mx-4 mb-5">
                <div class="w-full max-w-full md:max-w-[50%] px-4 mb-5 text-center md:text-left">
                    <nuxt-link 
                        to="/create-agent"
                        class="p-button p-component create-agent-button">
                        New Agent <i class="pi pi-plus ml-3"></i>
                    </nuxt-link>
                </div>
            </div>
            
            <div class="csm-table-area-1">
                <!-- Loading State -->
                <div v-if="loading" class="loading-container">
                    <i class="pi pi-spin pi-spinner" style="font-size: 2rem"></i>
                    <p>Loading agents...</p>
                </div>

                <!-- Error State -->
                <div v-else-if="error" class="error-message">
                    <i class="pi pi-exclamation-triangle" style="font-size: 2rem; color: #e74c3c;"></i>
                    <p>{{ error }}</p>
                    <Button label="Retry" @click="loadAgents" />
                </div>

                <!-- Data Table -->
                <DataTable 
                    v-else
                    :value="filteredAgents"
                    scrollable
                    :scrollHeight="SCROLL_HEIGHT"
                    responsiveLayout="scroll"
                    :sortOrder="1"
                    class="p-datatable-sm csm-dataTable-1"
                    tableStyle="min-width: 50rem"
                >
                    <Column 
                        header="Name" 
                        :sortable="true" 
                        style="min-width: 200px"
                    >
                        <template #body="slotProps">
                            <div class="agent-name">
                                {{ slotProps.data.resource.display_name || slotProps.data.resource.name }}
                            </div>
                        </template>
                    </Column>

                    <Column 
                        header="Description" 
                        :sortable="false" 
                        style="min-width: 250px; max-width: 250px;"
                    >
                        <template #body="slotProps">
                            <div class="agent-description">
                                {{ slotProps.data.resource.description || 'No description' }}
                            </div>
                        </template>
                    </Column>

                    <Column 
                        header="Owner" 
                        :sortable="true" 
                        style="min-width: 200px"
                    >
                        <template #body="slotProps">
                            <div class="agent-owner">
                                {{ slotProps.data.resource.created_by || 'Unknown' }}
                            </div>
                        </template>
                    </Column>

                    <Column 
                        header="Role" 
                        :sortable="true" 
                        style="min-width: 150px"
                    >
                        <template #body="slotProps">
                            <div class="agent-role">
                                {{ getPrimaryRole(slotProps.data.roles) }}
                            </div>
                        </template>
                    </Column>

                    <Column 
                        header="Expiration" 
                        :sortable="true" 
                        style="min-width: 120px"
                    >
                        <template #body="slotProps">
                            <div class="agent-expiration">
                                {{ formatExpirationDate(slotProps.data.resource.expiration_date) }}
                            </div>
                        </template>
                    </Column>

                </DataTable>

                <!-- Empty State -->
                <div v-if="!loading && !error && filteredAgents.length === 0" class="empty-state">
                    <i class="pi pi-info-circle" style="font-size: 2rem; color: #6c757d;"></i>
                    <p>No agents found where you have Owner or Contributor access.</p>
                </div>
            </div>
        </div>
    </main>
</template>


<script lang="ts">
import { defineComponent, ref, computed, onMounted } from 'vue';
import NavBarSettings from '~/components/NavBarSettings.vue';
import type { ResourceProviderGetResult, AgentBase } from '@/js/types';
import api from '@/js/api';

import Column from 'primevue/column';
import DataTable from 'primevue/datatable';
import Button from 'primevue/button';

// Constants
const SCROLL_HEIGHT = '500px';
const SUPPORTED_ROLES = ['Owner', 'Contributor', 'Reader'] as const;
const DEFAULT_LOCALE = 'en-US';

export default defineComponent({
    name: 'ManageAgents',

    components: {
        NavBarSettings,
        DataTable,
        Column,
        Button,
    },

    setup() {
        // Reactive state with ref
        const agents = ref<ResourceProviderGetResult<AgentBase>[]>([]);
        const loading = ref(false);
        const error = ref<string | null>(null);

        // Computed property
        const filteredAgents = computed(() => {
            return agents.value.filter(agent => 
                agent.roles.includes('Owner') || agent.roles.includes('Contributor')
            );
        });

        // Methods
        const loadAgents = async () => {
            loading.value = true;
            error.value = null;
            
            try {
                agents.value = await api.getAllowedAgents();
            } catch (err) {
                console.error('Failed to load agents:', err);
                const errorMessage = err instanceof Error 
                    ? `Failed to load agents: ${err.message}`
                    : 'Failed to load agents. Please try again later.';
                error.value = errorMessage;
            } finally {
                loading.value = false;
            }
        };

        const getPrimaryRole = (roles: string[]): string => {
            for (const role of SUPPORTED_ROLES) {
                if (roles.includes(role)) return role;
            }
            return roles[0] || 'Unknown';
        };

        const formatExpirationDate = (expirationDate: string | null): string => {
            if (!expirationDate) return 'Never';
            
            try {
                const date = new Date(expirationDate);
                return date.toLocaleDateString(DEFAULT_LOCALE);
            } catch (err) {
                console.warn('Invalid expiration date format:', expirationDate);
                return 'Invalid date';
            }
        };

        // Lifecycle
        onMounted(() => {
            loadAgents();
        });

        return {
            agents,
            loading,
            error,
            filteredAgents,
            loadAgents,
            getPrimaryRole,
            formatExpirationDate,
            SCROLL_HEIGHT
        };
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

.csm-dataTable-1 {
    border: 1px solid #d1d5db;
    
    table {
        thead {
            tr {
                th {
                    background-color: #eeeeee;
                    padding: 15px 10px;
                    border-right: 2px solid #ffffff;
                    font-weight: 500;
                    border-bottom: 0px;
                    
                    &:last-child {
                        border-right: 0px;
                    }
                    
                    .p-column-title {
                        width: 100%;
                    }
                }
            }
        }
        
        tbody {
            tr {
                td {
                    padding: 15px 10px;
                    border-bottom: 2px solid #dedede;
                }

                &:last-child {
                    td {
                        border-bottom: 0px;
                    }
                }
            }
        }
    }
}

// Loading, Error, and Empty States
.loading-container,
.error-message,
.empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 40px 20px;
    text-align: center;
    min-height: 200px;
    
    i {
        margin-bottom: 16px;
    }
    
    p {
        margin: 8px 0;
        color: #6c757d;
    }
}

.error-message {
    i {
        color: #e74c3c;
    }
    
    p {
        color: #e74c3c;
        font-weight: 500;
    }
}

.create-agent-button {
    text-decoration: none;
    font-weight: 600;
}
</style>

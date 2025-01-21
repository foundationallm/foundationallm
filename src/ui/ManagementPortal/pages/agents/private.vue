<template>
    <main id="main-content">
        <h2 class="page-header">My Agents</h2>
        <div class="page-subheader">View your agents.</div>

        <AgentsList
            :agents="agents"
            :loading="loading"
            :loadingStatusText="loadingStatusText"
            @refreshAgents="getAgents"
        />
    </main>
</template>

<script lang="ts">
import api from '@/js/api';
import AgentsList from '@/components/AgentsList.vue';
import type {
    Agent,
    ResourceProviderGetResult,
} from '@/js/types';

export default {
    name: 'PrivateAgents',

    components: {
        AgentsList,
    },

    data() {
        return {
            agents: [] as ResourceProviderGetResult<Agent>[],
            loading: false as boolean,
            loadingStatusText: 'Retrieving data...' as string,
        };
    },

    async created() {
        await this.getAgents();
    },

    methods: {
        async getAgents() {
            this.loading = true;
            try {
                const agents = await api.getAgents();
                // Only retrieve agents where the user is an owner.
                this.agents = agents.filter((agent) => agent.roles.includes('Owner'));
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

<style lang="scss">
.main-content {
    width: 100%;
}
</style>

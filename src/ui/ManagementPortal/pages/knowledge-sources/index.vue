<template>
  <main id="main-content">
    <div style="display: flex">
        <div style="flex: 1">
            <h2 class="page-header">Knowledge Sources</h2>
            <div class="page-subheader"></div>
        </div>

        <AccessControl
            v-if="selectedKnowledgeSource"
            :scopes="[
                {
                    label: 'Knowledge Source',
                    value: `providers/FoundationaLLM.Context/knowledgeSources/${selectedKnowledgeSource.resource.name}`,
                },
            ]"
        />
    </div>

    <div :class="{ 'grid--loading': loading }">
        <!-- Loading overlay -->
        <template v-if="loading">
        <div class="grid__loading-overlay" role="status" aria-live="polite">
            <LoadingGrid />
            <div>{{ loadingStatusText }}</div>
        </div>
        </template>

        <div class="w-full flex gap-4">
            <Dropdown
                v-model="selectedKnowledgeSource"
                :options="knowledgeSources"
                option-label="resource.name"
                placeholder="-- Select Knowledge Source --"
            />
            <Button
                type="button"
                icon="pi pi-refresh"
                @click="getKnowledgeSources"
            />
        </div>

        <div
            v-if="selectedKnowledgeSource"
            class="query-grid">

            <div style="grid-row: span 5;">
                <div style="display: flex; flex-direction: column;">
                    <label class="query-grid-label" for="input-name">User prompt to test:</label>
                    <InputTextArea
                        id="input-name"
                        v-model="queryRequest.user_prompt"
                        class="w-full"
                        placeholder="Enter the user prompt..."
                        style="width: 90%; height: 100%; resize: none;"
                    />
                    <Button
                        type="button"
                        style="margin-top: 10px; width: 250px; display: flex; align-items: center; justify-content: center;"
                        @click="queryKnowledgeSource"
                    >Query Knowledge Source</Button>
                </div>
            </div>

            <div>
                <div><h4 class="page-header">Text content</h4></div>
                <div>
                    <label class="query-grid-label">Text chunks maximum count: {{ queryRequest.text_chunks_max_count }}</label>
                    <Slider
                        v-model="queryRequest.text_chunks_max_count"
                        :min="0"
                        :max="30"
                        :step="1"
                        style="width: 200px;"
                    />
                </div>
                <div>
                    <div class="flex items-center gap-2">
                        <label>Use semantic ranking: </label>
                        <Checkbox
                            v-model="queryRequest.use_semantic_ranking"
                            :binary="true"
                        />
                    </div>
                </div>
                <div
                    v-if="hasKnowledgeGraph">
                    <h4 class="page-header">Knowledge Graph</h4>
                </div>
                <div
                    v-if="hasKnowledgeGraph">
                    <label class="query-grid-label">Mapped entities max count: {{ knowledge_graph_query.mapped_entities_max_count }}</label>
                    <Slider
                        v-model="knowledge_graph_query.mapped_entities_max_count"
                        :min="0"
                        :max="30"
                        :step="1"
                        style="width: 200px;"
                    />
                </div>
                <div
                    v-if="hasKnowledgeGraph">
                    <label class="query-grid-label">Relationships max depth: {{ knowledge_graph_query.relationships_max_depth }}</label>
                    <Slider
                        v-model="knowledge_graph_query.relationships_max_depth"
                        :min="0"
                        :max="3"
                        :step="1"
                        style="width: 200px;"
                    />
                </div>
            </div>
            <div>
                <div></div>
                <div>
                    <label class="query-grid-label">Text chunks similarity threshold: {{ queryRequest.text_chunks_similarity_threshold }}</label>
                    <Slider
                        v-model="queryRequest.text_chunks_similarity_threshold"
                        :min="0"
                        :max="1"
                        :step="0.01"
                        style="width: 200px;"
                    />
                </div>
                <div></div>
                <div></div>
                <div
                    v-if="hasKnowledgeGraph">
                    <label class="query-grid-label">Mapped entities similarity threshold: {{ knowledge_graph_query.mapped_entities_similarity_threshold }}</label>
                    <Slider
                        v-model="knowledge_graph_query.mapped_entities_similarity_threshold"
                        :min="0"
                        :max="1"
                        :step="0.01"
                        style="width: 200px;"
                    />
                </div>
                <div
                    v-if="hasKnowledgeGraph">
                    <label class="query-grid-label">All entities max count: {{ knowledge_graph_query.all_entities_max_count }}</label>
                    <Slider
                        v-model="knowledge_graph_query.all_entities_max_count"
                        :min="0"
                        :max="40"
                        :step="1"
                        style="width: 200px;"
                    />
                </div>
            </div>
        </div>

        <div
            v-if="selectedKnowledgeSource"
            style="display: flex; flex-direction: column; height: 100%;">
            <label class="query-grid-label">Query Result:</label>
            <JsonEditorVue
                v-if="queryResponse"
                v-model="queryResponse"
                :read-only="true"
                style="overflow-y: auto; height: 600px;"
            />
            <div v-else style="color: #888;">No result</div>
        </div>

        <div
            v-if="hasKnowledgeGraph" 
            class="sigma-graph-container">
            <label class="query-grid-label">Knowledge Graph Visualization:</label>
            <div ref="sigmaContainer" style="width: 100%; height: 400px; border: 1px solid #ccc; border-radius: 4px; background: #fafbfc;"></div>
        </div>
    </div>
  </main>
</template>

<script lang="ts">
import api from '@/js/api';
import type { ResourceProviderGetResult } from '@/js/types';
import JsonEditorVue from 'json-editor-vue';
import Sigma from "sigma";
import Graph from "graphology";

export default {
  name: 'KnowledgeSources',

  components: {
		JsonEditorVue,
	},
  
  data() {
    return {
      knowledgeSources: [] as any[],
      selectedKnowledgeSource: null,
      hasKnowledgeGraph: false as boolean,
      queryRequest: {
        user_prompt : null as string | null,
        text_chunks_max_count: 20 as number,
        text_chunks_similarity_threshold: 0.30 as number,
        use_semantic_ranking: true as boolean,
        knowledge_graph_query: null,
      },
      knowledge_graph_query: {
            mapped_entities_max_count: 10 as number,
            mapped_entities_similarity_threshold: 0.35 as number,
            relationships_max_depth: 2 as number,
            all_entities_max_count: 20 as number
        } as any,
      queryResponse: null,
      sigmaInstance: null as Sigma | null,
      graphRenderingData: {
        nodes: [
            { id: '1', label: 'Node 1'},
            { id: '2', label: 'Node 2'},
            { id: '3', label: 'Node 3'}
        ] as any[],
        edges: [
            { id: '1-2', source: '1', target: '2' },
            { id: '2-3', source: '2', target: '3' }
        ] as any[]
      },
      loading: false as boolean,
      loadingStatusText: 'Retrieving knowledge sources...' as string,
    };
  },

  async created() {
    await this.getKnowledgeSources();
  },
  
  watch: {
    selectedKnowledgeSource(newVal) {
        this.hasKnowledgeGraph = !!(newVal && newVal.resource && newVal.resource.has_knowledge_graph);
        if (this.hasKnowledgeGraph) {
            this.queryRequest.knowledge_graph_query = this.knowledge_graph_query;
            this.renderSigmaGraph(this.graphRenderingData);
        } else {
            this.queryRequest.knowledge_graph_query = null;
        }
    }
    },

  methods: {
    async getKnowledgeSources() {
        this.loadingStatusText = 'Retrieving knowledge sources...';
        this.loading = true;
        try {
            this.knowledgeSources = await api.getKnowledgeSources();
        } catch (error) {
            this.$toast?.add?.({
            severity: 'error',
            detail: error?.response?._data || error,
            life: 5000,
            });
        }
        this.loading = false;
    },

    async queryKnowledgeSource() {
        // Validation: ensure a knowledge source is selected and user prompt is not empty
        if (!this.selectedKnowledgeSource) {
            this.$toast?.add?.({
            severity: 'warn',
            detail: 'Please select a knowledge source.',
            life: 3000,
            });
            return;
        }
        if (!this.queryRequest.user_prompt || !this.queryRequest.user_prompt.trim()) {
            this.$toast?.add?.({
            severity: 'warn',
            detail: 'Please enter a user prompt.',
            life: 3000,
            });
            return;
        }
        // ...existing logic to query the knowledge source...
        this.loadingStatusText = `Querying knowledge source: ${this.selectedKnowledgeSource.resource.name}...`;
        this.loading = true;
        try {
            this.queryResponse = await api.queryKnowledgeSource(
                this.selectedKnowledgeSource.resource.name,
                this.queryRequest
            );
        } catch (error) {
            this.$toast?.add?.({
            severity: 'error',
            detail: error?.response?._data || error,
            life: 5000,
            });
        }
        this.loading = false;
    },

    renderSigmaGraph(data: any) {
        // Destroy previous instance if exists
        if (this.sigmaInstance) {
            this.sigmaInstance.kill();
            this.sigmaInstance = null;
        }
        // Example: expects data to have nodes and edges arrays
        if (!data || !Array.isArray(data.nodes) || !Array.isArray(data.edges)) return;

        this.$nextTick(() => {
            const container = this.$refs.sigmaContainer as HTMLElement;
            if (!container) return; // Defensive: skip if not rendered

            const graph = new Graph();
            // Add nodes
            data.nodes.forEach((node: any) => {
            graph.addNode(node.id, { label: node.label, x: Math.random(), y: Math.random(), size: 10 });
            });
            // Add edges
            data.edges.forEach((edge: any) => {
            graph.addEdge(edge.source, edge.target);
            });

            this.sigmaInstance = new Sigma(graph, container);
        });
    }
  },
};
</script>

<style lang="scss" scoped>

.grid--loading {
  pointer-events: none;
}

.grid__loading-overlay {
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

.query-grid-label {
  margin-bottom: 0.8rem;
}

.query-grid {
  display: grid;
  grid-template-columns: 3fr 1fr 1fr;
  grid-template-rows: repeat(6, 60px);
  gap: 1rem;
  margin-top: 1.5rem;
  margin-bottom: 1rem;
  justify-content: start; /* Align grid to the left if not filling container */
}

.query-grid > div > div {
  padding: 0.5rem;
  height: 100%; /* Make each cell fill the row height */
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  justify-content: center; /* Optional: center content vertically */
}

</style>
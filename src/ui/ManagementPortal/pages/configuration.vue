<template>
  <main id="main-content" class="page-container">
    <h2 class="page-header">Configuration</h2>
    <div class="page-subheader">
        <p>Manage configuration settings.</p>
    </div>

    <!-- Status Message Section -->
    <section class="status-message-section mt-6">
      <h3 class="text-lg font-semibold mb-2">Status Message</h3>
      <p class="text-sm text-gray-500 mb-4">
        Configure a status message to display to users in the User Portal.
      </p>
      <div v-if="loadingStatusMessage" class="flex items-center gap-2">
        <i class="pi pi-spinner pi-spin"></i>
        <span>Loading...</span>
      </div>
      <template v-else>
        <CustomQuillEditor
          id="statusMessage"
          aria-labelledby="statusMessage"
          :initial-content="JSON.parse(JSON.stringify(statusMessage))"
          :key="editorKey"
          @content-update="updateStatusMessage($event)"
        />
        <div class="mt-3 flex gap-2">
          <Button
            label="Save"
            icon="pi pi-save"
            :loading="savingStatusMessage"
            :disabled="!statusMessageChanged"
            @click="showSaveConfirmation = true"
          />
          <Button
            label="Clear"
            icon="pi pi-times"
            severity="secondary"
            :loading="clearingStatusMessage"
            :disabled="!originalStatusMessage"
            @click="showClearConfirmation = true"
          />
        </div>
      </template>
    </section>

    <!-- Self-Service Agent AI Models Section -->
    <section class="config-section mt-6">
      <h3 class="text-lg font-semibold mb-2">Self-Service Agent AI Models</h3>
      <p class="text-sm text-gray-500 mb-4">
        Select which AI models (marked as completion ones) are available for self-service agent creation in the User Portal.
      </p>
      <div v-if="loadingAIModels" class="flex items-center gap-2">
        <i class="pi pi-spinner pi-spin"></i>
        <span>Loading AI models...</span>
      </div>
      <template v-else>
        <MultiSelect
          v-model="selectedAIModels"
          :options="completionAIModels"
          optionLabel="display_name"
          optionValue="name"
          placeholder="Select AI Models"
          display="chip"
          class="w-full md:w-1/2"
          :filter="true"
          filterPlaceholder="Search AI models..."
        />
        <div class="mt-3 flex gap-2">
          <Button
            label="Save"
            icon="pi pi-save"
            :loading="savingAIModels"
            :disabled="!aiModelsChanged"
            @click="showSaveAIModelsConfirmation = true"
          />
        </div>
      </template>
    </section>

    <!-- Save Status Message Confirmation Dialog -->
    <ConfirmationDialog
      :visible="showSaveConfirmation"
      header="Save Status Message"
      confirmText="Save"
      cancelText="Cancel"
      @cancel="showSaveConfirmation = false"
      @confirm="confirmSaveStatusMessage"
    >
      <div>
        Are you sure you want to save the status message? This will be displayed to all users in the User Portal.
      </div>
    </ConfirmationDialog>

    <!-- Clear Status Message Confirmation Dialog -->
    <ConfirmationDialog
      :visible="showClearConfirmation"
      header="Clear Status Message"
      confirmText="Clear"
      cancelText="Cancel"
      confirm-button-severity="danger"
      @cancel="showClearConfirmation = false"
      @confirm="confirmClearStatusMessage"
    >
      <div>
        Are you sure you want to clear the status message? This will remove the message from the User Portal.
      </div>
    </ConfirmationDialog>

    <!-- Save AI Models Confirmation Dialog -->
    <ConfirmationDialog
      :visible="showSaveAIModelsConfirmation"
      header="Save Self-Service Agent AI Models"
      confirmText="Save"
      cancelText="Cancel"
      @cancel="showSaveAIModelsConfirmation = false"
      @confirm="confirmSaveAIModels"
    >
      <div>
        Are you sure you want to save the self-service agent AI models configuration? This will update which AI models are available for self-service agent creation.
      </div>
    </ConfirmationDialog>
  </main>
</template>

<script lang="ts">
import api from '@/js/api';
import type { AIModel, ResourceProviderGetResult } from '@/js/types';

export default {
  data() {
    return {
      // Status Message state
      statusMessage: '' as string,
      originalStatusMessage: '' as string,
      loadingStatusMessage: true,
      savingStatusMessage: false,
      clearingStatusMessage: false,
      editorKey: 0,
      showSaveConfirmation: false,
      showClearConfirmation: false,
      // Self-Service Agent AI Models state
      allAIModels: [] as ResourceProviderGetResult<AIModel>[],
      selectedAIModels: [] as string[],
      originalSelectedAIModels: [] as string[],
      loadingAIModels: true,
      savingAIModels: false,
      showSaveAIModelsConfirmation: false,
    };
  },

  computed: {
    statusMessageChanged(): boolean {
      return this.statusMessage !== this.originalStatusMessage;
    },
    completionAIModels(): AIModel[] {
      // Filter AI models to only show those with type 'completion'
      // Ensure display_name falls back to name, sort alphabetically by display_name
      return this.allAIModels
        .filter((model) => model.resource.type === 'completion')
        .map((model) => ({
          ...model.resource,
          display_name: model.resource.display_name || model.resource.name,
        }))
        .sort((a, b) => a.display_name.localeCompare(b.display_name));
    },
    aiModelsChanged(): boolean {
      // Compare arrays to determine if selection has changed
      if (this.selectedAIModels.length !== this.originalSelectedAIModels.length) {
        return true;
      }
      const sortedSelected = [...this.selectedAIModels].sort();
      const sortedOriginal = [...this.originalSelectedAIModels].sort();
      return sortedSelected.some((val, idx) => val !== sortedOriginal[idx]);
    },
  },

  async created() {
    await Promise.all([
      this.loadStatusMessage(),
      this.loadAIModelsConfig(),
    ]);
  },

  methods: {
    async loadStatusMessage() {
      try {
        this.loadingStatusMessage = true;
        const result = await api.getAppConfig('FoundationaLLM:Instance:StatusMessage');
        this.statusMessage = result?.resource?.value || '';
        this.originalStatusMessage = this.statusMessage;
      } catch (error: any) {
        // If the config doesn't exist yet, just start with empty string
        if (error?.statusCode !== 404) {
          this.$toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load status message',
            life: 5000,
          });
        }
        this.statusMessage = '';
        this.originalStatusMessage = '';
      } finally {
        this.loadingStatusMessage = false;
      }
    },

    updateStatusMessage(newContent: string) {
      this.statusMessage = newContent;
    },

    async confirmSaveStatusMessage() {
      this.showSaveConfirmation = false;
      await this.saveStatusMessage();
    },

    async confirmClearStatusMessage() {
      this.showClearConfirmation = false;
      await this.clearStatusMessage();
    },

    async saveStatusMessage() {
      try {
        this.savingStatusMessage = true;
        await api.upsertAppConfig({
          key: 'FoundationaLLM:Instance:StatusMessage',
          value: this.statusMessage,
          name: 'StatusMessage',
          display_name: 'Status Message',
          description: 'A status message to display to users in the User Portal.',
          content_type: 'text/html',
        });
        this.originalStatusMessage = this.statusMessage;
        this.$toast.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Status message saved successfully',
          life: 3000,
        });
      } catch (error) {
        this.$toast.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to save status message',
          life: 5000,
        });
      } finally {
        this.savingStatusMessage = false;
      }
    },

    async clearStatusMessage() {
      try {
        this.clearingStatusMessage = true;
        await api.upsertAppConfig({
          key: 'FoundationaLLM:Instance:StatusMessage',
          value: '',
          name: 'StatusMessage',
          display_name: 'Status Message',
          description: 'A status message to display to users in the User Portal.',
          content_type: 'text/html',
        });
        this.statusMessage = '';
        this.originalStatusMessage = '';
        this.editorKey++; // Force re-render of the editor
        this.$toast.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Status message cleared successfully',
          life: 3000,
        });
      } catch (error) {
        this.$toast.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to clear status message',
          life: 5000,
        });
      } finally {
        this.clearingStatusMessage = false;
      }
    },

    async loadAIModelsConfig() {
      try {
        this.loadingAIModels = true;
        // Load all AI models
        this.allAIModels = await api.getAIModels();
        
        // Load the current configuration value
        try {
          const result = await api.getAppConfig('FoundationaLLM:Instance:SelfServiceAgentAIModels');
          const configValue = result?.resource?.value || '';
          // Parse the comma-separated list into an array
          this.selectedAIModels = configValue
            ? configValue.split(',').map((name: string) => name.trim()).filter((name: string) => name)
            : [];
          this.originalSelectedAIModels = [...this.selectedAIModels];
        } catch (configError: any) {
          // If the config doesn't exist yet, just start with empty selection
          if (configError?.statusCode !== 404) {
            console.warn('Failed to load self-service agent AI models config:', configError);
          }
          this.selectedAIModels = [];
          this.originalSelectedAIModels = [];
        }
      } catch (error: any) {
        this.$toast.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load AI models',
          life: 5000,
        });
        this.selectedAIModels = [];
        this.originalSelectedAIModels = [];
      } finally {
        this.loadingAIModels = false;
      }
    },

    async confirmSaveAIModels() {
      this.showSaveAIModelsConfirmation = false;
      await this.saveAIModelsConfig();
    },

    async saveAIModelsConfig() {
      try {
        this.savingAIModels = true;
        // Convert the array to a comma-separated string
        const configValue = this.selectedAIModels.join(',');
        await api.upsertAppConfig({
          key: 'FoundationaLLM:Instance:SelfServiceAgentAIModels',
          value: configValue,
          name: 'SelfServiceAgentAIModels',
          display_name: 'Self-Service Agent AI Models',
          description: 'Comma-separated list of AI model names available for self-service agent creation.',
          content_type: 'text/plain',
        });
        this.originalSelectedAIModels = [...this.selectedAIModels];
        this.$toast.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Self-service agent AI models configuration saved successfully',
          life: 3000,
        });
      } catch (error) {
        this.$toast.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to save self-service agent AI models configuration',
          life: 5000,
        });
      } finally {
        this.savingAIModels = false;
      }
    },
  },
};
</script>

<style scoped>
.status-message-section,
.config-section {
  margin-top: 2rem;
  padding-top: 1.5rem;
  border-top: 1px solid var(--surface-border);
}
</style>
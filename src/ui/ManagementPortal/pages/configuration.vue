<template>
  <main id="main-content" class="page-container">
    <h2 class="page-header">Configuration</h2>
    <div class="page-subheader">
        <p>Manage configuration settings and role assignments.</p>
    </div>
    <AccessControl
        description="Manage access to User Portal and Management Portal"
        :scopes="[
            {
                label: 'User Portal access',
                labelOverride: 'Enable User Portal access',
                allowedRoleDefinitionNames: ['Reader'],
                value: `providers/FoundationaLLM.Configuration/appConfigurationSets/UserPortal`
            },
            {
                label: 'Management Portal access',
                labelOverride: 'Enable Management Portal access',
                allowedRoleDefinitionNames: ['Reader'],
                value: `providers/FoundationaLLM.Configuration/appConfigurationSets/ManagementPortal`
            }
        ]"
    />

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

    <!-- Save Confirmation Dialog -->
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

    <!-- Clear Confirmation Dialog -->
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
  </main>
</template>

<script lang="ts">
import api from '@/js/api';

export default {
  data() {
    return {
      statusMessage: '' as string,
      originalStatusMessage: '' as string,
      loadingStatusMessage: true,
      savingStatusMessage: false,
      clearingStatusMessage: false,
      editorKey: 0,
      showSaveConfirmation: false,
      showClearConfirmation: false,
    };
  },

  computed: {
    statusMessageChanged(): boolean {
      return this.statusMessage !== this.originalStatusMessage;
    },
  },

  async created() {
    await this.loadStatusMessage();
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
  },
};
</script>

<style scoped>
.status-message-section {
  margin-top: 2rem;
  padding-top: 1.5rem;
  border-top: 1px solid var(--surface-border);
}
</style>
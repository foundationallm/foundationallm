<template>
  <main id="main-content" class="page-container">
    <h2 class="page-header">Configuration</h2>
    <div class="page-subheader">
        <p>Manage configuration settings and role assignments.</p>
    </div>
    <AccessControl
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
  </main>
</template>

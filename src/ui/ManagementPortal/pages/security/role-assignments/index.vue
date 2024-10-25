<template>
	<div>
		<div style="display: flex">
			<div style="flex: 1">
				<h2 class="page-header">Role Assignments</h2>
				<div class="page-subheader">The following role assignments are available.</div>
			</div>

			<div style="display: flex; align-items: center">
				<NuxtLink to="/security/role-assignments/create">
					<Button>
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Create Role Assignment
					</Button>
				</NuxtLink>
			</div>
		</div>

		<RoleAssignmentsTable />
	</div>
</template>

<script lang="ts">
export default {
	name: 'RoleAssignments',
};
</script>

<style lang="scss"></style>

<template>
	<main id="main-content">
		<div class="flex">
			<div class="flex-1">
				<h2 class="page-header">Role Assignments</h2>
				<div class="page-subheader">The following role assignments are available.</div>
			</div>

			<div class="flex items-center">
				<NuxtLink to="/security/role-assignments/create" tabindex="-1">
					<Button aria-label="Create Role Assignment">
						<i
							class="pi pi-plus mr-2"
							style="color: var(--text-primary)"
							aria-hidden="true"
						></i>
						Create Role Assignment
					</Button>
				</NuxtLink>
			</div>
		</div>

		<RoleAssignmentsTable />
	</main>
</template>

<script lang="ts">
export default {
	name: 'RoleAssignments',
};
</script>

<style lang="scss" scoped></style>

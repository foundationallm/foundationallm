<template>
	<div>
		<h2 class="page-header">Welcome to the Management Portal</h2>
		<div class="page-subheader">Browse the available configuration options in the sidebar.</div>

		<!-- <div class="navigation-boxes">
			<NavigationBox
				header="API Status"
				body="Check the status of the various APIs in the system"
				icon="pi pi-server"
				link="/info"
			/>
		</div> -->
	</div>
</template>

<script lang="ts">
import NavigationBox from '@/components/NavigationBox.vue';

export default {
	name: 'ManagementPortal',
	components: {
		NavigationBox,
	},
};
</script>

<style lang="scss">
.navigation-boxes {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-around;
}
</style>

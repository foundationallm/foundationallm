<template>
	<div>
		<Toast position="top-center" />
		<div class="navbar navbar--collapsed">
			<!-- Sidebar header -->
			<div class="navbar__header type-2">
				<UserAvatar size="large" class="chat-sidebar__avatar" />

				<img v-if="$appConfigStore.logoUrl !== ''" :src="$appConfigStore.logoUrl"
					:alt="$appConfigStore.logoText" />
				<span v-else>{{ $appConfigStore.logoText }}</span>
			</div>

			<!-- Navbar content -->
			<div class="navbar__content"></div>
		</div>
	</div>
</template>

<script lang="ts">
	import '@/styles/navbar.scss';

	export default {
		name: 'NavBarSettings',
	}
</script>

<style lang="scss" scoped>
	.chat-sidebar__avatar {
		margin-right: 12px;
		height: 55px;
		width: 55px;
		border-radius: 50%;
		background: transparent;
		overflow: hidden;
		display: flex;
		align-items: center;
		justify-content: center;
	}

	.chat-sidebar__avatar img,
	.chat-sidebar__avatar picture,
	.chat-sidebar__avatar svg {
		width: 100%;
		height: 100%;
		object-fit: cover;
		border-radius: 10px;
		display: block;
	}

	@media only screen and (max-width: 620px) {
		.navbar__header {
			width: 280px;
		}
	}
</style>
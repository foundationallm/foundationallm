<template>
	<div>
		<Toast position="top-center" />
		<div class="navbar navbar--collapsed">
			<!-- Sidebar header -->
			<div class="navbar__header">
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
	export default {
		name: 'NavBarSettings',
	}
</script>

<style lang="scss" scoped>
	.navbar {
		height: 70px;
		width: 100%;
		overflow: hidden;
		display: flex;
		flex-direction: row;
		box-shadow: 0 5px 10px 0 rgba(27, 29, 33, 0.1);
	}

	.navbar--collapsed {
		.navbar__content {
			background-color: var(--primary-color);
			justify-content: flex-end;
			border-bottom: none;
		}
	}

	.navbar__header {
		width: 300px;
		padding-right: 24px;
		padding-left: 24px;
		padding-top: 12px;
		padding-bottom: 12px;
		display: flex;
		align-items: center;
		justify-content: space-between;
		color: var(--primary-text);
		background-color: var(--primary-color);

		img {
			max-height: 100%;
			width: auto;
			max-width: 148px;
			margin-right: 12px;
		}
	}

	.navbar__content {
		flex: 1;
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 24px;
		border-bottom: 1px solid #eaeaea;
		color: var(--accent-text);
		background-color: var(--accent-color);
	}

	@media only screen and (max-width: 620px) {
		.navbar__header {
			width: 280px;
		}
	}
</style>

<style lang="scss" scoped>
	.chat-sidebar__avatar {
		margin-right: 12px;
		height: 55px;
		width: 55px;
		border-radius: 10px;
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
</style>

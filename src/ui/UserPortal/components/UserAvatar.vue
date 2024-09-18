<template>
	<!-- <Avatar v-if="loading" icon="pi pi-spinner" /> -->
	<Avatar v-if="profilePhotoSrc" :image="profilePhotoSrc" />
	<Avatar v-else :label="userInitials" :style="`background-color: ${userColors.background}; color: ${userColors.text};`" />
</template>

<script lang="ts">
const paleColors = [
	'#FFFFCC', // Pale Yellow
	'#FFCCCC', // Pale Red
	'#FFCCFF', // Pale Pink
	'#CCFFFF', // Pale Cyan
	'#CCFFCC', // Pale Green
	'#CCCCFF', // Pale Lavender
	'#FFCC99', // Pale Peach
	'#FF9966', // Pale Orange
	'#FF9999', // Pale Salmon
	'#99CCFF'  // Pale Sky Blue
];

// const darkPaleColors = [
//   '#CCCC99', // Dark Yellow
//   '#CC9999', // Dark Red
//   '#CC99CC', // Dark Pink
//   '#99CCCC', // Dark Cyan
//   '#99CC99', // Dark Green
//   '#9999CC', // Dark Lavender
//   '#CC9966', // Dark Peach
//   '#CC6633', // Dark Orange
//   '#CC6666', // Dark Salmon
//   '#6699CC'  // Dark Sky Blue
// ];

const darkerPaleColors = [
	'#999966', // Darker Pale Yellow
	'#993333', // Darker Pale Red
	'#993399', // Darker Pale Pink
	'#339999', // Darker Pale Cyan
	'#339933', // Darker Pale Green
	'#333399', // Darker Pale Lavender
	'#993300', // Darker Pale Peach
	'#663300', // Darker Pale Orange
	'#993333', // Darker Pale Salmon
	'#336699'  // Darker Pale Sky Blue
];

function hashString(str) {
	let hash = 0;
	for (let i = 0; i < str.length; i++) {
		hash = str.charCodeAt(i) + ((hash << 5) - hash);
	}
	return hash;
}

function getColorsFromName(name) {
	const hash = hashString(name);
	const index = Math.abs(hash) % paleColors.length;
	return {
		base: paleColors[index],
		text: darkerPaleColors[index],
	};
}

function getInitials(text) {
	const words = text.split(/\s+/);
	return (words[0][0] + (words[words.length - 1][0] || '')).toUpperCase();
}

export default {
	data() {
		return {
			// loading: false,
			profilePhotoSrc: null,
		};
	},

	computed: {
		userInitials() {
			return getInitials(this.$authStore.currentAccount?.name);
		},

		userColors() {
			const colors = getColorsFromName(this.$authStore.currentAccount?.name);
			return {
				text: colors.text,
				background: colors.base,
			};
		},
	},

	async created() {
		// this.loading = true;
		try {
			this.profilePhotoSrc = await this.$authStore.getProfilePhoto();
		} catch(error) {
			// do something
		}
		// this.loading = false;
	},
};
</script>
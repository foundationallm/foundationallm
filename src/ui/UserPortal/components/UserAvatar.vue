<template>
	<!-- <Avatar v-if="loading" icon="pi pi-spinner" v-bind="$attrs" /> -->
	<Avatar v-if="profilePhotoSrc" :image="profilePhotoSrc" v-bind="$attrs" />
	<Avatar
		v-else
		:label="userInitials"
		:style="`background-color: ${userColors.background}; color: ${userColors.text};`"
		v-bind="$attrs"
	/>
</template>

<script lang="ts">
import md5 from 'md5';

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
	'#99CCFF', // Pale Sky Blue
];

// const paleColorsDarken1 = [
//   '#CCCC99', // Dark Pale Yellow
//   '#CC9999', // Dark Pale Red
//   '#CC99CC', // Dark Pale Pink
//   '#99CCCC', // Dark Pale Cyan
//   '#99CC99', // Dark Pale Green
//   '#9999CC', // Dark Pale Lavender
//   '#CC9966', // Dark Pale Peach
//   '#CC6633', // Dark Pale Orange
//   '#CC6666', // Dark Pale Salmon
//   '#6699CC'  // Dark Pale Sky Blue
// ];

// const paleColorsDarken2 = [
// 	'#999966', // Darker Pale Yellow
// 	'#993333', // Darker Pale Red
// 	'#993399', // Darker Pale Pink
// 	'#339999', // Darker Pale Cyan
// 	'#339933', // Darker Pale Green
// 	'#333399', // Darker Pale Lavender
// 	'#993300', // Darker Pale Peach
// 	'#663300', // Darker Pale Orange
// 	'#993333', // Darker Pale Salmon
// 	'#336699'  // Darker Pale Sky Blue
// ];

const paleColorsDarken3 = [
	'#666633', // Much Darker Pale Yellow
	'#993333', // Much Darker Pale Red
	'#993399', // Much Darker Pale Pink
	'#336666', // Much Darker Pale Cyan
	'#336633', // Much Darker Pale Green
	'#333366', // Much Darker Pale Lavender
	'#993300', // Much Darker Pale Peach
	'#663300', // Much Darker Pale Orange
	'#993333', // Much Darker Pale Salmon
	'#336699', // Much Darker Pale Sky Blue
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
		text: paleColorsDarken3[index],
	};
}

function getInitials(text) {
	const words = text.split(/\s+/);
	return (words[0][0] + (words[words.length - 1][0] || '')).toUpperCase();
}

export default {
	inheritAttrs: false,

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
			if (!this.profilePhotoSrc) {
				await this.loadGravatarImage();
			}
		} catch (error) {
			// console.error(error);
		}
		// this.loading = false;
	},

	methods: {
		loadImage(src) {
			return new Promise((resolve, reject) => {
				const image = new Image();
				image.onload = () => resolve(src);
				image.onerror = () => reject(new Error(`Failed to load image: ${src}`));
				image.src = src;
			});
		},

		async loadGravatarImage() {
			const emailHash = md5(this.$authStore.currentAccount?.username?.toLowerCase()); // Hash the email
			const gravatarUrl = `https://www.gravatar.com/avatar/${emailHash}?d=404`;
			const gravatarImage = await this.loadImage(gravatarUrl);
			this.profilePhotoSrc = gravatarImage;
		},
	},
};
</script>


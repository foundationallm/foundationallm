<template>
	<main>
		<Head>
			<Title>{{ pageTitle }}</Title>
			<Meta name="description" :content="pageTitle" />
		</Head>

		<NuxtPage />
	</main>
</template>

<script lang="ts">
export default {
	data() {
		return {
			pageTitle: 'FoundationaLLM',
		};
	},
};
</script>

<style lang="scss">
html,
body,
#__nuxt,
#__layout,
main {
	height: 100%;
	margin: 0;
	font-family: 'Poppins', sans-serif;
}

.p-component {
	border-radius: 0px;
}
</style>

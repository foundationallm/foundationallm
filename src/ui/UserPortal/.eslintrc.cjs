module.exports = {
	root: true,
	env: {
		browser: true,
		node: true,
	},
	parser: 'vue-eslint-parser',
	parserOptions: {
		parser: '@typescript-eslint/parser',
		warnOnUnsupportedTypeScriptVersion: false,
	},
	extends: ['@nuxtjs/eslint-config-typescript', 'plugin:prettier/recommended'],
	plugins: [],
	rules: {
		'prettier/prettier': ['error'],
		'vue/html-indent': ['error', 'tab'],
		'vue/script-indent': [
			'error',
			'tab',
			{
				switchCase: 1,
			},
		],
		'vue/singleline-html-element-content-newline': 0,
		'vue/component-name-in-template-casing': ['error', 'PascalCase'],
		'no-console': 'off',
		'vue/no-multiple-template-root': 'off',
		'vue/multi-word-component-names': 'off',
	},
};

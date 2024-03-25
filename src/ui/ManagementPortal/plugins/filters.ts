const filters = {
	/**
	 * Appends the base URL to the given path if it is not an absolute URL.
	 * 
	 * @param path - The path to be processed.
	 * @returns The processed path with the base URL appended if necessary.
	 */
	publicDirectory(path: string) {
		const config = useRuntimeConfig();
		// Only append the base URL if the path is not an absolute URL.
		if (path.startsWith('http')) {
			return path;
		}
		return config.app.baseURL + path;
	},
	
	/**
	 * Sanitizes the input value by removing whitespace and special characters.
	 * 
	 * @param event - The event object containing the input value.
	 * @returns The sanitized value.
	 */
	sanitizeNameInput(event: any) {
		// Remove spaces and any characters that are not letters, digits, dashes, or underscores.
		let sanitizedValue = event.target.value.replace(/\s/g, '').replace(/[^a-zA-Z0-9-_]/g, '');

		// Ensure the first character is a letter.
		while (sanitizedValue.length > 0 && !/^[a-zA-Z]/.test(sanitizedValue.charAt(0))) {
			sanitizedValue = sanitizedValue.substring(1);
		}
		event.target.value = sanitizedValue;
		return sanitizedValue;
	  }
};

export default defineNuxtPlugin((nuxtApp) => {
	nuxtApp.vueApp.config.globalProperties.$filters = filters;
});

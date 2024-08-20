import { defineNuxtPlugin } from '#app';
import { getClass, getClassWithColor } from 'file-icons-js';
import 'file-icons-js/css/style.css';

export default defineNuxtPlugin(() => {
	const getFileIconClass = (fileName: string, useColorVersion: boolean = false) => {
		var iconClass;
		if (useColorVersion) {
			iconClass = getClassWithColor(fileName.toLowerCase());
		} else {
			iconClass = getClass(fileName.toLowerCase());
		}
		return iconClass || 'pi pi-file'; // Use default icon if no class found
	};

	return {
		provide: {
			getFileIconClass,
		},
	};
});

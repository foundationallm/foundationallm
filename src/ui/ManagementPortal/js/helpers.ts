
export function renameObjectKey(object: Record<string, any>, oldKeyName: string, newKeyName: string): Record<string, any> {
	// Convert the object to an array of key value pairs
	const objectKeyValuePairs = Object.entries(object);

	// Find and rename the key
	const updatedKeyValuePairs = objectKeyValuePairs.map(([key, value]) => {
		if (key === oldKeyName) {
			return [newKeyName, value];
		}

		return [key, value];
	});

	// Convert the array back to an object
	return Object.fromEntries(updatedKeyValuePairs);
};

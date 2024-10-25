import { defineEventHandler } from 'h3';
import fetch from 'node-fetch';

export default defineEventHandler(async (event) => {
	const query = getQuery(event);
	const url = query.url;

	if (!url) {
		setResponseStatus(event, 400, 'The query parameter "url" is required.');
		return { error: 'The query parameter "url" is required.' };
	}

	try {
		const response = await fetch(url);
		if (!response.ok) {
			setResponseStatus(event, response.status, response.statusText);
			return { error: response.statusText };
		}
		const data = await response.json();
		return data;
	} catch (error) {
		console.error('Error fetching API status:', error);
		setResponseStatus(event, 500, 'Error fetching API status');
		return { error: 'Error fetching API status' };
	}
});

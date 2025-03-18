import type {
	ResourceProviderGetResult,
	Agent,
} from '@/js/types';

export const isAgentExpired = (agent: ResourceProviderGetResult<Agent>): boolean  => {
	return agent.resource.expiration_date !== null && new Date() > new Date(agent.resource.expiration_date);
}

export const modifyHexAlpha = (hex: string, alpha: number) => {
	// Ensure valid hex input
	hex = hex.replace(/^#/, '');
	if (![3, 6, 8].includes(hex.length)) {
		throw new Error("Invalid hex color");
	}

	// Expand short hex format (e.g., "abc" -> "aabbcc")
	if (hex.length === 3) {
		hex = hex.split('').map(c => c + c).join('');
	}

	// Extract RGB components
	let r = parseInt(hex.slice(0, 2), 16);
	let g = parseInt(hex.slice(2, 4), 16);
	let b = parseInt(hex.slice(4, 6), 16);
	let a = hex.length === 8 ? parseInt(hex.slice(6, 8), 16) / 255 : 1;

	// Adjust alpha, ensuring it's between 0 and 1
	a = Math.min(1, Math.max(0, alpha));

	// Convert to RGBA format
	return `rgba(${r}, ${g}, ${b}, ${a.toFixed(2)})`;
}

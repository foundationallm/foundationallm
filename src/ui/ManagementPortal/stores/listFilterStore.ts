import { defineStore } from 'pinia';

interface FilterState {
	filters: Record<string, string | null>;
}

export const useListFilterStore = defineStore('listFilter', {
	state: (): FilterState => ({
		filters: {},
	}),

	actions: {
		setFilter(listName: string, value: string | null) {
			this.filters[listName] = value;
		},

		getFilter(listName: string): string | null {
			return this.filters[listName] || null;
		},

		clearFilter(listName: string) {
			delete this.filters[listName];
		},
	},
});

import PrimeVue from 'primevue/config';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import Dialog from 'primevue/dialog';
import Toast from 'primevue/toast';
import Card from 'primevue/card';
import Chip from 'primevue/chip';
import Chips from 'primevue/chips';
import ToastService from 'primevue/toastservice';
import Tooltip from 'primevue/tooltip';
import Divider from 'primevue/divider';
import Dropdown from 'primevue/dropdown';
import Avatar from 'primevue/avatar';
import RadioButton from 'primevue/radiobutton';
import ToggleButton from 'primevue/togglebutton';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';

import { defineNuxtPlugin } from '#app';

export default defineNuxtPlugin((nuxtApp) => {
	nuxtApp.vueApp.use(PrimeVue, { ripple: true });
	nuxtApp.vueApp.component('Button', Button);
	nuxtApp.vueApp.component('InputText', InputText);
	nuxtApp.vueApp.component('Textarea', Textarea);
	nuxtApp.vueApp.component('Dialog', Dialog);
	nuxtApp.vueApp.component('Toast', Toast);
	nuxtApp.vueApp.component('Card', Card);
	nuxtApp.vueApp.component('Chip', Chip);
	nuxtApp.vueApp.component('Chips', Chips);
	nuxtApp.vueApp.component('Divider', Divider);
	nuxtApp.vueApp.component('Dropdown', Dropdown);
	nuxtApp.vueApp.component('Avatar', Avatar);
	nuxtApp.vueApp.component('RadioButton', RadioButton);
	nuxtApp.vueApp.component('ToggleButton', ToggleButton);
	nuxtApp.vueApp.component('DataTable', DataTable);
	nuxtApp.vueApp.component('Column', Column);

	nuxtApp.vueApp.use(ToastService);
	nuxtApp.vueApp.directive('tooltip', Tooltip);
});

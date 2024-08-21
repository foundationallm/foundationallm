import PrimeVue from 'primevue/config';
import Button from 'primevue/button';
import ContextMenu from 'primevue/contextmenu';
import InputText from 'primevue/inputtext';
import Dialog from 'primevue/dialog';
import Toast from 'primevue/toast';
import Chip from 'primevue/chip';
import Textarea from 'primevue/textarea';
import ToastService from 'primevue/toastservice';
import Divider from 'primevue/divider';
import Dropdown from 'primevue/dropdown';
import Avatar from 'primevue/avatar';
import FileUpload from 'primevue/fileupload';
import OverlayPanel from 'primevue/overlaypanel';
import Badge from 'primevue/badge';
import BadgeDirective from 'primevue/badgedirective';
import ConfirmDialog from 'primevue/confirmdialog';
import ConfirmationService from 'primevue/confirmationservice';
import ProgressBar from 'primevue/progressbar';
import Image from 'primevue/image';
import FocusTrap from 'primevue/focustrap';

import { defineNuxtPlugin } from '#app';

export default defineNuxtPlugin((nuxtApp) => {
	nuxtApp.vueApp.use(PrimeVue, { ripple: true });
	nuxtApp.vueApp.component('Button', Button);
	nuxtApp.vueApp.component('ContextMenu', ContextMenu);
	nuxtApp.vueApp.component('InputText', InputText);
	nuxtApp.vueApp.component('Dialog', Dialog);
	nuxtApp.vueApp.component('Textarea', Textarea);
	nuxtApp.vueApp.component('Toast', Toast);
	nuxtApp.vueApp.component('Chip', Chip);
	nuxtApp.vueApp.component('Divider', Divider);
	nuxtApp.vueApp.component('Dropdown', Dropdown);
	nuxtApp.vueApp.component('Avatar', Avatar);
	nuxtApp.vueApp.component('FileUpload', FileUpload);
	nuxtApp.vueApp.component('OverlayPanel', OverlayPanel);
	nuxtApp.vueApp.component('Badge', Badge);
	nuxtApp.vueApp.directive('badge', BadgeDirective);
	nuxtApp.vueApp.component('ConfirmDialog', ConfirmDialog);
	nuxtApp.vueApp.component('ProgressBar', ProgressBar);
	nuxtApp.vueApp.component('Image', Image);
	nuxtApp.vueApp.directive('focustrap', FocusTrap);

	nuxtApp.vueApp.use(ConfirmationService);

	nuxtApp.vueApp.use(ToastService);
});

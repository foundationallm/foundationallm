import { defineNuxtPlugin } from '#app';
import createChipOnBlur from '@/directives/createChipOnBlur';

export default defineNuxtPlugin((nuxtApp) => {
    nuxtApp.vueApp.directive('create-chip-on-blur', createChipOnBlur);
  });

import { nextTick } from 'vue';

const createChipOnBlur = {
  mounted(el, binding) {
    nextTick(() => {
      const input = el.querySelector('input');
      if (input) {
        input.addEventListener('blur', () => {
          const value = input.value.trim();

          if (value) {
            const propertyName = binding.arg; // Use the argument passed to the directive
            if (propertyName) {
              const targetArray = binding.instance[propertyName];
              if (Array.isArray(targetArray)) {
                targetArray.push(value);
                input.value = '';
              } else {
                console.warn(`The property "${propertyName}" is not an array or is undefined`);
              }
            } else {
              console.warn('No argument provided for the directive');
            }
          }
        });
      } else {
        console.warn('Input element not found within Chips component');
      }
    });
  }
};

export default createChipOnBlur;

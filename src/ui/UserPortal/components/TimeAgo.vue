<template>
    <span>{{ formattedTime }}</span>
  </template>
  
  <script setup>
  import { ref, onMounted, onUnmounted, watchEffect } from 'vue';
  import TimeAgo from 'javascript-time-ago';
  
  const props = defineProps({
    date: {
      type: Date,
      required: true,
    },
    interval: {
      type: Number,
      default: 60000, // Update every 60 seconds
    },
  });
  
  const formattedTime = ref('');
  const timeAgo = new TimeAgo('en-US');
  
  const updateFormattedTime = () => {
    formattedTime.value = timeAgo.format(props.date);
  };
  
  let timer;
  onMounted(() => {
    updateFormattedTime();
    timer = setInterval(updateFormattedTime, props.interval);
  });
  
  onUnmounted(() => {
    clearInterval(timer);
  });
  
  // Recompute if the date prop changes
  watchEffect(() => {
    updateFormattedTime();
  });
  </script>
  
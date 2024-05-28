<template>
    <div class="navigation-box" @click="navigate">
      <div class="icon-container">
        <i :class="icon"></i>
      </div>
      <div class="text-container">
        <h2>{{ header }}</h2>
        <p>{{ body }}</p>
      </div>
    </div>
  </template>
  
  <script>
  export default {
    props: {
      header: {
        type: String,
        required: true,
      },
      body: {
        type: String,
        required: true,
      },
      icon: {
        type: String,
        required: true,
      },
      link: {
        type: String,
        required: true,
      },
    },
    methods: {
      navigate() {
        this.$router.push(this.link);
      },
    },
  };
  </script>
  
  <style scoped lang="scss">
  .navigation-box {
    display: flex;
    align-items: center;
    padding: 1.5em;
    border: 1px solid #ddd;
    border-radius: 8px;
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    cursor: pointer;
    transition: transform 0.3s ease, box-shadow 0.3s ease;
    background-color: var(--primary-color);
  }
  
  .navigation-box:hover {
    transform: translateY(-5px);
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
  }
  
  .icon-container {
    font-size: 2em;
    margin-right: 1em;
    color: var(--primary-text);

    .pi {
      font-size: 1.3em;
    }
  }
  
  .text-container h2 {
    margin: 0;
    font-size: 1.5em;
    color: var(--primary-text);
  }
  
  .text-container p {
    margin: 0.5em 0 0;
    color: var(--primary-text);
  }
  </style>
  
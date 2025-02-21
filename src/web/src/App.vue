<template>
  <q-layout
    view="hHh lpR fFf"
    role="application"
    aria-live="polite"
    class="app-root"
  >
    <component
      :is="currentLayout"
      :key="layoutKey"
      class="layout-transition"
      v-if="!layoutError"
    />

    <!-- Error Boundary Fallback -->
    <div
      v-else
      role="alert"
      class="error-boundary"
    >
      <p>An error occurred while loading the application. Please try refreshing the page.</p>
    </div>
  </q-layout>
</template>

<script lang="ts">
import { defineComponent, computed, ref, onMounted, onErrorCaptured } from 'vue';
import { QLayout } from 'quasar'; // ^2.12.0
import { useTransition } from '@vueuse/core'; // ^10.0.0

// Internal imports
import MainLayout from './layouts/MainLayout';
import AuthLayout from './layouts/AuthLayout';
import { useAuth } from './composables/useAuth';

export default defineComponent({
  name: 'App',

  components: {
    QLayout,
    MainLayout,
    AuthLayout
  },

  setup() {
    // State
    const layoutError = ref<Error | null>(null);
    const layoutKey = ref(0);

    // Composables
    const { isAuthenticated, validateAuthState } = useAuth();
    const { transition } = useTransition();

    // Computed layout selection with error handling
    const currentLayout = computed(() => {
      try {
        return isAuthenticated.value ? MainLayout : AuthLayout;
      } catch (error) {
        console.error('Layout computation error:', error);
        layoutError.value = error as Error;
        return null;
      }
    });

    // Error boundary handler
    onErrorCaptured((error: Error, component, info) => {
      console.error('App error captured:', { error, component, info });
      layoutError.value = error;
      return false; // Prevent error propagation
    });

    // Initialize auth validation
    onMounted(async () => {
      try {
        await validateAuthState();
        // Force layout re-render on successful validation
        layoutKey.value++;
      } catch (error) {
        console.error('Auth validation error:', error);
        layoutError.value = error as Error;
      }
    });

    return {
      currentLayout,
      layoutError,
      layoutKey,
      transition
    };
  }
});
</script>

<style lang="scss" scoped>
.app-root {
  height: 100vh;
  width: 100vw;
  overflow: hidden;
}

.layout-transition {
  transition: opacity 0.3s ease-in-out;
  will-change: opacity;
}

.error-boundary {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100vh;
  padding: 2rem;
  text-align: center;
  background-color: #f8f9fa;
  color: #dc3545;
  font-size: 1.25rem;
}
</style>
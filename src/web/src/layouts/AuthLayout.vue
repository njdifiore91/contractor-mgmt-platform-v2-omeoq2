<template>
  <q-layout view="lHh Lpr lFf" class="auth-layout">
    <!-- Main page container -->
    <q-page-container class="auth-container">
      <!-- Router view for auth pages -->
      <router-view v-if="!isLoading" />

      <!-- Loading overlay -->
      <div v-if="isLoading" class="loading-overlay" role="status" aria-label="Loading">
        <q-spinner
          color="primary"
          size="3em"
          aria-hidden="true"
        />
        <span class="sr-only">Loading authentication...</span>
      </div>

      <!-- Error display -->
      <div 
        v-if="authError" 
        class="error-message" 
        role="alert"
        aria-live="polite"
      >
        {{ authError }}
      </div>
    </q-page-container>
  </q-layout>
</template>

<script lang="ts">
import { defineComponent, onMounted, onBeforeUnmount, watch } from 'vue'; // ^3.2.0
import { useRouter } from 'vue-router'; // ^4.0.0
import { QLayout, QPageContainer, QSpinner } from 'quasar'; // ^2.0.0
import { useAuth } from '../composables/useAuth';

export default defineComponent({
  name: 'AuthLayout',

  components: {
    QLayout,
    QPageContainer,
    QSpinner
  },

  setup() {
    const router = useRouter();
    const { isAuthenticated, isLoading, authError } = useAuth();

    /**
     * Checks authentication state and handles redirections
     */
    const checkAuthState = async (): Promise<void> => {
      if (isAuthenticated.value) {
        // Redirect authenticated users away from auth pages
        await router.push({ name: 'dashboard' });
      }
    };

    /**
     * Sets up watchers for auth state changes
     */
    const setupAuthWatchers = () => {
      const unwatch = watch(isAuthenticated, async (newValue) => {
        if (newValue) {
          await checkAuthState();
        }
      });

      // Cleanup watcher on component unmount
      onBeforeUnmount(() => {
        unwatch();
      });
    };

    // Initialize auth check and watchers
    onMounted(async () => {
      await checkAuthState();
      setupAuthWatchers();
    });

    return {
      isLoading,
      authError
    };
  }
});
</script>

<style lang="scss">
.auth-layout {
  min-height: 100vh;
  background: #f5f5f5;

  .auth-container {
    max-width: 480px;
    margin: 0 auto;
    padding: 2rem 1rem;

    @media (min-width: 600px) {
      padding: 4rem 2rem;
    }
  }

  .loading-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(255, 255, 255, 0.9);
    z-index: 2000;

    .sr-only {
      position: absolute;
      width: 1px;
      height: 1px;
      padding: 0;
      margin: -1px;
      overflow: hidden;
      clip: rect(0, 0, 0, 0);
      border: 0;
    }
  }

  .error-message {
    margin-top: 1rem;
    padding: 1rem;
    border-radius: 4px;
    background-color: #ffebee;
    color: #c62828;
    font-weight: 500;
    text-align: center;
  }
}
</style>
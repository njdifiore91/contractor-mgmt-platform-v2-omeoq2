<template>
  <QPage 
    class="demobilization-page q-pa-md"
    role="main"
    aria-labelledby="demob-page-title"
  >
    <!-- Loading State -->
    <div v-if="loading" class="demobilization-page__loading">
      <QSpinner
        color="primary"
        size="3em"
        aria-label="Loading inspector demobilization form"
      />
      <p class="text-subtitle1 q-mt-sm">Loading inspector details...</p>
    </div>

    <!-- Error State -->
    <div 
      v-else-if="error"
      role="alert"
      class="demobilization-page__error text-negative q-pa-md"
    >
      <p class="text-h6">{{ error.message }}</p>
      <QBtn
        label="Retry"
        color="primary"
        @click="initializeInspector"
        class="q-mt-sm"
      />
    </div>

    <!-- Content -->
    <div v-else class="demobilization-page__content">
      <h1 id="demob-page-title" class="text-h5 q-mb-lg">
        Demobilize Inspector: {{ selectedInspector?.firstName }} {{ selectedInspector?.lastName }}
      </h1>

      <DemobilizationForm
        :inspector-id="inspectorId"
        @success="handleDemobilizationSuccess"
        @error="handleError"
      />

      <!-- Unsaved Changes Dialog -->
      <QDialog v-model="showUnsavedDialog" persistent>
        <div class="bg-white q-pa-md">
          <p class="text-h6">Unsaved Changes</p>
          <p>You have unsaved changes. Would you like to save them before leaving?</p>
          <div class="row justify-end q-gutter-sm q-mt-md">
            <QBtn
              label="Discard"
              color="negative"
              flat
              @click="handleDiscardChanges"
            />
            <QBtn
              label="Save"
              color="primary"
              @click="handleSaveChanges"
            />
          </div>
        </div>
      </QDialog>
    </div>
  </QPage>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, onMounted, onUnmounted, nextTick } from 'vue';

// Vue Router imports - v4.2.0
import { useRouter, useRoute, onBeforeRouteLeave } from 'vue-router';

// Quasar imports - v2.12.0
import { QPage, useQuasar, QSpinner, QDialog } from 'quasar';

// Internal imports
import DemobilizationForm from '@/components/inspector/DemobilizationForm.vue';
import { useInspectorStore } from '@/stores/inspector';

// Component setup
const $q = useQuasar();
const router = useRouter();
const route = useRoute();
const inspectorStore = useInspectorStore();

// Component state
const inspectorId = ref<number>(parseInt(route.params.id as string));
const showUnsavedDialog = ref<boolean>(false);
const hasUnsavedChanges = ref<boolean>(false);
const formKey = ref<number>(0);

// Computed properties from store
const { selectedInspector, loading, error } = inspectorStore;

// Initialize inspector data
const initializeInspector = async () => {
  try {
    if (!inspectorId.value || isNaN(inspectorId.value)) {
      throw new Error('Invalid inspector ID');
    }

    // Load inspector details if not already loaded
    if (!selectedInspector.value || selectedInspector.value.id !== inspectorId.value) {
      await inspectorStore.loadInspectorById(inspectorId.value);
    }

    // Validate inspector state
    if (selectedInspector.value?.status !== 'MOBILIZED') {
      throw new Error('Inspector is not currently mobilized');
    }
  } catch (err) {
    handleError(err);
  }
};

// Success handler
const handleDemobilizationSuccess = async () => {
  hasUnsavedChanges.value = false;
  
  await nextTick();
  
  $q.notify({
    type: 'positive',
    message: `Successfully demobilized ${selectedInspector.value?.firstName} ${selectedInspector.value?.lastName}`,
    position: 'top',
    timeout: 3000
  });

  // Navigate back to inspector list
  router.push({
    name: 'inspectors',
    query: { demobilized: 'success' }
  });
};

// Error handler
const handleError = (error: unknown) => {
  const errorMessage = error instanceof Error ? error.message : 'An unknown error occurred';
  
  $q.notify({
    type: 'negative',
    message: errorMessage,
    position: 'top',
    timeout: 5000
  });

  // Log error for monitoring
  console.error('Demobilization error:', error);
};

// Navigation guard for unsaved changes
onBeforeRouteLeave((to, from, next) => {
  if (hasUnsavedChanges.value) {
    showUnsavedDialog.value = true;
    next(false);
  } else {
    next();
  }
});

// Handle form state changes
const handleFormChange = () => {
  hasUnsavedChanges.value = true;
};

// Handle dialog actions
const handleDiscardChanges = () => {
  hasUnsavedChanges.value = false;
  showUnsavedDialog.value = false;
  formKey.value++; // Reset form
  router.push({ name: 'inspectors' });
};

const handleSaveChanges = async () => {
  try {
    // Trigger form submission
    await nextTick();
    const form = document.querySelector('form');
    if (form) {
      form.requestSubmit();
    }
    showUnsavedDialog.value = false;
  } catch (err) {
    handleError(err);
  }
};

// Lifecycle hooks
onMounted(async () => {
  await initializeInspector();
  
  // Set up form change listener
  window.addEventListener('beforeunload', (e) => {
    if (hasUnsavedChanges.value) {
      e.preventDefault();
      e.returnValue = '';
    }
  });
});

onUnmounted(() => {
  window.removeEventListener('beforeunload', () => {});
});
</script>

<style lang="scss">
.demobilization-page {
  max-width: 1200px;
  margin: 0 auto;

  &__loading {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 300px;
  }

  &__error {
    border-radius: 4px;
    background-color: rgba(var(--q-negative-rgb), 0.1);
  }

  &__content {
    @media (max-width: 600px) {
      padding: 1rem;
    }
  }

  // Enhance accessibility
  :focus {
    outline: 2px solid var(--q-primary);
    outline-offset: 2px;
  }

  // Improve mobile responsiveness
  @media (max-width: 600px) {
    .q-page-container {
      padding: 0;
    }

    .text-h5 {
      font-size: 1.5rem;
    }
  }
}
</style>
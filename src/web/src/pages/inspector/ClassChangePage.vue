<template>
  <QCard class="class-change-page q-pa-md" role="main" aria-label="Inspector Class Change Form">
    <QCardSection>
      <!-- Page Header -->
      <h1 class="text-h5 q-mb-lg">Inspector Class Change</h1>

      <!-- Loading State -->
      <div v-if="isLoading" class="text-center q-pa-md">
        <QSpinner size="2em" color="primary" />
        <div class="text-caption q-mt-sm">Loading inspector data...</div>
      </div>

      <!-- Error State -->
      <div v-else-if="!hasPermission" class="text-negative q-pa-md">
        <div class="text-h6">Access Denied</div>
        <p>You do not have permission to perform class changes.</p>
      </div>

      <!-- No Inspector Selected -->
      <div v-else-if="!selectedInspector" class="text-warning q-pa-md">
        <div class="text-h6">No Inspector Selected</div>
        <p>Please select an inspector to proceed with class change.</p>
      </div>

      <!-- Class Change Form -->
      <ClassChangeForm
        v-else
        :inspector-id="selectedInspector.id"
        :disabled="!hasPermission || isLoading"
        @success="handleSuccess"
        @error="handleError"
        @cancel="handleCancel"
      />
    </QCardSection>
  </QCard>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, onMounted, onUnmounted, computed } from 'vue';

// Store imports
import { useInspectorStore } from '../../stores/inspector';

// Component imports
import { ClassChangeForm } from '../../components/inspector/ClassChangeForm';

// Composable imports
import { useAuth } from '../../composables/useAuth';

// UI Component imports - Quasar v2.12.0
import { QCard, QCardSection, QSpinner, useQuasar } from 'quasar';

// Router
import { useRouter } from 'vue-router';

// Initialize composables and stores
const router = useRouter();
const $q = useQuasar();
const { checkPermission } = useAuth();
const inspectorStore = useInspectorStore();
const { selectedInspector, isLoading } = inspectorStore;

// Reactive state
const hasPermission = computed(() => checkPermission('EditInspector'));
const cleanupHandlers = ref<(() => void)[]>([]);

// Lifecycle hooks
onMounted(() => {
  // Validate permissions and inspector selection
  if (!hasPermission.value) {
    $q.notify({
      type: 'negative',
      message: 'You do not have permission to perform class changes',
      position: 'top'
    });
    router.push('/inspectors');
    return;
  }

  if (!selectedInspector.value) {
    $q.notify({
      type: 'warning',
      message: 'No inspector selected',
      position: 'top'
    });
    router.push('/inspectors');
    return;
  }
});

onUnmounted(() => {
  // Clean up any registered handlers
  cleanupHandlers.value.forEach(handler => handler());
});

// Event handlers
const handleSuccess = async () => {
  try {
    $q.notify({
      type: 'positive',
      message: 'Inspector class change completed successfully',
      position: 'top'
    });

    // Navigate back to inspector details
    await router.push(`/inspectors/${selectedInspector.value?.id}`);
  } catch (error) {
    console.error('Navigation error:', error);
  }
};

const handleError = async (error: Error) => {
  console.error('Class change error:', error);
  
  $q.notify({
    type: 'negative',
    message: `Class change failed: ${error.message}`,
    position: 'top',
    timeout: 5000
  });
};

const handleCancel = async () => {
  try {
    await router.push(`/inspectors/${selectedInspector.value?.id}`);
  } catch (error) {
    console.error('Navigation error:', error);
  }
};
</script>

<style lang="scss" scoped>
.class-change-page {
  max-width: 800px;
  margin: 0 auto;
  padding: 1rem;

  @media (max-width: 600px) {
    margin: 0.5rem;
    padding: 0.5rem;
  }

  h1 {
    font-weight: 500;
    color: var(--q-primary);
    margin-bottom: 1.5rem;
  }

  .text-center {
    text-align: center;
  }

  .text-negative {
    color: var(--q-negative);
  }

  .text-warning {
    color: var(--q-warning);
  }
}
</style>
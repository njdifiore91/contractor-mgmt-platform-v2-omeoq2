<template>
  <QPage class="equipment-assignment-page q-pa-md">
    <!-- Loading state -->
    <div v-if="loading" class="full-width row justify-center items-center">
      <QSpinner color="primary" size="3em" />
    </div>

    <!-- Error state -->
    <div v-else-if="!hasPermission" class="text-center">
      <h4 class="text-negative">
        {{ $t('equipment.assignment.unauthorized') }}
      </h4>
    </div>

    <!-- Assignment form -->
    <div v-else class="row justify-center">
      <EquipmentAssignment
        :equipment="equipment"
        :inspectors="inspectors"
        @assigned="handleAssignment"
        @assignmentError="handleAssignmentError"
      />
    </div>
  </QPage>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, onMounted, computed } from 'vue';

// Quasar imports - v2.12.0
import { QPage, QSpinner } from 'quasar';
import { useQuasar } from 'quasar';

// Internal imports
import EquipmentAssignment from '../../components/equipment/EquipmentAssignment.vue';
import { usePermissions } from '../../composables/usePermissions';
import type { Equipment } from '../../types/equipment';

// Component setup
const $q = useQuasar();
const { checkPermission } = usePermissions();

// Component state
const loading = ref(true);
const hasPermission = ref(false);
const equipment = ref<Equipment[]>([]);
const inspectors = ref<{ id: number; firstName: string; lastName: string }[]>([]);

// Check permissions on mount
onMounted(async () => {
  try {
    hasPermission.value = checkPermission('edit_equipment');
    if (!hasPermission.value) {
      loading.value = false;
      return;
    }

    // Load initial data
    await Promise.all([
      loadEquipment(),
      loadInspectors()
    ]);
  } catch (error) {
    handleError(error);
  } finally {
    loading.value = false;
  }
});

// Data loading functions
const loadEquipment = async () => {
  try {
    // TODO: Replace with actual API call
    equipment.value = [];
  } catch (error) {
    handleError(error);
    throw error;
  }
};

const loadInspectors = async () => {
  try {
    // TODO: Replace with actual API call
    inspectors.value = [];
  } catch (error) {
    handleError(error);
    throw error;
  }
};

// Event handlers
const handleAssignment = async (assignmentData: any) => {
  try {
    loading.value = true;
    // TODO: Replace with actual API call to save assignment
    
    $q.notify({
      type: 'positive',
      message: 'Equipment assigned successfully',
      position: 'top'
    });

    // Refresh equipment list after successful assignment
    await loadEquipment();
  } catch (error) {
    handleAssignmentError(error);
  } finally {
    loading.value = false;
  }
};

const handleAssignmentError = (error: Error) => {
  handleError(error);
};

const handleError = (error: any) => {
  console.error('Equipment assignment error:', error);
  $q.notify({
    type: 'negative',
    message: error?.message || 'An error occurred during equipment assignment',
    position: 'top'
  });
};
</script>

<style lang="scss" scoped>
.equipment-assignment-page {
  max-width: 1200px;
  margin: 0 auto;

  :deep(.q-spinner) {
    margin: 2rem 0;
  }
}
</style>
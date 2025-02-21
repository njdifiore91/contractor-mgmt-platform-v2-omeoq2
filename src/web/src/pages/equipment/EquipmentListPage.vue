<template>
  <q-page class="equipment-list-page q-pa-md">
    <!-- Permission-based access check -->
    <template v-if="!canEditEquipment">
      <div class="text-center q-pa-lg">
        <q-icon name="error_outline" size="2em" color="negative" />
        <p class="text-negative q-mt-sm">You do not have permission to view equipment.</p>
      </div>
    </template>

    <template v-else>
      <!-- Page header with search -->
      <div class="row items-center justify-between q-mb-md">
        <h1 class="text-h5 q-my-none">Equipment List</h1>
        <BaseSearch
          :search-fields="searchFields"
          placeholder="Search by model, serial number or description..."
          @search="handleSearch"
        />
      </div>

      <!-- Loading state -->
      <div v-if="loading" class="row justify-center q-pa-md">
        <q-spinner color="primary" size="2em" />
        <span class="q-ml-sm">Loading equipment data...</span>
      </div>

      <!-- Error state -->
      <div v-else-if="error" class="row justify-center q-pa-md text-negative">
        <q-icon name="error" size="2em" />
        <span class="q-ml-sm">{{ error }}</span>
        <q-btn
          flat
          color="primary"
          label="Retry"
          class="q-ml-md"
          @click="refreshEquipment"
        />
      </div>

      <!-- Equipment list with virtual scrolling -->
      <template v-else>
        <EquipmentList
          ref="equipmentListRef"
          v-if="hasResults"
          :company-id="currentCompanyId"
          @refresh="refreshEquipment"
        />

        <!-- No results state -->
        <div v-else class="row justify-center q-pa-xl text-grey-7">
          <q-icon name="inventory_2" size="2em" />
          <p class="q-ml-sm q-my-none">No equipment found matching your search criteria.</p>
        </div>
      </template>
    </template>
  </q-page>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, computed, onMounted, onUnmounted } from 'vue';

// Pinia imports - v2.1.0
import { storeToRefs } from 'pinia';

// Quasar imports - v2.12.0
import { QPage, useQuasar, QSpinner } from 'quasar';

// Internal imports
import { EquipmentList } from '@/components/equipment/EquipmentList';
import { BaseSearch } from '@/components/common/BaseSearch';
import { usePermissions } from '@/composables/usePermissions';
import { useEquipmentStore } from '@/stores/equipment';
import type { Equipment } from '@/types/equipment';

// Component refs
const equipmentListRef = ref<InstanceType<typeof EquipmentList> | null>(null);
const currentCompanyId = ref<number>(1); // Default company ID, should be dynamic in real implementation

// Store initialization
const equipmentStore = useEquipmentStore();
const { equipmentList, loading, error } = storeToRefs(equipmentStore);

// Quasar initialization
const $q = useQuasar();

// Permissions setup
const { checkPermission } = usePermissions();
const canEditEquipment = computed(() => checkPermission('edit_equipment'));

// Search fields configuration based on List Fields specification
const searchFields = computed(() => [
  {
    field: 'model',
    label: 'Model',
    sortable: true
  },
  {
    field: 'serialNumber',
    label: 'Serial Number',
    sortable: true
  },
  {
    field: 'description',
    label: 'Description',
    sortable: false
  },
  {
    field: 'isOut',
    label: 'Out Status',
    sortable: true
  },
  {
    field: 'condition',
    label: 'Condition',
    sortable: true
  }
]);

// Computed properties
const hasResults = computed(() => equipmentList.value.length > 0);

// Methods
const initializePage = async (): Promise<void> => {
  try {
    if (!canEditEquipment.value) {
      return;
    }

    await equipmentStore.fetchEquipmentByCompany(currentCompanyId.value);
  } catch (err) {
    const errorMessage = err instanceof Error ? err.message : 'Failed to initialize equipment list';
    $q.notify({
      type: 'negative',
      message: errorMessage,
      position: 'top'
    });
  }
};

const handleSearch = async (searchParams: { 
  model?: string;
  serialNumber?: string;
  description?: string;
  condition?: string;
  outStatus?: boolean;
}): Promise<void> => {
  try {
    // Update filter criteria in store
    equipmentStore.$patch({
      filterCriteria: {
        companyId: currentCompanyId.value,
        searchTerm: searchParams.model || searchParams.serialNumber || searchParams.description || '',
        status: searchParams.outStatus ? ['assigned'] : []
      }
    });

    // Refresh list with new filters
    await refreshEquipment();
  } catch (err) {
    $q.notify({
      type: 'negative',
      message: 'Search failed. Please try again.',
      position: 'top'
    });
  }
};

const refreshEquipment = async (): Promise<void> => {
  if (equipmentListRef.value) {
    await equipmentListRef.value.refresh();
  }
};

// Lifecycle hooks
onMounted(() => {
  initializePage();
});

onUnmounted(() => {
  // Clean up any subscriptions or timers if needed
});
</script>

<style lang="scss" scoped>
.equipment-list-page {
  height: 100%;
  display: flex;
  flex-direction: column;

  :deep(.q-page) {
    padding: 1rem;
  }
}
</style>
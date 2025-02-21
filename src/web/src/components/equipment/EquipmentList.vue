<template>
  <BaseTable
    ref="tableRef"
    :columns="getTableColumns()"
    :data="tableData"
    :loading="loading"
    row-key="id"
    virtual-scroll
    :style="{ height: '100%' }"
    aria-label="Equipment List"
  >
    <template #loading>
      <div class="row justify-center q-pa-md">
        <q-spinner color="primary" size="2em" />
        <span class="q-ml-sm">Loading equipment data...</span>
      </div>
    </template>

    <template #error v-if="error">
      <div class="row justify-center q-pa-md text-negative">
        <q-icon name="error" size="2em" />
        <span class="q-ml-sm">{{ error }}</span>
      </div>
    </template>

    <template #no-data>
      <div class="row justify-center q-pa-md text-grey-7">
        No equipment found for this company
      </div>
    </template>
  </BaseTable>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, computed, onMounted, watchEffect } from 'vue';

// Pinia imports - v2.1.0
import { storeToRefs } from 'pinia';

// Lodash imports - v4.17.21
import debounce from 'lodash/debounce';

// Internal imports
import { BaseTable } from '@/components/common/BaseTable';
import { useEquipmentStore } from '@/stores/equipment';
import type { Equipment } from '@/types/equipment';

// Props definition with validation
const props = defineProps({
  companyId: {
    type: Number,
    required: true,
    validator: (value: number) => value > 0
  }
});

// Store initialization
const equipmentStore = useEquipmentStore();
const { equipmentList, loading, error } = storeToRefs(equipmentStore);

// Refs
const tableRef = ref<InstanceType<typeof BaseTable> | null>(null);
const lastCompanyId = ref<number | null>(null);

// Computed properties
const tableData = computed(() => {
  return equipmentList.value.map(equipment => ({
    id: equipment.id,
    model: equipment.model,
    serialNumber: equipment.serialNumber,
    description: equipment.description,
    isOut: equipment.isOut,
    condition: equipment.condition
  }));
});

// Methods
const loadEquipment = debounce(async () => {
  if (props.companyId === lastCompanyId.value) {
    return; // Avoid redundant fetches
  }

  try {
    await equipmentStore.fetchEquipmentByCompany(props.companyId);
    lastCompanyId.value = props.companyId;
  } catch (err) {
    console.error('Failed to load equipment:', err);
  }
}, 300);

const getTableColumns = () => [
  {
    name: 'model',
    label: 'Model',
    field: 'model',
    sortable: true,
    align: 'left' as const,
    required: true,
    classes: 'text-weight-medium'
  },
  {
    name: 'serialNumber',
    label: 'Serial Number',
    field: 'serialNumber',
    sortable: true,
    align: 'left' as const,
    required: true
  },
  {
    name: 'description',
    label: 'Description',
    field: 'description',
    sortable: false,
    align: 'left' as const,
    classes: 'ellipsis',
    style: 'max-width: 200px'
  },
  {
    name: 'isOut',
    label: 'Out',
    field: 'isOut',
    sortable: true,
    align: 'center' as const,
    format: (val: boolean) => val ? 'Yes' : 'No',
    classes: (val: boolean) => val ? 'text-negative' : 'text-positive'
  },
  {
    name: 'condition',
    label: 'Condition',
    field: 'condition',
    sortable: true,
    align: 'center' as const,
    format: (val: string) => val.charAt(0).toUpperCase() + val.slice(1).toLowerCase(),
    classes: (val: string) => {
      const conditionClasses = {
        'NEW': 'text-positive',
        'GOOD': 'text-primary',
        'FAIR': 'text-warning',
        'POOR': 'text-negative'
      };
      return conditionClasses[val] || '';
    }
  }
];

// Lifecycle hooks
onMounted(() => {
  loadEquipment();
});

watchEffect(() => {
  if (props.companyId) {
    loadEquipment();
  }
});

// Component exports
defineExpose({
  refresh: loadEquipment
});
</script>

<style lang="scss" scoped>
.ellipsis {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
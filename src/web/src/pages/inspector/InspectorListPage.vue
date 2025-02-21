<template>
  <div class="inspector-list-page q-pa-md">
    <!-- Search section -->
    <InspectorSearch
      ref="inspectorSearchRef"
      @search-results="handleSearchResults"
      @search-error="handleSearchError"
    />

    <!-- Results table with virtual scrolling -->
    <BaseTable
      v-if="hasSearchResults"
      :columns="columns"
      :data="inspectors"
      :loading="loading"
      :row-key="'id'"
      :user-permissions="userPermissions"
      class="q-mt-md"
      @row-click="handleRowClick"
    />

    <!-- No results message -->
    <div v-else-if="!loading" class="text-center q-mt-xl">
      <q-icon name="search" size="48px" color="grey-5" />
      <div class="text-h6 q-mt-sm text-grey-7">No inspectors found</div>
      <div class="text-caption text-grey-6">Try adjusting your search criteria</div>
    </div>

    <!-- Loading state -->
    <div v-if="loading" class="text-center q-mt-xl">
      <q-spinner color="primary" size="48px" />
      <div class="text-subtitle1 q-mt-sm">Searching for inspectors...</div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Vue imports
import { ref, computed, onMounted } from 'vue';
import { useQuasar } from 'quasar';
import { useRouter } from 'vue-router';

// Internal imports
import InspectorSearch from '@/components/inspector/InspectorSearch.vue';
import BaseTable from '@/components/common/BaseTable.vue';
import { searchInspectors } from '@/services/api/inspector';
import { usePermissions } from '@/composables/usePermissions';
import type { Inspector } from '@/types/inspector';
import { debounce } from 'lodash';

// Initialize composables
const $q = useQuasar();
const router = useRouter();
const { checkPermission } = usePermissions();

// Component refs
const inspectorSearchRef = ref<InstanceType<typeof InspectorSearch> | null>(null);

// Reactive state
const inspectors = ref<Inspector[]>([]);
const loading = ref(false);
const searchParams = ref({
  zipCode: '',
  radius: 50,
  specialties: [] as string[],
  certifications: [] as string[],
  availableOnly: false,
  status: 'ACTIVE',
  locations: [] as string[],
  classifications: [] as string[],
  hasValidDrugTest: true
});

// Table columns configuration based on requirements
const columns = [
  { name: 'status', label: 'Status', field: 'status', sortable: true, align: 'left' },
  { name: 'firstName', label: 'First', field: 'firstName', sortable: true, align: 'left' },
  { name: 'lastName', label: 'Last', field: 'lastName', sortable: true, align: 'left' },
  { name: 'state', label: 'State', field: 'state', sortable: true, align: 'left' },
  { name: 'customers', label: 'Customers', field: 'assignedCustomers', sortable: true, align: 'left',
    format: (val: any[]) => val?.map(c => c.name).join(', ') || '' },
  { name: 'companies', label: 'Companies', field: 'assignedContracts', sortable: true, align: 'left' },
  { name: 'title', label: 'Title', field: 'title', sortable: true, align: 'left' },
  { name: 'specialties', label: 'Specialties', field: 'specialties', sortable: false, align: 'left',
    format: (val: string[]) => val?.join(', ') || '' },
  { name: 'issues', label: 'Issues', field: 'hasIssues', sortable: true, align: 'center',
    format: (val: boolean) => val ? 'Yes' : 'No' },
  { name: 'approvalNeeded', label: 'Approval Needed', field: 'needsApproval', sortable: true, align: 'center',
    format: (val: boolean) => val ? 'Yes' : 'No' },
  { name: 'email', label: 'Email', field: 'email', sortable: true, align: 'left' },
  { name: 'inspectorId', label: 'Inspector ID', field: 'inspectorId', sortable: true, align: 'left' },
  { name: 'dob', label: 'DOB', field: 'dateOfBirth', sortable: true, align: 'left',
    format: (val: Date) => val ? new Date(val).toLocaleDateString() : '' },
  { name: 'nickname', label: 'Nickname', field: 'nickname', sortable: true, align: 'left' }
];

// Computed properties
const hasSearchResults = computed(() => inspectors.value.length > 0);

const userPermissions = computed(() => [
  'view_inspector_details',
  'edit_inspector',
  'manage_drug_tests'
]);

// Methods
const handleSearchResults = (results: Inspector[]) => {
  inspectors.value = results;
  loading.value = false;
};

const handleSearchError = (error: Error) => {
  loading.value = false;
  $q.notify({
    type: 'negative',
    message: `Search failed: ${error.message}`,
    position: 'top'
  });
};

const handleSearch = debounce(async (params: typeof searchParams.value) => {
  if (!checkPermission('view_inspector_details')) {
    $q.notify({
      type: 'negative',
      message: 'You do not have permission to search inspectors',
      position: 'top'
    });
    return;
  }

  loading.value = true;
  try {
    const results = await searchInspectors(params);
    handleSearchResults(results);
  } catch (error) {
    handleSearchError(error as Error);
  }
}, 300);

const handleRowClick = (inspector: Inspector) => {
  if (!checkPermission('view_inspector_details')) {
    $q.notify({
      type: 'negative',
      message: 'You do not have permission to view inspector details',
      position: 'top'
    });
    return;
  }

  router.push(`/inspectors/${inspector.id}`);
};

// Lifecycle hooks
onMounted(() => {
  // Initialize with default search if autoSearch is enabled
  if (inspectorSearchRef.value?.autoSearch) {
    handleSearch(searchParams.value);
  }
});
</script>

<style lang="scss">
.inspector-list-page {
  .q-table {
    height: calc(100vh - 250px);
    min-height: 400px;
  }

  .q-table__container {
    background: white;
    border-radius: 8px;
    box-shadow: 0 1px 5px rgba(0, 0, 0, 0.1);
  }
}
</style>
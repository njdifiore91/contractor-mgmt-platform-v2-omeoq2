<template>
  <div class="drug-test-page">
    <!-- Page header with title and actions -->
    <div class="drug-test-page__header">
      <h1 class="text-h5">{{ $t('drugTest.title') }}</h1>
      <q-btn
        v-if="hasEditPermission"
        color="primary"
        :label="$t('drugTest.addNew')"
        @click="openDrugTestForm"
        :loading="loading"
      />
    </div>

    <!-- Drug test records table with virtual scrolling -->
    <BaseTable
      ref="tableRef"
      :columns="tableColumns"
      :data="drugTests"
      :loading="loading"
      row-key="id"
      :user-permissions="['edit_users']"
      @row-click="handleRowClick"
      class="drug-test-page__table"
    >
      <template #top-right>
        <q-input
          v-model="searchQuery"
          :placeholder="$t('drugTest.search')"
          dense
          outlined
          class="q-mr-md"
        >
          <template #append>
            <q-icon name="search" />
          </template>
        </q-input>
      </template>
    </BaseTable>

    <!-- Drug test form dialog -->
    <q-dialog v-model="showForm" persistent>
      <q-card class="drug-test-page__form">
        <q-card-section class="row items-center">
          <span class="text-h6">{{ editMode ? $t('drugTest.edit') : $t('drugTest.add') }}</span>
          <q-space />
          <q-btn icon="close" flat round dense v-close-popup />
        </q-card-section>

        <q-card-section>
          <DrugTestForm
            :inspector-id="selectedInspectorId"
            @submit="handleFormSubmit"
            @cancel="showForm = false"
          />
        </q-card-section>
      </q-card>
    </q-dialog>
  </div>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useQuasar } from 'quasar'; // ^2.0.0
import { useI18n } from 'vue-i18n'; // ^9.0.0

// Component imports
import { DrugTestForm } from '../../components/inspector/DrugTestForm';
import { BaseTable } from '../../components/common/BaseTable';

// Composables and services
import { usePermissions } from '../../composables/usePermissions';
import { manageDrugTest } from '../../services/api/inspector';

// Types
import type { DrugTest } from '../../types/inspector';

// Initialize composables
const $q = useQuasar();
const { t } = useI18n();
const { checkPermission } = usePermissions();

// Component state
const loading = ref(false);
const drugTests = ref<DrugTest[]>([]);
const showForm = ref(false);
const editMode = ref(false);
const searchQuery = ref('');
const selectedInspectorId = ref<number | null>(null);
const tableRef = ref<InstanceType<typeof BaseTable> | null>(null);

// Computed properties
const hasEditPermission = computed(() => checkPermission('edit_users'));

// Table configuration
const tableColumns = [
  {
    name: 'created',
    field: 'created',
    label: t('drugTest.fields.created'),
    sortable: true,
    format: (val: Date) => new Date(val).toLocaleDateString()
  },
  {
    name: 'user',
    field: 'createdBy',
    label: t('drugTest.fields.user'),
    sortable: true
  },
  {
    name: 'modified',
    field: 'modified',
    label: t('drugTest.fields.modified'),
    sortable: true,
    format: (val: Date) => val ? new Date(val).toLocaleDateString() : '-'
  },
  {
    name: 'testDate',
    field: 'testDate',
    label: t('drugTest.fields.testDate'),
    sortable: true,
    format: (val: Date) => new Date(val).toLocaleDateString()
  },
  {
    name: 'testType',
    field: 'testType',
    label: t('drugTest.fields.testType'),
    sortable: true
  },
  {
    name: 'frequency',
    field: 'frequency',
    label: t('drugTest.fields.frequency'),
    sortable: true
  },
  {
    name: 'result',
    field: 'result',
    label: t('drugTest.fields.result'),
    sortable: true
  },
  {
    name: 'comment',
    field: 'comment',
    label: t('drugTest.fields.comment')
  },
  {
    name: 'company',
    field: 'company',
    label: t('drugTest.fields.company'),
    sortable: true
  }
];

// Methods
const loadDrugTests = async () => {
  if (!hasEditPermission.value) return;

  loading.value = true;
  try {
    // Implementation would fetch drug test records from API
    // Placeholder for actual API call
    await new Promise(resolve => setTimeout(resolve, 1000));
    drugTests.value = [];
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : 'Failed to load drug tests';
    $q.notify({
      type: 'negative',
      message: errorMessage,
      position: 'top',
      timeout: 3000
    });
  } finally {
    loading.value = false;
  }
};

const openDrugTestForm = (inspectorId?: number) => {
  selectedInspectorId.value = inspectorId || null;
  editMode.value = !!inspectorId;
  showForm.value = true;
};

const handleRowClick = (row: DrugTest) => {
  if (hasEditPermission.value) {
    openDrugTestForm(row.inspectorId);
  }
};

const handleFormSubmit = async (drugTestData: DrugTest) => {
  try {
    loading.value = true;
    const result = await manageDrugTest(drugTestData.inspectorId, drugTestData);
    
    $q.notify({
      type: 'positive',
      message: t('drugTest.saveSuccess'),
      position: 'top',
      timeout: 3000
    });

    showForm.value = false;
    await loadDrugTests();
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : 'Failed to save drug test';
    $q.notify({
      type: 'negative',
      message: errorMessage,
      position: 'top',
      timeout: 3000
    });
  } finally {
    loading.value = false;
  }
};

// Lifecycle hooks
onMounted(async () => {
  await loadDrugTests();
});

onUnmounted(() => {
  // Cleanup if needed
});
</script>

<style lang="scss">
.drug-test-page {
  padding: 1rem;
  height: 100%;
  display: flex;
  flex-direction: column;

  &__header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
  }

  &__table {
    flex: 1;
    min-height: 0;
  }

  &__form {
    width: 600px;
    max-width: 90vw;
  }
}
</style>
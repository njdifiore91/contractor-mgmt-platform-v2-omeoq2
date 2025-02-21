<template>
  <div class="code-types-page q-pa-md">
    <!-- Header section with title and add button -->
    <div class="row items-center justify-between q-mb-lg">
      <h1 class="text-h5 q-my-none">{{ $t('admin.codeType.title') }}</h1>
      <QBtn
        v-if="hasEditPermission"
        color="primary"
        :label="$t('admin.codeType.add')"
        icon="add"
        :loading="loading"
        @click="handleAddCodeType"
        data-cy="add-code-type-btn"
      />
    </div>

    <!-- Error alert -->
    <q-banner
      v-if="error"
      class="bg-negative text-white q-mb-md"
      rounded
      dense
    >
      {{ error }}
      <template v-slot:action>
        <QBtn
          flat
          color="white"
          :label="$t('common.retry')"
          @click="loadCodeTypes"
        />
      </template>
    </q-banner>

    <!-- Code types table -->
    <BaseTable
      ref="tableRef"
      :columns="tableColumns"
      :data="codeTypes"
      :loading="loading"
      row-key="id"
      :virtual-scroll="true"
      @row-click="handleEditCodeType"
    />

    <!-- Code type editor dialog -->
    <CodeTypeEditor
      ref="editorRef"
      v-model="selectedCodeType"
      @save="handleSaveCodeType"
    />
  </div>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, onMounted, computed } from 'vue';

// Quasar imports - v2.12.0
import { QBtn, useQuasar } from 'quasar';

// VueUse imports - v10.0.0
import { useAuditLog } from '@vueuse/core';

// Internal imports
import CodeTypeEditor from '@/components/admin/CodeTypeEditor.vue';
import BaseTable from '@/components/common/BaseTable.vue';
import { usePermissions } from '@/composables/usePermissions';
import { adminApi } from '@/services/api/admin';
import type { CodeType } from '@/types/admin';

// Initialize composables
const $q = useQuasar();
const { checkPermission } = usePermissions();
const auditLog = useAuditLog();

// Component refs
const tableRef = ref<InstanceType<typeof BaseTable> | null>(null);
const editorRef = ref<InstanceType<typeof CodeTypeEditor> | null>(null);

// Component state
const loading = ref(false);
const error = ref<string | null>(null);
const codeTypes = ref<CodeType[]>([]);
const selectedCodeType = ref<CodeType | null>(null);

// Computed properties
const hasEditPermission = computed(() => checkPermission('edit_codes'));

const tableColumns = computed(() => [
  {
    name: 'name',
    label: 'Code Type',
    field: 'name',
    sortable: true,
    align: 'left' as const
  },
  {
    name: 'description',
    label: 'Description',
    field: 'description',
    sortable: false,
    align: 'left' as const
  },
  {
    name: 'codesCount',
    label: 'Codes',
    field: (row: CodeType) => row.codes?.length || 0,
    sortable: false,
    align: 'center' as const
  },
  {
    name: 'isActive',
    label: 'Status',
    field: 'isActive',
    sortable: true,
    align: 'center' as const,
    format: (val: boolean) => val ? 'Active' : 'Inactive'
  }
]);

// Methods
const loadCodeTypes = async () => {
  if (!hasEditPermission.value) {
    error.value = 'Permission denied: Cannot access code types';
    return;
  }

  loading.value = true;
  error.value = null;

  try {
    const response = await adminApi.getCodeTypes();
    codeTypes.value = response;
    auditLog.log('CodeTypes.Load', { count: response.length });
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to load code types';
    auditLog.error('CodeTypes.Load.Error', { error: error.value });
  } finally {
    loading.value = false;
  }
};

const handleAddCodeType = () => {
  if (!hasEditPermission.value) return;

  selectedCodeType.value = {
    name: '',
    description: '',
    codes: [],
    isActive: true
  } as CodeType;

  editorRef.value?.openDialog();
  auditLog.log('CodeType.Add.Dialog.Open');
};

const handleEditCodeType = (codeType: CodeType) => {
  if (!hasEditPermission.value) return;

  selectedCodeType.value = { ...codeType };
  editorRef.value?.openDialog();
  auditLog.log('CodeType.Edit.Dialog.Open', { id: codeType.id });
};

const handleSaveCodeType = async (updatedCodeType: CodeType) => {
  if (!hasEditPermission.value) return;

  loading.value = true;
  error.value = null;

  try {
    let savedCodeType: CodeType;

    if (updatedCodeType.id) {
      savedCodeType = await adminApi.updateCodeType(updatedCodeType.id, updatedCodeType);
      auditLog.log('CodeType.Update', { id: updatedCodeType.id });
    } else {
      savedCodeType = await adminApi.createCodeType(updatedCodeType);
      auditLog.log('CodeType.Create', { id: savedCodeType.id });
    }

    // Update local data
    const index = codeTypes.value.findIndex(ct => ct.id === savedCodeType.id);
    if (index >= 0) {
      codeTypes.value[index] = savedCodeType;
    } else {
      codeTypes.value.push(savedCodeType);
    }

    $q.notify({
      type: 'positive',
      message: 'Code type saved successfully'
    });

    editorRef.value?.closeDialog();
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to save code type';
    auditLog.error('CodeType.Save.Error', { error: error.value });
    
    $q.notify({
      type: 'negative',
      message: error.value
    });
  } finally {
    loading.value = false;
  }
};

// Lifecycle hooks
onMounted(async () => {
  await loadCodeTypes();
});
</script>

<style lang="scss" scoped>
.code-types-page {
  height: 100%;
  display: flex;
  flex-direction: column;

  .base-table {
    flex: 1;
    min-height: 0;
  }
}
</style>
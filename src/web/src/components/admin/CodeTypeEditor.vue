<template>
  <base-dialog
    ref="dialogRef"
    :persistent="true"
    :loading="loading"
    :confirm-text="$t('admin.codeType.save')"
    :cancel-text="$t('common.cancel')"
    @confirm="handleSave"
  >
    <template #header>
      <div class="text-h6">{{ $t('admin.codeType.editor.title') }}</div>
    </template>

    <base-form
      ref="formRef"
      v-model="formData"
      :fields="formFields"
      @update:model-value="handleFormUpdate"
    >
      <!-- Codes Table Section -->
      <div class="q-mt-md">
        <div class="row items-center justify-between q-mb-sm">
          <div class="text-subtitle1">{{ $t('admin.codeType.codes.title') }}</div>
          <q-btn
            color="primary"
            icon="add"
            :label="$t('admin.codeType.codes.add')"
            @click="addNewCode"
            :disable="loading"
          />
        </div>

        <q-table
          :rows="formData.codes"
          :columns="codeColumns"
          row-key="id"
          :loading="loading"
          virtual-scroll
          :rows-per-page-options="[15, 25, 50, 100]"
          v-model:pagination="pagination"
        >
          <template #body="props">
            <q-tr :props="props">
              <q-td key="value" :props="props">
                <q-input
                  v-model="props.row.value"
                  dense
                  :disable="loading"
                  @update:model-value="validateCode(props.row)"
                  :error="!!codeErrors[props.row.id]?.value"
                  :error-message="codeErrors[props.row.id]?.value"
                />
              </q-td>
              <q-td key="description" :props="props">
                <q-input
                  v-model="props.row.description"
                  dense
                  :disable="loading"
                  @update:model-value="validateCode(props.row)"
                  :error="!!codeErrors[props.row.id]?.description"
                  :error-message="codeErrors[props.row.id]?.description"
                />
              </q-td>
              <q-td key="isExpireable" :props="props">
                <q-checkbox
                  v-model="props.row.isExpireable"
                  :disable="loading"
                />
              </q-td>
              <q-td key="actions" :props="props">
                <q-btn
                  flat
                  round
                  color="negative"
                  icon="delete"
                  @click="removeCode(props.row)"
                  :disable="loading"
                >
                  <q-tooltip>{{ $t('common.delete') }}</q-tooltip>
                </q-btn>
              </q-td>
            </q-tr>
          </template>
        </q-table>
      </div>
    </base-form>
  </base-dialog>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'; // vue@^3.3.0
import { useI18n } from 'vue-i18n'; // vue-i18n@^9.2.0
import { QTable, QBtn, QInput, QCheckbox, QTooltip, useQuasar } from 'quasar'; // quasar@^2.12.0
import { debounce } from 'lodash'; // lodash@^4.17.21
import BaseDialog from '../common/BaseDialog.vue';
import BaseForm from '../common/BaseForm.vue';
import type { CodeType, Code } from '@/types/admin';
import { adminApi } from '@/services/api/admin';

const { t } = useI18n();
const $q = useQuasar();

// Component refs
const dialogRef = ref<InstanceType<typeof BaseDialog> | null>(null);
const formRef = ref<InstanceType<typeof BaseForm> | null>(null);

// State
const loading = ref(false);
const formData = ref<Partial<CodeType>>({
  name: '',
  description: '',
  codes: []
});
const codeErrors = ref<Record<number, Record<string, string>>>({});
const pagination = ref({
  rowsPerPage: 15
});

// Form fields configuration
const formFields = computed(() => [
  {
    field: 'name',
    label: t('admin.codeType.fields.name'),
    type: 'text',
    required: true,
    validation: [{
      pattern: /^[a-zA-Z0-9\s-_]{2,50}$/,
      errorMessage: t('admin.codeType.validation.nameFormat')
    }]
  },
  {
    field: 'description',
    label: t('admin.codeType.fields.description'),
    type: 'text',
    required: false,
    validation: [{
      maxLength: 200,
      errorMessage: t('admin.codeType.validation.descriptionLength')
    }]
  }
]);

// Table columns configuration
const codeColumns = computed(() => [
  {
    name: 'value',
    label: t('admin.codeType.fields.code'),
    field: 'value',
    required: true,
    align: 'left'
  },
  {
    name: 'description',
    label: t('admin.codeType.fields.description'),
    field: 'description',
    align: 'left'
  },
  {
    name: 'isExpireable',
    label: t('admin.codeType.fields.expireable'),
    field: 'isExpireable',
    align: 'center'
  },
  {
    name: 'actions',
    label: t('common.actions'),
    field: 'actions',
    align: 'center'
  }
]);

// Methods
const validatePermission = async () => {
  try {
    const hasPermission = await adminApi.checkPermission('Edit Codes');
    if (!hasPermission) {
      throw new Error(t('admin.errors.noPermission'));
    }
    return true;
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: t('admin.errors.permissionCheck')
    });
    return false;
  }
};

const openDialog = async (codeType?: CodeType) => {
  if (!await validatePermission()) return;

  formData.value = codeType ? { ...codeType } : {
    name: '',
    description: '',
    codes: []
  };
  
  codeErrors.value = {};
  dialogRef.value?.open();
};

const validateCode = debounce((code: Code) => {
  const errors: Record<string, string> = {};
  
  if (!code.value?.trim()) {
    errors.value = t('admin.codeType.validation.codeRequired');
  } else if (!/^[A-Z0-9_-]{1,20}$/.test(code.value)) {
    errors.value = t('admin.codeType.validation.codeFormat');
  }

  if (code.description && code.description.length > 200) {
    errors.description = t('admin.codeType.validation.descriptionLength');
  }

  codeErrors.value[code.id] = errors;
}, 300);

const addNewCode = () => {
  const newCode: Code = {
    id: Date.now(), // Temporary ID for new codes
    value: '',
    description: '',
    isExpireable: false,
    isActive: true,
    expiresAt: null,
    createdAt: new Date(),
    createdBy: 0,
    updatedAt: null,
    updatedBy: null
  };
  
  formData.value.codes = [...(formData.value.codes || []), newCode];
};

const removeCode = (code: Code) => {
  formData.value.codes = formData.value.codes?.filter(c => c.id !== code.id) || [];
  delete codeErrors.value[code.id];
};

const handleFormUpdate = () => {
  formData.value.codes?.forEach(validateCode);
};

const handleSave = async () => {
  try {
    loading.value = true;

    // Validate form
    const isFormValid = await formRef.value?.validate();
    if (!isFormValid) return;

    // Validate codes
    formData.value.codes?.forEach(validateCode);
    const hasCodeErrors = Object.values(codeErrors.value).some(errors => 
      Object.keys(errors).length > 0
    );
    if (hasCodeErrors) {
      throw new Error(t('admin.codeType.validation.codesInvalid'));
    }

    // Save code type
    const savedCodeType = formData.value.id
      ? await adminApi.updateCodeType(formData.value.id, formData.value)
      : await adminApi.createCodeType(formData.value as Omit<CodeType, 'id'>);

    $q.notify({
      type: 'positive',
      message: t('admin.codeType.saveSuccess')
    });

    dialogRef.value?.close();
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: error instanceof Error ? error.message : t('admin.errors.saveFailed')
    });
  } finally {
    loading.value = false;
  }
};

// Expose public methods
defineExpose({
  openDialog
});
</script>

<style lang="scss" scoped>
.q-table {
  :deep(.q-table__top) {
    padding: 8px 0;
  }

  :deep(.q-table__container) {
    max-height: 400px;
  }
}
</style>
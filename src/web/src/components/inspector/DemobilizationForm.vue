<template>
  <QCard class="demobilization-form" role="form" aria-labelledby="demob-form-title">
    <div class="demobilization-form__header">
      <h2 id="demob-form-title" class="text-h6">Inspector Demobilization</h2>
    </div>

    <BaseForm
      ref="formRef"
      v-model="formData"
      :fields="formFields"
      :loading="loading"
      @submit="handleSubmit"
      @error="handleError"
    >
      <template #default="{ field }">
        <div v-if="field.field === 'demobReason'" class="demobilization-form__reason">
          <QSelect
            v-model="formData.demobReason"
            :options="demobReasonOptions"
            :label="field.label"
            :rules="[val => !!val || 'Demobilization reason is required']"
            emit-value
            map-options
            outlined
            :disable="loading"
          />
        </div>

        <div v-if="field.field === 'demobDate'" class="demobilization-form__date">
          <QDate
            v-model="formData.demobDate"
            :label="field.label"
            :rules="[val => !!val || 'Demobilization date is required']"
            :options="dateOptions"
            :disable="loading"
            outlined
            today-btn
          />
        </div>

        <div v-if="field.field === 'note'" class="demobilization-form__note">
          <QInput
            v-model="formData.note"
            type="textarea"
            :label="field.label"
            :rules="[val => !val || val.length <= 1000 || 'Note cannot exceed 1000 characters']"
            :disable="loading"
            outlined
            autogrow
          />
        </div>
      </template>
    </BaseForm>

    <div v-if="error" role="alert" class="demobilization-form__error text-negative q-mt-sm">
      {{ error }}
    </div>
  </QCard>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, computed } from 'vue';

// Quasar imports - v2.12.0
import { QCard, QSelect, QDate, QInput, useQuasar } from 'quasar';

// Internal imports
import BaseForm from '../common/BaseForm.vue';
import { InspectorDemobilization, DemobilizationReason } from '@/types/inspector';
import { demobilizeInspector } from '@/services/api/inspector';

// Props
const props = defineProps<{
  inspectorId: number;
}>();

// Emits
const emit = defineEmits<{
  (e: 'success', value: InspectorDemobilization): void;
  (e: 'error', value: string): void;
}>();

// Component state
const $q = useQuasar();
const formRef = ref<typeof BaseForm | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);
const formData = ref<InspectorDemobilization>({
  demobReason: '' as DemobilizationReason,
  demobDate: new Date().toISOString().split('T')[0],
  note: ''
});

// Computed properties
const demobReasonOptions = computed(() => [
  { label: 'Project Completion', value: DemobilizationReason.PROJECT_COMPLETE },
  { label: 'Performance Issues', value: DemobilizationReason.PERFORMANCE },
  { label: 'Voluntary Resignation', value: DemobilizationReason.VOLUNTARY },
  { label: 'Medical', value: DemobilizationReason.MEDICAL },
  { label: 'Other', value: DemobilizationReason.OTHER }
]);

const dateOptions = computed(() => {
  const today = new Date();
  return (date: string) => {
    const testDate = new Date(date);
    return testDate <= today;
  };
});

// Form fields configuration
const formFields = [
  {
    field: 'demobReason',
    label: 'Demobilization Reason',
    type: 'select',
    required: true,
    validation: [
      {
        pattern: null,
        minLength: null,
        maxLength: null,
        minValue: null,
        maxValue: null,
        customValidator: (value: string) => Object.values(DemobilizationReason).includes(value as DemobilizationReason),
        errorMessage: 'Please select a valid demobilization reason'
      }
    ]
  },
  {
    field: 'demobDate',
    label: 'Demobilization Date',
    type: 'date',
    required: true,
    validation: [
      {
        pattern: null,
        minLength: null,
        maxLength: null,
        minValue: null,
        maxValue: null,
        customValidator: (value: string) => new Date(value) <= new Date(),
        errorMessage: 'Demobilization date cannot be in the future'
      }
    ]
  },
  {
    field: 'note',
    label: 'Notes',
    type: 'textarea',
    required: false,
    validation: [
      {
        pattern: null,
        minLength: null,
        maxLength: 1000,
        minValue: null,
        maxValue: null,
        customValidator: null,
        errorMessage: 'Notes cannot exceed 1000 characters'
      }
    ]
  }
];

// Form submission handler
const handleSubmit = async () => {
  try {
    loading.value = true;
    error.value = null;

    const result = await demobilizeInspector(props.inspectorId, formData.value);
    
    if (result) {
      $q.notify({
        type: 'positive',
        message: 'Inspector successfully demobilized',
        position: 'top'
      });
      
      emit('success', formData.value);
    }
  } catch (err) {
    const errorMessage = err instanceof Error ? err.message : 'Failed to demobilize inspector';
    error.value = errorMessage;
    emit('error', errorMessage);
    
    $q.notify({
      type: 'negative',
      message: errorMessage,
      position: 'top'
    });
  } finally {
    loading.value = false;
  }
};

// Form error handler
const handleError = (errors: string[]) => {
  error.value = errors.join(', ');
  emit('error', error.value);
};
</script>

<style lang="scss">
.demobilization-form {
  padding: 1.5rem;
  max-width: 600px;
  margin: 0 auto;

  &__header {
    margin-bottom: 1.5rem;
  }

  &__reason,
  &__date,
  &__note {
    margin-bottom: 1rem;
  }

  &__error {
    font-size: 0.875rem;
    font-weight: 500;
    padding: 0.5rem;
    border-radius: 4px;
    background-color: rgba(var(--q-negative-rgb), 0.1);
  }
}
</style>
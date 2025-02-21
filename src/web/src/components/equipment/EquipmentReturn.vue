<template>
  <BaseForm
    ref="formRef"
    :fields="formFields"
    v-model="formData"
    :loading="loading"
    @submit="handleSubmit"
  >
    <template #default>
      <div class="equipment-return__header">
        <h2 class="text-h6">{{ $t('equipment.return.title') }}</h2>
      </div>
    </template>
  </BaseForm>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, onMounted } from 'vue';

// Quasar imports - v2.12.0
import { useQuasar } from 'quasar';

// Internal imports
import BaseForm from '../common/BaseForm.vue';
import type { EquipmentReturn } from '../../types/equipment';
import { recordEquipmentReturn } from '../../services/api/equipment';

// Props and emits
const props = defineProps<{
  equipmentId: number;
}>();

const emit = defineEmits<{
  (e: 'completed', value: EquipmentReturn): void;
  (e: 'error', value: string): void;
}>();

// Component state
const $q = useQuasar();
const formRef = ref<InstanceType<typeof BaseForm> | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);

const formData = ref<Partial<EquipmentReturn>>({
  equipmentId: props.equipmentId,
  returnCondition: '',
  returnDate: new Date()
});

// Form field definitions with validation and accessibility
const formFields = [
  {
    field: 'returnCondition',
    label: 'Return Condition',
    type: 'select',
    required: true,
    options: ['Excellent', 'Good', 'Fair', 'Poor', 'Damaged'],
    validation: [
      {
        pattern: /^(Excellent|Good|Fair|Poor|Damaged)$/,
        errorMessage: 'Please select a valid condition',
        minLength: null,
        maxLength: null,
        minValue: null,
        maxValue: null,
        customValidator: null
      }
    ],
    placeholder: 'Select equipment condition',
    defaultValue: '',
    'aria-label': 'Select equipment return condition'
  },
  {
    field: 'returnDate',
    label: 'Return Date',
    type: 'date',
    required: true,
    validation: [
      {
        pattern: null,
        errorMessage: 'Return date cannot be in the future',
        minLength: null,
        maxLength: null,
        minValue: null,
        maxValue: null,
        customValidator: (value: Date) => value <= new Date()
      }
    ],
    placeholder: 'Select return date',
    defaultValue: new Date(),
    'aria-label': 'Select equipment return date'
  }
];

// Form submission handler
const handleSubmit = async (returnData: EquipmentReturn) => {
  try {
    loading.value = true;
    error.value = null;

    // Validate form using BaseForm validation
    if (formRef.value) {
      const isValid = await formRef.value.validate();
      if (!isValid) {
        throw new Error('Please check the form for errors');
      }
    }

    // Ensure required fields are present
    if (!returnData.equipmentId || !returnData.returnCondition || !returnData.returnDate) {
      throw new Error('Missing required fields');
    }

    // Record equipment return
    const result = await recordEquipmentReturn({
      equipmentId: returnData.equipmentId,
      returnCondition: returnData.returnCondition,
      returnDate: returnData.returnDate
    });

    // Show success notification
    $q.notify({
      type: 'positive',
      message: 'Equipment return recorded successfully',
      position: 'top',
      timeout: 3000
    });

    // Emit completion event
    emit('completed', result);

  } catch (err) {
    const errorMessage = (err as Error).message;
    error.value = errorMessage;
    emit('error', errorMessage);

    // Show error notification
    $q.notify({
      type: 'negative',
      message: errorMessage,
      position: 'top',
      timeout: 5000
    });
  } finally {
    loading.value = false;
  }
};

// Initialize component
onMounted(() => {
  if (!props.equipmentId) {
    error.value = 'Equipment ID is required';
    emit('error', 'Equipment ID is required');
  }
});
</script>

<style lang="scss">
.equipment-return {
  &__header {
    margin-bottom: 1.5rem;
    
    h2 {
      margin: 0;
      color: var(--q-primary);
    }
  }
}
</style>
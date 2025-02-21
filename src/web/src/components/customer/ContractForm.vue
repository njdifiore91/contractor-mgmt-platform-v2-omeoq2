<template>
  <BaseForm
    ref="formRef"
    :fields="formFields"
    v-model="formData"
    @submit="handleSubmit"
    @error="handleError"
  >
    <template #after-fields>
      <div class="contract-form__toggle-wrapper">
        <QToggle
          v-model="formData.active"
          :label="$t('contract.form.active')"
          :true-value="true"
          :false-value="false"
          color="primary"
          :disable="loading"
          :aria-label="$t('contract.form.activeAriaLabel')"
        />
      </div>
    </template>
  </BaseForm>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, computed, onMounted } from 'vue';

// Quasar imports - v2.12.0
import { QToggle } from 'quasar';

// Utility imports
import { debounce } from 'lodash'; // v4.17.21

// Internal imports
import BaseForm from '@/components/common/BaseForm.vue';
import type { Contract } from '@/types/customer';
import { useCustomerStore } from '@/stores/customer';
import type { DialogField } from '@/types/common';

// Props and emits
const props = defineProps<{
  modelValue: Contract;
  loading?: boolean;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: Contract): void;
  (e: 'submit', value: Contract): void;
  (e: 'error', value: string[]): void;
}>();

// Store
const customerStore = useCustomerStore();

// Component state
const formRef = ref<InstanceType<typeof BaseForm> | null>(null);
const formData = ref<Partial<Contract>>({
  name: '',
  active: true,
});

// Form fields configuration
const formFields = computed<DialogField[]>(() => [
  {
    field: 'name',
    label: 'Contract Name',
    type: 'text',
    required: true,
    validation: [
      {
        pattern: /^[a-zA-Z0-9\s-_]{3,50}$/,
        minLength: 3,
        maxLength: 50,
        minValue: null,
        maxValue: null,
        customValidator: null,
        errorMessage: 'Contract name must be 3-50 characters and contain only letters, numbers, spaces, hyphens and underscores'
      }
    ],
    placeholder: 'Enter contract name',
    defaultValue: '',
  }
]);

// Validation handler with debounce
const validateField = debounce(async (fieldName: string, value: any) => {
  if (!formRef.value) return false;
  return await formRef.value.validateField(fieldName);
}, 300);

// Form submission handler
const handleSubmit = async (formValue: Partial<Contract>) => {
  try {
    const contractData: Contract = {
      ...formValue as Contract,
      customerId: customerStore.selectedCustomer?.id ?? 0,
    };

    if (props.modelValue?.id) {
      await customerStore.updateContract(
        contractData.customerId,
        props.modelValue.id,
        contractData
      );
    } else {
      await customerStore.addContract(contractData.customerId, contractData);
    }

    emit('submit', contractData);
  } catch (error) {
    emit('error', [(error as Error).message]);
  }
};

// Error handler
const handleError = (errors: string[]) => {
  emit('error', errors);
};

// Lifecycle hooks
onMounted(() => {
  if (props.modelValue) {
    formData.value = { ...props.modelValue };
  }
});

// Watch for external updates
watch(
  () => props.modelValue,
  (newValue) => {
    if (newValue) {
      formData.value = { ...newValue };
    }
  },
  { deep: true }
);
</script>

<style lang="scss">
.contract-form {
  &__toggle-wrapper {
    margin-top: 1rem;
    padding: 0.5rem 0;
    
    .q-toggle {
      &__label {
        font-size: 1rem;
        color: var(--q-primary);
      }
    }
  }
}
</style>
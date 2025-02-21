<template>
  <BaseForm
    ref="formRef"
    :fields="formFields"
    v-model="formData"
    :loading="loading"
    @submit="handleSubmit"
    @error="handleValidationError"
  >
    <template #after-fields>
      <!-- Contacts List -->
      <div class="customer-form__contacts" v-if="customer">
        <h3>{{ $t('customer.contacts.title') }}</h3>
        <div class="customer-form__contacts-list">
          <div v-for="contact in customer.contacts" :key="contact.id" class="contact-item">
            <span>{{ contact.firstName }} {{ contact.lastName }}</span>
            <span>{{ contact.jobTitle }}</span>
            <span>{{ formatDate(contact.dateCreated) }}</span>
          </div>
        </div>
      </div>

      <!-- Contracts List -->
      <div class="customer-form__contracts" v-if="customer">
        <h3>{{ $t('customer.contracts.title') }}</h3>
        <div class="customer-form__contracts-list">
          <div v-for="contract in customer.contracts" :key="contract.id" class="contract-item">
            <span>{{ contract.name }}</span>
            <span>{{ formatDate(contract.createdAt) }}</span>
            <span>{{ contract.createdBy }}</span>
            <span>{{ contract.active ? $t('common.active') : $t('common.inactive') }}</span>
          </div>
        </div>
      </div>
    </template>
  </BaseForm>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, computed, onMounted } from 'vue';

// Component imports
import BaseForm from '@/components/common/BaseForm.vue';

// Store imports
import { useCustomerStore } from '@/stores/customer';

// Type imports
import type { Customer } from '@/types/customer';
import type { DialogField } from '@/types/common';

// External imports
import { useI18n } from 'vue-i18n'; // v9.2.0
import { useForm } from 'vee-validate'; // v4.9.0
import { QInput, QBtn } from 'quasar'; // v2.12.0

// Props and emits
const props = defineProps<{
  customer?: Customer;
}>();

const emit = defineEmits<{
  (e: 'submit', customer: Customer): void;
  (e: 'cancel'): void;
  (e: 'validation-error', errors: Record<string, string>): void;
}>();

// Store and i18n setup
const customerStore = useCustomerStore();
const { t } = useI18n();

// Component state
const loading = ref(false);
const formRef = ref<InstanceType<typeof BaseForm> | null>(null);
const formData = ref<Partial<Customer>>({
  name: '',
  code: '',
  isActive: true
});

// Form fields configuration
const formFields = computed<DialogField[]>(() => [
  {
    field: 'name',
    label: t('customer.fields.name'),
    type: 'text',
    required: true,
    validation: [
      {
        pattern: /^[a-zA-Z0-9\s-]{2,100}$/,
        minLength: 2,
        maxLength: 100,
        errorMessage: t('customer.validation.name'),
        customValidator: null,
        minValue: null,
        maxValue: null
      }
    ],
    placeholder: t('customer.placeholders.name'),
    defaultValue: ''
  },
  {
    field: 'code',
    label: t('customer.fields.code'),
    type: 'text',
    required: true,
    validation: [
      {
        pattern: /^[A-Z0-9-]{3,20}$/,
        minLength: 3,
        maxLength: 20,
        errorMessage: t('customer.validation.code'),
        customValidator: null,
        minValue: null,
        maxValue: null
      }
    ],
    placeholder: t('customer.placeholders.code'),
    defaultValue: ''
  }
]);

// Initialize form
onMounted(() => {
  if (props.customer) {
    formData.value = {
      ...props.customer,
      // Exclude contacts and contracts from form data
      contacts: undefined,
      contracts: undefined
    };
  }
});

// Form submission handler
const handleSubmit = async (data: Partial<Customer>) => {
  try {
    loading.value = true;
    
    const customerData = {
      ...data,
      id: props.customer?.id
    } as Customer;

    if (props.customer) {
      await customerStore.updateCustomer(props.customer.id, customerData);
    } else {
      await customerStore.createCustomer(customerData);
    }

    emit('submit', customerData);
  } catch (error) {
    console.error('Customer form submission error:', error);
    // Handle API errors
    handleValidationError({
      form: t('customer.errors.submission')
    });
  } finally {
    loading.value = false;
  }
};

// Validation error handler
const handleValidationError = (errors: Record<string, string>) => {
  emit('validation-error', errors);
};

// Date formatter utility
const formatDate = (date: Date): string => {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short'
  }).format(new Date(date));
};
</script>

<style lang="scss">
.customer-form {
  &__contacts,
  &__contracts {
    margin-top: 2rem;

    h3 {
      font-size: 1.2rem;
      margin-bottom: 1rem;
      color: var(--q-primary);
    }
  }

  &__contacts-list,
  &__contracts-list {
    display: grid;
    gap: 1rem;
    
    .contact-item,
    .contract-item {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: 1rem;
      padding: 0.75rem;
      background: var(--q-grey-2);
      border-radius: 4px;
      
      @media (max-width: 600px) {
        grid-template-columns: 1fr;
      }
    }
  }
}
</style>
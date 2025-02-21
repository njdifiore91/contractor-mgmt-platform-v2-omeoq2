<template>
  <QCard class="user-editor q-pa-md">
    <BaseForm
      ref="formRef"
      :fields="formFields"
      v-model="formData"
      :loading="loading"
      @submit="handleSubmit"
      @error="handleError"
    >
      <template #loading>
        <QSpinner color="primary" size="2em" />
      </template>
      
      <template #error="{ error }">
        <div class="text-negative q-mt-sm">{{ error }}</div>
      </template>
    </BaseForm>
  </QCard>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, computed } from 'vue';

// Quasar imports - v2.12.0
import { QCard, QSpinner } from 'quasar';

// Internal imports
import BaseForm from '../common/BaseForm.vue';
import { User } from '@/types/admin';
import { createUser, updateUser } from '@/services/api/admin';
import { validateEmail } from '@/utils/validation';

// Props and emits
const props = defineProps<{
  user: User | null;
}>();

const emit = defineEmits<{
  (e: 'success', user: User): void;
  (e: 'error', message: string): void;
}>();

// Component state
const formRef = ref<InstanceType<typeof BaseForm> | null>(null);
const loading = ref(false);
const formData = ref<Partial<User>>({
  firstName: '',
  lastName: '',
  email: '',
  roles: [],
  receiveEmails: false,
  isActive: true
});

// Form fields configuration
const formFields = computed(() => [
  {
    field: 'firstName',
    label: 'First Name',
    type: 'text',
    required: true,
    validation: [{
      pattern: /^[a-zA-Z\s-']{2,50}$/,
      minLength: 2,
      maxLength: 50,
      errorMessage: 'First name must be between 2 and 50 characters'
    }],
    placeholder: 'Enter first name',
    defaultValue: ''
  },
  {
    field: 'lastName',
    label: 'Last Name',
    type: 'text',
    required: true,
    validation: [{
      pattern: /^[a-zA-Z\s-']{2,50}$/,
      minLength: 2,
      maxLength: 50,
      errorMessage: 'Last name must be between 2 and 50 characters'
    }],
    placeholder: 'Enter last name',
    defaultValue: ''
  },
  {
    field: 'email',
    label: 'Email',
    type: 'text',
    required: true,
    validation: [{
      customValidator: (value: string) => validateEmail(value).isValid,
      errorMessage: 'Please enter a valid email address'
    }],
    placeholder: 'Enter email address',
    defaultValue: ''
  },
  {
    field: 'roles',
    label: 'Roles',
    type: 'multiselect',
    required: true,
    validation: [{
      minLength: 1,
      errorMessage: 'At least one role must be selected'
    }],
    placeholder: 'Select roles',
    defaultValue: []
  },
  {
    field: 'receiveEmails',
    label: 'Receive Emails',
    type: 'boolean',
    required: false,
    validation: [],
    placeholder: '',
    defaultValue: false
  }
]);

// Initialize form with user data if editing
const initializeForm = () => {
  if (props.user) {
    formData.value = {
      firstName: props.user.firstName,
      lastName: props.user.lastName,
      email: props.user.email,
      roles: [...props.user.roles],
      receiveEmails: props.user.receiveEmails,
      isActive: props.user.isActive
    };
  }
};

// Handle form submission
const handleSubmit = async (data: Partial<User>) => {
  try {
    loading.value = true;

    const userData = {
      ...data,
      isActive: true,
      emailConfirmed: false
    };

    const result = props.user
      ? await updateUser(props.user.id, userData)
      : await createUser(userData as Omit<User, 'id'>);

    emit('success', result);
    
    if (!props.user) {
      formRef.value?.reset();
    }
  } catch (error) {
    if (error instanceof Error) {
      emit('error', error.message);
    } else {
      emit('error', 'An unexpected error occurred');
    }
  } finally {
    loading.value = false;
  }
};

// Handle form validation errors
const handleError = (errors: string[]) => {
  emit('error', errors.join(', '));
};

// Initialize form on component mount
initializeForm();
</script>

<style lang="scss">
.user-editor {
  max-width: 600px;
  margin: 0 auto;

  .q-field {
    margin-bottom: 1rem;
  }

  .q-card {
    box-shadow: 0 1px 5px rgba(0, 0, 0, 0.2);
  }
}
</style>
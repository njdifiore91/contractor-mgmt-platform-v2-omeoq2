<template>
  <QForm
    ref="formRef"
    class="base-form"
    @submit="handleSubmit"
    :aria-label="$t('form.ariaLabel')"
  >
    <div class="base-form__fields">
      <div
        v-for="field in fields"
        :key="field.field"
        class="base-form__field"
        :class="{ 'base-form__field--mobile': $q.platform.is.mobile && field.mobileConfig }"
      >
        <QInput
          v-model="formData[field.field]"
          :label="field.label"
          :type="field.type"
          :required="field.required"
          :aria-label="field.ariaLabel || field.label"
          :error="!!validationErrors[field.field]"
          :error-message="validationErrors[field.field]"
          :disable="loading"
          :placeholder="field.placeholder"
          :hint="field.hint"
          :autofocus="field.autofocus"
          :class="{
            'base-form__input--mobile': $q.platform.is.mobile && field.mobileConfig,
            'base-form__input--error': !!validationErrors[field.field]
          }"
          @update:model-value="(val) => handleFieldUpdate(field.field, val)"
          @blur="validateField(field.field)"
        />
      </div>
    </div>

    <div class="base-form__actions">
      <QBtn
        type="submit"
        :loading="loading"
        :disable="hasErrors"
        color="primary"
        :label="$t('form.submit')"
      />
      <QBtn
        type="button"
        flat
        :disable="loading"
        :label="$t('form.reset')"
        @click="resetForm"
      />
    </div>
  </QForm>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, onMounted, watch } from 'vue';

// Quasar imports - v2.12.0
import { QForm, QInput, QBtn } from 'quasar';

// Utility imports
import { debounce } from 'lodash'; // v4.17.21
import { useI18n } from 'vue-i18n'; // v9.2.0
import { validateFormField } from '@/utils/validation';
import type { DialogField } from '@/types/common';

// Props and emits definition
const props = defineProps<{
  fields: DialogField[];
  modelValue: Record<string, any>;
  loading?: boolean;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: Record<string, any>): void;
  (e: 'submit', value: Record<string, any>): void;
  (e: 'error', value: Record<string, string[]>): void;
}>();

// Component state
const { t } = useI18n();
const formRef = ref<typeof QForm | null>(null);
const formData = ref<Record<string, any>>({});
const validationErrors = ref<Record<string, string>>({});
const hasErrors = ref(false);

// Initialize form data
onMounted(() => {
  formData.value = { ...props.modelValue };
});

// Watch for external model updates
watch(
  () => props.modelValue,
  (newValue) => {
    formData.value = { ...newValue };
  },
  { deep: true }
);

// Debounced validation function
const debouncedValidateField = debounce(async (fieldName: string, value: any) => {
  const field = props.fields.find(f => f.field === fieldName);
  if (!field) return;

  const result = await validateFormField(value, field);
  validationErrors.value[fieldName] = result.errorMessage;
  hasErrors.value = Object.values(validationErrors.value).some(error => !!error);

  // Announce validation errors for screen readers
  if (result.errorMessage) {
    const announcement = t('form.validationError', { 
      field: field.label, 
      error: result.errorMessage 
    });
    announceValidationError(announcement);
  }
}, 300);

// Field validation handler
const validateField = async (fieldName: string) => {
  await debouncedValidateField(fieldName, formData.value[fieldName]);
};

// Form-wide validation
const validateForm = async (): Promise<boolean> => {
  const validationPromises = props.fields.map(field => 
    validateField(field.field)
  );
  
  await Promise.all(validationPromises);
  
  if (hasErrors.value) {
    emit('error', Object.values(validationErrors.value).filter(Boolean));
    return false;
  }
  
  return true;
};

// Form submission handler
const handleSubmit = async (event: Event) => {
  event.preventDefault();
  
  if (props.loading) return;
  
  const isValid = await validateForm();
  if (!isValid) return;

  // Sanitize and emit form data
  const sanitizedData = { ...formData.value };
  emit('submit', sanitizedData);
};

// Field update handler
const handleFieldUpdate = (fieldName: string, value: any) => {
  formData.value[fieldName] = value;
  emit('update:modelValue', formData.value);
  validateField(fieldName);
};

// Form reset handler
const resetForm = () => {
  formData.value = {};
  validationErrors.value = {};
  hasErrors.value = false;
  emit('update:modelValue', {});
  announceValidationError(t('form.reset'));
};

// Accessibility helper
const announceValidationError = (message: string) => {
  const announcement = document.createElement('div');
  announcement.setAttribute('role', 'alert');
  announcement.setAttribute('aria-live', 'polite');
  announcement.textContent = message;
  document.body.appendChild(announcement);
  setTimeout(() => document.body.removeChild(announcement), 1000);
};
</script>

<style lang="scss">
.base-form {
  &__fields {
    display: grid;
    gap: 1rem;
    margin-bottom: 1.5rem;
  }

  &__field {
    width: 100%;

    &--mobile {
      @media (max-width: 600px) {
        font-size: 16px; // Prevent iOS zoom
      }
    }
  }

  &__input {
    &--mobile {
      @media (max-width: 600px) {
        .q-field__native {
          font-size: 16px;
        }
      }
    }

    &--error {
      animation: shake 0.5s cubic-bezier(0.36, 0.07, 0.19, 0.97) both;
    }
  }

  &__actions {
    display: flex;
    gap: 1rem;
    justify-content: flex-end;
  }
}

@keyframes shake {
  10%, 90% { transform: translateX(-1px); }
  20%, 80% { transform: translateX(2px); }
  30%, 50%, 70% { transform: translateX(-4px); }
  40%, 60% { transform: translateX(4px); }
}
</style>
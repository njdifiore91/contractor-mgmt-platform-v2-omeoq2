<template>
  <BaseForm
    class="class-change-form"
    @submit="handleSubmit"
    @cancel="$emit('cancel')"
    :loading="isSubmitting"
  >
    <div class="class-change-form__content">
      <!-- Current Classification Display -->
      <div class="class-change-form__current">
        <QInput
          v-model="formData.currentClassification"
          label="Current Classification"
          readonly
          disable
          stack-label
        />
      </div>

      <!-- New Classification Selection -->
      <QSelect
        v-model="formData.newClassification"
        :options="classificationOptions"
        label="New Classification"
        :error="!!validationErrors.newClassification"
        :error-message="validationErrors.newClassification"
        emit-value
        map-options
        required
        :disable="isSubmitting"
        aria-required="true"
      />

      <!-- Effective Date Selection -->
      <QInput
        v-model="formData.effectiveDate"
        type="date"
        label="Effective Date"
        :min="minEffectiveDate"
        :error="!!validationErrors.effectiveDate"
        :error-message="validationErrors.effectiveDate"
        required
        :disable="isSubmitting"
        aria-required="true"
      />

      <!-- Reason for Change -->
      <QInput
        v-model="formData.reason"
        type="textarea"
        label="Reason for Change"
        :error="!!validationErrors.reason"
        :error-message="validationErrors.reason"
        rows="4"
        required
        :disable="isSubmitting"
        aria-required="true"
      />
    </div>

    <!-- Form Actions -->
    <template #actions>
      <QBtn
        type="submit"
        color="primary"
        :loading="isSubmitting"
        :disable="!isValid || isSubmitting"
        label="Submit Class Change"
      >
        <template #loading>
          <QSpinner color="white" />
        </template>
      </QBtn>
    </template>
  </BaseForm>
</template>

<script setup lang="ts">
// Vue imports - v3.2.0
import { ref, computed, watch, onMounted } from 'vue';

// Store imports
import { useInspectorStore } from '../../stores/inspector';

// Component imports
import { BaseForm } from '../common/BaseForm';

// UI Component imports - Quasar v2.0.0
import { QSelect, QInput, QBtn, QSpinner } from 'quasar';

// Notification utility - VueUse v9.0.0
import { useNotification } from '@vueuse/core';

// Types
import { Classification } from '../../types/inspector';

// Props
const props = defineProps<{
  inspectorId: number;
}>();

// Emits
const emit = defineEmits<{
  (e: 'success'): void;
  (e: 'error', message: string): void;
  (e: 'cancel'): void;
}>();

// Store
const inspectorStore = useInspectorStore();
const { selectedInspector, mobilizeInspector, isLoading } = inspectorStore;

// Notifications
const notification = useNotification();

// Form Data
const formData = ref({
  currentClassification: null as Classification | null,
  newClassification: null as Classification | null,
  effectiveDate: null as string | null,
  reason: ''
});

// Validation Errors
const validationErrors = ref({
  newClassification: '',
  effectiveDate: '',
  reason: ''
});

// Loading State
const isSubmitting = ref(false);

// Computed Properties
const classificationOptions = computed(() => [
  { label: 'Level 1', value: Classification.LEVEL_1 },
  { label: 'Level 2', value: Classification.LEVEL_2 },
  { label: 'Level 3', value: Classification.LEVEL_3 },
  { label: 'Senior', value: Classification.SENIOR },
  { label: 'Expert', value: Classification.EXPERT }
]);

const minEffectiveDate = computed(() => {
  const today = new Date();
  return today.toISOString().split('T')[0];
});

const isValid = computed(() => {
  return (
    formData.value.newClassification !== null &&
    formData.value.effectiveDate !== null &&
    formData.value.reason.length >= 10 &&
    Object.values(validationErrors.value).every(error => !error)
  );
});

// Lifecycle Hooks
onMounted(async () => {
  if (selectedInspector.value) {
    formData.value.currentClassification = selectedInspector.value.classification;
  }
});

// Methods
const validateForm = (): boolean => {
  validationErrors.value = {
    newClassification: '',
    effectiveDate: '',
    reason: ''
  };

  if (!formData.value.newClassification) {
    validationErrors.value.newClassification = 'New classification is required';
  }

  if (!formData.value.effectiveDate) {
    validationErrors.value.effectiveDate = 'Effective date is required';
  } else {
    const selectedDate = new Date(formData.value.effectiveDate);
    const today = new Date();
    if (selectedDate < today) {
      validationErrors.value.effectiveDate = 'Effective date must be in the future';
    }
  }

  if (!formData.value.reason) {
    validationErrors.value.reason = 'Reason for change is required';
  } else if (formData.value.reason.length < 10) {
    validationErrors.value.reason = 'Reason must be at least 10 characters';
  }

  return Object.values(validationErrors.value).every(error => !error);
};

const handleSubmit = async () => {
  if (!validateForm()) return;

  try {
    isSubmitting.value = true;

    const mobilizationDetails = {
      employeeName: `${selectedInspector.value?.firstName} ${selectedInspector.value?.lastName}`,
      classification: formData.value.newClassification!,
      mobDate: new Date(formData.value.effectiveDate!),
      // Preserve existing mobilization details
      ...selectedInspector.value?.mobilizationDate && {
        customer: selectedInspector.value.assignedCustomers[0],
        contract: selectedInspector.value.assignedContracts[0],
        department: selectedInspector.value.department,
        function: selectedInspector.value.function
      }
    };

    const result = await mobilizeInspector(props.inspectorId, mobilizationDetails);

    if (result.success) {
      notification.success({
        title: 'Success',
        message: 'Inspector classification changed successfully'
      });
      emit('success');
    } else {
      throw new Error(result.error);
    }
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : 'Failed to change classification';
    notification.error({
      title: 'Error',
      message: errorMessage
    });
    emit('error', errorMessage);
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<style lang="scss" scoped>
.class-change-form {
  &__content {
    display: grid;
    gap: 1.5rem;
    margin-bottom: 2rem;
  }

  &__current {
    opacity: 0.7;
  }
}
</style>
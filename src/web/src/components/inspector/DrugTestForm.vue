<template>
  <BaseForm
    ref="formRef"
    class="drug-test-form"
    :loading="loading"
    @submit="handleSubmit"
    @cancel="$emit('cancel')"
  >
    <div class="drug-test-form__content">
      <!-- Test Date -->
      <QDatePicker
        v-model="formData.testDate"
        class="drug-test-form__field"
        :rules="[val => !!val || 'Test date is required']"
        label="Test Date"
        filled
        :max="new Date()"
        :disable="loading"
        emit-immediately
      />

      <!-- Test Type -->
      <QSelect
        v-model="formData.testType"
        class="drug-test-form__field"
        :options="testTypes"
        :rules="[val => !!val || 'Test type is required']"
        label="Test Type"
        filled
        :disable="loading"
        emit-value
        map-options
      />

      <!-- Frequency -->
      <QSelect
        v-model="formData.frequency"
        class="drug-test-form__field"
        :options="frequencies"
        :rules="[val => !!val || 'Frequency is required']"
        label="Frequency"
        filled
        :disable="loading"
        emit-value
        map-options
      />

      <!-- Result -->
      <QSelect
        v-model="formData.result"
        class="drug-test-form__field"
        :options="results"
        :rules="[val => !!val || 'Result is required']"
        label="Result"
        filled
        :disable="loading"
        emit-value
        map-options
      />

      <!-- Company -->
      <QInput
        v-model="formData.company"
        class="drug-test-form__field"
        label="Company"
        :rules="[val => val.length <= 100 || 'Company name must be 100 characters or less']"
        filled
        :disable="loading"
      />

      <!-- Comment -->
      <QInput
        v-model="formData.comment"
        class="drug-test-form__field"
        type="textarea"
        label="Comment"
        :rules="[val => val.length <= 500 || 'Comment must be 500 characters or less']"
        filled
        :disable="loading"
        autogrow
      />
    </div>

    <div v-if="error" class="drug-test-form__error" role="alert">
      {{ error }}
    </div>

    <div class="drug-test-form__actions">
      <QSpinner v-if="loading" color="primary" size="2em" />
      <template v-else>
        <q-btn
          type="submit"
          color="primary"
          :disable="!isValid || !hasPermission"
          label="Save"
        />
        <q-btn
          flat
          color="grey"
          label="Cancel"
          @click="$emit('cancel')"
        />
      </template>
    </div>
  </BaseForm>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { QDatePicker, QSelect, QInput, QSpinner } from 'quasar'; // ^2.0.0
import { DrugTest } from '../../types/inspector';
import { BaseForm } from '../common/BaseForm';
import { usePermissions } from '../../composables/usePermissions';
import { manageDrugTest } from '../../services/api/inspector';

// Props
const props = defineProps<{
  inspectorId: number;
}>();

// Emits
const emit = defineEmits<{
  (e: 'submit', drugTest: DrugTest): void;
  (e: 'cancel'): void;
  (e: 'error', message: string): void;
}>();

// Form data
const formData = ref<Partial<DrugTest>>({
  testDate: null,
  testType: '',
  frequency: '',
  result: '',
  comment: '',
  company: ''
});

// Form state
const loading = ref(false);
const error = ref<string | null>(null);
const formRef = ref<typeof BaseForm | null>(null);

// Options for select fields
const testTypes = ['Random', 'Pre-Employment', 'Post-Accident', 'Reasonable Suspicion'];
const frequencies = ['Annual', 'Semi-Annual', 'Quarterly', 'Monthly'];
const results = ['Negative', 'Positive', 'Inconclusive', 'Refused'];

// Permissions check
const { checkPermission } = usePermissions();
const hasPermission = computed(() => checkPermission('edit_users'));

// Form validation
const isValid = computed(() => {
  return !!(
    formData.value.testDate &&
    formData.value.testType &&
    formData.value.frequency &&
    formData.value.result
  );
});

// Form submission handler
const handleSubmit = async () => {
  try {
    if (!hasPermission.value) {
      throw new Error('Unauthorized: Missing required permissions');
    }

    if (!isValid.value) {
      throw new Error('Please fill in all required fields');
    }

    loading.value = true;
    error.value = null;

    const drugTestData: DrugTest = {
      inspectorId: props.inspectorId,
      testDate: formData.value.testDate!,
      testType: formData.value.testType!,
      frequency: formData.value.frequency!,
      result: formData.value.result!,
      comment: formData.value.comment || '',
      company: formData.value.company || ''
    };

    const result = await manageDrugTest(props.inspectorId, drugTestData);
    emit('submit', result);
  } catch (err) {
    const errorMessage = err instanceof Error ? err.message : 'Failed to save drug test';
    error.value = errorMessage;
    emit('error', errorMessage);
  } finally {
    loading.value = false;
  }
};
</script>

<style lang="scss" scoped>
.drug-test-form {
  &__content {
    display: grid;
    gap: 1rem;
    margin-bottom: 1.5rem;
  }

  &__field {
    width: 100%;
  }

  &__error {
    color: var(--q-negative);
    margin-bottom: 1rem;
    font-size: 0.875rem;
  }

  &__actions {
    display: flex;
    gap: 1rem;
    justify-content: flex-end;
    align-items: center;
  }
}
</style>
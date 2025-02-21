<template>
  <QCard class="equipment-assignment">
    <QCardSection>
      <h2 class="text-h6">{{ $t('equipment.assignment.title') }}</h2>
      
      <BaseForm
        ref="formRef"
        :fields="formFields"
        v-model="formData"
        :loading="isLoading"
        @submit="handleAssignment"
      />
    </QCardSection>

    <QSpinner v-if="isLoading" size="2em" color="primary" />
  </QCard>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, toRef, computed } from 'vue';

// Quasar imports - v2.12.0
import { QCard, QCardSection, QSpinner } from 'quasar';

// Internal imports
import BaseForm from '../common/BaseForm.vue';
import type { EquipmentAssignment } from '../../types/equipment';
import { usePermissions } from '../../composables/usePermissions';

// External imports - v10.0.0
import { useNotification } from '@vueuse/core';

// Props and emits
const props = defineProps<{
  equipment: Array<{ id: number; model: string; serialNumber: string }>;
  inspectors: Array<{ id: number; firstName: string; lastName: string }>;
}>();

const emit = defineEmits<{
  (e: 'assigned', data: EquipmentAssignment): void;
  (e: 'assignmentError', error: Error): void;
}>();

// Component state
const formRef = ref<InstanceType<typeof BaseForm> | null>(null);
const isLoading = ref(false);
const formData = ref<Partial<EquipmentAssignment>>({});
const { checkPermission } = usePermissions();
const notification = useNotification();

// Form field definitions
const formFields = computed(() => [
  {
    field: 'equipmentId',
    label: 'Equipment',
    type: 'select',
    required: true,
    options: props.equipment.map(item => ({
      label: `${item.model} (${item.serialNumber})`,
      value: item.id
    })),
    validation: [
      {
        validator: (value: number) => !!value,
        message: 'Equipment selection is required'
      }
    ]
  },
  {
    field: 'inspectorId',
    label: 'Inspector',
    type: 'select',
    required: true,
    options: props.inspectors.map(item => ({
      label: `${item.firstName} ${item.lastName}`,
      value: item.id
    })),
    validation: [
      {
        validator: (value: number) => !!value,
        message: 'Inspector selection is required'
      }
    ]
  },
  {
    field: 'condition',
    label: 'Out Condition',
    type: 'select',
    required: true,
    options: [
      { label: 'New', value: 'NEW' },
      { label: 'Good', value: 'GOOD' },
      { label: 'Fair', value: 'FAIR' },
      { label: 'Poor', value: 'POOR' }
    ],
    validation: [
      {
        validator: (value: string) => !!value,
        message: 'Equipment condition is required'
      }
    ]
  },
  {
    field: 'assignmentDate',
    label: 'Assignment Date',
    type: 'date',
    required: true,
    defaultValue: new Date().toISOString().split('T')[0],
    validation: [
      {
        validator: (value: string) => !!value && new Date(value) <= new Date(),
        message: 'Assignment date cannot be in the future'
      }
    ]
  }
]);

// Validation function
const validateAssignmentData = (data: Partial<EquipmentAssignment>): { isValid: boolean; errors: string[] } => {
  const errors: string[] = [];

  if (!data.equipmentId) {
    errors.push('Equipment selection is required');
  }
  if (!data.inspectorId) {
    errors.push('Inspector selection is required');
  }
  if (!data.condition) {
    errors.push('Equipment condition is required');
  }
  if (!data.assignmentDate) {
    errors.push('Assignment date is required');
  } else if (new Date(data.assignmentDate) > new Date()) {
    errors.push('Assignment date cannot be in the future');
  }

  return {
    isValid: errors.length === 0,
    errors
  };
};

// Assignment handler
const handleAssignment = async (assignmentData: Partial<EquipmentAssignment>) => {
  try {
    // Check permissions
    if (!checkPermission('edit_equipment')) {
      throw new Error('Unauthorized: Missing required permission');
    }

    // Validate data
    const validation = validateAssignmentData(assignmentData);
    if (!validation.isValid) {
      throw new Error(validation.errors.join(', '));
    }

    isLoading.value = true;

    // Create assignment data
    const assignment: EquipmentAssignment = {
      equipmentId: assignmentData.equipmentId!,
      inspectorId: assignmentData.inspectorId!,
      condition: assignmentData.condition!,
      assignmentDate: new Date(assignmentData.assignmentDate!)
    };

    // Emit successful assignment
    emit('assigned', assignment);

    // Show success notification
    notification.success({
      title: 'Success',
      message: 'Equipment assigned successfully'
    });

    // Reset form
    formRef.value?.reset();
    formData.value = {};

  } catch (error) {
    // Handle errors
    const errorMessage = error instanceof Error ? error.message : 'Failed to assign equipment';
    notification.error({
      title: 'Error',
      message: errorMessage
    });
    emit('assignmentError', error as Error);

  } finally {
    isLoading.value = false;
  }
};
</script>

<style lang="scss" scoped>
.equipment-assignment {
  max-width: 600px;
  margin: 0 auto;
  
  :deep(.q-card__section) {
    padding: 24px;
  }

  :deep(.q-spinner) {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
  }
}
</style>
<template>
  <div class="equipment-return-page q-pa-md">
    <!-- Page Header -->
    <div class="equipment-return-page__header q-mb-lg">
      <h1 class="text-h5 q-mb-sm">{{ $t('equipment.return.pageTitle') }}</h1>
      <p class="text-subtitle2 text-grey-7">
        {{ $t('equipment.return.pageDescription') }}
      </p>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="equipment-return-page__loading q-pa-xl flex flex-center">
      <q-spinner size="3em" color="primary" />
      <span class="q-ml-sm">{{ $t('common.loading') }}</span>
    </div>

    <!-- Error State -->
    <q-banner v-if="error" class="bg-negative text-white q-mb-md">
      <template v-slot:avatar>
        <q-icon name="error" />
      </template>
      {{ error }}
    </q-banner>

    <!-- Equipment Details -->
    <div v-if="equipment && !isLoading" class="equipment-return-page__details q-mb-lg">
      <q-card flat bordered>
        <q-card-section>
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-6">
              <div class="text-subtitle2">{{ $t('equipment.model') }}</div>
              <div>{{ equipment.model }}</div>
            </div>
            <div class="col-12 col-md-6">
              <div class="text-subtitle2">{{ $t('equipment.serialNumber') }}</div>
              <div>{{ equipment.serialNumber }}</div>
            </div>
            <div class="col-12">
              <div class="text-subtitle2">{{ $t('equipment.description') }}</div>
              <div>{{ equipment.description }}</div>
            </div>
            <div class="col-12 col-md-6">
              <div class="text-subtitle2">{{ $t('equipment.assignedTo') }}</div>
              <div>{{ equipment.assignedToInspectorName }}</div>
            </div>
            <div class="col-12 col-md-6">
              <div class="text-subtitle2">{{ $t('equipment.assignedDate') }}</div>
              <div>{{ formatDate(equipment.assignedDate) }}</div>
            </div>
          </div>
        </q-card-section>
      </q-card>
    </div>

    <!-- Return Form -->
    <EquipmentReturn
      v-if="equipment && !isLoading"
      :equipment-id="equipment.id"
      @completed="handleReturnComplete"
      @error="handleError"
    />

    <!-- Not Found State -->
    <div v-if="!equipment && !isLoading && !error" class="equipment-return-page__not-found q-pa-xl">
      <q-icon name="inventory_2" size="3em" color="grey-5" />
      <div class="text-h6 q-mt-md">{{ $t('equipment.notFound') }}</div>
      <q-btn
        flat
        color="primary"
        :label="$t('common.backToList')"
        @click="router.push('/equipment')"
        class="q-mt-md"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router'; // v4.2.0
import { useQuasar } from 'quasar'; // v2.12.0

// Internal imports
import EquipmentReturn from '../../components/equipment/EquipmentReturn.vue';
import { useEquipmentStore } from '../../stores/equipment';
import type { Equipment, EquipmentReturn as EquipmentReturnType } from '../../types/equipment';

// Component setup
const route = useRoute();
const router = useRouter();
const $q = useQuasar();
const equipmentStore = useEquipmentStore();

// Component state
const isLoading = ref(false);
const error = ref<string | null>(null);
const equipment = ref<Equipment | null>(null);

// Date formatter
const formatDate = (date: Date | null): string => {
  if (!date) return '';
  return new Date(date).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
};

// Load equipment details
const loadEquipmentDetails = async (): Promise<void> => {
  try {
    isLoading.value = true;
    error.value = null;

    const equipmentId = parseInt(route.params.id as string);
    if (isNaN(equipmentId)) {
      throw new Error('Invalid equipment ID');
    }

    // Fetch equipment details from store
    await equipmentStore.fetchEquipmentByCompany(equipmentId);
    equipment.value = equipmentStore.equipmentList.find(e => e.id === equipmentId) || null;

    if (!equipment.value) {
      throw new Error('Equipment not found');
    }

    // Validate equipment is assigned
    if (!equipment.value.isOut) {
      throw new Error('Equipment is not currently assigned');
    }

  } catch (err) {
    error.value = err instanceof Error ? err.message : 'An unknown error occurred';
    $q.notify({
      type: 'negative',
      message: error.value,
      position: 'top',
      timeout: 5000
    });
  } finally {
    isLoading.value = false;
  }
};

// Handle return completion
const handleReturnComplete = async (): Promise<void> => {
  try {
    $q.notify({
      type: 'positive',
      message: 'Equipment return processed successfully',
      position: 'top',
      timeout: 3000
    });
    await router.push('/equipment');
  } catch (err) {
    handleError(err instanceof Error ? err.message : 'Failed to process return');
  }
};

// Handle errors
const handleError = (message: string): void => {
  error.value = message;
  $q.notify({
    type: 'negative',
    message,
    position: 'top',
    timeout: 5000
  });
};

// Initialize component
onMounted(() => {
  loadEquipmentDetails();
});
</script>

<style lang="scss">
.equipment-return-page {
  max-width: 1200px;
  margin: 0 auto;

  &__header {
    h1 {
      margin: 0;
      color: var(--q-primary);
    }
  }

  &__loading,
  &__not-found {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 200px;
    text-align: center;
  }

  &__details {
    .q-card {
      background-color: var(--q-grey-1);
    }
  }
}
</style>
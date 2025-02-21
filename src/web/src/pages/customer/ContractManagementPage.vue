<template>
  <div class="contract-management">
    <!-- Header with actions -->
    <div class="contract-management__header q-mb-md">
      <h1 class="text-h5">{{ $t('contract.management.title') }}</h1>
      <QBtn
        v-if="selectedCustomer"
        color="primary"
        :label="$t('contract.management.addContract')"
        :disable="loading"
        @click="openContractDialog()"
        :aria-label="$t('contract.management.addContractAriaLabel')"
      >
        <template #loading>
          <QSpinner color="white" size="1em" />
        </template>
      </QBtn>
    </div>

    <!-- Customer selection warning -->
    <div v-if="!selectedCustomer" class="contract-management__warning q-pa-md">
      <p class="text-warning">{{ $t('contract.management.selectCustomer') }}</p>
    </div>

    <!-- Contracts table -->
    <BaseTable
      v-else
      :columns="tableColumns"
      :data="customerContracts"
      :loading="loading"
      :error="error"
      row-key="id"
      class="contract-management__table"
    />

    <!-- Contract dialog -->
    <QDialog
      v-model="showDialog"
      persistent
      maximized
      transition-show="slide-up"
      transition-hide="slide-down"
    >
      <ContractForm
        v-model="selectedContract"
        :loading="loading"
        @submit="handleContractSubmit"
        @error="handleError"
      />
    </QDialog>
  </div>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, computed, onMounted, watch } from 'vue';

// Quasar imports - v2.12.0
import { QBtn, useQuasar, QDialog, QSpinner } from 'quasar';

// Internal imports
import BaseTable from '@/components/common/BaseTable.vue';
import ContractForm from '@/components/customer/ContractForm.vue';
import { useCustomerStore } from '@/stores/customer';
import type { Contract } from '@/types/customer';

// Initialize composables
const $q = useQuasar();
const customerStore = useCustomerStore();

// Component state
const showDialog = ref(false);
const selectedContract = ref<Contract | null>(null);
const error = ref<string>('');

// Computed properties
const loading = computed(() => customerStore.loading);
const selectedCustomer = computed(() => customerStore.selectedCustomer);
const customerContracts = computed(() => selectedCustomer.value?.contracts || []);

// Table configuration
const tableColumns = setupTableColumns();

// Methods
function setupTableColumns() {
  return [
    {
      name: 'name',
      label: 'Contract Name',
      field: 'name',
      sortable: true,
      align: 'left'
    },
    {
      name: 'createdAt',
      label: 'Created',
      field: 'createdAt',
      sortable: true,
      format: (val: Date) => new Date(val).toLocaleDateString()
    },
    {
      name: 'createdBy',
      label: 'Created By',
      field: 'createdBy',
      sortable: true
    },
    {
      name: 'active',
      label: 'Active',
      field: 'active',
      sortable: true,
      format: (val: boolean) => val ? 'Yes' : 'No'
    },
    {
      name: 'actions',
      label: 'Actions',
      field: 'actions',
      align: 'right'
    }
  ];
}

function openContractDialog(contract: Contract | null = null) {
  selectedContract.value = contract ? { ...contract } : null;
  showDialog.value = true;
}

async function handleContractSubmit(contractData: Contract) {
  try {
    if (!selectedCustomer.value?.id) {
      throw new Error('No customer selected');
    }

    const customerId = selectedCustomer.value.id;
    
    if (contractData.id) {
      await customerStore.updateContract(
        customerId,
        contractData.id,
        contractData
      );
      $q.notify({
        type: 'positive',
        message: 'Contract updated successfully'
      });
    } else {
      await customerStore.addContract(customerId, contractData);
      $q.notify({
        type: 'positive',
        message: 'Contract created successfully'
      });
    }

    showDialog.value = false;
  } catch (err) {
    handleError([(err as Error).message]);
  }
}

async function deleteContract(contract: Contract) {
  try {
    if (!selectedCustomer.value?.id) {
      throw new Error('No customer selected');
    }

    const confirmed = await $q.dialog({
      title: 'Confirm Deletion',
      message: `Are you sure you want to delete contract "${contract.name}"?`,
      cancel: true,
      persistent: true
    });

    if (confirmed) {
      await customerStore.removeContract(
        selectedCustomer.value.id,
        contract.id
      );
      $q.notify({
        type: 'positive',
        message: 'Contract deleted successfully'
      });
    }
  } catch (err) {
    handleError([(err as Error).message]);
  }
}

function handleError(errors: string[]) {
  error.value = errors.join('. ');
  $q.notify({
    type: 'negative',
    message: error.value
  });
}

// Lifecycle hooks
onMounted(() => {
  if (selectedCustomer.value?.id) {
    customerStore.fetchCustomerById(selectedCustomer.value.id);
  }
});

// Watch for customer changes
watch(
  () => selectedCustomer.value?.id,
  (newId) => {
    if (newId) {
      customerStore.fetchCustomerById(newId);
    }
  }
);
</script>

<style lang="scss">
.contract-management {
  padding: 1rem;

  &__header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
  }

  &__warning {
    background-color: var(--q-warning);
    border-radius: 4px;
    color: white;
    text-align: center;
  }

  &__table {
    height: calc(100vh - 200px);
    
    .q-table__top {
      padding: 8px 16px;
    }
    
    .q-table__bottom {
      min-height: 40px;
    }
  }
}
</style>
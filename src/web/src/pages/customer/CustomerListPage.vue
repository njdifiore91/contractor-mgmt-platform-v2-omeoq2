<template>
  <q-page class="customer-list-page q-pa-md">
    <!-- Page header with title and accessibility -->
    <div class="row items-center justify-between q-mb-md">
      <h1 class="text-h5 q-my-none" id="customerListTitle">Customer List</h1>
      <q-btn
        color="primary"
        icon="add"
        label="New Customer"
        @click="router.push('/customers/new')"
        aria-label="Create new customer"
      />
    </div>

    <!-- Search component -->
    <CustomerSearch
      @search="handleSearch"
      @search-complete="handleSearchComplete"
      @search-error="handleSearchError"
      aria-labelledby="customerListTitle"
    />

    <!-- Error alert -->
    <q-banner
      v-if="error"
      class="bg-negative text-white q-mb-md"
      rounded
      role="alert"
    >
      {{ error }}
      <template v-slot:action>
        <q-btn flat color="white" label="Retry" @click="fetchCustomers" />
      </template>
    </q-banner>

    <!-- Customer table with virtual scrolling -->
    <BaseTable
      ref="customerTable"
      :columns="tableColumns"
      :data="customers"
      :loading="loading"
      :row-key="'id'"
      :virtual-scroll="true"
      @row-click="handleCustomerClick"
      aria-label="Customer list table"
    >
      <!-- Custom loading slot -->
      <template #loading>
        <div class="row justify-center q-pa-md">
          <q-spinner color="primary" size="2em" />
          <span class="q-ml-sm">Loading customers...</span>
        </div>
      </template>

      <!-- No data slot -->
      <template #no-data>
        <div class="row justify-center q-pa-md">
          {{ loading ? 'Loading...' : 'No customers found' }}
        </div>
      </template>
    </BaseTable>
  </q-page>
</template>

<script lang="ts">
// Vue imports - v3.3.0
import { ref, computed, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router'; // v4.2.0
import { QPage, useQuasar } from 'quasar'; // v2.12.0
import debounce from 'lodash/debounce'; // v4.17.21

// Internal imports
import CustomerSearch from '@/components/customer/CustomerSearch.vue';
import BaseTable from '@/components/common/BaseTable.vue';
import { useCustomerStore } from '@/stores/customer';
import { Customer } from '@/types/customer';

export default {
  name: 'CustomerListPage',

  components: {
    CustomerSearch,
    BaseTable,
    QPage
  },

  setup() {
    // Composables
    const router = useRouter();
    const customerStore = useCustomerStore();
    const $q = useQuasar();

    // Refs
    const customerTable = ref(null);
    const error = ref<string | null>(null);
    const searchAbortController = ref<AbortController | null>(null);

    // Computed properties
    const tableColumns = computed(() => [
      {
        name: 'code',
        label: 'Code',
        field: 'code',
        sortable: true,
        align: 'left' as const
      },
      {
        name: 'name',
        label: 'Company Name',
        field: 'name',
        sortable: true,
        align: 'left' as const
      },
      {
        name: 'contacts',
        label: 'Contacts',
        field: (row: Customer) => row.contacts?.length || 0,
        sortable: true,
        align: 'center' as const
      },
      {
        name: 'contracts',
        label: 'Contracts',
        field: (row: Customer) => row.contracts?.length || 0,
        sortable: true,
        align: 'center' as const
      },
      {
        name: 'actions',
        label: 'Actions',
        field: 'actions',
        align: 'right' as const
      }
    ]);

    // Methods
    const fetchCustomers = async () => {
      try {
        error.value = null;
        await customerStore.fetchCustomers();
      } catch (err) {
        error.value = 'Failed to load customers. Please try again.';
        console.error('Error fetching customers:', err);
      }
    };

    const handleSearch = debounce(async (searchParams: { name?: string; code?: string }) => {
      try {
        // Cancel any pending search
        if (searchAbortController.value) {
          searchAbortController.value.abort();
        }
        searchAbortController.value = new AbortController();

        error.value = null;
        await customerStore.searchCustomers(searchParams.name || searchParams.code || '');
      } catch (err) {
        if (err.name !== 'AbortError') {
          error.value = 'Search failed. Please try again.';
          console.error('Search error:', err);
        }
      }
    }, 300);

    const handleSearchComplete = (results: Customer[]) => {
      if (results.length === 0) {
        $q.notify({
          message: 'No customers found matching your search criteria',
          color: 'info',
          position: 'top'
        });
      }
    };

    const handleSearchError = (errorMessage: string) => {
      error.value = errorMessage;
      $q.notify({
        message: errorMessage,
        color: 'negative',
        position: 'top'
      });
    };

    const handleCustomerClick = (customer: Customer) => {
      if (!customer?.id) return;
      router.push(`/customers/${customer.id}`);
    };

    // Lifecycle hooks
    onMounted(async () => {
      await fetchCustomers();
    });

    // Cleanup
    onBeforeUnmount(() => {
      if (searchAbortController.value) {
        searchAbortController.value.abort();
      }
    });

    // Expose to template
    return {
      // State
      customers: computed(() => customerStore.customers),
      loading: computed(() => customerStore.loading),
      error,
      customerTable,
      router,

      // Computed
      tableColumns,

      // Methods
      fetchCustomers,
      handleSearch,
      handleSearchComplete,
      handleSearchError,
      handleCustomerClick
    };
  }
};
</script>

<style lang="scss">
.customer-list-page {
  .q-table {
    height: calc(100vh - 200px);
    min-height: 300px;
  }

  // Accessibility focus styles
  .q-btn:focus-visible,
  .q-table tbody tr:focus-visible {
    outline: 2px solid var(--q-primary);
    outline-offset: 2px;
  }
}
</style>
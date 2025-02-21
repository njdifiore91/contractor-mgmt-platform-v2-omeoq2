<template>
  <div class="customer-search q-pa-md">
    <!-- Search form with accessibility support -->
    <BaseSearch
      :search-fields="searchFields"
      placeholder="Search by customer name or code..."
      @search="handleCustomerSearch"
      aria-label="Customer search form"
    />

    <!-- Loading and error states with aria-live regions -->
    <div 
      v-if="loading" 
      class="text-center q-pa-md"
      role="status"
      aria-live="polite"
    >
      <q-spinner size="2em" color="primary" />
      <span class="q-ml-sm">Searching customers...</span>
    </div>

    <div
      v-if="error"
      class="text-negative q-pa-md"
      role="alert"
      aria-live="assertive"
    >
      {{ error }}
    </div>

    <!-- Results table with virtual scrolling -->
    <BaseTable
      v-if="customers.length > 0"
      :columns="tableColumns"
      :data="customers"
      :loading="loading"
      row-key="id"
      :virtual-scroll-config="{ itemHeight: 48, buffer: 10 }"
      @row-click="handleRowClick"
      aria-label="Customer search results"
    >
      <template #no-data>
        <div 
          class="text-center q-pa-md"
          role="status"
          aria-live="polite"
        >
          No customers found matching your search criteria
        </div>
      </template>
    </BaseTable>
  </div>
</template>

<script lang="ts">
// Vue imports - v3.3.0
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router'; // v4.2.0
import debounce from 'lodash/debounce'; // v4.17.21

// Internal imports
import BaseSearch from '@/components/common/BaseSearch.vue';
import BaseTable from '@/components/common/BaseTable.vue';
import { Customer } from '@/types/customer';
import { searchCustomers } from '@/services/api/customer';

export default {
  name: 'CustomerSearch',

  components: {
    BaseSearch,
    BaseTable
  },

  emits: ['search-complete', 'search-error'],

  setup(props, { emit }) {
    // Router instance
    const router = useRouter();

    // Reactive state
    const customers = ref<Customer[]>([]);
    const loading = ref(false);
    const error = ref<string | null>(null);
    const searchParams = ref<{ name?: string; code?: string }>({});

    // Search fields configuration
    const searchFields = [
      { field: 'name', label: 'Customer Name' },
      { field: 'code', label: 'Customer Code' }
    ];

    // Table columns configuration
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
        name: 'actions',
        label: 'Actions',
        field: 'id',
        align: 'right' as const,
        format: (val: string) => `
          <q-btn
            flat
            round
            dense
            color="primary"
            icon="edit"
            aria-label="Edit customer"
          />`
      }
    ]);

    // Debounced search handler
    const handleCustomerSearch = debounce(async (params: { name?: string; code?: string }) => {
      try {
        loading.value = true;
        error.value = null;
        searchParams.value = params;

        // Validate search parameters
        if (!params.name && !params.code) {
          throw new Error('Please enter a customer name or code to search');
        }

        // Perform search
        const searchTerm = params.name || params.code;
        const results = await searchCustomers(searchTerm);
        customers.value = results;

        // Update URL query parameters
        router.replace({ 
          query: { 
            ...router.currentRoute.value.query,
            search: searchTerm 
          }
        });

        emit('search-complete', results);
      } catch (err) {
        error.value = err instanceof Error ? err.message : 'An error occurred while searching';
        emit('search-error', error.value);
      } finally {
        loading.value = false;
      }
    }, 300);

    // Row click handler
    const handleRowClick = (customer: Customer) => {
      try {
        if (!customer?.id) {
          throw new Error('Invalid customer data');
        }
        router.push(`/customers/${customer.id}`);
      } catch (err) {
        error.value = err instanceof Error ? err.message : 'Navigation error occurred';
      }
    };

    // Initialize search from URL parameters
    const initializeSearch = () => {
      const searchQuery = router.currentRoute.value.query.search as string;
      if (searchQuery) {
        handleCustomerSearch({ name: searchQuery });
      }
    };

    // Lifecycle hooks
    onMounted(() => {
      initializeSearch();
    });

    return {
      // State
      customers,
      loading,
      error,
      searchParams,

      // Computed
      tableColumns,
      searchFields,

      // Methods
      handleCustomerSearch,
      handleRowClick
    };
  }
};
</script>

<style lang="scss" scoped>
.customer-search {
  .q-table {
    height: calc(100vh - 200px);
    min-height: 300px;
  }
}
</style>
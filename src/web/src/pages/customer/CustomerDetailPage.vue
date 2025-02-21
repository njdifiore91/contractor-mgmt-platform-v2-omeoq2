<template>
  <div class="customer-detail-page q-pa-md">
    <!-- Loading state -->
    <div v-if="loading" class="flex justify-center items-center q-pa-xl">
      <QSpinner size="3em" color="primary" />
    </div>

    <!-- Error state -->
    <div v-else-if="error" class="text-negative q-pa-md">
      {{ error }}
    </div>

    <!-- Content -->
    <div v-else class="customer-detail-page__content">
      <div class="text-h5 q-mb-lg">
        {{ customer?.name || $t('customer.new') }}
      </div>

      <!-- Tabs navigation -->
      <QTabs
        v-model="activeTab"
        class="text-primary"
        align="left"
        narrow-indicator
      >
        <QTab name="details" :label="$t('customer.tabs.details')" />
        <QTab name="contacts" :label="$t('customer.tabs.contacts')" />
        <QTab name="contracts" :label="$t('customer.tabs.contracts')" />
      </QTabs>

      <!-- Tab panels -->
      <QTabPanels v-model="activeTab" animated>
        <!-- Details tab -->
        <QTabPanel name="details">
          <CustomerForm
            :customer="customer"
            @submit="handleCustomerUpdate"
            @validation-error="handleValidationError"
          />
        </QTabPanel>

        <!-- Contacts tab -->
        <QTabPanel name="contacts">
          <div class="contacts-section">
            <div class="text-h6 q-mb-md">{{ $t('customer.contacts.title') }}</div>
            <div v-if="customer?.contacts?.length" class="contacts-grid">
              <div 
                v-for="contact in customer.contacts" 
                :key="contact.id"
                class="contact-card q-pa-md q-mb-sm"
              >
                <div class="text-subtitle1">
                  {{ contact.firstName }} {{ contact.lastName }}
                </div>
                <div class="text-caption">{{ contact.jobTitle }}</div>
                <div class="text-caption">
                  {{ new Date(contact.dateCreated).toLocaleDateString() }}
                </div>
              </div>
            </div>
            <div v-else class="text-grey q-pa-md">
              {{ $t('customer.contacts.empty') }}
            </div>
          </div>
        </QTabPanel>

        <!-- Contracts tab -->
        <QTabPanel name="contracts">
          <div class="contracts-section">
            <div class="text-h6 q-mb-md">{{ $t('customer.contracts.title') }}</div>
            <div v-if="customer?.contracts?.length" class="contracts-grid">
              <div 
                v-for="contract in customer.contracts" 
                :key="contract.id"
                class="contract-card q-pa-md q-mb-sm"
              >
                <div class="text-subtitle1">{{ contract.name }}</div>
                <div class="text-caption">
                  {{ new Date(contract.createdAt).toLocaleDateString() }}
                </div>
                <div class="text-caption">
                  {{ $t(`customer.contracts.status.${contract.active}`) }}
                </div>
              </div>
            </div>
            <div v-else class="text-grey q-pa-md">
              {{ $t('customer.contracts.empty') }}
            </div>
          </div>
        </QTabPanel>
      </QTabPanels>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useRoute, onBeforeRouteLeave } from 'vue-router';
import { QTabs, QTab, QTabPanels, QTabPanel, QSpinner } from 'quasar';
import { Customer } from '@/types/customer';
import { useCustomerStore } from '@/stores/customer';
import CustomerForm from '@/components/customer/CustomerForm.vue';

// Component state
const loading = ref(false);
const error = ref<string | null>(null);
const activeTab = ref('details');
const hasUnsavedChanges = ref(false);

// Route and store setup
const route = useRoute();
const customerStore = useCustomerStore();

// Load customer data
const loadCustomer = async () => {
  try {
    loading.value = true;
    error.value = null;
    
    const customerId = parseInt(route.params.id as string);
    if (isNaN(customerId)) {
      throw new Error('Invalid customer ID');
    }
    
    await customerStore.fetchCustomerById(customerId);
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to load customer';
    console.error('Error loading customer:', err);
  } finally {
    loading.value = false;
  }
};

// Handle customer updates
const handleCustomerUpdate = async (updatedCustomer: Customer) => {
  try {
    loading.value = true;
    error.value = null;
    
    await customerStore.updateCustomer(updatedCustomer.id, updatedCustomer);
    hasUnsavedChanges.value = false;
    
    // Refresh customer data
    await loadCustomer();
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to update customer';
    console.error('Error updating customer:', err);
  } finally {
    loading.value = false;
  }
};

// Handle validation errors
const handleValidationError = (errors: Record<string, string>) => {
  console.error('Validation errors:', errors);
  // Additional error handling logic if needed
};

// Navigation guard for unsaved changes
onBeforeRouteLeave((to, from, next) => {
  if (hasUnsavedChanges.value) {
    const answer = window.confirm('You have unsaved changes. Do you want to leave?');
    if (answer) {
      next();
    } else {
      next(false);
    }
  } else {
    next();
  }
});

// Lifecycle hooks
onMounted(() => {
  loadCustomer();
});

onUnmounted(() => {
  // Cleanup
  customerStore.$reset();
});

// Computed property for current customer
const customer = computed(() => customerStore.selectedCustomer);
</script>

<style lang="scss">
.customer-detail-page {
  &__content {
    max-width: 1200px;
    margin: 0 auto;
  }

  .contacts-grid,
  .contracts-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 1rem;
    
    @media (max-width: 600px) {
      grid-template-columns: 1fr;
    }
  }

  .contact-card,
  .contract-card {
    background: var(--q-grey-2);
    border-radius: 8px;
    transition: all 0.3s ease;

    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }
  }

  // Accessibility improvements
  :focus-visible {
    outline: 2px solid var(--q-primary);
    outline-offset: 2px;
  }

  // Mobile responsiveness
  @media (max-width: 600px) {
    .q-tab {
      padding: 0.5rem;
    }
    
    .q-tab-panels {
      margin-top: 1rem;
    }
  }
}
</style>
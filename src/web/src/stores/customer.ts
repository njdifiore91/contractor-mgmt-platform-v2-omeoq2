// External imports
import { defineStore } from 'pinia'; // v2.1.0
import { ref, computed } from 'vue'; // v3.3.0
import { storeToRefs } from 'pinia'; // v2.1.0

// Internal imports
import { Customer, Contact, Contract } from '@/types/customer';
import {
  getAllCustomers,
  getCustomerById,
  searchCustomers,
  createCustomer,
  updateCustomer,
  deleteCustomer,
  getCustomerContacts,
  createCustomerContact,
  updateCustomerContact,
  deleteCustomerContact,
  getCustomerContracts,
  createCustomerContract,
  updateCustomerContract,
  deleteCustomerContract
} from '@/services/api/customer';

// Types for store state
interface CustomerState {
  customers: Customer[];
  selectedCustomer: Customer | null;
  loading: boolean;
  error: Error | null;
  lastUpdated: Date | null;
  searchCache: Map<string, { data: Customer[]; timestamp: number }>;
  searchTerm: string;
}

// Cache expiration time (5 minutes)
const CACHE_EXPIRATION = 5 * 60 * 1000;

export const useCustomerStore = defineStore('customer', () => {
  // State
  const state = ref<CustomerState>({
    customers: [],
    selectedCustomer: null,
    loading: false,
    error: null,
    lastUpdated: null,
    searchCache: new Map(),
    searchTerm: ''
  });

  // Getters
  const filteredCustomers = computed(() => {
    const term = state.value.searchTerm.toLowerCase();
    return state.value.customers.filter(customer => 
      customer.name.toLowerCase().includes(term) || 
      customer.code.toLowerCase().includes(term)
    );
  });

  const sortedCustomers = computed(() => {
    return [...filteredCustomers.value].sort((a, b) => a.name.localeCompare(b.name));
  });

  const activeCustomers = computed(() => {
    return sortedCustomers.value.filter(customer => customer.isActive);
  });

  const customersByContract = computed(() => {
    const grouped: Record<string, Customer[]> = {};
    state.value.customers.forEach(customer => {
      customer.contracts.forEach(contract => {
        if (!grouped[contract.name]) {
          grouped[contract.name] = [];
        }
        grouped[contract.name].push(customer);
      });
    });
    return grouped;
  });

  // Actions
  const setLoading = (value: boolean) => {
    state.value.loading = value;
  };

  const setError = (error: Error | null) => {
    state.value.error = error;
  };

  const fetchCustomers = async () => {
    try {
      setLoading(true);
      setError(null);
      const customers = await getAllCustomers();
      state.value.customers = customers;
      state.value.lastUpdated = new Date();
    } catch (error) {
      setError(error as Error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const fetchCustomerById = async (id: number) => {
    try {
      setLoading(true);
      setError(null);
      const customer = await getCustomerById(id);
      state.value.selectedCustomer = customer;
      return customer;
    } catch (error) {
      setError(error as Error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const searchCustomersWithCache = async (term: string) => {
    if (!term) {
      return state.value.customers;
    }

    const cached = state.value.searchCache.get(term);
    const now = Date.now();

    if (cached && (now - cached.timestamp) < CACHE_EXPIRATION) {
      return cached.data;
    }

    try {
      setLoading(true);
      setError(null);
      const results = await searchCustomers(term);
      state.value.searchCache.set(term, { data: results, timestamp: now });
      return results;
    } catch (error) {
      setError(error as Error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const createNewCustomer = async (customerData: Omit<Customer, 'id'>) => {
    try {
      setLoading(true);
      setError(null);
      const newCustomer = await createCustomer(customerData);
      state.value.customers.push(newCustomer);
      return newCustomer;
    } catch (error) {
      setError(error as Error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const updateExistingCustomer = async (id: number, customerData: Customer) => {
    try {
      setLoading(true);
      setError(null);
      const updatedCustomer = await updateCustomer(id, customerData);
      const index = state.value.customers.findIndex(c => c.id === id);
      if (index !== -1) {
        state.value.customers[index] = updatedCustomer;
      }
      return updatedCustomer;
    } catch (error) {
      setError(error as Error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const removeCustomer = async (id: number) => {
    try {
      setLoading(true);
      setError(null);
      await deleteCustomer(id);
      state.value.customers = state.value.customers.filter(c => c.id !== id);
    } catch (error) {
      setError(error as Error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  // Contact management actions
  const addContact = async (customerId: number, contact: Omit<Contact, 'id'>) => {
    try {
      setLoading(true);
      setError(null);
      const newContact = await createCustomerContact(customerId, contact);
      const customer = state.value.customers.find(c => c.id === customerId);
      if (customer) {
        customer.contacts.push(newContact);
      }
      return newContact;
    } catch (error) {
      setError(error as Error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const updateContact = async (customerId: number, contactId: number, contact: Contact) => {
    try {
      setLoading(true);
      setError(null);
      const updatedContact = await updateCustomerContact(customerId, contactId, contact);
      const customer = state.value.customers.find(c => c.id === customerId);
      if (customer) {
        const index = customer.contacts.findIndex(c => c.id === contactId);
        if (index !== -1) {
          customer.contacts[index] = updatedContact;
        }
      }
      return updatedContact;
    } catch (error) {
      setError(error as Error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const removeContact = async (customerId: number, contactId: number) => {
    try {
      setLoading(true);
      setError(null);
      await deleteCustomerContact(customerId, contactId);
      const customer = state.value.customers.find(c => c.id === customerId);
      if (customer) {
        customer.contacts = customer.contacts.filter(c => c.id !== contactId);
      }
    } catch (error) {
      setError(error as Error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  // Contract management actions
  const addContract = async (customerId: number, contract: Omit<Contract, 'id'>) => {
    try {
      setLoading(true);
      setError(null);
      const newContract = await createCustomerContract(customerId, contract);
      const customer = state.value.customers.find(c => c.id === customerId);
      if (customer) {
        customer.contracts.push(newContract);
      }
      return newContract;
    } catch (error) {
      setError(error as Error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const updateContract = async (customerId: number, contractId: number, contract: Contract) => {
    try {
      setLoading(true);
      setError(null);
      const updatedContract = await updateCustomerContract(customerId, contractId, contract);
      const customer = state.value.customers.find(c => c.id === customerId);
      if (customer) {
        const index = customer.contracts.findIndex(c => c.id === contractId);
        if (index !== -1) {
          customer.contracts[index] = updatedContract;
        }
      }
      return updatedContract;
    } catch (error) {
      setError(error as Error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const removeContract = async (customerId: number, contractId: number) => {
    try {
      setLoading(true);
      setError(null);
      await deleteCustomerContract(customerId, contractId);
      const customer = state.value.customers.find(c => c.id === customerId);
      if (customer) {
        customer.contracts = customer.contracts.filter(c => c.id !== contractId);
      }
    } catch (error) {
      setError(error as Error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  return {
    // State
    customers: computed(() => state.value.customers),
    selectedCustomer: computed(() => state.value.selectedCustomer),
    loading: computed(() => state.value.loading),
    error: computed(() => state.value.error),
    lastUpdated: computed(() => state.value.lastUpdated),

    // Getters
    filteredCustomers,
    sortedCustomers,
    activeCustomers,
    customersByContract,

    // Actions
    fetchCustomers,
    fetchCustomerById,
    searchCustomers: searchCustomersWithCache,
    createCustomer: createNewCustomer,
    updateCustomer: updateExistingCustomer,
    deleteCustomer: removeCustomer,
    addContact,
    updateContact,
    removeContact,
    addContract,
    updateContract,
    removeContract
  };
});
/**
 * @file Customer API service module
 * Provides type-safe HTTP client functions for customer-related operations
 * @version 1.0.0
 */

// External imports - v1.4.0
import axios from 'axios';
import { handleAxiosError } from 'axios-error-handler'; // v1.0.0

// Internal type imports
import { 
  Customer,
  Contact,
  Contract
} from '@/types/customer';

// Base API URL configuration
const API_BASE_URL = '/api/customer';

/**
 * Retrieves all customers from the API
 * @returns Promise resolving to array of customers
 */
export const getAllCustomers = async (): Promise<Customer[]> => {
  try {
    const response = await axios.get<Customer[]>(API_BASE_URL);
    return response.data;
  } catch (error) {
    throw handleAxiosError(error);
  }
};

/**
 * Retrieves a specific customer by ID
 * @param id - Customer ID
 * @returns Promise resolving to customer object
 */
export const getCustomerById = async (id: number): Promise<Customer> => {
  try {
    const response = await axios.get<Customer>(`${API_BASE_URL}/${id}`);
    return response.data;
  } catch (error) {
    throw handleAxiosError(error);
  }
};

/**
 * Searches for customers by name or code
 * @param searchTerm - Search query string
 * @returns Promise resolving to array of matching customers
 */
export const searchCustomers = async (searchTerm: string): Promise<Customer[]> => {
  try {
    const response = await axios.get<Customer[]>(`${API_BASE_URL}/search`, {
      params: { q: searchTerm }
    });
    return response.data;
  } catch (error) {
    throw handleAxiosError(error);
  }
};

/**
 * Creates a new customer
 * @param customer - Customer data object
 * @returns Promise resolving to created customer
 */
export const createCustomer = async (customer: Omit<Customer, 'id'>): Promise<Customer> => {
  try {
    const response = await axios.post<Customer>(API_BASE_URL, customer);
    return response.data;
  } catch (error) {
    throw handleAxiosError(error);
  }
};

/**
 * Updates an existing customer
 * @param id - Customer ID
 * @param customer - Updated customer data
 * @returns Promise resolving to updated customer
 */
export const updateCustomer = async (id: number, customer: Customer): Promise<Customer> => {
  try {
    const response = await axios.put<Customer>(`${API_BASE_URL}/${id}`, customer);
    return response.data;
  } catch (error) {
    throw handleAxiosError(error);
  }
};

/**
 * Deletes a customer
 * @param id - Customer ID
 */
export const deleteCustomer = async (id: number): Promise<void> => {
  try {
    await axios.delete(`${API_BASE_URL}/${id}`);
  } catch (error) {
    throw handleAxiosError(error);
  }
};

// Contact Management Functions

/**
 * Retrieves all contacts for a specific customer
 * @param customerId - Customer ID
 * @returns Promise resolving to array of contacts
 */
export const getCustomerContacts = async (customerId: number): Promise<Contact[]> => {
  try {
    const response = await axios.get<Contact[]>(`${API_BASE_URL}/${customerId}/contacts`);
    return response.data;
  } catch (error) {
    throw handleAxiosError(error);
  }
};

/**
 * Creates a new contact for a customer
 * @param customerId - Customer ID
 * @param contact - Contact data object
 * @returns Promise resolving to created contact
 */
export const createCustomerContact = async (
  customerId: number, 
  contact: Omit<Contact, 'id'>
): Promise<Contact> => {
  try {
    const response = await axios.post<Contact>(
      `${API_BASE_URL}/${customerId}/contacts`,
      contact
    );
    return response.data;
  } catch (error) {
    throw handleAxiosError(error);
  }
};

/**
 * Updates an existing customer contact
 * @param customerId - Customer ID
 * @param contactId - Contact ID
 * @param contact - Updated contact data
 * @returns Promise resolving to updated contact
 */
export const updateCustomerContact = async (
  customerId: number,
  contactId: number,
  contact: Contact
): Promise<Contact> => {
  try {
    const response = await axios.put<Contact>(
      `${API_BASE_URL}/${customerId}/contacts/${contactId}`,
      contact
    );
    return response.data;
  } catch (error) {
    throw handleAxiosError(error);
  }
};

/**
 * Deletes a customer contact
 * @param customerId - Customer ID
 * @param contactId - Contact ID
 */
export const deleteCustomerContact = async (
  customerId: number,
  contactId: number
): Promise<void> => {
  try {
    await axios.delete(`${API_BASE_URL}/${customerId}/contacts/${contactId}`);
  } catch (error) {
    throw handleAxiosError(error);
  }
};

// Contract Management Functions

/**
 * Retrieves all contracts for a specific customer
 * @param customerId - Customer ID
 * @returns Promise resolving to array of contracts
 */
export const getCustomerContracts = async (customerId: number): Promise<Contract[]> => {
  try {
    const response = await axios.get<Contract[]>(`${API_BASE_URL}/${customerId}/contracts`);
    return response.data;
  } catch (error) {
    throw handleAxiosError(error);
  }
};

/**
 * Creates a new contract for a customer
 * @param customerId - Customer ID
 * @param contract - Contract data object
 * @returns Promise resolving to created contract
 */
export const createCustomerContract = async (
  customerId: number,
  contract: Omit<Contract, 'id'>
): Promise<Contract> => {
  try {
    const response = await axios.post<Contract>(
      `${API_BASE_URL}/${customerId}/contracts`,
      contract
    );
    return response.data;
  } catch (error) {
    throw handleAxiosError(error);
  }
};

/**
 * Updates an existing customer contract
 * @param customerId - Customer ID
 * @param contractId - Contract ID
 * @param contract - Updated contract data
 * @returns Promise resolving to updated contract
 */
export const updateCustomerContract = async (
  customerId: number,
  contractId: number,
  contract: Contract
): Promise<Contract> => {
  try {
    const response = await axios.put<Contract>(
      `${API_BASE_URL}/${customerId}/contracts/${contractId}`,
      contract
    );
    return response.data;
  } catch (error) {
    throw handleAxiosError(error);
  }
};

/**
 * Deletes a customer contract
 * @param customerId - Customer ID
 * @param contractId - Contract ID
 */
export const deleteCustomerContract = async (
  customerId: number,
  contractId: number
): Promise<void> => {
  try {
    await axios.delete(`${API_BASE_URL}/${customerId}/contracts/${contractId}`);
  } catch (error) {
    throw handleAxiosError(error);
  }
};
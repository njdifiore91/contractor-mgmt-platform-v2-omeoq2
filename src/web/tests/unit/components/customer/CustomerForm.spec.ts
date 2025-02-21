import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { mount } from '@vue/test-utils';
import { createTestingPinia } from '@pinia/testing';
import CustomerForm from '@/components/customer/CustomerForm.vue';
import { useCustomerStore } from '@/stores/customer';
import type { Customer } from '@/types/customer';

// Helper function to create a mounted component wrapper
const createWrapper = (options = {}, storeOptions = {}) => {
  const pinia = createTestingPinia({
    createSpy: vi.fn,
    initialState: {
      customer: {
        loading: false,
        error: null,
        ...storeOptions
      }
    }
  });

  return mount(CustomerForm, {
    global: {
      plugins: [pinia],
      mocks: {
        $t: (key: string) => key, // i18n mock
      }
    },
    ...options
  });
};

// Mock customer data helper
const mockCustomerData = (overrides = {}): Customer => ({
  id: 1,
  name: 'Test Company',
  code: 'TEST-001',
  isActive: true,
  createdAt: new Date(),
  createdBy: 'test-user',
  contacts: [
    {
      id: 1,
      firstName: 'John',
      lastName: 'Doe',
      jobTitle: 'Manager',
      dateCreated: new Date(),
      middleName: '',
      suffix: '',
      nickname: '',
      isDeceased: false,
      isInactive: false,
      rating: 5,
      birthday: null,
      customerId: 1,
      addresses: [],
      emails: [],
      phoneNumbers: [],
      notes: []
    }
  ],
  contracts: [
    {
      id: 1,
      name: 'Service Contract',
      active: true,
      createdAt: new Date(),
      createdBy: 'test-user',
      customerId: 1,
      endDate: null,
      contractType: 'SERVICE'
    }
  ],
  ...overrides
});

describe('CustomerForm.vue', () => {
  let wrapper: ReturnType<typeof createWrapper>;
  let store: ReturnType<typeof useCustomerStore>;

  beforeEach(() => {
    wrapper = createWrapper();
    store = useCustomerStore();
  });

  afterEach(() => {
    wrapper.unmount();
    vi.clearAllMocks();
  });

  describe('Component Rendering', () => {
    it('renders all required form fields', () => {
      const nameInput = wrapper.find('[data-test="customer-name-input"]');
      const codeInput = wrapper.find('[data-test="customer-code-input"]');
      
      expect(nameInput.exists()).toBe(true);
      expect(codeInput.exists()).toBe(true);
    });

    it('applies proper ARIA attributes for accessibility', () => {
      const form = wrapper.find('form');
      expect(form.attributes('aria-label')).toBe('form.ariaLabel');
      
      const nameInput = wrapper.find('[data-test="customer-name-input"]');
      expect(nameInput.attributes('aria-required')).toBe('true');
    });

    it('displays validation error messages correctly', async () => {
      const nameInput = wrapper.find('[data-test="customer-name-input"]');
      await nameInput.setValue('');
      await nameInput.trigger('blur');
      
      const errorMessage = wrapper.find('.base-form__input--error');
      expect(errorMessage.exists()).toBe(true);
    });

    it('renders in create mode by default', () => {
      expect(wrapper.find('[data-test="form-title"]').text()).toBe('customer.form.create');
      expect(wrapper.find('[data-test="submit-button"]').text()).toBe('form.submit');
    });

    it('renders in edit mode with customer data', async () => {
      const customer = mockCustomerData();
      wrapper = createWrapper({ props: { customer } });
      
      await wrapper.vm.$nextTick();
      
      expect(wrapper.find('[data-test="customer-name-input"]').element.value).toBe(customer.name);
      expect(wrapper.find('[data-test="customer-code-input"]').element.value).toBe(customer.code);
    });
  });

  describe('Form Validation', () => {
    it('validates required name field', async () => {
      const nameInput = wrapper.find('[data-test="customer-name-input"]');
      await nameInput.setValue('');
      await nameInput.trigger('blur');
      
      const errorMessage = wrapper.find('[data-test="name-error"]');
      expect(errorMessage.exists()).toBe(true);
      expect(errorMessage.text()).toBe('validation.required');
    });

    it('validates name field format', async () => {
      const nameInput = wrapper.find('[data-test="customer-name-input"]');
      await nameInput.setValue('a'); // Too short
      await nameInput.trigger('blur');
      
      const errorMessage = wrapper.find('[data-test="name-error"]');
      expect(errorMessage.text()).toBe('customer.validation.name');
    });

    it('validates required code field', async () => {
      const codeInput = wrapper.find('[data-test="customer-code-input"]');
      await codeInput.setValue('');
      await codeInput.trigger('blur');
      
      const errorMessage = wrapper.find('[data-test="code-error"]');
      expect(errorMessage.exists()).toBe(true);
      expect(errorMessage.text()).toBe('validation.required');
    });

    it('validates code field format', async () => {
      const codeInput = wrapper.find('[data-test="customer-code-input"]');
      await codeInput.setValue('invalid-code'); // Invalid format
      await codeInput.trigger('blur');
      
      const errorMessage = wrapper.find('[data-test="code-error"]');
      expect(errorMessage.text()).toBe('customer.validation.code');
    });
  });

  describe('Store Interactions', () => {
    it('calls createCustomer with correct data on form submission', async () => {
      const customerData = {
        name: 'New Company',
        code: 'NEW-001'
      };

      const nameInput = wrapper.find('[data-test="customer-name-input"]');
      const codeInput = wrapper.find('[data-test="customer-code-input"]');
      
      await nameInput.setValue(customerData.name);
      await codeInput.setValue(customerData.code);
      await wrapper.find('form').trigger('submit');

      expect(store.createCustomer).toHaveBeenCalledWith(expect.objectContaining(customerData));
    });

    it('calls updateCustomer with correct data when editing', async () => {
      const customer = mockCustomerData();
      wrapper = createWrapper({ props: { customer } });
      
      const updatedData = { ...customer, name: 'Updated Company' };
      const nameInput = wrapper.find('[data-test="customer-name-input"]');
      
      await nameInput.setValue(updatedData.name);
      await wrapper.find('form').trigger('submit');

      expect(store.updateCustomer).toHaveBeenCalledWith(customer.id, expect.objectContaining(updatedData));
    });

    it('handles store errors properly', async () => {
      const error = new Error('API Error');
      store.createCustomer.mockRejectedValue(error);

      await wrapper.find('form').trigger('submit');
      
      const errorMessage = wrapper.find('[data-test="form-error"]');
      expect(errorMessage.exists()).toBe(true);
      expect(errorMessage.text()).toBe('customer.errors.submission');
    });
  });

  describe('Event Handling', () => {
    it('emits submit event on successful creation', async () => {
      const customerData = mockCustomerData();
      store.createCustomer.mockResolvedValue(customerData);

      await wrapper.find('form').trigger('submit');
      
      expect(wrapper.emitted('submit')).toBeTruthy();
      expect(wrapper.emitted('submit')[0]).toEqual([customerData]);
    });

    it('emits cancel event when cancel button is clicked', async () => {
      await wrapper.find('[data-test="cancel-button"]').trigger('click');
      expect(wrapper.emitted('cancel')).toBeTruthy();
    });

    it('emits validation-error event on validation failures', async () => {
      const nameInput = wrapper.find('[data-test="customer-name-input"]');
      await nameInput.setValue('');
      await wrapper.find('form').trigger('submit');
      
      expect(wrapper.emitted('validation-error')).toBeTruthy();
    });
  });

  describe('Error Scenarios', () => {
    it('handles network errors gracefully', async () => {
      store.createCustomer.mockRejectedValue(new Error('Network Error'));
      await wrapper.find('form').trigger('submit');
      
      const errorMessage = wrapper.find('[data-test="form-error"]');
      expect(errorMessage.text()).toBe('customer.errors.submission');
    });

    it('displays field-specific validation errors', async () => {
      const nameInput = wrapper.find('[data-test="customer-name-input"]');
      const codeInput = wrapper.find('[data-test="customer-code-input"]');
      
      await nameInput.setValue('a');
      await codeInput.setValue('invalid');
      await wrapper.find('form').trigger('submit');
      
      const nameError = wrapper.find('[data-test="name-error"]');
      const codeError = wrapper.find('[data-test="code-error"]');
      
      expect(nameError.exists()).toBe(true);
      expect(codeError.exists()).toBe(true);
    });

    it('clears error messages when form is reset', async () => {
      const nameInput = wrapper.find('[data-test="customer-name-input"]');
      await nameInput.setValue('a');
      await nameInput.trigger('blur');
      
      await wrapper.find('[data-test="reset-button"]').trigger('click');
      
      const errorMessage = wrapper.find('.base-form__input--error');
      expect(errorMessage.exists()).toBe(false);
    });
  });
});
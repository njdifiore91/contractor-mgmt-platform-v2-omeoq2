import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { mount, VueWrapper } from '@vue/test-utils';
import { axe, toHaveNoViolations } from '@axe-core/vue'; // v4.7.0
import BaseForm from '@/components/common/BaseForm.vue';
import type { DialogField } from '@/types/common';

// Add axe accessibility matcher
expect.extend(toHaveNoViolations);

describe('BaseForm.vue', () => {
  let wrapper: VueWrapper<any>;
  let mockFields: DialogField[];

  // Mock i18n
  const mockT = vi.fn((key: string) => key);
  const mockI18n = {
    t: mockT,
    global: {
      t: mockT
    }
  };

  // Create mock fields for testing
  const createMockFields = (): DialogField[] => [
    {
      field: 'firstName',
      label: 'First Name',
      type: 'text',
      required: true,
      validation: [{
        pattern: /^[a-zA-Z\s]{2,}$/,
        minLength: 2,
        maxLength: 50,
        minValue: null,
        maxValue: null,
        customValidator: null,
        errorMessage: 'validation.firstName.invalid'
      }],
      placeholder: 'Enter first name',
      defaultValue: ''
    },
    {
      field: 'email',
      label: 'Email',
      type: 'text',
      required: true,
      validation: [{
        pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
        minLength: 5,
        maxLength: 100,
        minValue: null,
        maxValue: null,
        customValidator: null,
        errorMessage: 'validation.email.invalid'
      }],
      placeholder: 'Enter email address',
      defaultValue: ''
    }
  ];

  // Helper function to create wrapper with props
  const createWrapper = (props = {}) => {
    return mount(BaseForm, {
      props: {
        fields: mockFields,
        modelValue: {},
        loading: false,
        ...props
      },
      global: {
        mocks: {
          $t: mockT,
          $q: {
            platform: {
              is: {
                mobile: false
              }
            }
          }
        }
      }
    });
  };

  beforeEach(() => {
    mockFields = createMockFields();
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.restoreAllMocks();
    if (wrapper) {
      wrapper.unmount();
    }
  });

  describe('Component Mounting', () => {
    it('mounts successfully with required props', () => {
      wrapper = createWrapper();
      expect(wrapper.exists()).toBe(true);
      expect(wrapper.find('form').exists()).toBe(true);
    });

    it('renders all fields from props', () => {
      wrapper = createWrapper();
      const fields = wrapper.findAll('.base-form__field');
      expect(fields).toHaveLength(mockFields.length);
    });

    it('applies correct accessibility attributes', async () => {
      wrapper = createWrapper();
      const results = await axe(wrapper.element);
      expect(results).toHaveNoViolations();
    });
  });

  describe('Form Validation', () => {
    it('validates required fields with proper error messages', async () => {
      wrapper = createWrapper();
      await wrapper.find('form').trigger('submit');
      await wrapper.vm.$nextTick();

      const errorMessages = wrapper.findAll('.q-field__messages');
      expect(errorMessages.length).toBeGreaterThan(0);
      expect(wrapper.emitted('error')).toBeTruthy();
    });

    it('validates field formats correctly', async () => {
      wrapper = createWrapper();
      
      // Test invalid email
      const emailInput = wrapper.findComponent({ ref: 'email' });
      await emailInput.setValue('invalid-email');
      await emailInput.trigger('blur');
      await wrapper.vm.$nextTick();
      
      expect(wrapper.vm.validationErrors.email).toBeTruthy();

      // Test valid email
      await emailInput.setValue('valid@email.com');
      await emailInput.trigger('blur');
      await wrapper.vm.$nextTick();
      
      expect(wrapper.vm.validationErrors.email).toBeFalsy();
    });

    it('handles async validation with debounce', async () => {
      wrapper = createWrapper();
      const input = wrapper.findComponent({ ref: 'firstName' });
      
      await input.setValue('A');
      await vi.advanceTimersByTime(200);
      expect(wrapper.vm.validationErrors.firstName).toBeFalsy();
      
      await vi.advanceTimersByTime(100);
      expect(wrapper.vm.validationErrors.firstName).toBeTruthy();
    });
  });

  describe('Field Updates', () => {
    it('emits update events with correct payload', async () => {
      wrapper = createWrapper();
      const input = wrapper.findComponent({ ref: 'firstName' });
      
      await input.setValue('John');
      expect(wrapper.emitted('update:modelValue')).toBeTruthy();
      expect(wrapper.emitted('update:modelValue')[0][0]).toEqual({
        firstName: 'John'
      });
    });

    it('maintains field state during updates', async () => {
      const initialValue = { firstName: 'John' };
      wrapper = createWrapper({ modelValue: initialValue });
      
      const input = wrapper.findComponent({ ref: 'firstName' });
      expect(input.element.value).toBe('John');
      
      await wrapper.setProps({ modelValue: { firstName: 'Jane' } });
      expect(input.element.value).toBe('Jane');
    });
  });

  describe('Mobile Responsiveness', () => {
    it('applies mobile-specific classes when on mobile', async () => {
      wrapper = createWrapper({
        global: {
          mocks: {
            $q: {
              platform: {
                is: {
                  mobile: true
                }
              }
            }
          }
        }
      });

      const mobileFields = wrapper.findAll('.base-form__field--mobile');
      expect(mobileFields.length).toBeGreaterThan(0);
    });
  });

  describe('Security Features', () => {
    it('sanitizes input data', async () => {
      wrapper = createWrapper();
      const input = wrapper.findComponent({ ref: 'firstName' });
      
      await input.setValue('<script>alert("xss")</script>John');
      expect(wrapper.vm.formData.firstName).not.toContain('<script>');
    });

    it('prevents form submission during loading state', async () => {
      wrapper = createWrapper({ loading: true });
      await wrapper.find('form').trigger('submit');
      expect(wrapper.emitted('submit')).toBeFalsy();
    });
  });

  describe('Form Reset', () => {
    it('resets form to initial state', async () => {
      wrapper = createWrapper({
        modelValue: { firstName: 'John', email: 'john@example.com' }
      });
      
      const resetButton = wrapper.find('button[type="button"]');
      await resetButton.trigger('click');
      
      expect(wrapper.vm.formData).toEqual({});
      expect(wrapper.vm.validationErrors).toEqual({});
      expect(wrapper.emitted('update:modelValue')[0][0]).toEqual({});
    });
  });
});
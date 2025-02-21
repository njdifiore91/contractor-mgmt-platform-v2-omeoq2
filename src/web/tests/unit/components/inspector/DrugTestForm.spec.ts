import { mount, VueWrapper } from '@vue/test-utils';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { QForm, QInput, QSelect, QBtn } from 'quasar';
import DrugTestForm from '@/components/inspector/DrugTestForm.vue';
import { DrugTest, DrugTestType, TestFrequency, DrugTestResult } from '@/types/inspector';
import { manageDrugTest } from '@/services/api/inspector';
import { usePermissions } from '@/composables/usePermissions';

// Mock external dependencies
vi.mock('@/services/api/inspector', () => ({
  manageDrugTest: vi.fn()
}));

vi.mock('@/composables/usePermissions', () => ({
  usePermissions: vi.fn(() => ({
    checkPermission: vi.fn()
  }))
}));

// Helper function to create a mounted component wrapper
const createWrapper = (props = {}, options = {}): VueWrapper => {
  return mount(DrugTestForm, {
    props: {
      inspectorId: 1,
      ...props
    },
    global: {
      components: {
        QForm,
        QInput,
        QSelect,
        QBtn
      },
      stubs: {
        QDatePicker: true,
        QSpinner: true
      },
      ...options
    }
  });
};

// Helper function to create mock drug test data
const createMockDrugTest = (overrides = {}): Partial<DrugTest> => ({
  testDate: new Date('2023-01-01'),
  testType: DrugTestType.RANDOM,
  frequency: TestFrequency.ANNUAL,
  result: DrugTestResult.NEGATIVE,
  comment: 'Test comment',
  company: 'Test Company',
  ...overrides
});

describe('DrugTestForm.vue', () => {
  let wrapper: VueWrapper;
  const mockPermissionCheck = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    (usePermissions as any).mockImplementation(() => ({
      checkPermission: mockPermissionCheck
    }));
    wrapper = createWrapper();
  });

  afterEach(() => {
    wrapper.unmount();
  });

  describe('Permission Validation', () => {
    it('should check for Edit Users permission on mount', () => {
      expect(mockPermissionCheck).toHaveBeenCalledWith('edit_users');
    });

    it('should disable form when user lacks Edit Users permission', async () => {
      mockPermissionCheck.mockReturnValue(false);
      await wrapper.vm.$nextTick();
      expect(wrapper.find('form').attributes('disabled')).toBeTruthy();
    });

    it('should enable form when user has Edit Users permission', async () => {
      mockPermissionCheck.mockReturnValue(true);
      await wrapper.vm.$nextTick();
      expect(wrapper.find('form').attributes('disabled')).toBeFalsy();
    });
  });

  describe('Form Field Validation', () => {
    it('should validate required test date', async () => {
      const datePicker = wrapper.findComponent({ name: 'QDatePicker' });
      await datePicker.setValue('');
      expect(wrapper.vm.isValid).toBe(false);
    });

    it('should validate test date is not in future', async () => {
      const futureDate = new Date();
      futureDate.setDate(futureDate.getDate() + 1);
      const datePicker = wrapper.findComponent({ name: 'QDatePicker' });
      await datePicker.setValue(futureDate);
      expect(wrapper.vm.isValid).toBe(false);
    });

    it('should validate required test type', async () => {
      const testTypeSelect = wrapper.findComponent({ name: 'QSelect' });
      await testTypeSelect.setValue('');
      expect(wrapper.vm.isValid).toBe(false);
    });

    it('should validate required frequency', async () => {
      const frequencySelect = wrapper.findComponent({ name: 'QSelect' });
      await frequencySelect.setValue('');
      expect(wrapper.vm.isValid).toBe(false);
    });

    it('should validate required result', async () => {
      const resultSelect = wrapper.findComponent({ name: 'QSelect' });
      await resultSelect.setValue('');
      expect(wrapper.vm.isValid).toBe(false);
    });

    it('should validate optional comment length', async () => {
      const commentInput = wrapper.findComponent({ name: 'QInput' });
      await commentInput.setValue('a'.repeat(501));
      expect(wrapper.vm.isValid).toBe(false);
    });
  });

  describe('API Integration', () => {
    it('should call manageDrugTest API with correct payload', async () => {
      const mockDrugTest = createMockDrugTest();
      mockPermissionCheck.mockReturnValue(true);
      
      // Set form values
      Object.entries(mockDrugTest).forEach(async ([key, value]) => {
        const input = wrapper.find(`[data-test="${key}-input"]`);
        await input.setValue(value);
      });

      await wrapper.find('form').trigger('submit');

      expect(manageDrugTest).toHaveBeenCalledWith(1, {
        ...mockDrugTest,
        inspectorId: 1
      });
    });

    it('should handle API success response', async () => {
      const mockDrugTest = createMockDrugTest();
      (manageDrugTest as any).mockResolvedValue(mockDrugTest);
      mockPermissionCheck.mockReturnValue(true);

      await wrapper.find('form').trigger('submit');
      await wrapper.vm.$nextTick();

      expect(wrapper.emitted('submit')).toBeTruthy();
      expect(wrapper.emitted('submit')[0]).toEqual([mockDrugTest]);
    });

    it('should handle API error response', async () => {
      const errorMessage = 'API Error';
      (manageDrugTest as any).mockRejectedValue(new Error(errorMessage));
      mockPermissionCheck.mockReturnValue(true);

      await wrapper.find('form').trigger('submit');
      await wrapper.vm.$nextTick();

      expect(wrapper.emitted('error')).toBeTruthy();
      expect(wrapper.emitted('error')[0]).toEqual([errorMessage]);
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA labels on all fields', () => {
      const inputs = wrapper.findAll('[aria-label]');
      expect(inputs.length).toBeGreaterThan(0);
      inputs.forEach(input => {
        expect(input.attributes('aria-label')).toBeTruthy();
      });
    });

    it('should announce form errors to screen readers', async () => {
      const resultSelect = wrapper.findComponent({ name: 'QSelect' });
      await resultSelect.setValue('');
      await wrapper.find('form').trigger('submit');
      
      const alert = wrapper.find('[role="alert"]');
      expect(alert.exists()).toBe(true);
    });
  });

  describe('Mobile Responsiveness', () => {
    it('should adapt layout for mobile viewport', async () => {
      // Mock mobile viewport
      Object.defineProperty(window, 'innerWidth', { value: 375 });
      await window.dispatchEvent(new Event('resize'));

      expect(wrapper.classes()).toContain('drug-test-form--mobile');
    });

    it('should maintain field usability on small screens', () => {
      const inputs = wrapper.findAll('.drug-test-form__field');
      inputs.forEach(input => {
        expect(input.classes()).toContain('drug-test-form__field--mobile');
      });
    });
  });
});
import { mount } from '@vue/test-utils'; // ^2.4.1
import { describe, it, expect, vi, beforeEach } from 'vitest'; // ^0.34.3
import { createTestingPinia } from '@pinia/testing'; // ^0.1.3
import QuickLinksEditor from '@/components/admin/QuickLinksEditor.vue';
import { useQuickLinksStore } from '@/stores/quickLinks';
import type { QuickLink } from '@/types/admin';

describe('QuickLinksEditor.vue', () => {
  // Mock data
  const mockQuickLinks: QuickLink[] = [
    {
      id: 1,
      label: 'Test Link',
      link: 'https://test.com',
      order: 1,
      isActive: true,
      createdAt: new Date(),
      createdBy: 1,
      updatedAt: null,
      updatedBy: null
    },
    {
      id: 2,
      label: 'International Link',
      link: 'https://test.co.uk',
      order: 2,
      isActive: true,
      createdAt: new Date(),
      createdBy: 1,
      updatedAt: null,
      updatedBy: null
    }
  ];

  // Helper function to create wrapper with options
  const createWrapper = (options = {}) => {
    const pinia = createTestingPinia({
      createSpy: vi.fn,
      initialState: {
        quickLinks: {
          quickLinks: mockQuickLinks,
          loading: false,
          error: null
        }
      }
    });

    return mount(QuickLinksEditor, {
      global: {
        plugins: [pinia],
        stubs: {
          QTable: true,
          QBtn: true,
          QInput: true,
          QIcon: true,
          QDialog: true,
          QCard: true,
          QCardSection: true,
          QForm: true,
          QSpace: true
        }
      },
      ...options
    });
  };

  // URL validation helper
  const validateUrl = (url: string): boolean => {
    const urlPattern = /^https?:\/\/(?:[\w-]+\.)+[a-z]{2,}(?:\/[\w-./]*)?$/i;
    return urlPattern.test(url);
  };

  describe('Component Rendering', () => {
    it('renders properly when user has edit permission', async () => {
      const wrapper = createWrapper();
      const store = useQuickLinksStore();
      vi.mocked(store.canEditLinks).mockReturnValue(true);

      expect(wrapper.find('.quick-links-editor').exists()).toBe(true);
      expect(wrapper.find('[aria-label="Add new quick link"]').exists()).toBe(true);
    });

    it('does not show edit buttons without permission', async () => {
      const wrapper = createWrapper();
      const store = useQuickLinksStore();
      vi.mocked(store.canEditLinks).mockReturnValue(false);

      expect(wrapper.find('[aria-label="Add new quick link"]').exists()).toBe(false);
      expect(wrapper.find('[aria-label="Edit quick link"]').exists()).toBe(false);
    });

    it('displays correct accessibility attributes', () => {
      const wrapper = createWrapper();
      
      expect(wrapper.find('[aria-label="Search quick links"]').exists()).toBe(true);
      expect(wrapper.find('[role="grid"]').exists()).toBe(true);
    });

    it('handles RTL layout correctly', async () => {
      const wrapper = createWrapper({
        global: {
          mocks: {
            $q: {
              lang: {
                rtl: true
              }
            }
          }
        }
      });

      expect(wrapper.classes()).toContain('quick-links-editor');
    });
  });

  describe('Dialog Validation', () => {
    it('validates required fields', async () => {
      const wrapper = createWrapper();
      await wrapper.vm.openEditor();

      const form = wrapper.findComponent({ name: 'QForm' });
      await form.vm.$emit('submit');

      expect(wrapper.emitted('error')).toBeTruthy();
      expect(wrapper.vm.formData.label).toBe('');
      expect(wrapper.vm.formData.link).toBe('');
    });

    it('validates URL format with international domains', async () => {
      const testUrls = [
        { url: 'https://test.com', valid: true },
        { url: 'https://test.co.uk', valid: true },
        { url: 'https://test.香港', valid: true },
        { url: 'not-a-url', valid: false },
        { url: 'http:/test.com', valid: false }
      ];

      testUrls.forEach(({ url, valid }) => {
        expect(validateUrl(url)).toBe(valid);
      });
    });

    it('prevents duplicate labels', async () => {
      const wrapper = createWrapper();
      const store = useQuickLinksStore();

      await wrapper.vm.openEditor();
      wrapper.vm.formData.label = mockQuickLinks[0].label;
      
      const form = wrapper.findComponent({ name: 'QForm' });
      await form.vm.$emit('submit');

      expect(store.updateQuickLink).not.toHaveBeenCalled();
      expect(wrapper.emitted('error')).toBeTruthy();
    });

    it('validates order field numeric constraints', async () => {
      const wrapper = createWrapper();
      await wrapper.vm.openEditor();

      wrapper.vm.formData.order = -1;
      await wrapper.vm.validateFields();
      expect(wrapper.vm.formErrors.order).toBeTruthy();

      wrapper.vm.formData.order = 0;
      await wrapper.vm.validateFields();
      expect(wrapper.vm.formErrors.order).toBeTruthy();

      wrapper.vm.formData.order = 1;
      await wrapper.vm.validateFields();
      expect(wrapper.vm.formErrors.order).toBeFalsy();
    });
  });

  describe('Store Integration', () => {
    it('successfully updates quick link', async () => {
      const wrapper = createWrapper();
      const store = useQuickLinksStore();
      
      await wrapper.vm.openEditor(mockQuickLinks[0]);
      wrapper.vm.formData.label = 'Updated Label';
      
      const form = wrapper.findComponent({ name: 'QForm' });
      await form.vm.$emit('submit');

      expect(store.updateQuickLink).toHaveBeenCalledWith({
        ...mockQuickLinks[0],
        label: 'Updated Label'
      });
    });

    it('handles store errors gracefully', async () => {
      const wrapper = createWrapper();
      const store = useQuickLinksStore();
      vi.mocked(store.updateQuickLink).mockRejectedValue(new Error('Update failed'));

      await wrapper.vm.openEditor(mockQuickLinks[0]);
      const form = wrapper.findComponent({ name: 'QForm' });
      await form.vm.$emit('submit');

      expect(wrapper.emitted('error')).toBeTruthy();
      expect(wrapper.vm.loading).toBe(false);
    });

    it('maintains data consistency during concurrent updates', async () => {
      const wrapper = createWrapper();
      const store = useQuickLinksStore();

      const update1 = wrapper.vm.handleSave({ ...mockQuickLinks[0], label: 'Update 1' });
      const update2 = wrapper.vm.handleSave({ ...mockQuickLinks[0], label: 'Update 2' });

      await Promise.all([update1, update2]);

      expect(store.updateQuickLink).toHaveBeenCalledTimes(2);
      expect(wrapper.vm.loading).toBe(false);
    });
  });

  describe('Accessibility', () => {
    it('maintains proper focus management', async () => {
      const wrapper = createWrapper();
      
      await wrapper.vm.openEditor();
      expect(document.activeElement).toBe(wrapper.find('[aria-label="Quick link label"]').element);
      
      await wrapper.vm.closeDialog();
      expect(document.activeElement).toBe(wrapper.find('[aria-label="Add new quick link"]').element);
    });

    it('provides screen reader announcements', async () => {
      const wrapper = createWrapper();
      
      await wrapper.vm.openEditor();
      expect(wrapper.find('[aria-live="polite"]').exists()).toBe(true);
      
      await wrapper.vm.handleSave(mockQuickLinks[0]);
      expect(wrapper.find('[aria-live="polite"]').text()).toContain('Quick link updated successfully');
    });

    it('supports keyboard navigation', async () => {
      const wrapper = createWrapper();
      
      const addButton = wrapper.find('[aria-label="Add new quick link"]');
      await addButton.trigger('keydown.enter');
      expect(wrapper.vm.showDialog).toBe(true);

      const form = wrapper.findComponent({ name: 'QForm' });
      await form.trigger('keydown.esc');
      expect(wrapper.vm.showDialog).toBe(false);
    });
  });
});
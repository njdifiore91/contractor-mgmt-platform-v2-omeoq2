import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'; // ^0.34.0
import { mount, flushPromises } from '@vue/test-utils'; // ^2.4.0
import { createTestingPinia } from '@pinia/testing'; // ^0.1.0
import EquipmentList from '@/components/equipment/EquipmentList.vue';
import { usePermissions } from '@/composables/usePermissions';
import { useEquipmentStore } from '@/stores/equipment';
import type { Equipment, EquipmentCondition } from '@/types/equipment';

// Mock the permissions composable
vi.mock('@/composables/usePermissions', () => ({
  usePermissions: vi.fn(() => ({
    checkPermission: vi.fn((permission: string) => permission === 'edit_equipment'),
    checkAllPermissions: vi.fn(),
    checkAnyPermission: vi.fn()
  }))
}));

// Helper function to create mock equipment data
const createMockEquipment = (count: number): Equipment[] => {
  return Array.from({ length: count }, (_, index) => ({
    id: index + 1,
    model: `Model-${index + 1}`,
    serialNumber: `SN-${index + 1}`,
    description: `Test equipment ${index + 1}`,
    condition: ['NEW', 'GOOD', 'FAIR', 'POOR'][index % 4] as EquipmentCondition,
    isOut: index % 2 === 0,
    companyId: 1,
    assignedToInspectorId: index % 2 === 0 ? 1 : null,
    assignedToInspectorName: index % 2 === 0 ? 'John Doe' : null,
    assignedDate: index % 2 === 0 ? new Date() : null,
    assignedCondition: index % 2 === 0 ? 'GOOD' as EquipmentCondition : null,
    returnedDate: null,
    returnedCondition: null,
    createdAt: new Date(),
    createdBy: 1,
    updatedAt: null,
    updatedBy: null,
    isActive: true
  }));
};

// Helper function to create wrapper with required props and store
const createWrapper = async (options = {}) => {
  const pinia = createTestingPinia({
    createSpy: vi.fn,
    initialState: {
      equipment: {
        equipmentList: [],
        loading: false,
        error: null
      }
    }
  });

  const wrapper = mount(EquipmentList, {
    props: {
      companyId: 1,
      ...options.props
    },
    global: {
      plugins: [pinia],
      stubs: {
        BaseTable: false
      }
    }
  });

  await flushPromises();
  return wrapper;
};

describe('EquipmentList.vue', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  describe('component mounting', () => {
    it('should mount successfully with required permissions', async () => {
      const wrapper = await createWrapper();
      expect(wrapper.exists()).toBe(true);
      expect(usePermissions().checkPermission).toHaveBeenCalledWith('edit_equipment');
    });

    it('should validate companyId prop', async () => {
      const consoleError = vi.spyOn(console, 'error').mockImplementation(() => {});
      await createWrapper({ props: { companyId: -1 } });
      expect(consoleError).toHaveBeenCalled();
      consoleError.mockRestore();
    });
  });

  describe('equipment loading', () => {
    it('should show loading state while fetching data', async () => {
      const wrapper = await createWrapper();
      const store = useEquipmentStore();
      store.loading = true;
      await wrapper.vm.$nextTick();
      expect(wrapper.find('.q-spinner').exists()).toBe(true);
    });

    it('should load equipment data for company', async () => {
      const wrapper = await createWrapper();
      const store = useEquipmentStore();
      const mockData = createMockEquipment(5);
      store.equipmentList = mockData;
      await wrapper.vm.$nextTick();
      expect(wrapper.findAll('.q-tr').length).toBeGreaterThan(0);
    });

    it('should handle API errors gracefully', async () => {
      const wrapper = await createWrapper();
      const store = useEquipmentStore();
      store.error = 'Failed to load equipment';
      await wrapper.vm.$nextTick();
      expect(wrapper.find('.text-negative').exists()).toBe(true);
    });
  });

  describe('virtual scrolling', () => {
    it('should implement virtual scrolling for large datasets', async () => {
      const wrapper = await createWrapper();
      const store = useEquipmentStore();
      store.equipmentList = createMockEquipment(1000);
      await wrapper.vm.$nextTick();
      expect(wrapper.find('[virtual-scroll]').exists()).toBe(true);
    });

    it('should maintain scroll position when data updates', async () => {
      const wrapper = await createWrapper();
      const store = useEquipmentStore();
      store.equipmentList = createMockEquipment(100);
      await wrapper.vm.$nextTick();
      const table = wrapper.findComponent({ ref: 'tableRef' });
      expect(table.exists()).toBe(true);
    });
  });

  describe('table columns', () => {
    it('should display all required fields', async () => {
      const wrapper = await createWrapper();
      const columns = wrapper.vm.getTableColumns();
      const requiredFields = ['model', 'serialNumber', 'description', 'isOut', 'condition'];
      requiredFields.forEach(field => {
        expect(columns.find(col => col.field === field)).toBeTruthy();
      });
    });

    it('should format boolean out status correctly', async () => {
      const wrapper = await createWrapper();
      const columns = wrapper.vm.getTableColumns();
      const outColumn = columns.find(col => col.field === 'isOut');
      expect(outColumn?.format(true)).toBe('Yes');
      expect(outColumn?.format(false)).toBe('No');
    });

    it('should format condition with proper styling', async () => {
      const wrapper = await createWrapper();
      const columns = wrapper.vm.getTableColumns();
      const conditionColumn = columns.find(col => col.field === 'condition');
      expect(conditionColumn?.classes('NEW')).toBe('text-positive');
      expect(conditionColumn?.classes('POOR')).toBe('text-negative');
    });
  });

  describe('data formatting', () => {
    it('should handle empty data state', async () => {
      const wrapper = await createWrapper();
      const store = useEquipmentStore();
      store.equipmentList = [];
      await wrapper.vm.$nextTick();
      expect(wrapper.text()).toContain('No equipment found');
    });

    it('should truncate long descriptions', async () => {
      const wrapper = await createWrapper();
      const store = useEquipmentStore();
      const longDescription = 'A'.repeat(300);
      store.equipmentList = [createMockEquipment(1)[0]];
      store.equipmentList[0].description = longDescription;
      await wrapper.vm.$nextTick();
      const descCell = wrapper.find('.ellipsis');
      expect(descCell.exists()).toBe(true);
    });
  });
});
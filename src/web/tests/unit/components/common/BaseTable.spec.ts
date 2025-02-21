// External imports - versions specified in package.json
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'; // ^0.34.0
import { mount, flushPromises } from '@vue/test-utils'; // ^2.4.0
import { QTable } from 'quasar'; // ^2.12.0

// Internal imports
import BaseTable from '@/components/common/BaseTable.vue';
import VirtualScroller from '@/components/common/VirtualScroller.vue';

// Mock data
const mockColumns = [
  { name: 'id', label: 'ID', field: 'id', sortable: true, align: 'left' },
  { name: 'name', label: 'Name', field: 'name', sortable: true, align: 'left', requiresPermission: 'view_name' },
  { name: 'status', label: 'Status', field: 'status', sortable: true, align: 'left', format: (value: string) => value.toUpperCase() }
];

const mockData = [
  { id: 1, name: 'Test Item 1', status: 'active' },
  { id: 2, name: 'Test Item 2', status: 'inactive' }
];

const mockPermissions = ['view_name', 'edit_status'];

// Helper function to create wrapper
const createWrapper = async (props = {}, options = {}) => {
  const wrapper = mount(BaseTable, {
    props: {
      columns: mockColumns,
      data: mockData,
      rowKey: 'id',
      userPermissions: mockPermissions,
      ...props
    },
    global: {
      components: {
        QTable,
        VirtualScroller
      },
      ...options
    }
  });
  await flushPromises();
  return wrapper;
};

describe('BaseTable Component', () => {
  // Mock ResizeObserver
  beforeEach(() => {
    vi.stubGlobal('ResizeObserver', vi.fn().mockImplementation(() => ({
      observe: vi.fn(),
      unobserve: vi.fn(),
      disconnect: vi.fn()
    })));
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('Component Mounting', () => {
    it('mounts successfully with required props', async () => {
      const wrapper = await createWrapper();
      expect(wrapper.exists()).toBe(true);
      expect(wrapper.findComponent(QTable).exists()).toBe(true);
      expect(wrapper.findComponent(VirtualScroller).exists()).toBe(true);
    });

    it('throws error when required props are missing', async () => {
      const consoleError = vi.spyOn(console, 'error').mockImplementation(() => {});
      await expect(createWrapper({ columns: undefined })).rejects.toThrow();
      expect(consoleError).toHaveBeenCalled();
    });

    it('renders with correct initial state', async () => {
      const wrapper = await createWrapper();
      expect(wrapper.vm.visibleColumns.length).toBe(mockColumns.length);
      expect(wrapper.find('.base-table').exists()).toBe(true);
    });
  });

  describe('Virtual Scrolling', () => {
    it('initializes virtual scroller with correct buffer size', async () => {
      const wrapper = await createWrapper();
      const virtualScroller = wrapper.findComponent(VirtualScroller);
      expect(virtualScroller.props('items')).toEqual(mockData);
      expect(virtualScroller.props('itemHeight')).toBe(48);
    });

    it('maintains scroll position when data updates', async () => {
      const wrapper = await createWrapper();
      const virtualScroller = wrapper.findComponent(VirtualScroller);
      const scrollToIndex = vi.spyOn(virtualScroller.vm, 'scrollToIndex');
      
      await wrapper.setProps({ data: [...mockData, { id: 3, name: 'Test Item 3', status: 'active' }] });
      expect(scrollToIndex).not.toHaveBeenCalled();
    });

    it('updates container size on data changes', async () => {
      const wrapper = await createWrapper();
      const updateContainerSize = vi.spyOn(wrapper.vm.virtualScrollerRef, 'updateContainerSize');
      
      await wrapper.setProps({ data: [...mockData, { id: 3, name: 'Test Item 3', status: 'active' }] });
      expect(updateContainerSize).toHaveBeenCalled();
    });
  });

  describe('Permission-Based Features', () => {
    it('filters columns based on user permissions', async () => {
      const wrapper = await createWrapper({ userPermissions: [] });
      const visibleColumns = wrapper.vm.visibleColumns;
      expect(visibleColumns.find(col => col.requiresPermission === 'view_name')).toBeUndefined();
    });

    it('updates visible columns when permissions change', async () => {
      const wrapper = await createWrapper();
      await wrapper.setProps({ userPermissions: [] });
      expect(wrapper.vm.visibleColumns.length).toBeLessThan(mockColumns.length);
    });

    it('caches permission checks for performance', async () => {
      const wrapper = await createWrapper();
      const checkPermission = vi.spyOn(wrapper.vm, 'checkPermission');
      
      wrapper.vm.getVisibleColumns(mockColumns, mockPermissions, { lt: { sm: false, md: false } });
      wrapper.vm.getVisibleColumns(mockColumns, mockPermissions, { lt: { sm: false, md: false } });
      
      expect(checkPermission).toHaveBeenCalledTimes(mockColumns.length);
    });
  });

  describe('Sorting Functionality', () => {
    it('emits sort event with correct parameters', async () => {
      const wrapper = await createWrapper();
      const column = mockColumns[0];
      
      await wrapper.find('.q-th').trigger('click');
      expect(wrapper.emitted('sort')).toBeTruthy();
      expect(wrapper.emitted('sort')[0][0]).toEqual({
        fields: [{ field: column.name, direction: 'asc' }],
        primary: column.name
      });
    });

    it('handles multiple column sorting', async () => {
      const wrapper = await createWrapper();
      const headers = wrapper.findAll('.q-th');
      
      await headers[0].trigger('click');
      await headers[1].trigger('click');
      
      const sortEvents = wrapper.emitted('sort');
      expect(sortEvents[sortEvents.length - 1][0].fields.length).toBe(2);
    });

    it('maintains sort state across data updates', async () => {
      const wrapper = await createWrapper();
      await wrapper.find('.q-th').trigger('click');
      
      await wrapper.setProps({ data: [...mockData] });
      expect(wrapper.vm.sortState.get(mockColumns[0].name)).toBe('asc');
    });
  });

  describe('Data Formatting', () => {
    it('applies custom formatters correctly', async () => {
      const wrapper = await createWrapper();
      const formattedValue = wrapper.vm.formatCellValue('active', mockColumns[2], mockData[0]);
      expect(formattedValue).toBe('ACTIVE');
    });

    it('handles default formatting rules', async () => {
      const wrapper = await createWrapper();
      const numericValue = wrapper.vm.formatCellValue(1000, { field: 'amount' }, {});
      expect(numericValue).toBe('1,000');
    });

    it('manages type-specific formatting', async () => {
      const wrapper = await createWrapper();
      const date = new Date('2023-01-01');
      const formattedDate = wrapper.vm.formatCellValue(date, { field: 'date' }, {});
      expect(formattedDate).toBe(date.toLocaleDateString());
    });

    it('gracefully handles formatting errors', async () => {
      const wrapper = await createWrapper();
      const formattedValue = wrapper.vm.formatCellValue(undefined, { field: 'invalid', format: () => { throw new Error(); } }, {});
      expect(formattedValue).toBe('');
    });
  });

  describe('Event Handling', () => {
    it('emits row-click events with correct data', async () => {
      const wrapper = await createWrapper();
      await wrapper.find('.base-table__row').trigger('click');
      expect(wrapper.emitted('row-click')).toBeTruthy();
      expect(wrapper.emitted('row-click')[0][0]).toEqual(mockData[0]);
    });

    it('handles selection events properly', async () => {
      const wrapper = await createWrapper({
        selected: []
      });
      await wrapper.find('.base-table__row').trigger('click');
      expect(wrapper.emitted('update:selected')).toBeFalsy();
    });
  });

  describe('Accessibility', () => {
    it('maintains proper ARIA attributes', async () => {
      const wrapper = await createWrapper();
      const table = wrapper.find('.q-table');
      expect(table.attributes('role')).toBe('table');
    });

    it('supports keyboard navigation', async () => {
      const wrapper = await createWrapper();
      const firstRow = wrapper.find('.base-table__row');
      await firstRow.trigger('keydown.enter');
      expect(wrapper.emitted('row-click')).toBeTruthy();
    });
  });
});
<template>
  <q-table
    ref="qTableRef"
    :columns="visibleColumns"
    :rows="data"
    :loading="loading"
    :row-key="rowKey"
    :style="tableStyle"
    class="base-table"
    virtual-scroll
  >
    <!-- Custom header slot for enhanced sorting -->
    <template #header="props">
      <q-tr :props="props">
        <q-th
          v-for="col in props.cols"
          :key="col.name"
          :props="props"
          :class="{ 'cursor-pointer': col.sortable }"
          @click="col.sortable && handleSort(col)"
        >
          {{ col.label }}
          <q-icon
            v-if="col.sortable"
            :name="getSortIcon(col.name)"
            size="sm"
            class="q-ml-sm"
          />
        </q-th>
      </q-tr>
    </template>

    <!-- Virtual scroll body using VirtualScroller -->
    <template #body="props">
      <virtual-scroller
        ref="virtualScrollerRef"
        :items="data"
        :item-height="48"
        class="base-table__scroller"
      >
        <template #default="{ item, index }">
          <q-tr
            :props="{ ...props, row: item }"
            :key="`${rowKey}-${item[rowKey]}`"
            @click="$emit('row-click', item)"
            class="base-table__row"
            :class="{ 'cursor-pointer': $listeners['row-click'] }"
          >
            <q-td
              v-for="col in visibleColumns"
              :key="col.name"
              :props="props"
              :class="[`text-${col.align || 'left'}`, col.classes]"
            >
              {{ formatCellValue(item[col.field], col, item) }}
            </q-td>
          </q-tr>
        </template>
      </virtual-scroller>
    </template>
  </q-table>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, computed, onMounted, onBeforeUnmount, nextTick } from 'vue';

// Quasar imports - v2.12.0
import { QTable, QTh, QTr, QTd, useQuasar } from 'quasar';

// Internal imports
import { VirtualScroller } from '@/components/common/VirtualScroller';
import { usePermissions } from '@/composables/usePermissions';

// Constants
const SORT_ICONS = {
  asc: 'arrow_upward',
  desc: 'arrow_downward',
  none: 'unfold_more'
};

// Props
const props = defineProps({
  columns: {
    type: Array,
    required: true,
    validator: (columns) => {
      return columns.every(col => 
        col.field && 
        typeof col.field === 'string' &&
        col.label && 
        typeof col.label === 'string'
      );
    }
  },
  data: {
    type: Array,
    required: true
  },
  loading: {
    type: Boolean,
    default: false
  },
  rowKey: {
    type: String,
    required: true
  },
  userPermissions: {
    type: Array,
    default: () => []
  },
  initialSort: {
    type: Object,
    default: () => ({})
  },
  responsive: {
    type: Boolean,
    default: true
  }
});

// Emits
const emit = defineEmits<{
  (e: 'row-click', rowData: any): void
  (e: 'sort', sortConfig: { fields: Array<{field: string, direction: 'asc' | 'desc'}>, primary: string }): void
  (e: 'visibility-change', change: { column: string, visible: boolean }): void
}>();

// Refs
const qTableRef = ref<InstanceType<typeof QTable> | null>(null);
const virtualScrollerRef = ref<InstanceType<typeof VirtualScroller> | null>(null);
const sortState = ref<Map<string, 'asc' | 'desc' | null>>(new Map());

// Composables
const $q = useQuasar();
const { checkPermission, cachePermission } = usePermissions();

// Computed
const visibleColumns = computed(() => {
  return getVisibleColumns(props.columns, props.userPermissions, $q.screen);
});

const tableStyle = computed(() => ({
  height: `${$q.screen.height * 0.7}px`,
  maxHeight: '800px',
  minHeight: '200px'
}));

// Methods
function getVisibleColumns(columns, userPermissions, screen) {
  return columns.filter(col => {
    // Check permission requirements
    if (col.requiresPermission) {
      const hasPermission = checkPermission(col.requiresPermission);
      cachePermission(col.requiresPermission, hasPermission);
      if (!hasPermission) return false;
    }

    // Check responsive visibility
    if (props.responsive && col.responsive) {
      if (
        (col.responsive.hideOn === 'sm' && screen.lt.sm) ||
        (col.responsive.hideOn === 'md' && screen.lt.md)
      ) {
        return false;
      }
    }

    return true;
  });
}

function formatCellValue(value, column, row) {
  try {
    if (value == null) return '';

    // Use column formatter if provided
    if (column.format && typeof column.format === 'function') {
      return column.format(value, row);
    }

    // Default formatting based on value type
    if (typeof value === 'number') {
      return value.toLocaleString();
    } else if (value instanceof Date) {
      return value.toLocaleDateString();
    }

    return value.toString();
  } catch (error) {
    console.error(`Error formatting cell value:`, error);
    return '';
  }
}

function handleSort(column) {
  if (!column.sortable) return;

  const currentDirection = sortState.value.get(column.name) || null;
  const newDirection = currentDirection === 'asc' ? 'desc' : 
                      currentDirection === 'desc' ? null : 'asc';

  sortState.value.set(column.name, newDirection);

  const sortFields = Array.from(sortState.value.entries())
    .filter(([, direction]) => direction)
    .map(([field, direction]) => ({ field, direction }));

  emit('sort', {
    fields: sortFields,
    primary: column.name
  });
}

function getSortIcon(columnName) {
  const direction = sortState.value.get(columnName);
  return SORT_ICONS[direction || 'none'];
}

// Lifecycle hooks
onMounted(() => {
  // Initialize sort state from props
  if (props.initialSort?.field) {
    sortState.value.set(props.initialSort.field, props.initialSort.direction || 'asc');
  }
});

onBeforeUnmount(() => {
  sortState.value.clear();
});

// Expose public methods
defineExpose({
  scrollToIndex: (index: number) => {
    virtualScrollerRef.value?.scrollToIndex(index);
  },
  formatCellValue,
  getVisibleColumns
});
</script>

<style lang="scss">
.base-table {
  &__scroller {
    height: 100%;
    width: 100%;
  }

  &__row {
    transition: background-color 0.2s ease;

    &:hover {
      background-color: rgba(0, 0, 0, 0.03);
    }

    &.cursor-pointer {
      cursor: pointer;
    }
  }

  // Responsive adjustments
  @media (max-width: 600px) {
    .q-table__card {
      border-radius: 0;
    }
  }
}
</style>
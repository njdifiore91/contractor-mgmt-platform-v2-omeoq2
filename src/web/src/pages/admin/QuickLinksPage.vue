<template>
  <q-page class="quick-links-page q-pa-md">
    <!-- Main table with quick links -->
    <QTable
      :rows="quickLinks"
      :columns="columns"
      :loading="loading"
      row-key="id"
      flat
      bordered
      binary-state-sort
      aria-label="Quick Links Table"
    >
      <!-- Loading state -->
      <template v-slot:loading>
        <QSpinner size="2em" color="primary" />
        <span class="q-ml-sm">Loading quick links...</span>
      </template>

      <!-- Error state -->
      <template v-slot:top v-if="error">
        <div class="text-negative q-pb-md">
          {{ error }}
        </div>
      </template>

      <!-- Link cell with target blank -->
      <template v-slot:body-cell-link="props">
        <QTd>
          <a 
            :href="props.value" 
            target="_blank" 
            rel="noopener noreferrer"
            class="text-primary"
          >
            {{ props.value }}
          </a>
        </QTd>
      </template>

      <!-- Actions cell -->
      <template v-slot:body-cell-actions="props">
        <QTd class="text-right">
          <QBtn
            v-if="canEditLinks"
            flat
            round
            color="primary"
            icon="edit"
            @click="handleEdit(props.row)"
            :aria-label="`Edit ${props.row.label}`"
          />
        </QTd>
      </template>
    </QTable>

    <!-- Editor component -->
    <QuickLinksEditor
      v-model="showEditor"
      @success="refreshData"
    />
  </q-page>
</template>

<script setup lang="ts">
// Vue imports - v3.3.4
import { ref, onMounted, onUnmounted, computed } from 'vue';

// Quasar imports - v2.12.0
import { QPage, QTable, QBtn, QSpinner, useQuasar } from 'quasar';

// Internal imports
import QuickLinksEditor from '@/components/admin/QuickLinksEditor.vue';
import { useQuickLinksStore } from '@/stores/quickLinks';
import type { QuickLink } from '@/types/admin';

// Initialize Quasar utilities
const $q = useQuasar();

// Store integration
const quickLinksStore = useQuickLinksStore();
const { quickLinks, loading, canEditLinks, error } = quickLinksStore;

// Component state
const showEditor = ref(false);

// Table columns configuration
const columns = computed(() => [
  {
    name: 'label',
    label: 'Label',
    field: 'label',
    sortable: true,
    align: 'left'
  },
  {
    name: 'link',
    label: 'Link',
    field: 'link',
    sortable: true,
    align: 'left'
  },
  {
    name: 'order',
    label: 'Order',
    field: 'order',
    sortable: true,
    align: 'center'
  },
  {
    name: 'actions',
    label: 'Actions',
    field: 'actions',
    align: 'right'
  }
]);

/**
 * Handles opening the quick link editor
 * @param quickLink Optional quick link to edit
 */
const handleEdit = (quickLink?: QuickLink) => {
  if (!canEditLinks.value) {
    $q.notify({
      type: 'negative',
      message: 'You do not have permission to edit quick links'
    });
    return;
  }

  try {
    showEditor.value = true;
  } catch (err) {
    $q.notify({
      type: 'negative',
      message: 'Failed to open editor'
    });
    console.error('Editor open error:', err);
  }
};

/**
 * Refreshes the quick links data
 */
const refreshData = async () => {
  try {
    await quickLinksStore.fetchQuickLinks();
    $q.notify({
      type: 'positive',
      message: 'Quick links refreshed successfully'
    });
  } catch (err) {
    $q.notify({
      type: 'negative',
      message: 'Failed to refresh quick links'
    });
    console.error('Refresh error:', err);
  }
};

// Lifecycle hooks
onMounted(async () => {
  try {
    await refreshData();
  } catch (err) {
    console.error('Initial load error:', err);
  }
});

onUnmounted(() => {
  // Cleanup any subscriptions or timers if needed
});
</script>

<style scoped>
.quick-links-page {
  height: 100%;
  display: flex;
  flex-direction: column;
}

/* Ensure table takes remaining height */
.q-table__container {
  flex: 1;
  overflow: auto;
}

/* Improve accessibility for links */
a {
  text-decoration: none;
  &:hover, &:focus {
    text-decoration: underline;
  }
  &:focus {
    outline: 2px solid var(--q-primary);
    outline-offset: 2px;
  }
}

/* Mobile optimizations */
@media (max-width: 600px) {
  .q-table {
    height: calc(100vh - 120px);
  }
}
</style>
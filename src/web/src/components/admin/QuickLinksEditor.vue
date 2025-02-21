<template>
  <div class="quick-links-editor q-pa-md">
    <!-- Main table with quick links -->
    <QTable
      :rows="quickLinks"
      :columns="columns"
      row-key="id"
      :loading="loading"
      :filter="filter"
      flat
      bordered
      virtual-scroll
    >
      <!-- Top section with title and actions -->
      <template v-slot:top>
        <div class="row full-width q-pb-md items-center">
          <div class="text-h6">Quick Links Management</div>
          <q-space />
          <QInput
            v-model="filter"
            placeholder="Search links..."
            dense
            outlined
            class="q-mr-md"
            aria-label="Search quick links"
          >
            <template v-slot:append>
              <QIcon name="search" />
            </template>
          </QInput>
          <QBtn
            v-if="canEditLinks"
            color="primary"
            icon="add"
            label="Add Link"
            @click="openEditor()"
            :disable="loading"
            aria-label="Add new quick link"
          />
        </div>
      </template>

      <!-- Actions column slot -->
      <template v-slot:body-cell-actions="props">
        <QTd>
          <QBtn
            v-if="canEditLinks"
            flat
            round
            color="primary"
            icon="edit"
            @click="openEditor(props.row)"
            aria-label="Edit quick link"
          />
        </QTd>
      </template>
    </QTable>

    <!-- Editor Dialog -->
    <QDialog
      v-model="showDialog"
      persistent
      maximized
      :fullscreen="$q.screen.lt.sm"
      transition-show="slide-up"
      transition-hide="slide-down"
    >
      <QCard class="column">
        <QCardSection class="row items-center q-pb-none">
          <div class="text-h6">{{ editingLink ? 'Edit Quick Link' : 'Add Quick Link' }}</div>
          <q-space />
          <QBtn
            icon="close"
            flat
            round
            dense
            v-close-popup
            aria-label="Close dialog"
          />
        </QCardSection>

        <QCardSection class="q-pt-none">
          <QForm
            @submit="handleSave"
            @reset="closeDialog"
            class="q-gutter-md"
          >
            <QInput
              v-model="formData.label"
              :rules="[val => !!val || 'Label is required']"
              label="Label"
              outlined
              :disable="loading"
              aria-label="Quick link label"
            />

            <QInput
              v-model="formData.link"
              :rules="[
                val => !!val || 'Link is required',
                val => validateUrl(val) || 'Please enter a valid URL'
              ]"
              label="Link"
              outlined
              :disable="loading"
              aria-label="Quick link URL"
            />

            <QInput
              v-model.number="formData.order"
              type="number"
              :rules="[
                val => !!val || 'Order is required',
                val => val > 0 || 'Order must be greater than 0'
              ]"
              label="Display Order"
              outlined
              :disable="loading"
              aria-label="Display order"
            />

            <div class="row justify-end q-gutter-sm">
              <QBtn
                label="Cancel"
                type="reset"
                flat
                :disable="loading"
                aria-label="Cancel changes"
              />
              <QBtn
                label="Save"
                type="submit"
                color="primary"
                :loading="loading"
                aria-label="Save changes"
              />
            </div>
          </QForm>
        </QCardSection>
      </QCard>
    </QDialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'; // ^3.3.4
import { QTable, QBtn, useQuasar } from 'quasar'; // ^2.12.0
import { validateUrl } from 'vuelidate'; // ^2.0.0
import { useQuickLinksStore } from '@/stores/quickLinks';
import type { QuickLink } from '@/types/admin';

// Initialize Quasar utilities
const $q = useQuasar();

// Store integration
const { quickLinks, updateQuickLink, canEditLinks } = useQuickLinksStore();

// Component state
const loading = ref(false);
const showDialog = ref(false);
const filter = ref('');
const editingLink = ref<QuickLink | null>(null);
const formData = ref<Partial<QuickLink>>({
  label: '',
  link: '',
  order: 1
});

// Table configuration
const columns = [
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
    name: 'lastModified',
    label: 'Last Modified',
    field: 'lastModified',
    sortable: true,
    format: (val: Date) => val.toLocaleDateString()
  },
  {
    name: 'modifiedBy',
    label: 'Modified By',
    field: 'modifiedBy',
    sortable: true
  },
  {
    name: 'actions',
    label: 'Actions',
    field: 'actions',
    align: 'center'
  }
];

// Methods
const openEditor = (link?: QuickLink) => {
  if (!canEditLinks.value) {
    $q.notify({
      type: 'negative',
      message: 'You do not have permission to edit quick links'
    });
    return;
  }

  editingLink.value = link || null;
  formData.value = link
    ? { ...link }
    : {
        label: '',
        link: '',
        order: quickLinks.value.length + 1
      };
  showDialog.value = true;
};

const closeDialog = () => {
  showDialog.value = false;
  editingLink.value = null;
  formData.value = {
    label: '',
    link: '',
    order: 1
  };
};

const handleSave = async () => {
  try {
    loading.value = true;

    const updatedLink: QuickLink = {
      id: editingLink.value?.id || 0,
      ...formData.value as Required<Omit<QuickLink, 'id'>>,
      lastModified: new Date(),
      modifiedBy: 'current_user', // This should come from auth context
      status: 'active'
    };

    await updateQuickLink(updatedLink);

    $q.notify({
      type: 'positive',
      message: `Quick link ${editingLink.value ? 'updated' : 'created'} successfully`
    });

    closeDialog();
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: `Failed to ${editingLink.value ? 'update' : 'create'} quick link: ${error.message}`
    });
  } finally {
    loading.value = false;
  }
};

// Lifecycle hooks
onMounted(async () => {
  try {
    loading.value = true;
    await useQuickLinksStore().fetchQuickLinks();
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: `Failed to load quick links: ${error.message}`
    });
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.quick-links-editor {
  height: 100%;
  display: flex;
  flex-direction: column;
}

/* Ensure table takes remaining height */
.q-table__container {
  flex: 1;
  overflow: auto;
}

/* Mobile optimizations */
@media (max-width: 600px) {
  .q-table {
    height: calc(100vh - 120px);
  }
}
</style>
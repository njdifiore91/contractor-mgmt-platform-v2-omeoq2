<template>
  <div class="contact-management">
    <div class="contact-management__header q-pa-md">
      <h1 class="text-h4">{{ $t('contact.management.title') }}</h1>
      <QBtn
        color="primary"
        :label="$t('contact.management.addContact')"
        @click="openAddDialog"
        :loading="loading"
      />
    </div>

    <BaseTable
      ref="tableRef"
      :columns="columns"
      :data="contacts"
      :loading="loading"
      row-key="id"
      virtual-scroll
      @row-click="openEditDialog"
    >
      <template #top-right>
        <QInput
          v-model="searchQuery"
          :placeholder="$t('common.search')"
          dense
          outlined
          class="q-mr-sm"
        >
          <template #append>
            <QIcon name="search" />
          </template>
        </QInput>
      </template>

      <template #body-cell-actions="props">
        <QTd :props="props">
          <QBtn
            flat
            round
            color="primary"
            icon="edit"
            @click.stop="openEditDialog(props.row)"
          />
          <QBtn
            flat
            round
            color="negative"
            icon="delete"
            @click.stop="handleDelete(props.row)"
          />
        </QTd>
      </template>
    </BaseTable>

    <QDialog
      v-model="showDialog"
      persistent
      maximized
      transition-show="slide-up"
      transition-hide="slide-down"
    >
      <QCard class="contact-management__dialog">
        <QCardSection class="row items-center q-pb-none">
          <div class="text-h6">
            {{ isEditing ? $t('contact.management.editContact') : $t('contact.management.addContact') }}
          </div>
          <QSpace />
          <QBtn
            icon="close"
            flat
            round
            dense
            v-close-popup
            @click="closeDialog"
          />
        </QCardSection>

        <QCardSection class="q-pa-md">
          <ContactForm
            ref="contactFormRef"
            v-model="selectedContact"
            :customer-id="customerId"
            @submit="handleSubmit"
            @error="handleFormError"
          />
        </QCardSection>

        <QCardActions align="right" class="q-pa-md">
          <QBtn
            flat
            :label="$t('common.cancel')"
            @click="closeDialog"
          />
          <QBtn
            color="primary"
            :loading="saving"
            :label="$t('common.save')"
            @click="submitForm"
          />
        </QCardActions>
      </QCard>
    </QDialog>
  </div>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';

// Quasar imports - v2.12.0
import { 
  QBtn, 
  QCard, 
  QCardSection, 
  QCardActions,
  QDialog, 
  QIcon, 
  QInput,
  QSpace,
  QTd,
  useQuasar
} from 'quasar';

// Internal imports
import BaseTable from '@/components/common/BaseTable.vue';
import ContactForm from '@/components/customer/ContactForm.vue';
import type { Contact } from '@/types/customer';
import { usePermissions } from '@/composables/usePermissions';

// Constants
const CONTACTS_CACHE_DURATION = 5 * 60 * 1000; // 5 minutes

// Component setup
const route = useRoute();
const $q = useQuasar();
const { checkPermission } = usePermissions();

// State
const customerId = computed(() => Number(route.params.customerId));
const contacts = ref<Contact[]>([]);
const selectedContact = ref<Contact | null>(null);
const showDialog = ref(false);
const isEditing = ref(false);
const loading = ref(false);
const saving = ref(false);
const searchQuery = ref('');
const lastCacheUpdate = ref(0);
const contactsCache = ref<Map<number, Contact>>(new Map());

// Refs
const tableRef = ref<InstanceType<typeof BaseTable> | null>(null);
const contactFormRef = ref<InstanceType<typeof ContactForm> | null>(null);

// Table configuration
const columns = [
  {
    name: 'firstName',
    field: 'firstName',
    label: 'First Name',
    sortable: true,
    align: 'left'
  },
  {
    name: 'lastName',
    field: 'lastName',
    label: 'Last Name',
    sortable: true,
    align: 'left'
  },
  {
    name: 'dateCreated',
    field: 'dateCreated',
    label: 'Date Created',
    sortable: true,
    align: 'left',
    format: (val: Date) => new Date(val).toLocaleDateString()
  },
  {
    name: 'jobTitle',
    field: 'jobTitle',
    label: 'Job Title',
    sortable: true,
    align: 'left'
  },
  {
    name: 'actions',
    field: 'actions',
    label: 'Actions',
    align: 'center'
  }
];

// Methods
const loadContacts = async () => {
  if (!customerId.value) return;

  try {
    loading.value = true;

    // Check cache first
    if (Date.now() - lastCacheUpdate.value < CONTACTS_CACHE_DURATION) {
      return;
    }

    const response = await fetch(`/api/customers/${customerId.value}/contacts`);
    if (!response.ok) throw new Error('Failed to load contacts');

    const data = await response.json();
    contacts.value = data;

    // Update cache
    contactsCache.value.clear();
    contacts.value.forEach(contact => {
      contactsCache.value.set(contact.id, contact);
    });
    lastCacheUpdate.value = Date.now();
  } catch (error) {
    console.error('Error loading contacts:', error);
    $q.notify({
      type: 'negative',
      message: 'Failed to load contacts'
    });
  } finally {
    loading.value = false;
  }
};

const openAddDialog = () => {
  selectedContact.value = {
    id: 0,
    firstName: '',
    middleName: '',
    lastName: '',
    suffix: '',
    nickname: '',
    isDeceased: false,
    isInactive: false,
    rating: 0,
    jobTitle: '',
    birthday: null,
    dateCreated: new Date(),
    customerId: customerId.value,
    addresses: [],
    emails: [],
    phoneNumbers: [],
    notes: []
  };
  isEditing.value = false;
  showDialog.value = true;
};

const openEditDialog = (contact: Contact) => {
  selectedContact.value = JSON.parse(JSON.stringify(contact)); // Deep copy
  isEditing.value = true;
  showDialog.value = true;
};

const closeDialog = () => {
  showDialog.value = false;
  selectedContact.value = null;
  if (contactFormRef.value) {
    contactFormRef.value.reset();
  }
};

const handleSubmit = async (contactData: Contact) => {
  if (!contactData || saving.value) return;

  try {
    saving.value = true;

    const url = `/api/customers/${customerId.value}/contacts${isEditing.value ? `/${contactData.id}` : ''}`;
    const method = isEditing.value ? 'PUT' : 'POST';

    const response = await fetch(url, {
      method,
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(contactData)
    });

    if (!response.ok) throw new Error('Failed to save contact');

    const savedContact = await response.json();

    // Update cache and list optimistically
    if (isEditing.value) {
      const index = contacts.value.findIndex(c => c.id === savedContact.id);
      if (index !== -1) contacts.value[index] = savedContact;
      contactsCache.value.set(savedContact.id, savedContact);
    } else {
      contacts.value.unshift(savedContact);
      contactsCache.value.set(savedContact.id, savedContact);
    }

    $q.notify({
      type: 'positive',
      message: `Contact ${isEditing.value ? 'updated' : 'added'} successfully`
    });

    closeDialog();
  } catch (error) {
    console.error('Error saving contact:', error);
    $q.notify({
      type: 'negative',
      message: `Failed to ${isEditing.value ? 'update' : 'add'} contact`
    });
  } finally {
    saving.value = false;
  }
};

const handleDelete = async (contact: Contact) => {
  try {
    const confirmed = await $q.dialog({
      title: 'Confirm Deletion',
      message: `Are you sure you want to delete ${contact.firstName} ${contact.lastName}?`,
      cancel: true,
      persistent: true
    });

    if (!confirmed) return;

    const response = await fetch(`/api/customers/${customerId.value}/contacts/${contact.id}`, {
      method: 'DELETE'
    });

    if (!response.ok) throw new Error('Failed to delete contact');

    // Update cache and list optimistically
    contacts.value = contacts.value.filter(c => c.id !== contact.id);
    contactsCache.value.delete(contact.id);

    $q.notify({
      type: 'positive',
      message: 'Contact deleted successfully'
    });
  } catch (error) {
    console.error('Error deleting contact:', error);
    $q.notify({
      type: 'negative',
      message: 'Failed to delete contact'
    });
  }
};

const handleFormError = (errors: string[]) => {
  errors.forEach(error => {
    $q.notify({
      type: 'warning',
      message: error
    });
  });
};

const submitForm = () => {
  if (contactFormRef.value) {
    contactFormRef.value.validate();
  }
};

// Watchers
watch(searchQuery, async (newQuery) => {
  if (tableRef.value) {
    await tableRef.value.handleFilter(newQuery);
  }
});

// Lifecycle hooks
onMounted(async () => {
  if (!checkPermission('MANAGE_CUSTOMERS')) {
    $q.notify({
      type: 'negative',
      message: 'You do not have permission to manage contacts'
    });
    return;
  }
  
  await loadContacts();
});
</script>

<style lang="scss">
.contact-management {
  height: 100%;
  display: flex;
  flex-direction: column;

  &__header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  &__dialog {
    display: flex;
    flex-direction: column;
    height: 100%;
  }
}
</style>
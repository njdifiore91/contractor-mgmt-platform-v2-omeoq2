<template>
  <QPage class="users-page q-pa-md">
    <!-- Page Header -->
    <div class="users-page__header row items-center justify-between q-mb-md">
      <h1 class="text-h5 q-my-none">User Management</h1>
      <QBtn
        v-if="canEditUsers"
        color="primary"
        icon="add"
        label="Add User"
        @click="showUserDialog(null)"
      />
    </div>

    <!-- Error Alert -->
    <QBanner
      v-if="error"
      class="bg-negative text-white q-mb-md"
      rounded
      dense
    >
      {{ error }}
      <template v-slot:action>
        <QBtn flat color="white" label="Dismiss" @click="error = null" />
      </template>
    </QBanner>

    <!-- Users Table -->
    <BaseTable
      ref="tableRef"
      :columns="tableColumns"
      :data="filteredUsers"
      :loading="loading"
      :row-key="'id'"
      virtual-scroll
      @row-click="handleRowClick"
    >
      <template #top>
        <QInput
          v-model="searchQuery"
          dense
          outlined
          placeholder="Search users..."
          class="q-mb-md"
        >
          <template #append>
            <QIcon name="search" />
          </template>
        </QInput>
      </template>

      <template #body-cell-actions="props">
        <QTd :props="props">
          <QBtn
            v-if="canEditUsers"
            flat
            round
            color="primary"
            icon="edit"
            @click.stop="showUserDialog(props.row)"
          >
            <QTooltip>Edit User</QTooltip>
          </QBtn>
        </QTd>
      </template>
    </BaseTable>

    <!-- User Editor Dialog -->
    <QDialog
      v-model="showDialog"
      persistent
      maximized
      :full-height="$q.platform.is.mobile"
    >
      <UserEditor
        :user="selectedUser"
        @success="handleUserSaved"
        @error="handleError"
      />
    </QDialog>
  </QPage>
</template>

<script setup lang="ts">
// Vue imports - v3.3.0
import { ref, computed, onMounted, onUnmounted, watch } from 'vue';

// Quasar imports - v2.12.0
import { QPage, QBtn, QBanner, QInput, QIcon, QDialog, QTd, QTooltip, useQuasar } from 'quasar';

// Internal imports
import BaseTable from '@/components/common/BaseTable.vue';
import UserEditor from '@/components/admin/UserEditor.vue';
import { usePermissions } from '@/composables/usePermissions';
import { User } from '@/types/admin';
import { getUsers } from '@/services/api/admin';

// Composables
const $q = useQuasar();
const { checkPermission } = usePermissions();

// Component state
const loading = ref(false);
const error = ref<string | null>(null);
const showDialog = ref(false);
const selectedUser = ref<User | null>(null);
const users = ref<User[]>([]);
const searchQuery = ref('');
const tableRef = ref<InstanceType<typeof BaseTable> | null>(null);

// Computed properties
const canEditUsers = computed(() => checkPermission('edit_users'));

const tableColumns = computed(() => [
  {
    name: 'firstName',
    label: 'First Name',
    field: 'firstName',
    sortable: true,
    align: 'left'
  },
  {
    name: 'lastName',
    label: 'Last Name',
    field: 'lastName',
    sortable: true,
    align: 'left'
  },
  {
    name: 'email',
    label: 'Email',
    field: 'email',
    sortable: true,
    align: 'left'
  },
  {
    name: 'emailConfirmed',
    label: 'Confirmed',
    field: 'emailConfirmed',
    sortable: true,
    align: 'center',
    format: (val: boolean) => val ? 'Yes' : 'No'
  },
  {
    name: 'actions',
    label: 'Actions',
    field: 'actions',
    align: 'center',
    sortable: false
  }
]);

const filteredUsers = computed(() => {
  if (!searchQuery.value) return users.value;
  
  const query = searchQuery.value.toLowerCase();
  return users.value.filter(user => 
    user.firstName.toLowerCase().includes(query) ||
    user.lastName.toLowerCase().includes(query) ||
    user.email.toLowerCase().includes(query)
  );
});

// Methods
const loadUsers = async () => {
  try {
    loading.value = true;
    error.value = null;
    users.value = await getUsers();
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to load users';
    $q.notify({
      type: 'negative',
      message: error.value,
      position: 'top'
    });
  } finally {
    loading.value = false;
  }
};

const showUserDialog = (user: User | null) => {
  selectedUser.value = user;
  showDialog.value = true;
};

const handleUserSaved = async (user: User) => {
  showDialog.value = false;
  await loadUsers();
  $q.notify({
    type: 'positive',
    message: `User ${user.firstName} ${user.lastName} ${selectedUser.value ? 'updated' : 'created'} successfully`,
    position: 'top'
  });
};

const handleError = (message: string) => {
  error.value = message;
  $q.notify({
    type: 'negative',
    message,
    position: 'top'
  });
};

const handleRowClick = (user: User) => {
  if (canEditUsers.value) {
    showUserDialog(user);
  }
};

// Watchers
watch(searchQuery, () => {
  if (tableRef.value) {
    tableRef.value.scrollToIndex(0);
  }
});

// Lifecycle hooks
onMounted(async () => {
  if (!canEditUsers.value) {
    error.value = 'You do not have permission to manage users';
    return;
  }
  await loadUsers();
});

onUnmounted(() => {
  error.value = null;
  users.value = [];
});
</script>

<style lang="scss">
.users-page {
  &__header {
    @media (max-width: 600px) {
      flex-direction: column;
      align-items: stretch;
      gap: 1rem;
    }
  }

  .q-table {
    &__card {
      box-shadow: 0 1px 5px rgba(0, 0, 0, 0.2);
    }
  }

  .q-dialog {
    &__inner {
      @media (min-width: 601px) {
        min-width: 500px;
        max-width: 800px;
      }
    }
  }
}
</style>
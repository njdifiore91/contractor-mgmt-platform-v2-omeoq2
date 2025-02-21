<template>
  <div class="onedrive-files-page q-pa-md">
    <!-- Header section -->
    <div class="row items-center justify-between q-mb-md">
      <h1 class="text-h5 q-my-none">Inspector Files</h1>
      
      <!-- File upload section -->
      <q-uploader
        v-if="hasFilePermission"
        :accept="allowedFileTypes.join(',')"
        :max-file-size="maxFileSize"
        :max-files="1"
        flat
        bordered
        @added="handleFileUpload"
        class="col-auto"
      >
        <template v-slot:header="scope">
          <div class="row items-center q-pa-sm">
            <q-btn
              icon="cloud_upload"
              flat
              dense
              @click="scope.pickFiles"
              :loading="scope.isUploading"
            >
              Upload File
            </q-btn>
          </div>
        </template>
      </q-uploader>
    </div>

    <!-- Files table -->
    <base-table
      :columns="tableColumns"
      :data="files"
      :loading="loading"
      row-key="id"
      virtual-scroll
    >
      <!-- Custom cell slot for actions -->
      <template v-slot:body-cell-actions="props">
        <q-td :props="props">
          <div class="row items-center justify-start q-gutter-x-sm">
            <q-btn
              flat
              dense
              round
              icon="download"
              @click="handleFileDownload(props.row)"
              :loading="loading"
            >
              <q-tooltip>Download File</q-tooltip>
            </q-btn>
            <q-btn
              v-if="hasFilePermission"
              flat
              dense
              round
              icon="delete"
              color="negative"
              @click="handleFileDelete(props.row)"
              :loading="loading"
            >
              <q-tooltip>Delete File</q-tooltip>
            </q-btn>
          </div>
        </q-td>
      </template>
    </base-table>

    <!-- Loading overlay -->
    <q-inner-loading :showing="loading">
      <q-spinner size="50px" color="primary" />
    </q-inner-loading>

    <!-- Error handling -->
    <q-dialog v-model="!!error">
      <q-card>
        <q-card-section class="row items-center">
          <q-avatar icon="error" color="negative" text-color="white" />
          <span class="q-ml-sm">{{ error }}</span>
        </q-card-section>
        <q-card-actions align="right">
          <q-btn flat label="Dismiss" color="primary" v-close-popup />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'; // ^3.3.0
import { QBtn, QUploader, useQuasar, QSpinner } from 'quasar'; // ^2.12.0
import useOneDrive from '@/composables/useOneDrive';
import BaseTable from '@/components/common/BaseTable';
import usePermissions from '@/composables/usePermissions';
import { DriveItem } from '@microsoft/microsoft-graph-types';

// Props
const props = defineProps({
  inspector: {
    type: Object,
    required: true
  }
});

// Composables
const $q = useQuasar();
const { checkPermission } = usePermissions();
const { files, uploadFile, getFileUrl, deleteFile, refreshFiles } = useOneDrive(props.inspector);

// Constants
const allowedFileTypes = ref(['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.txt', '.jpg', '.png']);
const maxFileSize = ref(104857600); // 100MB
const loading = ref(false);
const error = ref<string | null>(null);

// Computed
const hasFilePermission = computed(() => 
  checkPermission('view_inspector_files')
);

const tableColumns = computed(() => [
  {
    name: 'name',
    label: 'File Name',
    field: 'name',
    align: 'left',
    sortable: true
  },
  {
    name: 'lastModifiedDateTime',
    label: 'Modified',
    field: 'lastModifiedDateTime',
    align: 'left',
    sortable: true,
    format: (val: string) => new Date(val).toLocaleString()
  },
  {
    name: 'size',
    label: 'Size',
    field: 'size',
    align: 'right',
    sortable: true,
    format: (val: number) => formatFileSize(val)
  },
  {
    name: 'actions',
    label: 'Actions',
    field: 'actions',
    align: 'left'
  }
]);

// Methods
const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};

const handleFileUpload = async (files: File[]) => {
  if (!files.length) return;
  
  const file = files[0];
  loading.value = true;
  error.value = null;

  try {
    // Validate file type
    const fileExt = `.${file.name.split('.').pop()?.toLowerCase()}`;
    if (!allowedFileTypes.value.includes(fileExt)) {
      throw new Error('File type not allowed');
    }

    // Validate file size
    if (file.size > maxFileSize.value) {
      throw new Error(`File size exceeds ${formatFileSize(maxFileSize.value)}`);
    }

    await uploadFile(file);
    await refreshFiles();
    
    $q.notify({
      type: 'positive',
      message: 'File uploaded successfully'
    });
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Upload failed';
    $q.notify({
      type: 'negative',
      message: error.value
    });
  } finally {
    loading.value = false;
  }
};

const handleFileDownload = async (file: DriveItem) => {
  loading.value = true;
  error.value = null;

  try {
    const url = await getFileUrl(file.name as string);
    window.open(url, '_blank');
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Download failed';
    $q.notify({
      type: 'negative',
      message: error.value
    });
  } finally {
    loading.value = false;
  }
};

const handleFileDelete = async (file: DriveItem) => {
  const confirmed = await $q.dialog({
    title: 'Confirm Deletion',
    message: `Are you sure you want to delete ${file.name}?`,
    cancel: true,
    persistent: true
  });

  if (!confirmed) return;

  loading.value = true;
  error.value = null;

  try {
    await deleteFile(file.name as string);
    await refreshFiles();
    
    $q.notify({
      type: 'positive',
      message: 'File deleted successfully'
    });
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Delete failed';
    $q.notify({
      type: 'negative',
      message: error.value
    });
  } finally {
    loading.value = false;
  }
};

// Lifecycle hooks
onMounted(async () => {
  loading.value = true;
  try {
    await refreshFiles();
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load files';
  } finally {
    loading.value = false;
  }
});
</script>

<style lang="scss">
.onedrive-files-page {
  height: 100%;
  display: flex;
  flex-direction: column;

  .q-table {
    flex: 1;
    
    &__container {
      height: 100%;
    }
  }

  .q-uploader {
    max-width: 300px;
  }
}
</style>
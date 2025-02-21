/**
 * @file Vue composable for OneDrive file management functionality
 * Provides reactive state and methods for managing inspector documents in OneDrive
 * @version 1.0.0
 */

import { ref, computed } from 'vue'; // ^3.2.0
import { OneDriveFile } from '@microsoft/microsoft-graph-types'; // ^2.0.0
import { Inspector } from '../types/inspector';
import { 
  uploadFile as uploadFileService,
  getFileUrl as getFileUrlService,
  listFiles as listFilesService,
  deleteFile as deleteFileService
} from '../services/oneDrive';

// Constants
const DEBOUNCE_DELAY = 300;
const FILE_CACHE_DURATION = 5 * 60 * 1000; // 5 minutes

/**
 * Vue composable that provides reactive OneDrive file management functionality
 * @param inspector - Inspector object containing inspector details
 * @returns Object containing reactive state and file management methods
 */
export default function useOneDrive(inspector: Inspector) {
  // Reactive state
  const files = ref<OneDriveFile[]>([]);
  const isUploading = ref(false);
  const isLoading = ref(false);
  const isDeleting = ref(false);
  const error = ref<Error | null>(null);
  const uploadProgress = ref(0);
  const fileCache = new Map<string, { url: string; timestamp: number }>();

  // Computed properties
  const folderUrl = computed(() => 
    `/onedrive/inspectors/${inspector.inspectorId}`
  );

  const fileCount = computed(() => files.value.length);

  const isProcessing = computed(() => 
    isUploading.value || isLoading.value || isDeleting.value
  );

  /**
   * Uploads a file to the inspector's OneDrive folder
   * @param file - File to upload
   * @returns Promise resolving to the uploaded file information
   */
  const upload = async (file: File): Promise<OneDriveFile> => {
    try {
      isUploading.value = true;
      error.value = null;
      uploadProgress.value = 0;

      const result = await uploadFileService(
        inspector.inspectorId,
        file,
        (progress: number) => {
          uploadProgress.value = progress;
        }
      );

      await refreshFiles();
      return result;
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Upload failed');
      throw error.value;
    } finally {
      isUploading.value = false;
      uploadProgress.value = 0;
    }
  };

  /**
   * Gets a shareable URL for a file with caching
   * @param fileName - Name of the file
   * @returns Promise resolving to the shareable URL
   */
  const getUrl = async (fileName: string): Promise<string> => {
    const cacheKey = `${inspector.inspectorId}_${fileName}`;
    const cached = fileCache.get(cacheKey);

    if (cached && cached.timestamp > Date.now()) {
      return cached.url;
    }

    try {
      const url = await getFileUrlService(
        inspector.inspectorId,
        fileName,
        60 // URL expires in 60 minutes
      );

      fileCache.set(cacheKey, {
        url,
        timestamp: Date.now() + FILE_CACHE_DURATION
      });

      return url;
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Failed to get file URL');
      throw error.value;
    }
  };

  /**
   * Refreshes the list of files with debouncing
   */
  let refreshTimeout: NodeJS.Timeout;
  const refreshFiles = async (): Promise<void> => {
    clearTimeout(refreshTimeout);
    refreshTimeout = setTimeout(async () => {
      try {
        isLoading.value = true;
        error.value = null;
        files.value = await listFilesService(inspector.inspectorId);
      } catch (e) {
        error.value = e instanceof Error ? e : new Error('Failed to list files');
        throw error.value;
      } finally {
        isLoading.value = false;
      }
    }, DEBOUNCE_DELAY);
  };

  /**
   * Deletes a file from OneDrive
   * @param fileName - Name of the file to delete
   * @param permanent - Whether to permanently delete the file
   */
  const deleteFile = async (
    fileName: string,
    permanent: boolean = false
  ): Promise<void> => {
    try {
      isDeleting.value = true;
      error.value = null;

      await deleteFileService(inspector.inspectorId, fileName, permanent);
      fileCache.delete(`${inspector.inspectorId}_${fileName}`);
      await refreshFiles();
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Failed to delete file');
      throw error.value;
    } finally {
      isDeleting.value = false;
    }
  };

  // Initialize files list
  refreshFiles().catch(e => {
    error.value = e instanceof Error ? e : new Error('Initial file load failed');
  });

  // Clear error after 5 seconds
  const clearError = () => {
    if (error.value) {
      setTimeout(() => {
        error.value = null;
      }, 5000);
    }
  };

  return {
    // State
    files,
    isUploading,
    isLoading,
    isDeleting,
    isProcessing,
    error,
    uploadProgress,
    folderUrl,
    fileCount,

    // Methods
    upload,
    getUrl,
    refreshFiles,
    deleteFile,
    clearError
  };
}
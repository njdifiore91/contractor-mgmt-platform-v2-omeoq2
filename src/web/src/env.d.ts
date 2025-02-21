/// <reference types="vite/client" // Version: ^4.0.0" />

/**
 * Type definitions for environment variables used throughout the application.
 * These environment variables should be defined in the appropriate .env files
 * and follow the VITE_ prefix convention for client-side exposure.
 */
interface ImportMetaEnv {
  /** Base URL for backend API endpoints */
  readonly VITE_API_BASE_URL: string;

  /** Authentication provider configuration */
  readonly VITE_AUTH_DOMAIN: string;
  readonly VITE_AUTH_CLIENT_ID: string;

  /** Microsoft OneDrive integration settings */
  readonly VITE_ONEDRIVE_CLIENT_ID: string;
  readonly VITE_INSPECTOR_FILES_BASE_PATH: string;

  /** Azure Storage configuration */
  readonly VITE_STORAGE_ACCOUNT_NAME: string;
  readonly VITE_STORAGE_CONTAINER_NAME: string;

  /** Email service configuration */
  readonly VITE_EMAIL_SERVICE_URL: string;

  /** File handling configuration */
  readonly VITE_MAX_FILE_UPLOAD_SIZE: string;
  readonly VITE_ALLOWED_FILE_TYPES: string;

  /** Search and pagination settings */
  readonly VITE_DEFAULT_SEARCH_RADIUS: string;
  readonly VITE_LIST_PAGE_SIZE: string;
}

/**
 * Augment the ImportMeta interface to include our custom environment variables
 * This provides type safety when accessing import.meta.env throughout the application
 */
interface ImportMeta {
  readonly env: ImportMetaEnv;
}

// Export the ImportMetaEnv interface to be used in other parts of the application
export { ImportMetaEnv };
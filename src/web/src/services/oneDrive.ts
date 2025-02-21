/**
 * @file OneDrive service module for managing inspector files and folders
 * @version 1.0.0
 */

import axios, { AxiosError, AxiosRequestConfig } from 'axios';
import { DriveItem } from '@microsoft/microsoft-graph-types';
import { Inspector } from '../types/inspector';

// Constants for file operations and caching
const FILE_SIZE_LIMIT = 100 * 1024 * 1024; // 100MB
const ALLOWED_FILE_TYPES = ['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.txt', '.jpg', '.png'];
const CACHE_DURATION = 3600000; // 1 hour in milliseconds
const MAX_RETRY_ATTEMPTS = 3;

// Types for file operations
interface FileFilter {
  fileTypes?: string[];
  searchTerm?: string;
  dateRange?: {
    start: Date;
    end: Date;
  };
}

interface SortOptions {
  field: 'name' | 'size' | 'lastModifiedDateTime';
  direction: 'asc' | 'desc';
}

type ProgressCallback = (progress: number) => void;

// Cache interface
interface CacheEntry<T> {
  data: T;
  timestamp: number;
}

/**
 * OneDrive service class for managing inspector files
 */
export class OneDriveService {
  private cache: Map<string, CacheEntry<any>> = new Map();
  private baseUrl: string = '/api/onedrive';

  /**
   * Uploads a file to an inspector's OneDrive folder
   * @param inspectorId - Unique identifier for the inspector
   * @param file - File to upload
   * @param onProgress - Optional callback for upload progress
   * @returns Promise resolving to the uploaded file information
   */
  async uploadFile(
    inspectorId: string,
    file: File,
    onProgress?: ProgressCallback
  ): Promise<DriveItem> {
    // Validate file size
    if (file.size > FILE_SIZE_LIMIT) {
      throw new Error(`File size exceeds limit of ${FILE_SIZE_LIMIT / 1024 / 1024}MB`);
    }

    // Validate file type
    const fileExtension = `.${file.name.split('.').pop()?.toLowerCase()}`;
    if (!ALLOWED_FILE_TYPES.includes(fileExtension)) {
      throw new Error(`File type ${fileExtension} is not allowed`);
    }

    const formData = new FormData();
    formData.append('file', file);
    formData.append('inspectorId', inspectorId);

    let attempt = 0;
    while (attempt < MAX_RETRY_ATTEMPTS) {
      try {
        const response = await axios.post<DriveItem>(
          `${this.baseUrl}/upload`,
          formData,
          {
            headers: {
              'Content-Type': 'multipart/form-data',
            },
            onUploadProgress: (progressEvent) => {
              if (onProgress && progressEvent.total) {
                const progress = (progressEvent.loaded / progressEvent.total) * 100;
                onProgress(progress);
              }
            },
          }
        );

        // Cache the uploaded file information
        this.setCache(`file_${inspectorId}_${file.name}`, response.data);
        return response.data;
      } catch (error) {
        attempt++;
        if (attempt === MAX_RETRY_ATTEMPTS) {
          throw this.handleError(error as AxiosError);
        }
        await new Promise(resolve => setTimeout(resolve, 1000 * attempt));
      }
    }
    throw new Error('Upload failed after maximum retry attempts');
  }

  /**
   * Gets a time-limited shareable URL for accessing a specific file
   * @param inspectorId - Unique identifier for the inspector
   * @param fileName - Name of the file
   * @param expirationMinutes - Number of minutes until URL expires
   * @returns Promise resolving to the shareable URL
   */
  async getFileUrl(
    inspectorId: string,
    fileName: string,
    expirationMinutes: number
  ): Promise<string> {
    const cacheKey = `url_${inspectorId}_${fileName}`;
    const cached = this.getCache<string>(cacheKey);
    if (cached) return cached;

    const encodedFileName = encodeURIComponent(fileName);
    const response = await axios.get<string>(
      `${this.baseUrl}/share/${inspectorId}/${encodedFileName}`,
      {
        params: { expirationMinutes },
      }
    );

    this.setCache(cacheKey, response.data, expirationMinutes * 60 * 1000);
    return response.data;
  }

  /**
   * Lists all files in an inspector's OneDrive folder
   * @param inspectorId - Unique identifier for the inspector
   * @param filter - Optional filtering criteria
   * @param sort - Optional sorting options
   * @returns Promise resolving to an array of file information
   */
  async listFiles(
    inspectorId: string,
    filter?: FileFilter,
    sort?: SortOptions
  ): Promise<DriveItem[]> {
    const cacheKey = `files_${inspectorId}_${JSON.stringify(filter)}_${JSON.stringify(sort)}`;
    const cached = this.getCache<DriveItem[]>(cacheKey);
    if (cached) return cached;

    const response = await axios.get<DriveItem[]>(
      `${this.baseUrl}/files/${inspectorId}`,
      {
        params: {
          ...filter,
          sortField: sort?.field,
          sortDirection: sort?.direction,
        },
      }
    );

    this.setCache(cacheKey, response.data);
    return response.data;
  }

  /**
   * Deletes a file from an inspector's OneDrive folder
   * @param inspectorId - Unique identifier for the inspector
   * @param fileName - Name of the file to delete
   * @param permanent - Whether to permanently delete the file
   * @returns Promise resolving to deletion success status
   */
  async deleteFile(
    inspectorId: string,
    fileName: string,
    permanent: boolean = false
  ): Promise<boolean> {
    const encodedFileName = encodeURIComponent(fileName);
    await axios.delete(`${this.baseUrl}/delete/${inspectorId}/${encodedFileName}`, {
      params: { permanent },
    });

    // Clear related cache entries
    this.clearFileCache(inspectorId, fileName);
    return true;
  }

  /**
   * Sets a cache entry with optional expiration
   */
  private setCache<T>(key: string, data: T, duration: number = CACHE_DURATION): void {
    this.cache.set(key, {
      data,
      timestamp: Date.now() + duration,
    });
  }

  /**
   * Retrieves a cache entry if valid
   */
  private getCache<T>(key: string): T | null {
    const entry = this.cache.get(key);
    if (entry && entry.timestamp > Date.now()) {
      return entry.data as T;
    }
    this.cache.delete(key);
    return null;
  }

  /**
   * Clears cache entries related to a specific file
   */
  private clearFileCache(inspectorId: string, fileName: string): void {
    const keysToDelete = [
      `file_${inspectorId}_${fileName}`,
      `url_${inspectorId}_${fileName}`,
      `files_${inspectorId}`,
    ];
    keysToDelete.forEach(key => this.cache.delete(key));
  }

  /**
   * Handles and transforms API errors
   */
  private handleError(error: AxiosError): Error {
    if (error.response) {
      return new Error(
        `OneDrive operation failed: ${error.response.status} - ${error.response.statusText}`
      );
    }
    return new Error(`OneDrive operation failed: ${error.message}`);
  }
}

// Export singleton instance
export const oneDriveService = new OneDriveService();
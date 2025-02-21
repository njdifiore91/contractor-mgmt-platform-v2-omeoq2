import { defineStore } from 'pinia'; // ^2.1.4
import { ref, computed } from 'vue'; // ^3.3.4
import { QuickLink } from '../types/admin';
import { getQuickLinks, updateQuickLink } from '../services/api/admin';
import { PERMISSION_TYPES, hasPermission } from '../utils/permissions';
import { validateUrl } from '../utils/validation';

/**
 * Pinia store for managing quick links state and operations
 */
export const useQuickLinksStore = defineStore('quickLinks', () => {
  // State
  const quickLinks = ref<QuickLink[]>([]);
  const loading = ref<boolean>(false);
  const error = ref<string | null>(null);

  // Maximum retry attempts for API calls
  const MAX_RETRIES = 3;

  // Computed properties
  const sortedQuickLinks = computed(() => {
    return quickLinks.value
      .filter(link => link.isActive)
      .sort((a, b) => a.order - b.order);
  });

  const canEditLinks = computed(() => {
    return hasPermission([PERMISSION_TYPES.EDIT_LINKS], PERMISSION_TYPES.EDIT_LINKS);
  });

  /**
   * Validates quick link data before saving
   * @param quickLink Quick link to validate
   * @returns Validation result object
   */
  const validateQuickLink = (quickLink: QuickLink): { isValid: boolean; error?: string } => {
    if (!quickLink.label?.trim()) {
      return { isValid: false, error: 'Label is required' };
    }

    if (!quickLink.link?.trim()) {
      return { isValid: false, error: 'Link URL is required' };
    }

    if (!validateUrl(quickLink.link)) {
      return { isValid: false, error: 'Invalid URL format' };
    }

    if (typeof quickLink.order !== 'number' || quickLink.order < 0) {
      return { isValid: false, error: 'Order must be a non-negative number' };
    }

    return { isValid: true };
  };

  /**
   * Fetches quick links from the API with retry logic
   */
  const fetchQuickLinks = async () => {
    loading.value = true;
    error.value = null;
    let retries = 0;

    while (retries < MAX_RETRIES) {
      try {
        const links = await getQuickLinks();
        quickLinks.value = links;
        loading.value = false;
        return;
      } catch (err) {
        retries++;
        if (retries === MAX_RETRIES) {
          error.value = err instanceof Error ? err.message : 'Failed to fetch quick links';
          loading.value = false;
          throw err;
        }
        // Exponential backoff
        await new Promise(resolve => setTimeout(resolve, Math.pow(2, retries) * 1000));
      }
    }
  };

  /**
   * Updates a quick link with validation and optimistic updates
   * @param quickLink Quick link to update
   */
  const updateQuickLink = async (quickLink: QuickLink) => {
    if (!canEditLinks.value) {
      error.value = 'Permission denied: Cannot edit quick links';
      throw new Error('Permission denied');
    }

    const validation = validateQuickLink(quickLink);
    if (!validation.isValid) {
      error.value = validation.error;
      throw new Error(validation.error);
    }

    // Store original state for rollback
    const originalLinks = [...quickLinks.value];
    const linkIndex = quickLinks.value.findIndex(link => link.id === quickLink.id);

    try {
      // Optimistic update
      if (linkIndex !== -1) {
        quickLinks.value[linkIndex] = { ...quickLink };
      }

      const updatedLink = await updateQuickLink(quickLink);
      
      // Update with server response
      if (linkIndex !== -1) {
        quickLinks.value[linkIndex] = updatedLink;
      }

      error.value = null;
    } catch (err) {
      // Rollback on failure
      quickLinks.value = originalLinks;
      error.value = err instanceof Error ? err.message : 'Failed to update quick link';
      throw err;
    }
  };

  return {
    // State
    quickLinks,
    loading,
    error,

    // Computed
    sortedQuickLinks,
    canEditLinks,

    // Actions
    fetchQuickLinks,
    updateQuickLink
  };
});
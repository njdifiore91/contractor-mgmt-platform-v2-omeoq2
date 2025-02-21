/**
 * @file Inspector Pinia store implementation
 * Manages inspector state and operations with comprehensive error handling
 * @version 1.0.0
 */

import { defineStore } from 'pinia'; // ^2.0.0
import { ref, computed, watch } from 'vue'; // ^3.2.0
import type { ApiResponse } from '@types/api-response'; // ^1.0.0
import { 
  Inspector, 
  InspectorSearch, 
  InspectorStatus, 
  InspectorMobilization, 
  InspectorDemobilization,
  DrugTest,
  DrugTestResult 
} from '../types/inspector';

interface ErrorState {
  code: string;
  message: string;
}

export const useInspectorStore = defineStore('inspector', () => {
  // State
  const inspectors = ref<Inspector[]>([]);
  const selectedInspector = ref<Inspector | null>(null);
  const loading = ref<boolean>(false);
  const error = ref<ErrorState | null>(null);
  const lastSearch = ref<InspectorSearch | null>(null);

  // Getters
  const hasInspectors = computed(() => inspectors.value.length > 0);

  const sortedInspectors = computed(() => {
    return [...inspectors.value].sort((a, b) => {
      const lastNameCompare = a.lastName.localeCompare(b.lastName);
      return lastNameCompare !== 0 ? lastNameCompare : a.firstName.localeCompare(b.firstName);
    });
  });

  const activeInspectors = computed(() => {
    return inspectors.value.filter(inspector => inspector.status === InspectorStatus.ACTIVE);
  });

  // Actions
  const clearError = () => {
    error.value = null;
  };

  const setError = (code: string, message: string) => {
    error.value = { code, message };
  };

  const validateZipCode = (zipCode: string): boolean => {
    return /^\d{5}(-\d{4})?$/.test(zipCode);
  };

  const validateRadius = (radius: number): boolean => {
    return radius >= 0 && radius <= 500;
  };

  const searchInspectors = async (searchParams: InspectorSearch): Promise<ApiResponse<Inspector[]>> => {
    try {
      if (!validateZipCode(searchParams.zipCode)) {
        throw new Error('Invalid zip code format');
      }

      if (!validateRadius(searchParams.radius)) {
        throw new Error('Radius must be between 0 and 500 miles');
      }

      loading.value = true;
      clearError();
      lastSearch.value = { ...searchParams };

      const response = await fetch('/api/inspectors/search', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(searchParams),
      });

      if (!response.ok) {
        throw new Error(`Search failed: ${response.statusText}`);
      }

      const data = await response.json();
      inspectors.value = data.inspectors;

      return {
        success: true,
        data: inspectors.value,
        message: `Found ${inspectors.value.length} inspectors`,
      };

    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An unknown error occurred';
      setError('SEARCH_ERROR', errorMessage);
      return {
        success: false,
        error: errorMessage,
        data: [],
      };
    } finally {
      loading.value = false;
    }
  };

  const mobilizeInspector = async (
    inspectorId: number, 
    details: InspectorMobilization
  ): Promise<ApiResponse<boolean>> => {
    try {
      loading.value = true;
      clearError();

      const inspector = inspectors.value.find(i => i.id === inspectorId);
      if (!inspector) {
        throw new Error('Inspector not found');
      }

      if (inspector.status === InspectorStatus.MOBILIZED) {
        throw new Error('Inspector is already mobilized');
      }

      const response = await fetch(`/api/inspectors/${inspectorId}/mobilize`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(details),
      });

      if (!response.ok) {
        throw new Error(`Mobilization failed: ${response.statusText}`);
      }

      // Update local state
      inspector.status = InspectorStatus.MOBILIZED;
      inspector.mobilizationDate = new Date();

      return {
        success: true,
        data: true,
        message: `Successfully mobilized inspector ${inspector.firstName} ${inspector.lastName}`,
      };

    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An unknown error occurred';
      setError('MOBILIZATION_ERROR', errorMessage);
      return {
        success: false,
        error: errorMessage,
        data: false,
      };
    } finally {
      loading.value = false;
    }
  };

  const demobilizeInspector = async (
    inspectorId: number,
    details: InspectorDemobilization
  ): Promise<ApiResponse<boolean>> => {
    try {
      loading.value = true;
      clearError();

      const inspector = inspectors.value.find(i => i.id === inspectorId);
      if (!inspector) {
        throw new Error('Inspector not found');
      }

      if (inspector.status !== InspectorStatus.MOBILIZED) {
        throw new Error('Inspector is not currently mobilized');
      }

      const response = await fetch(`/api/inspectors/${inspectorId}/demobilize`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(details),
      });

      if (!response.ok) {
        throw new Error(`Demobilization failed: ${response.statusText}`);
      }

      // Update local state
      inspector.status = InspectorStatus.DEMOBILIZED;
      inspector.demobilizationDate = details.demobDate;
      inspector.demobilizationReason = details.demobReason;

      return {
        success: true,
        data: true,
        message: `Successfully demobilized inspector ${inspector.firstName} ${inspector.lastName}`,
      };

    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An unknown error occurred';
      setError('DEMOBILIZATION_ERROR', errorMessage);
      return {
        success: false,
        error: errorMessage,
        data: false,
      };
    } finally {
      loading.value = false;
    }
  };

  const addDrugTest = async (
    inspectorId: number,
    drugTest: DrugTest
  ): Promise<ApiResponse<DrugTest>> => {
    try {
      loading.value = true;
      clearError();

      const inspector = inspectors.value.find(i => i.id === inspectorId);
      if (!inspector) {
        throw new Error('Inspector not found');
      }

      const response = await fetch(`/api/inspectors/${inspectorId}/drug-tests`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(drugTest),
      });

      if (!response.ok) {
        throw new Error(`Failed to add drug test: ${response.statusText}`);
      }

      const newDrugTest = await response.json();
      
      // Update local state
      inspector.drugTests.push(newDrugTest);
      inspector.lastDrugTestDate = newDrugTest.testDate;
      inspector.lastDrugTestResult = newDrugTest.result;

      return {
        success: true,
        data: newDrugTest,
        message: 'Drug test record added successfully',
      };

    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An unknown error occurred';
      setError('DRUG_TEST_ERROR', errorMessage);
      return {
        success: false,
        error: errorMessage,
        data: null,
      };
    } finally {
      loading.value = false;
    }
  };

  // Watch for changes in selected inspector
  watch(selectedInspector, (newInspector) => {
    if (newInspector) {
      // Clear any existing errors when selecting a new inspector
      clearError();
    }
  });

  return {
    // State
    inspectors,
    selectedInspector,
    loading,
    error,
    lastSearch,

    // Getters
    hasInspectors,
    sortedInspectors,
    activeInspectors,

    // Actions
    searchInspectors,
    mobilizeInspector,
    demobilizeInspector,
    addDrugTest,
    clearError,
  };
});
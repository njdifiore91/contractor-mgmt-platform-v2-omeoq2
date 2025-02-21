// @version 1.0.0
// axios ^1.3.0
import axios, { AxiosError } from 'axios';
import type { ApiResponse } from '@types/api-response';
import {
  Inspector,
  InspectorMobilization,
  InspectorSearch,
  InspectorStatus,
  DrugTest,
  DrugTestType,
  TestFrequency,
  DrugTestResult,
  DemobilizationReason,
  InspectorDemobilization
} from '../types/inspector';

/**
 * Base API URL for inspector endpoints
 */
const INSPECTOR_API_BASE = '/api/inspector';

/**
 * Configuration for request timeouts and retries
 */
const API_CONFIG = {
  timeout: 30000,
  retries: 3,
  retryDelay: 1000
};

/**
 * Searches for inspectors based on geographical and other criteria
 * @param searchParams - Search parameters including zip code and radius
 * @returns Promise resolving to array of matching inspectors
 */
export async function searchInspectors(searchParams: InspectorSearch): Promise<Inspector[]> {
  try {
    // Validate zip code format
    const zipCodeRegex = /^\d{5}(-\d{4})?$/;
    if (!zipCodeRegex.test(searchParams.zipCode)) {
      throw new Error('Invalid zip code format');
    }

    // Validate radius range (assumed to be in miles)
    if (searchParams.radius < 0 || searchParams.radius > 500) {
      throw new Error('Radius must be between 0 and 500 miles');
    }

    const response = await axios.get<ApiResponse<Inspector[]>>(
      `${INSPECTOR_API_BASE}/search`,
      {
        params: searchParams,
        timeout: API_CONFIG.timeout
      }
    );

    return response.data.data;
  } catch (error) {
    if (error instanceof AxiosError) {
      throw new Error(`Inspector search failed: ${error.response?.data?.message || error.message}`);
    }
    throw error;
  }
}

/**
 * Mobilizes an inspector and sends email notification
 * @param inspectorId - ID of the inspector to mobilize
 * @param mobilizationDetails - Mobilization information including project details
 * @returns Promise resolving to mobilization status
 */
export async function mobilizeInspector(
  inspectorId: number,
  mobilizationDetails: InspectorMobilization
): Promise<{ success: boolean; emailSent: boolean }> {
  try {
    // Validate required mobilization fields
    if (!mobilizationDetails.mobDate || !mobilizationDetails.hireType || !mobilizationDetails.projectDetails) {
      throw new Error('Missing required mobilization details');
    }

    const response = await axios.post<ApiResponse<{ success: boolean; emailSent: boolean }>>(
      `${INSPECTOR_API_BASE}/${inspectorId}/mobilize`,
      mobilizationDetails,
      { timeout: API_CONFIG.timeout }
    );

    return response.data.data;
  } catch (error) {
    if (error instanceof AxiosError) {
      throw new Error(`Mobilization failed: ${error.response?.data?.message || error.message}`);
    }
    throw error;
  }
}

/**
 * Demobilizes an inspector
 * @param inspectorId - ID of the inspector to demobilize
 * @param demobilizationDetails - Demobilization information including reason and date
 * @returns Promise resolving to demobilization success status
 */
export async function demobilizeInspector(
  inspectorId: number,
  demobilizationDetails: InspectorDemobilization
): Promise<boolean> {
  try {
    const response = await axios.post<ApiResponse<boolean>>(
      `${INSPECTOR_API_BASE}/${inspectorId}/demobilize`,
      demobilizationDetails,
      { timeout: API_CONFIG.timeout }
    );

    return response.data.data;
  } catch (error) {
    if (error instanceof AxiosError) {
      throw new Error(`Demobilization failed: ${error.response?.data?.message || error.message}`);
    }
    throw error;
  }
}

/**
 * Manages drug test records for an inspector
 * @param inspectorId - ID of the inspector
 * @param drugTest - Drug test record details
 * @returns Promise resolving to created/updated drug test record
 */
export async function manageDrugTest(
  inspectorId: number,
  drugTest: DrugTest
): Promise<DrugTest> {
  try {
    const response = await axios.post<ApiResponse<DrugTest>>(
      `${INSPECTOR_API_BASE}/${inspectorId}/drug-tests`,
      drugTest,
      { timeout: API_CONFIG.timeout }
    );

    return response.data.data;
  } catch (error) {
    if (error instanceof AxiosError) {
      throw new Error(`Drug test management failed: ${error.response?.data?.message || error.message}`);
    }
    throw error;
  }
}

/**
 * Assigns equipment to an inspector
 * @param inspectorId - ID of the inspector
 * @param equipmentId - ID of the equipment to assign
 * @param assignmentDetails - Equipment assignment details including condition
 * @returns Promise resolving to assignment success status
 */
export async function assignEquipment(
  inspectorId: number,
  equipmentId: number,
  assignmentDetails: {
    outCondition: string;
    outDate: Date;
  }
): Promise<boolean> {
  try {
    const response = await axios.post<ApiResponse<boolean>>(
      `${INSPECTOR_API_BASE}/${inspectorId}/equipment/${equipmentId}/assign`,
      assignmentDetails,
      { timeout: API_CONFIG.timeout }
    );

    return response.data.data;
  } catch (error) {
    if (error instanceof AxiosError) {
      throw new Error(`Equipment assignment failed: ${error.response?.data?.message || error.message}`);
    }
    throw error;
  }
}

/**
 * Tracks equipment return from an inspector
 * @param inspectorId - ID of the inspector
 * @param equipmentId - ID of the equipment being returned
 * @param returnDetails - Equipment return details including condition
 * @returns Promise resolving to return tracking success status
 */
export async function trackEquipmentReturn(
  inspectorId: number,
  equipmentId: number,
  returnDetails: {
    returnDate: Date;
    returnCondition: string;
    notes?: string;
  }
): Promise<boolean> {
  try {
    const response = await axios.post<ApiResponse<boolean>>(
      `${INSPECTOR_API_BASE}/${inspectorId}/equipment/${equipmentId}/return`,
      returnDetails,
      { timeout: API_CONFIG.timeout }
    );

    return response.data.data;
  } catch (error) {
    if (error instanceof AxiosError) {
      throw new Error(`Equipment return tracking failed: ${error.response?.data?.message || error.message}`);
    }
    throw error;
  }
}

/**
 * Retrieves inspector details by ID
 * @param inspectorId - ID of the inspector
 * @returns Promise resolving to inspector details
 */
export async function getInspectorById(inspectorId: number): Promise<Inspector> {
  try {
    const response = await axios.get<ApiResponse<Inspector>>(
      `${INSPECTOR_API_BASE}/${inspectorId}`,
      { timeout: API_CONFIG.timeout }
    );

    return response.data.data;
  } catch (error) {
    if (error instanceof AxiosError) {
      throw new Error(`Failed to retrieve inspector details: ${error.response?.data?.message || error.message}`);
    }
    throw error;
  }
}
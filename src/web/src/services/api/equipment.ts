/**
 * @file Equipment API service module
 * Handles equipment-related HTTP requests with comprehensive error handling
 * @version 1.0.0
 */

import axios, { AxiosError } from 'axios'; // ^1.4.0
import createError from 'http-errors'; // ^2.0.0
import { Equipment, EquipmentAssignment, EquipmentReturn } from '../../types/equipment';

// Constants for API configuration
const API_BASE_URL = '/api/equipment';
const RETRY_ATTEMPTS = 3;
const INITIAL_RETRY_DELAY = 1000;

/**
 * Configuration for axios retry behavior
 */
const axiosConfig = {
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
};

/**
 * Validates if user has Edit Equipment permission
 * @throws {Error} If permission check fails
 */
const validateEditPermission = async (): Promise<void> => {
  try {
    const response = await axios.get('/api/permissions/check/EditEquipment');
    if (!response.data.hasPermission) {
      throw createError(403, 'User lacks Edit Equipment permission');
    }
  } catch (error) {
    throw createError(403, 'Permission validation failed');
  }
};

/**
 * Implements exponential backoff retry logic
 * @param attempt Current retry attempt number
 * @returns Delay in milliseconds before next retry
 */
const getRetryDelay = (attempt: number): number => {
  return Math.min(INITIAL_RETRY_DELAY * Math.pow(2, attempt), 10000);
};

/**
 * Retrieves equipment list for a specific company
 * @param companyId Company identifier
 * @returns Promise resolving to array of equipment items
 * @throws {Error} On API or permission failures
 */
export const getEquipmentByCompany = async (companyId: number): Promise<Equipment[]> => {
  await validateEditPermission();

  let attempt = 0;
  while (attempt < RETRY_ATTEMPTS) {
    try {
      const response = await axios.get(
        `${API_BASE_URL}/company/${companyId}`,
        axiosConfig
      );

      // Transform date strings to Date objects
      return response.data.map((item: any) => ({
        ...item,
        assignedDate: item.assignedDate ? new Date(item.assignedDate) : null,
        returnedDate: item.returnedDate ? new Date(item.returnedDate) : null
      }));

    } catch (error) {
      attempt++;
      if (attempt === RETRY_ATTEMPTS || (error as AxiosError).response?.status === 403) {
        throw createError(
          (error as AxiosError).response?.status || 500,
          `Failed to fetch equipment: ${(error as Error).message}`
        );
      }
      await new Promise(resolve => setTimeout(resolve, getRetryDelay(attempt)));
    }
  }
  throw createError(500, 'Maximum retry attempts reached');
};

/**
 * Assigns equipment to an inspector
 * @param assignment Equipment assignment details
 * @returns Promise resolving to updated equipment data
 * @throws {Error} On validation or API failures
 */
export const assignEquipmentToInspector = async (
  assignment: EquipmentAssignment
): Promise<Equipment> => {
  await validateEditPermission();

  if (!assignment.equipmentId || !assignment.inspectorId) {
    throw createError(400, 'Invalid assignment data: Missing required fields');
  }

  try {
    const response = await axios.post(
      `${API_BASE_URL}/assign`,
      {
        ...assignment,
        assignmentDate: assignment.assignmentDate.toISOString()
      },
      axiosConfig
    );

    return {
      ...response.data,
      assignmentDate: new Date(response.data.assignmentDate)
    };

  } catch (error) {
    throw createError(
      (error as AxiosError).response?.status || 500,
      `Equipment assignment failed: ${(error as Error).message}`
    );
  }
};

/**
 * Records equipment return from an inspector
 * @param returnData Equipment return details
 * @returns Promise resolving to updated equipment data
 * @throws {Error} On validation or API failures
 */
export const recordEquipmentReturn = async (
  returnData: EquipmentReturn
): Promise<Equipment> => {
  await validateEditPermission();

  if (!returnData.equipmentId || !returnData.returnCondition) {
    throw createError(400, 'Invalid return data: Missing required fields');
  }

  try {
    const response = await axios.post(
      `${API_BASE_URL}/return`,
      {
        ...returnData,
        returnDate: returnData.returnDate.toISOString()
      },
      axiosConfig
    );

    return {
      ...response.data,
      returnDate: new Date(response.data.returnDate)
    };

  } catch (error) {
    throw createError(
      (error as AxiosError).response?.status || 500,
      `Equipment return recording failed: ${(error as Error).message}`
    );
  }
};
/**
 * @file Admin API service module
 * Handles API calls to admin endpoints for managing users, code types, and quick links
 * @version 1.0.0
 */

import axios from 'axios'; // ^1.4.0
import { CodeType, QuickLink, User } from '../types/admin';

/**
 * Custom error class for admin API errors with enhanced error details
 */
class AdminApiError extends Error {
  constructor(
    message: string,
    public statusCode?: number,
    public endpoint?: string
  ) {
    super(message);
    this.name = 'AdminApiError';
  }
}

/**
 * Permission validation error
 */
class PermissionError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'PermissionError';
  }
}

/**
 * Base API configuration
 */
const API_CONFIG = {
  baseURL: '/api/admin',
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json'
  }
};

/**
 * Retrieves all users from the system
 * @throws {PermissionError} If user lacks 'Edit Users' permission
 * @throws {AdminApiError} If API request fails
 * @returns {Promise<User[]>} Array of users
 */
export async function getUsers(): Promise<User[]> {
  try {
    const response = await axios.get<User[]>(`${API_CONFIG.baseURL}/users`, API_CONFIG);
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 403) {
        throw new PermissionError('User lacks permission to edit users');
      }
      throw new AdminApiError(
        error.message,
        error.response?.status,
        'getUsers'
      );
    }
    throw error;
  }
}

/**
 * Creates a new user
 * @param user User data to create
 * @throws {PermissionError} If user lacks 'Edit Users' permission
 * @throws {AdminApiError} If API request fails
 * @returns {Promise<User>} Created user
 */
export async function createUser(user: Omit<User, 'id'>): Promise<User> {
  try {
    const response = await axios.post<User>(
      `${API_CONFIG.baseURL}/users`,
      user,
      API_CONFIG
    );
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 403) {
        throw new PermissionError('User lacks permission to create users');
      }
      throw new AdminApiError(
        error.message,
        error.response?.status,
        'createUser'
      );
    }
    throw error;
  }
}

/**
 * Updates an existing user
 * @param id User ID to update
 * @param user Updated user data
 * @throws {PermissionError} If user lacks 'Edit Users' permission
 * @throws {AdminApiError} If API request fails
 * @returns {Promise<User>} Updated user
 */
export async function updateUser(id: number, user: Partial<User>): Promise<User> {
  try {
    const response = await axios.put<User>(
      `${API_CONFIG.baseURL}/users/${id}`,
      user,
      API_CONFIG
    );
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 403) {
        throw new PermissionError('User lacks permission to update users');
      }
      throw new AdminApiError(
        error.message,
        error.response?.status,
        'updateUser'
      );
    }
    throw error;
  }
}

/**
 * Retrieves all code types and their associated codes
 * @throws {PermissionError} If user lacks 'Edit Codes' permission
 * @throws {AdminApiError} If API request fails
 * @returns {Promise<CodeType[]>} Array of code types
 */
export async function getCodeTypes(): Promise<CodeType[]> {
  try {
    const response = await axios.get<CodeType[]>(
      `${API_CONFIG.baseURL}/code-types`,
      API_CONFIG
    );
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 403) {
        throw new PermissionError('User lacks permission to view code types');
      }
      throw new AdminApiError(
        error.message,
        error.response?.status,
        'getCodeTypes'
      );
    }
    throw error;
  }
}

/**
 * Creates a new code type
 * @param codeType Code type data to create
 * @throws {PermissionError} If user lacks 'Edit Codes' permission
 * @throws {AdminApiError} If API request fails
 * @returns {Promise<CodeType>} Created code type
 */
export async function createCodeType(codeType: Omit<CodeType, 'id'>): Promise<CodeType> {
  try {
    const response = await axios.post<CodeType>(
      `${API_CONFIG.baseURL}/code-types`,
      codeType,
      API_CONFIG
    );
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 403) {
        throw new PermissionError('User lacks permission to create code types');
      }
      throw new AdminApiError(
        error.message,
        error.response?.status,
        'createCodeType'
      );
    }
    throw error;
  }
}

/**
 * Updates an existing code type
 * @param id Code type ID to update
 * @param codeType Updated code type data
 * @throws {PermissionError} If user lacks 'Edit Codes' permission
 * @throws {AdminApiError} If API request fails
 * @returns {Promise<CodeType>} Updated code type
 */
export async function updateCodeType(
  id: number,
  codeType: Partial<CodeType>
): Promise<CodeType> {
  try {
    const response = await axios.put<CodeType>(
      `${API_CONFIG.baseURL}/code-types/${id}`,
      codeType,
      API_CONFIG
    );
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 403) {
        throw new PermissionError('User lacks permission to update code types');
      }
      throw new AdminApiError(
        error.message,
        error.response?.status,
        'updateCodeType'
      );
    }
    throw error;
  }
}

/**
 * Retrieves all quick links
 * @throws {PermissionError} If user lacks 'Edit Links' permission
 * @throws {AdminApiError} If API request fails
 * @returns {Promise<QuickLink[]>} Array of quick links
 */
export async function getQuickLinks(): Promise<QuickLink[]> {
  try {
    const response = await axios.get<QuickLink[]>(
      `${API_CONFIG.baseURL}/quick-links`,
      API_CONFIG
    );
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 403) {
        throw new PermissionError('User lacks permission to view quick links');
      }
      throw new AdminApiError(
        error.message,
        error.response?.status,
        'getQuickLinks'
      );
    }
    throw error;
  }
}

/**
 * Creates a new quick link
 * @param quickLink Quick link data to create
 * @throws {PermissionError} If user lacks 'Edit Links' permission
 * @throws {AdminApiError} If API request fails
 * @returns {Promise<QuickLink>} Created quick link
 */
export async function createQuickLink(
  quickLink: Omit<QuickLink, 'id'>
): Promise<QuickLink> {
  try {
    const response = await axios.post<QuickLink>(
      `${API_CONFIG.baseURL}/quick-links`,
      quickLink,
      API_CONFIG
    );
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 403) {
        throw new PermissionError('User lacks permission to create quick links');
      }
      throw new AdminApiError(
        error.message,
        error.response?.status,
        'createQuickLink'
      );
    }
    throw error;
  }
}

/**
 * Updates an existing quick link
 * @param id Quick link ID to update
 * @param quickLink Updated quick link data
 * @throws {PermissionError} If user lacks 'Edit Links' permission
 * @throws {AdminApiError} If API request fails
 * @returns {Promise<QuickLink>} Updated quick link
 */
export async function updateQuickLink(
  id: number,
  quickLink: Partial<QuickLink>
): Promise<QuickLink> {
  try {
    const response = await axios.put<QuickLink>(
      `${API_CONFIG.baseURL}/quick-links/${id}`,
      quickLink,
      API_CONFIG
    );
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 403) {
        throw new PermissionError('User lacks permission to update quick links');
      }
      throw new AdminApiError(
        error.message,
        error.response?.status,
        'updateQuickLink'
      );
    }
    throw error;
  }
}
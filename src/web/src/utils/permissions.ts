// @version 1.0.0
// @package vue-router@4.2.4

import { RouteLocationNormalized } from 'vue-router';
import { UserRole, ValidationRule } from '../types/common';

/**
 * Enum defining all available permission types in the application
 */
export enum PERMISSION_TYPES {
  EDIT_LINKS = 'edit_links',
  EDIT_CODES = 'edit_codes',
  EDIT_USERS = 'edit_users',
  EDIT_EQUIPMENT = 'edit_equipment',
  MANAGE_DRUG_TESTS = 'manage_drug_tests',
  MANAGE_CUSTOMERS = 'manage_customers',
  VIEW_INSPECTOR_FILES = 'view_inspector_files',
  MOBILIZE_INSPECTOR = 'mobilize_inspector'
}

/**
 * Default permission policy when no specific permissions are defined
 */
const DEFAULT_PERMISSION_POLICY = 'deny';

/**
 * Comprehensive mapping of route paths to required permissions
 * Based on section 3 of the technical specification
 */
const ROUTE_PERMISSIONS: Record<string, PERMISSION_TYPES[]> = {
  // Admin Pages
  '/admin/quick-links': [PERMISSION_TYPES.EDIT_LINKS],
  '/admin/codes': [PERMISSION_TYPES.EDIT_CODES],
  '/admin/users': [PERMISSION_TYPES.EDIT_USERS],
  
  // Equipment Pages
  '/equipment': [PERMISSION_TYPES.EDIT_EQUIPMENT],
  '/equipment/assign': [PERMISSION_TYPES.EDIT_EQUIPMENT],
  '/equipment/return': [PERMISSION_TYPES.EDIT_EQUIPMENT],
  
  // Inspector Pages
  '/inspectors/drug-tests': [PERMISSION_TYPES.MANAGE_DRUG_TESTS, PERMISSION_TYPES.EDIT_USERS],
  '/inspectors/files': [PERMISSION_TYPES.VIEW_INSPECTOR_FILES],
  '/inspectors/mobilize': [PERMISSION_TYPES.MOBILIZE_INSPECTOR],
  
  // Customer Pages
  '/customers': [PERMISSION_TYPES.MANAGE_CUSTOMERS],
  '/customers/contracts': [PERMISSION_TYPES.MANAGE_CUSTOMERS],
  '/customers/contacts': [PERMISSION_TYPES.MANAGE_CUSTOMERS]
};

/**
 * Interface for permission validation results
 */
interface ValidationResult {
  isValid: boolean;
  errors: string[];
}

/**
 * Checks if a user has a specific permission
 * @param userPermissions - Array of user's assigned permissions
 * @param requiredPermission - Permission to check for
 * @returns boolean indicating if user has the permission
 */
export function hasPermission(userPermissions: string[], requiredPermission: PERMISSION_TYPES): boolean {
  if (!userPermissions || !requiredPermission) {
    return false;
  }
  
  // Log permission check for audit
  console.debug(`Checking permission: ${requiredPermission} for user with permissions:`, userPermissions);
  
  return userPermissions.includes(requiredPermission);
}

/**
 * Checks if a user has all specified permissions
 * @param userPermissions - Array of user's assigned permissions
 * @param requiredPermissions - Array of permissions to check for
 * @returns boolean indicating if user has all required permissions
 */
export function hasAllPermissions(userPermissions: string[], requiredPermissions: PERMISSION_TYPES[]): boolean {
  if (!userPermissions?.length || !requiredPermissions?.length) {
    return false;
  }
  
  // Log permission check for audit
  console.debug(`Checking all permissions: ${requiredPermissions} for user with permissions:`, userPermissions);
  
  return requiredPermissions.every(permission => userPermissions.includes(permission));
}

/**
 * Checks if a user has at least one of the specified permissions
 * @param userPermissions - Array of user's assigned permissions
 * @param requiredPermissions - Array of permissions to check for
 * @returns boolean indicating if user has any of the required permissions
 */
export function hasAnyPermission(userPermissions: string[], requiredPermissions: PERMISSION_TYPES[]): boolean {
  if (!userPermissions?.length || !requiredPermissions?.length) {
    return false;
  }
  
  // Log permission check for audit
  console.debug(`Checking any permissions: ${requiredPermissions} for user with permissions:`, userPermissions);
  
  return requiredPermissions.some(permission => userPermissions.includes(permission));
}

/**
 * Gets the required permissions for a given route
 * @param route - Vue Router route object
 * @returns Array of required permission strings for the route
 */
export function getRequiredPermissionsForRoute(route: RouteLocationNormalized): string[] {
  if (!route?.path) {
    return [];
  }

  // Handle nested routes by checking parent paths
  const pathSegments = route.path.split('/').filter(Boolean);
  let currentPath = '';
  const requiredPermissions: Set<string> = new Set();

  for (const segment of pathSegments) {
    currentPath += `/${segment}`;
    const permissions = ROUTE_PERMISSIONS[currentPath];
    if (permissions) {
      permissions.forEach(permission => requiredPermissions.add(permission));
    }
  }

  // If no permissions are defined, apply default policy
  if (requiredPermissions.size === 0 && DEFAULT_PERMISSION_POLICY === 'deny') {
    console.warn(`No permissions defined for route: ${route.path}, access denied by default policy`);
    return ['*']; // Require impossible permission effectively denying access
  }

  return Array.from(requiredPermissions);
}

/**
 * Validates permission strings against defined types
 * @param permissions - Array of permission strings to validate
 * @returns Validation result object
 */
export function validatePermissions(permissions: string[]): ValidationResult {
  const result: ValidationResult = {
    isValid: true,
    errors: []
  };

  if (!permissions?.length) {
    result.isValid = false;
    result.errors.push('Permissions array is empty or undefined');
    return result;
  }

  const validPermissions = Object.values(PERMISSION_TYPES);
  
  permissions.forEach(permission => {
    if (!validPermissions.includes(permission as PERMISSION_TYPES)) {
      result.isValid = false;
      result.errors.push(`Invalid permission: ${permission}`);
    }
  });

  return result;
}
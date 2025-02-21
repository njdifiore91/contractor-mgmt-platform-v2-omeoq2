/**
 * @file Enhanced permission management composable
 * @version 1.0.0
 * Provides reactive permission checking functionality with caching, validation and error handling
 */

import { ref, computed, watchEffect } from 'vue'; // ^3.3.0
import { useRoute } from 'vue-router'; // ^4.2.4
import { useAuthStore } from '../stores/auth';
import { 
  PERMISSION_TYPES, 
  hasAllPermissions, 
  hasAnyPermission, 
  getRequiredPermissionsForRoute,
  validatePermissions 
} from '../utils/permissions';

// Constants
const CACHE_SIZE_LIMIT = 100;
const PERMISSION_CHECK_RATE_LIMIT = 1000; // 1 second
const AUDIT_LOG_RETENTION = 1000 * 60 * 60; // 1 hour

// LRU Cache implementation for permission results
class PermissionCache {
  private cache = new Map<string, boolean>();
  private lastAccessed = new Map<string, number>();

  set(key: string, value: boolean): void {
    if (this.cache.size >= CACHE_SIZE_LIMIT) {
      // Remove least recently used entry
      const oldest = [...this.lastAccessed.entries()]
        .sort(([, a], [, b]) => a - b)[0][0];
      this.cache.delete(oldest);
      this.lastAccessed.delete(oldest);
    }
    this.cache.set(key, value);
    this.lastAccessed.set(key, Date.now());
  }

  get(key: string): boolean | undefined {
    const value = this.cache.get(key);
    if (value !== undefined) {
      this.lastAccessed.set(key, Date.now());
    }
    return value;
  }

  clear(): void {
    this.cache.clear();
    this.lastAccessed.clear();
  }
}

// Permission check rate limiting
const lastCheckTime = ref<number>(0);
const permissionCache = new PermissionCache();
const permissionAuditLog: { timestamp: number; permission: string; result: boolean }[] = [];

/**
 * Enhanced composable that provides reactive permission checking functionality
 */
export function usePermissions() {
  const auth = useAuthStore();
  const route = useRoute();

  // Computed property for current route required permissions
  const currentRoutePermissions = computed(() => {
    return getRequiredPermissionsForRoute(route);
  });

  // Watch for user changes to clear cache
  watchEffect(() => {
    if (!auth.currentUser) {
      permissionCache.clear();
    }
  });

  /**
   * Enhanced function to check single permission with caching and validation
   */
  const checkPermission = (permission: string): boolean => {
    // Rate limiting check
    const now = Date.now();
    if (now - lastCheckTime.value < PERMISSION_CHECK_RATE_LIMIT) {
      const cachedResult = permissionCache.get(permission);
      if (cachedResult !== undefined) {
        return cachedResult;
      }
    }
    lastCheckTime.value = now;

    // Validate permission
    const validationResult = validatePermissions([permission]);
    if (!validationResult.isValid) {
      console.error('Permission validation failed:', validationResult.errors);
      return false;
    }

    // Check actual permission
    const result = auth.hasPermission(permission);

    // Update cache
    permissionCache.set(permission, result);

    // Log for audit
    permissionAuditLog.push({
      timestamp: now,
      permission,
      result
    });

    // Cleanup old audit logs
    while (
      permissionAuditLog.length > 0 && 
      now - permissionAuditLog[0].timestamp > AUDIT_LOG_RETENTION
    ) {
      permissionAuditLog.shift();
    }

    return result;
  };

  /**
   * Enhanced function to check multiple required permissions with optimization
   */
  const checkAllPermissions = (permissions: string[]): boolean => {
    const validationResult = validatePermissions(permissions);
    if (!validationResult.isValid) {
      console.error('Permissions validation failed:', validationResult.errors);
      return false;
    }

    return hasAllPermissions(
      auth.currentUser?.permissions || [], 
      permissions as PERMISSION_TYPES[]
    );
  };

  /**
   * Enhanced function to check if user has any of the specified permissions
   */
  const checkAnyPermission = (permissions: string[]): boolean => {
    const validationResult = validatePermissions(permissions);
    if (!validationResult.isValid) {
      console.error('Permissions validation failed:', validationResult.errors);
      return false;
    }

    return hasAnyPermission(
      auth.currentUser?.permissions || [], 
      permissions as PERMISSION_TYPES[]
    );
  };

  return {
    // Constants
    PERMISSION_TYPES,

    // Permission checking functions
    checkPermission,
    checkAllPermissions,
    checkAnyPermission,

    // Computed properties
    currentRoutePermissions,
  };
}
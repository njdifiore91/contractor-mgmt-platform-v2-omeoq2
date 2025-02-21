/**
 * @file Vue Router navigation guards implementation
 * @version 1.0.0
 * Implements authentication and permission checks for route navigation
 */

import { NavigationGuardNext, RouteLocationNormalized } from 'vue-router'; // ^4.0.0
import { useAuth } from '../composables/useAuth';
import { getRequiredPermissionsForRoute } from '../utils/permissions';

/**
 * Navigation guard that checks if user is authenticated for protected routes
 * @param to Target route
 * @param from Source route
 * @param next Navigation guard resolver
 * @returns Promise resolving to navigation action
 */
export const authGuard = async (
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
): Promise<void | string> => {
  const { isAuthenticated } = useAuth();
  
  // Check if route requires authentication
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);

  // Allow navigation if authentication is not required
  if (!requiresAuth) {
    return next();
  }

  // Handle login page access when already authenticated
  if (isAuthenticated.value && to.path === '/login') {
    return next('/');
  }

  // Allow navigation for authenticated users
  if (isAuthenticated.value) {
    return next();
  }

  // Redirect to login for unauthenticated users trying to access protected routes
  if (requiresAuth && !isAuthenticated.value) {
    // Store intended destination for post-login redirect
    return next({
      path: '/login',
      query: { redirect: to.fullPath }
    });
  }

  return next();
};

/**
 * Navigation guard that checks if user has required permissions for route
 * @param to Target route
 * @param from Source route
 * @param next Navigation guard resolver
 * @returns Promise resolving to navigation action
 */
export const permissionGuard = async (
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
): Promise<void | string> => {
  const { checkPermission } = useAuth();

  // Get required permissions for the route
  const requiredPermissions = getRequiredPermissionsForRoute(to);

  // Allow navigation if no permissions are required
  if (!requiredPermissions.length) {
    return next();
  }

  // Check if user has all required permissions
  const hasAllRequiredPermissions = requiredPermissions.every(permission => 
    checkPermission(permission)
  );

  // Allow navigation if user has all required permissions
  if (hasAllRequiredPermissions) {
    return next();
  }

  // Log unauthorized access attempt
  console.warn(
    `Access denied to route ${to.path} - Missing required permissions:`,
    requiredPermissions
  );

  // Redirect to home page if permissions check fails
  return next({
    path: '/',
    query: { 
      error: 'insufficient_permissions',
      path: to.fullPath
    }
  });
};
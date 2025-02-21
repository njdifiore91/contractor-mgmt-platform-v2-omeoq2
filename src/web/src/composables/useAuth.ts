/**
 * @file Vue composable for secure authentication and permission management
 * @version 1.0.0
 * Provides comprehensive authentication functionality with automatic token refresh
 */

import { ref, computed, onMounted } from 'vue'; // ^3.2.0
import { useAuthStore } from '../stores/auth';
import authService from '../services/api/auth';

// Types
interface AuthError {
  message: string;
  code: string;
  timestamp: Date;
}

interface LoginCredentials {
  email: string;
  password: string;
  rememberMe?: boolean;
}

// Constants
const RATE_LIMIT_WINDOW = 5 * 60 * 1000; // 5 minutes
const MAX_LOGIN_ATTEMPTS = 5;

export function useAuth() {
  // Initialize store
  const authStore = useAuthStore();

  // Reactive state
  const isLoading = ref(false);
  const error = ref<AuthError | null>(null);
  const loginAttempts = ref(0);
  const lastLoginAttempt = ref<Date | null>(null);

  // Computed properties
  const user = computed(() => authStore.user);
  const isAuthenticated = computed(() => authStore.isAuthenticated);

  /**
   * Handles user authentication
   * @param credentials Login credentials
   * @returns Promise resolving to login success status
   */
  const login = async (credentials: LoginCredentials): Promise<boolean> => {
    // Rate limiting check
    if (loginAttempts.value >= MAX_LOGIN_ATTEMPTS && 
        lastLoginAttempt.value && 
        Date.now() - lastLoginAttempt.value.getTime() < RATE_LIMIT_WINDOW) {
      error.value = {
        message: 'Too many login attempts. Please try again later.',
        code: 'RATE_LIMIT_EXCEEDED',
        timestamp: new Date()
      };
      return false;
    }

    isLoading.value = true;
    error.value = null;

    try {
      const { user, accessToken, refreshToken } = await authService.login(credentials);
      
      // Update store
      authStore.setUser(user);
      authStore.setTokens(accessToken, refreshToken);
      
      // Reset rate limiting
      loginAttempts.value = 0;
      lastLoginAttempt.value = null;
      
      return true;
    } catch (e) {
      loginAttempts.value++;
      lastLoginAttempt.value = new Date();
      
      error.value = {
        message: e instanceof Error ? e.message : 'Authentication failed',
        code: 'AUTH_ERROR',
        timestamp: new Date()
      };
      return false;
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * Handles user logout
   */
  const logout = async (): Promise<void> => {
    isLoading.value = true;
    error.value = null;

    try {
      await authService.logout();
      authStore.clearUser();
      authStore.clearTokens();
    } catch (e) {
      error.value = {
        message: e instanceof Error ? e.message : 'Logout failed',
        code: 'LOGOUT_ERROR',
        timestamp: new Date()
      };
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * Checks if user has specific permission
   * @param permission Permission to check
   * @returns Boolean indicating permission status
   */
  const checkPermission = (permission: string): boolean => {
    if (!isAuthenticated.value) return false;

    // Check cache first
    const cachedResult = authStore.permissionCache.get(permission);
    if (cachedResult !== undefined) return cachedResult;

    // Get fresh result
    const hasPermission = authStore.hasPermission(permission);
    authStore.permissionCache.set(permission, hasPermission);

    return hasPermission;
  };

  /**
   * Refreshes authentication tokens
   */
  const refreshTokens = async (): Promise<void> => {
    isLoading.value = true;
    error.value = null;

    try {
      const { accessToken, refreshToken } = await authService.refreshToken();
      authStore.setTokens(accessToken, refreshToken);
    } catch (e) {
      error.value = {
        message: e instanceof Error ? e.message : 'Token refresh failed',
        code: 'REFRESH_ERROR',
        timestamp: new Date()
      };
      await logout();
    } finally {
      isLoading.value = false;
    }
  };

  // Initialize session validation on mount
  onMounted(async () => {
    if (isAuthenticated.value) {
      try {
        await authService.validateSession();
      } catch (e) {
        await logout();
      }
    }
  });

  return {
    // State
    user,
    isAuthenticated,
    isLoading,
    error,

    // Methods
    login,
    logout,
    checkPermission,
    refreshTokens
  };
}
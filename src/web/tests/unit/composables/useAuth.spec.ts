import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { useAuth } from '@/composables/useAuth';
import { useAuthStore } from '@/stores/auth';
import authService from '@/services/api/auth';

// Mock auth service
vi.mock('@/services/api/auth', () => ({
  default: {
    login: vi.fn(),
    logout: vi.fn(),
    refreshToken: vi.fn(),
    validateSession: vi.fn(),
    getCurrentUser: vi.fn()
  }
}));

describe('useAuth', () => {
  beforeEach(() => {
    // Setup fresh pinia instance
    setActivePinia(createPinia());
    
    // Reset all mocks
    vi.clearAllMocks();
    
    // Reset localStorage and sessionStorage
    localStorage.clear();
    sessionStorage.clear();
  });

  afterEach(() => {
    vi.clearAllTimers();
  });

  describe('login', () => {
    it('should handle successful login', async () => {
      const mockUser = {
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        emailConfirmed: true,
        roles: ['admin'],
        permissions: ['edit_users'],
        isActive: true,
        createdAt: new Date(),
        lastLoginAt: new Date(),
        receiveEmails: true,
        password: 'hashed_password'
      };

      const mockTokens = {
        accessToken: 'mock_access_token',
        refreshToken: 'mock_refresh_token'
      };

      vi.mocked(authService.login).mockResolvedValueOnce({
        user: mockUser,
        ...mockTokens
      });

      const { login, isAuthenticated, user, error } = useAuth();
      const credentials = {
        email: 'john@example.com',
        password: 'password123',
        rememberMe: true
      };

      const result = await login(credentials);

      expect(result).toBe(true);
      expect(isAuthenticated.value).toBe(true);
      expect(user.value).toEqual(mockUser);
      expect(error.value).toBeNull();
      expect(authService.login).toHaveBeenCalledWith(credentials);
    });

    it('should handle login failure with invalid credentials', async () => {
      vi.mocked(authService.login).mockRejectedValueOnce(new Error('Invalid credentials'));

      const { login, isAuthenticated, user, error } = useAuth();
      const result = await login({
        email: 'invalid@example.com',
        password: 'wrong'
      });

      expect(result).toBe(false);
      expect(isAuthenticated.value).toBe(false);
      expect(user.value).toBeNull();
      expect(error.value).toEqual({
        message: 'Invalid credentials',
        code: 'AUTH_ERROR',
        timestamp: expect.any(Date)
      });
    });

    it('should handle rate limiting', async () => {
      const { login, error } = useAuth();
      const credentials = {
        email: 'test@example.com',
        password: 'password'
      };

      // Simulate 6 failed attempts
      vi.mocked(authService.login).mockRejectedValue(new Error('Invalid credentials'));
      
      for (let i = 0; i < 6; i++) {
        await login(credentials);
      }

      expect(error.value).toEqual({
        message: 'Too many login attempts. Please try again later.',
        code: 'RATE_LIMIT_EXCEEDED',
        timestamp: expect.any(Date)
      });
    });
  });

  describe('logout', () => {
    it('should handle successful logout', async () => {
      vi.mocked(authService.logout).mockResolvedValueOnce();

      const authStore = useAuthStore();
      const { logout, isAuthenticated } = useAuth();

      // Setup initial authenticated state
      authStore.setUser({
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        emailConfirmed: true,
        roles: ['admin'],
        permissions: ['edit_users'],
        isActive: true,
        createdAt: new Date(),
        lastLoginAt: new Date(),
        receiveEmails: true,
        password: 'hashed_password'
      });
      authStore.setTokens('mock_access_token', 'mock_refresh_token');

      await logout();

      expect(isAuthenticated.value).toBe(false);
      expect(authStore.user).toBeNull();
      expect(authStore.accessToken).toBeNull();
      expect(authStore.refreshToken).toBeNull();
    });

    it('should clear state even if logout request fails', async () => {
      vi.mocked(authService.logout).mockRejectedValueOnce(new Error('Network error'));

      const authStore = useAuthStore();
      const { logout, isAuthenticated, error } = useAuth();

      await logout();

      expect(isAuthenticated.value).toBe(false);
      expect(authStore.user).toBeNull();
      expect(error.value).toEqual({
        message: 'Network error',
        code: 'LOGOUT_ERROR',
        timestamp: expect.any(Date)
      });
    });
  });

  describe('checkPermission', () => {
    it('should return false when not authenticated', () => {
      const { checkPermission } = useAuth();
      expect(checkPermission('edit_users')).toBe(false);
    });

    it('should check permission from store', () => {
      const authStore = useAuthStore();
      const { checkPermission } = useAuth();

      // Setup authenticated state with permissions
      authStore.setUser({
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        emailConfirmed: true,
        roles: ['admin'],
        permissions: ['edit_users'],
        isActive: true,
        createdAt: new Date(),
        lastLoginAt: new Date(),
        receiveEmails: true,
        password: 'hashed_password'
      });
      authStore.setTokens('mock_access_token', 'mock_refresh_token');

      expect(checkPermission('edit_users')).toBe(true);
      expect(checkPermission('invalid_permission')).toBe(false);
    });

    it('should use permission cache', () => {
      const authStore = useAuthStore();
      const { checkPermission } = useAuth();
      const hasPermissionSpy = vi.spyOn(authStore, 'hasPermission');

      authStore.setUser({
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        emailConfirmed: true,
        roles: ['admin'],
        permissions: ['edit_users'],
        isActive: true,
        createdAt: new Date(),
        lastLoginAt: new Date(),
        receiveEmails: true,
        password: 'hashed_password'
      });
      authStore.setTokens('mock_access_token', 'mock_refresh_token');

      // First call should check store
      checkPermission('edit_users');
      expect(hasPermissionSpy).toHaveBeenCalledTimes(1);

      // Second call should use cache
      checkPermission('edit_users');
      expect(hasPermissionSpy).toHaveBeenCalledTimes(1);
    });
  });

  describe('refreshTokens', () => {
    it('should handle successful token refresh', async () => {
      const mockTokens = {
        accessToken: 'new_access_token',
        refreshToken: 'new_refresh_token'
      };

      vi.mocked(authService.refreshToken).mockResolvedValueOnce(mockTokens);

      const authStore = useAuthStore();
      const { refreshTokens, error } = useAuth();

      await refreshTokens();

      expect(authStore.accessToken).toBe(mockTokens.accessToken);
      expect(authStore.refreshToken).toBe(mockTokens.refreshToken);
      expect(error.value).toBeNull();
    });

    it('should handle token refresh failure', async () => {
      vi.mocked(authService.refreshToken).mockRejectedValueOnce(new Error('Refresh failed'));

      const authStore = useAuthStore();
      const { refreshTokens, isAuthenticated, error } = useAuth();

      await refreshTokens();

      expect(isAuthenticated.value).toBe(false);
      expect(authStore.accessToken).toBeNull();
      expect(authStore.refreshToken).toBeNull();
      expect(error.value).toEqual({
        message: 'Refresh failed',
        code: 'REFRESH_ERROR',
        timestamp: expect.any(Date)
      });
    });
  });

  describe('session validation', () => {
    it('should validate session on mount when authenticated', async () => {
      const authStore = useAuthStore();
      vi.mocked(authService.validateSession).mockResolvedValueOnce();

      // Setup authenticated state
      authStore.setUser({
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        emailConfirmed: true,
        roles: ['admin'],
        permissions: ['edit_users'],
        isActive: true,
        createdAt: new Date(),
        lastLoginAt: new Date(),
        receiveEmails: true,
        password: 'hashed_password'
      });
      authStore.setTokens('mock_access_token', 'mock_refresh_token');

      const { isAuthenticated } = useAuth();

      // Wait for onMounted to complete
      await vi.runAllTimersAsync();

      expect(authService.validateSession).toHaveBeenCalled();
      expect(isAuthenticated.value).toBe(true);
    });

    it('should logout on invalid session', async () => {
      const authStore = useAuthStore();
      vi.mocked(authService.validateSession).mockRejectedValueOnce(new Error('Invalid session'));

      // Setup authenticated state
      authStore.setUser({
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        emailConfirmed: true,
        roles: ['admin'],
        permissions: ['edit_users'],
        isActive: true,
        createdAt: new Date(),
        lastLoginAt: new Date(),
        receiveEmails: true,
        password: 'hashed_password'
      });
      authStore.setTokens('mock_access_token', 'mock_refresh_token');

      const { isAuthenticated } = useAuth();

      // Wait for onMounted to complete
      await vi.runAllTimersAsync();

      expect(isAuthenticated.value).toBe(false);
      expect(authStore.user).toBeNull();
      expect(authStore.accessToken).toBeNull();
      expect(authStore.refreshToken).toBeNull();
    });
  });
});
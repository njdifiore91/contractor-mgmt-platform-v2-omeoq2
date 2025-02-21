import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'; // ^0.29.0
import { setActivePinia, createPinia } from 'pinia'; // ^2.0.0
import { useAuthStore } from '../../../src/stores/auth';
import { User } from '../../../src/types/admin';
import authService from '../../../src/services/api/auth';

// Mock auth service
vi.mock('../../../src/services/api/auth', () => ({
  default: {
    login: vi.fn(),
    logout: vi.fn(),
    refreshToken: vi.fn(),
    getCurrentUser: vi.fn(),
    validateSession: vi.fn(),
  },
}));

// Test user factory
const createTestUser = (overrides: Partial<User> = {}): User => ({
  id: 1,
  firstName: 'Test',
  lastName: 'User',
  email: 'test@example.com',
  emailConfirmed: true,
  roles: ['user'],
  isActive: true,
  createdAt: new Date('2024-01-01'),
  lastLoginAt: new Date('2024-01-01'),
  receiveEmails: true,
  password: 'hashedPassword123',
  ...overrides,
});

// Test setup helper
const setupTest = () => {
  const pinia = createPinia();
  setActivePinia(pinia);
  vi.useFakeTimers();
  const store = useAuthStore();
  return { store };
};

describe('useAuthStore', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.clearAllTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  describe('initial state', () => {
    it('should have correct initial state', () => {
      const { store } = setupTest();
      
      expect(store.user).toBeNull();
      expect(store.accessToken).toBeNull();
      expect(store.refreshToken).toBeNull();
      expect(store.isLoading).toBe(false);
      expect(store.lastActivity).toBeNull();
      expect(store.permissionCache.size).toBe(0);
      expect(store.isAuthenticated).toBe(false);
    });
  });

  describe('authentication', () => {
    it('should handle successful login', async () => {
      const { store } = setupTest();
      const testUser = createTestUser();
      const mockTokens = {
        accessToken: 'test-access-token',
        refreshToken: 'test-refresh-token',
      };

      vi.mocked(authService.login).mockResolvedValueOnce({
        user: testUser,
        ...mockTokens,
      });

      await store.login('test@example.com', 'password123');

      expect(store.user).toEqual(testUser);
      expect(store.accessToken).toBe(mockTokens.accessToken);
      expect(store.refreshToken).toBe(mockTokens.refreshToken);
      expect(store.isAuthenticated).toBe(true);
      expect(store.lastActivity).toBeInstanceOf(Date);
    });

    it('should handle login failure', async () => {
      const { store } = setupTest();
      vi.mocked(authService.login).mockRejectedValueOnce(new Error('Invalid credentials'));

      await expect(store.login('test@example.com', 'wrongpass')).rejects.toThrow('Invalid credentials');
      expect(store.isAuthenticated).toBe(false);
      expect(store.user).toBeNull();
    });

    it('should handle logout', async () => {
      const { store } = setupTest();
      store.setUser(createTestUser());
      store.setTokens('test-token', 'refresh-token');

      await store.logout();

      expect(store.user).toBeNull();
      expect(store.accessToken).toBeNull();
      expect(store.refreshToken).toBeNull();
      expect(store.isAuthenticated).toBe(false);
      expect(store.permissionCache.size).toBe(0);
    });

    it('should refresh tokens successfully', async () => {
      const { store } = setupTest();
      const newTokens = {
        accessToken: 'new-access-token',
        refreshToken: 'new-refresh-token',
      };

      store.setTokens('old-token', 'old-refresh-token');
      vi.mocked(authService.refreshToken).mockResolvedValueOnce(newTokens);

      await store.refreshUserSession();

      expect(store.accessToken).toBe(newTokens.accessToken);
      expect(store.refreshToken).toBe(newTokens.refreshToken);
      expect(store.lastActivity).toBeInstanceOf(Date);
    });
  });

  describe('session management', () => {
    it('should track last activity', async () => {
      const { store } = setupTest();
      const testUser = createTestUser();
      
      store.setUser(testUser);
      const activityTime = new Date();
      vi.setSystemTime(activityTime);

      expect(store.lastActivity?.getTime()).toBe(activityTime.getTime());
    });

    it('should detect session expiration', () => {
      const { store } = setupTest();
      const testUser = createTestUser();
      
      store.setUser(testUser);
      vi.advanceTimersByTime(31 * 60 * 1000); // Advance past 30 minute timeout

      expect(store.isSessionExpired).toBe(true);
    });

    it('should validate session periodically', async () => {
      const { store } = setupTest();
      const testUser = createTestUser();
      
      vi.mocked(authService.getCurrentUser).mockResolvedValueOnce(testUser);
      store.setupSessionMonitoring();

      vi.advanceTimersByTime(15 * 60 * 1000); // Half of session timeout
      
      expect(authService.getCurrentUser).toHaveBeenCalled();
    });
  });

  describe('permissions', () => {
    it('should check permissions correctly', () => {
      const { store } = setupTest();
      const testUser = createTestUser({
        roles: ['admin'],
        permissions: ['edit_users', 'view_reports'],
      });

      store.setUser(testUser);

      expect(store.hasPermission('edit_users')).toBe(true);
      expect(store.hasPermission('delete_users')).toBe(false);
    });

    it('should cache permission results', () => {
      const { store } = setupTest();
      const testUser = createTestUser({
        roles: ['admin'],
        permissions: ['edit_users'],
      });

      store.setUser(testUser);
      store.hasPermission('edit_users');

      expect(store.permissionCache.get('edit_users')).toBe(true);
    });

    it('should clear permission cache on role changes', () => {
      const { store } = setupTest();
      const testUser = createTestUser({
        roles: ['admin'],
        permissions: ['edit_users'],
      });

      store.setUser(testUser);
      store.hasPermission('edit_users');
      store.clearPermissionCache();

      expect(store.permissionCache.size).toBe(0);
    });
  });

  describe('security features', () => {
    it('should handle concurrent sessions', async () => {
      const { store } = setupTest();
      const testUser = createTestUser();
      
      vi.mocked(authService.getCurrentUser).mockRejectedValueOnce(new Error('Session invalidated'));
      store.setUser(testUser);
      
      await store.validateSession();

      expect(store.user).toBeNull();
      expect(store.isAuthenticated).toBe(false);
    });

    it('should sanitize sensitive data on logout', async () => {
      const { store } = setupTest();
      store.setUser(createTestUser());
      store.setTokens('sensitive-token', 'sensitive-refresh-token');
      
      await store.logout();

      expect(store.accessToken).toBeNull();
      expect(store.refreshToken).toBeNull();
      expect(store.user).toBeNull();
      expect(store.permissionCache.size).toBe(0);
    });

    it('should prevent token refresh with invalid refresh token', async () => {
      const { store } = setupTest();
      store.setTokens('test-token', null);

      await expect(store.refreshUserSession()).rejects.toThrow('No refresh token available');
    });
  });
});
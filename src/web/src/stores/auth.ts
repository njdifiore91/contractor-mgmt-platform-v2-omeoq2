/**
 * @file Authentication store implementation using Pinia
 * @version 1.0.0
 * Provides centralized authentication state management with enhanced security features
 */

import { defineStore } from 'pinia'; // ^2.0.0
import { User } from '../../types/admin';
import authService from '../services/api/auth';

// Constants
const SESSION_TIMEOUT = 30 * 60 * 1000; // 30 minutes
const PERMISSION_CACHE_DURATION = 5 * 60 * 1000; // 5 minutes

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isLoading: boolean;
  lastActivity: Date | null;
  permissionCache: Map<string, boolean>;
  sessionCheckInterval?: NodeJS.Timeout;
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    user: null,
    accessToken: null,
    refreshToken: null,
    isLoading: false,
    lastActivity: null,
    permissionCache: new Map(),
    sessionCheckInterval: undefined,
  }),

  getters: {
    isAuthenticated(): boolean {
      return !!this.user && !!this.accessToken;
    },

    fullName(): string {
      return this.user ? `${this.user.firstName} ${this.user.lastName}` : '';
    },

    isSessionExpired(): boolean {
      if (!this.lastActivity) return true;
      return Date.now() - this.lastActivity.getTime() > SESSION_TIMEOUT;
    },
  },

  actions: {
    /**
     * Updates user data in store
     * @param user User data to set
     */
    setUser(user: User | null): void {
      this.user = user;
      this.lastActivity = user ? new Date() : null;
      this.clearPermissionCache();
    },

    /**
     * Clears user data from store
     */
    clearUser(): void {
      this.user = null;
      this.lastActivity = null;
      this.clearPermissionCache();
    },

    /**
     * Updates authentication tokens
     * @param accessToken New access token
     * @param refreshToken New refresh token
     */
    setTokens(accessToken: string | null, refreshToken: string | null): void {
      this.accessToken = accessToken;
      this.refreshToken = refreshToken;
    },

    /**
     * Clears authentication tokens
     */
    clearTokens(): void {
      this.accessToken = null;
      this.refreshToken = null;
    },

    /**
     * Checks if user has specific permission
     * @param permission Permission to check
     * @returns Boolean indicating if user has permission
     */
    hasPermission(permission: string): boolean {
      if (!this.user?.permissions) return false;

      // Check cache first
      const cachedResult = this.permissionCache.get(permission);
      if (cachedResult !== undefined) return cachedResult;

      // Calculate permission
      const hasPermission = this.user.permissions.includes(permission);
      this.permissionCache.set(permission, hasPermission);

      return hasPermission;
    },

    /**
     * Checks if user has specific role
     * @param role Role to check
     * @returns Boolean indicating if user has role
     */
    hasRole(role: string): boolean {
      return this.user?.roles?.includes(role) ?? false;
    },

    /**
     * Clears permission cache
     */
    clearPermissionCache(): void {
      this.permissionCache.clear();
    },

    /**
     * Performs user login
     * @param email User email
     * @param password User password
     * @param rememberMe Remember user preference
     */
    async login(email: string, password: string, rememberMe: boolean = false): Promise<void> {
      this.isLoading = true;
      try {
        const { user, accessToken, refreshToken } = await authService.login({
          email,
          password,
          rememberMe,
        });

        this.setUser(user);
        this.setTokens(accessToken, refreshToken);
        this.setupSessionMonitoring();
      } finally {
        this.isLoading = false;
      }
    },

    /**
     * Performs user logout
     */
    async logout(): Promise<void> {
      this.isLoading = true;
      try {
        await authService.logout();
      } finally {
        this.clearUser();
        this.clearTokens();
        this.clearSessionMonitoring();
        this.isLoading = false;
      }
    },

    /**
     * Refreshes user session
     */
    async refreshUserSession(): Promise<void> {
      if (!this.refreshToken) throw new Error('No refresh token available');

      this.isLoading = true;
      try {
        const { accessToken, refreshToken } = await authService.refreshToken(this.refreshToken);
        this.setTokens(accessToken, refreshToken);
        this.lastActivity = new Date();
      } finally {
        this.isLoading = false;
      }
    },

    /**
     * Validates current session
     */
    async validateSession(): Promise<void> {
      if (this.isSessionExpired) {
        await this.logout();
        return;
      }

      try {
        const user = await authService.getCurrentUser();
        this.setUser(user);
        this.lastActivity = new Date();
      } catch (error) {
        await this.logout();
      }
    },

    /**
     * Sets up session monitoring
     */
    setupSessionMonitoring(): void {
      this.clearSessionMonitoring();
      this.sessionCheckInterval = setInterval(() => {
        this.validateSession();
      }, SESSION_TIMEOUT / 2);
    },

    /**
     * Clears session monitoring
     */
    clearSessionMonitoring(): void {
      if (this.sessionCheckInterval) {
        clearInterval(this.sessionCheckInterval);
        this.sessionCheckInterval = undefined;
      }
    },
  },
});
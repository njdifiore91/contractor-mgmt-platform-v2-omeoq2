/**
 * @file Authentication service implementation
 * @version 1.0.0
 * Provides secure authentication operations with token management and monitoring
 */

// External imports
import axios from 'axios'; // ^1.3.0
import jwtDecode from 'jwt-decode'; // ^3.1.2

// Internal imports
import { User } from '../../types/admin';

// Constants
const TOKEN_REFRESH_THRESHOLD = 5 * 60 * 1000; // 5 minutes in milliseconds
const MAX_RETRY_ATTEMPTS = 3;
const STORAGE_KEYS = {
  ACCESS_TOKEN: 'auth_access_token',
  REFRESH_TOKEN: 'auth_refresh_token',
  USER_DATA: 'auth_user_data'
} as const;

// Types
interface AuthTokens {
  accessToken: string;
  refreshToken: string;
}

interface LoginCredentials {
  email: string;
  password: string;
  rememberMe?: boolean;
}

interface TokenPayload {
  exp: number;
  sub: string;
  roles: string[];
}

// Utility functions
const isValidEmail = (email: string): boolean => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};

const isTokenExpired = (token: string): boolean => {
  try {
    const decoded = jwtDecode<TokenPayload>(token);
    return Date.now() >= decoded.exp * 1000;
  } catch {
    return true;
  }
};

const secureStorage = {
  setItem: (key: string, value: string, persistent: boolean = false): void => {
    if (persistent) {
      localStorage.setItem(key, value);
    } else {
      sessionStorage.setItem(key, value);
    }
  },
  getItem: (key: string): string | null => {
    return sessionStorage.getItem(key) || localStorage.getItem(key);
  },
  removeItem: (key: string): void => {
    sessionStorage.removeItem(key);
    localStorage.removeItem(key);
  }
};

// Service implementation
class AuthService {
  private refreshTokenTimeout?: NodeJS.Timeout;
  private userCache: User | null = null;
  private retryCount = 0;

  /**
   * Authenticates user with provided credentials
   * @param credentials Login credentials including email and password
   * @returns Promise resolving to user data and tokens
   */
  public async login(credentials: LoginCredentials): Promise<{ user: User; accessToken: string; refreshToken: string }> {
    try {
      if (!isValidEmail(credentials.email)) {
        throw new Error('Invalid email format');
      }

      if (!credentials.password || credentials.password.length < 8) {
        throw new Error('Invalid password format');
      }

      const response = await axios.post<{ user: User } & AuthTokens>('/api/auth/login', credentials);
      const { user, accessToken, refreshToken } = response.data;

      // Validate tokens
      if (!accessToken || !refreshToken || isTokenExpired(accessToken)) {
        throw new Error('Invalid token received');
      }

      // Store tokens and user data
      this.storeAuthData(user, accessToken, refreshToken, credentials.rememberMe);

      // Initialize token refresh
      this.setupTokenRefresh(refreshToken);

      return { user, accessToken, refreshToken };
    } catch (error) {
      if (this.retryCount < MAX_RETRY_ATTEMPTS) {
        this.retryCount++;
        return this.login(credentials);
      }
      throw error;
    }
  }

  /**
   * Logs out current user and cleans up session data
   */
  public async logout(): Promise<void> {
    try {
      const refreshToken = secureStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
      if (refreshToken) {
        await axios.post('/api/auth/logout', { refreshToken });
      }
    } catch (error) {
      console.error('Logout request failed:', error);
    } finally {
      this.cleanup();
    }
  }

  /**
   * Refreshes authentication tokens
   * @param refreshToken Current refresh token
   * @returns Promise resolving to new tokens
   */
  public async refreshToken(refreshToken: string): Promise<AuthTokens> {
    try {
      if (!refreshToken || isTokenExpired(refreshToken)) {
        throw new Error('Invalid refresh token');
      }

      const response = await axios.post<AuthTokens>('/api/auth/refresh', { refreshToken });
      const { accessToken, refreshToken: newRefreshToken } = response.data;

      if (!accessToken || !newRefreshToken) {
        throw new Error('Invalid tokens received');
      }

      // Update stored tokens
      secureStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, accessToken);
      secureStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken);

      // Setup next refresh
      this.setupTokenRefresh(newRefreshToken);

      return { accessToken, refreshToken: newRefreshToken };
    } catch (error) {
      if (this.retryCount < MAX_RETRY_ATTEMPTS) {
        this.retryCount++;
        await new Promise(resolve => setTimeout(resolve, 1000 * this.retryCount));
        return this.refreshToken(refreshToken);
      }
      throw error;
    }
  }

  /**
   * Retrieves current authenticated user
   * @returns Promise resolving to current user data
   */
  public async getCurrentUser(): Promise<User> {
    if (this.userCache) {
      return this.userCache;
    }

    try {
      const response = await axios.get<User>('/api/auth/me');
      const user = response.data;

      if (!user.id || !user.email) {
        throw new Error('Invalid user data received');
      }

      this.userCache = user;
      return user;
    } catch (error) {
      this.cleanup();
      throw error;
    }
  }

  /**
   * Sets up automatic token refresh
   * @param refreshToken Current refresh token
   */
  private setupTokenRefresh(refreshToken: string): void {
    if (this.refreshTokenTimeout) {
      clearTimeout(this.refreshTokenTimeout);
    }

    const decoded = jwtDecode<TokenPayload>(refreshToken);
    const expiresIn = decoded.exp * 1000 - Date.now() - TOKEN_REFRESH_THRESHOLD;

    if (expiresIn > 0) {
      this.refreshTokenTimeout = setTimeout(() => {
        this.refreshToken(refreshToken).catch(() => this.cleanup());
      }, expiresIn);
    }
  }

  /**
   * Stores authentication data securely
   */
  private storeAuthData(user: User, accessToken: string, refreshToken: string, persistent: boolean = false): void {
    secureStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, accessToken, persistent);
    secureStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, refreshToken, persistent);
    secureStorage.setItem(STORAGE_KEYS.USER_DATA, JSON.stringify(user), persistent);
    this.userCache = user;
  }

  /**
   * Cleans up authentication data and state
   */
  private cleanup(): void {
    if (this.refreshTokenTimeout) {
      clearTimeout(this.refreshTokenTimeout);
    }
    Object.values(STORAGE_KEYS).forEach(key => secureStorage.removeItem(key));
    this.userCache = null;
    this.retryCount = 0;
  }
}

// Export singleton instance
export default new AuthService();
/**
 * @file Admin TypeScript interfaces and types
 * Defines types for admin-related functionality including code types, quick links, and user management
 * @version 1.0.0
 */

import { BaseEntity } from './common';

/**
 * Interface representing a code within a code type
 * Used for managing individual codes in the admin system
 */
export interface Code extends BaseEntity {
  /** Unique identifier for the code */
  id: number;
  
  /** Code value */
  value: string;
  
  /** Detailed description of the code */
  description: string;
  
  /** Indicates if the code can expire */
  isExpireable: boolean;
  
  /** Indicates if the code is currently active */
  isActive: boolean;
  
  /** Expiration date for expireable codes */
  expiresAt: Date | null;
}

/**
 * Interface representing a code type that contains multiple codes
 * Used for organizing and managing groups of related codes
 */
export interface CodeType extends BaseEntity {
  /** Unique identifier for the code type */
  id: number;
  
  /** Name of the code type */
  name: string;
  
  /** Detailed description of the code type */
  description: string;
  
  /** Indicates if the code type is currently active */
  isActive: boolean;
  
  /** Array of codes belonging to this code type */
  codes: Code[];
}

/**
 * Interface representing a quick link configuration
 * Used for managing customizable navigation links in the admin system
 */
export interface QuickLink extends BaseEntity {
  /** Unique identifier for the quick link */
  id: number;
  
  /** Display label for the quick link */
  label: string;
  
  /** URL or path for the quick link */
  link: string;
  
  /** Display order for the quick link */
  order: number;
  
  /** Indicates if the quick link is currently active */
  isActive: boolean;
}

/**
 * Interface representing a user in the system
 * Used for managing user accounts and their associated permissions
 */
export interface User extends BaseEntity {
  /** Unique identifier for the user */
  id: number;
  
  /** User's first name */
  firstName: string;
  
  /** User's last name */
  lastName: string;
  
  /** User's email address */
  email: string;
  
  /** Indicates if the email has been confirmed */
  emailConfirmed: boolean;
  
  /** Array of role names assigned to the user */
  roles: string[];
  
  /** Indicates if the user account is currently active */
  isActive: boolean;
  
  /** Timestamp when the user was created */
  createdAt: Date;
  
  /** Timestamp of the user's last login */
  lastLoginAt: Date | null;
  
  /** Indicates if the user has opted to receive emails */
  receiveEmails: boolean;
  
  /** User's password (hashed) */
  password: string;
}
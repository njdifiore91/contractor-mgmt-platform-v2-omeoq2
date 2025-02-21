/**
 * @file Customer-related TypeScript interfaces and types
 * Defines data structures for customers, contacts, and contracts
 * @version 1.0.0
 */

import { 
  BaseEntity, 
  Address, 
  PhoneNumber, 
  Email, 
  Note 
} from './common';

/**
 * Enum defining different types of contracts available in the system
 */
export enum ContractType {
  SERVICE = 'SERVICE',
  MAINTENANCE = 'MAINTENANCE',
  SUPPORT = 'SUPPORT'
}

/**
 * Interface defining the structure of a customer contract
 * Extends BaseEntity for common tracking fields
 */
export interface Contract extends BaseEntity {
  id: number;
  name: string;
  active: boolean;
  createdAt: Date;
  createdBy: string;
  customerId: number;
  endDate: Date | null;
  contractType: ContractType;
}

/**
 * Interface defining the structure of a customer contact
 * Includes personal information, contact details, and related metadata
 */
export interface Contact {
  id: number;
  firstName: string;
  middleName: string;
  lastName: string;
  suffix: string;
  nickname: string;
  isDeceased: boolean;
  isInactive: boolean;
  rating: number;
  jobTitle: string;
  birthday: Date | null;
  dateCreated: Date;
  customerId: number;
  
  // Related contact information
  addresses: Address[];
  emails: Email[];
  phoneNumbers: PhoneNumber[];
  notes: Note[];
}

/**
 * Interface defining the structure of a customer
 * Represents a business entity with associated contacts and contracts
 */
export interface Customer extends BaseEntity {
  id: number;
  name: string;
  code: string;
  isActive: boolean;
  createdAt: Date;
  createdBy: string;
  
  // Related entities
  contacts: Contact[];
  contracts: Contract[];
}
/**
 * @file Inspector-related TypeScript interfaces and types
 * Defines comprehensive data structures for inspector management
 * @version 1.0.0
 */

import { Vue } from 'vue'; // ^3.2.0
import { Customer } from '../types/customer';

/**
 * Enum defining possible inspector statuses
 */
export enum InspectorStatus {
  ACTIVE = 'ACTIVE',
  INACTIVE = 'INACTIVE',
  PENDING = 'PENDING',
  MOBILIZED = 'MOBILIZED',
  DEMOBILIZED = 'DEMOBILIZED'
}

/**
 * Enum defining hire types for inspectors
 */
export enum HireType {
  FULL_TIME = 'FULL_TIME',
  CONTRACT = 'CONTRACT',
  TEMPORARY = 'TEMPORARY'
}

/**
 * Enum defining inspector classifications
 */
export enum Classification {
  LEVEL_1 = 'LEVEL_1',
  LEVEL_2 = 'LEVEL_2',
  LEVEL_3 = 'LEVEL_3',
  SENIOR = 'SENIOR',
  EXPERT = 'EXPERT'
}

/**
 * Enum defining drug test types
 */
export enum DrugTestType {
  PRE_EMPLOYMENT = 'PRE_EMPLOYMENT',
  RANDOM = 'RANDOM',
  POST_INCIDENT = 'POST_INCIDENT',
  REASONABLE_SUSPICION = 'REASONABLE_SUSPICION'
}

/**
 * Enum defining drug test frequencies
 */
export enum TestFrequency {
  ANNUAL = 'ANNUAL',
  SEMI_ANNUAL = 'SEMI_ANNUAL',
  QUARTERLY = 'QUARTERLY',
  MONTHLY = 'MONTHLY'
}

/**
 * Enum defining drug test results
 */
export enum DrugTestResult {
  NEGATIVE = 'NEGATIVE',
  POSITIVE = 'POSITIVE',
  INCONCLUSIVE = 'INCONCLUSIVE',
  PENDING = 'PENDING'
}

/**
 * Enum defining demobilization reasons
 */
export enum DemobilizationReason {
  PROJECT_COMPLETE = 'PROJECT_COMPLETE',
  PERFORMANCE = 'PERFORMANCE',
  VOLUNTARY = 'VOLUNTARY',
  MEDICAL = 'MEDICAL',
  OTHER = 'OTHER'
}

/**
 * Enum defining address types
 */
export enum AddressType {
  HOME = 'HOME',
  WORK = 'WORK',
  MAILING = 'MAILING'
}

/**
 * Enum defining shipment options
 */
export enum ShipmentOption {
  STANDARD = 'STANDARD',
  EXPRESS = 'EXPRESS',
  OVERNIGHT = 'OVERNIGHT'
}

/**
 * Interface defining the structure of a drug test record
 */
export interface DrugTest {
  id: number;
  inspectorId: number;
  testDate: Date;
  testType: DrugTestType;
  frequency: TestFrequency;
  result: DrugTestResult;
  comment: string;
  company: string;
  created: Date;
  createdBy: string;
  modified: Date;
  modifiedBy: string;
}

/**
 * Interface defining inspector mobilization data
 */
export interface InspectorMobilization {
  employeeName: string;
  primaryEmail: string;
  phone: string;
  dateOfBirth: Date;
  mobDate: Date;
  hireType: HireType;
  hirePack: string;
  training: boolean;
  ispTrans: boolean;
  drugKit: boolean;
  daPool: boolean;
  project: string;
  customer: Customer;
  contract: string;
  department: string;
  function: string;
  type: string;
  location: string;
  classification: Classification;
  certRequired: boolean;
  certsRequired: string[];
  addressType: AddressType;
  shipOpt: ShipmentOption;
  projectContact: string;
  invoiceContact: string;
}

/**
 * Interface defining inspector demobilization data
 */
export interface InspectorDemobilization {
  demobReason: DemobilizationReason;
  demobDate: Date;
  note: string;
}

/**
 * Interface defining inspector search parameters
 */
export interface InspectorSearch {
  zipCode: string;
  radius: number;
  specialties: string[];
  certifications: string[];
  availableOnly: boolean;
  status: InspectorStatus;
  locations: string[];
  classifications: Classification[];
  hasValidDrugTest: boolean;
}

/**
 * Comprehensive interface for inspector data
 */
export interface Inspector {
  id: number;
  firstName: string;
  lastName: string;
  middleName: string;
  nickname: string;
  email: string;
  phone: string;
  dateOfBirth: Date;
  inspectorId: string;
  status: InspectorStatus;
  state: string;
  title: string;
  specialties: string[];
  hasIssues: boolean;
  needsApproval: boolean;
  mobilizationDate: Date | null;
  hireType: HireType;
  classification: Classification;
  certificationRequired: boolean;
  requiredCertifications: string[];
  department: string;
  function: string;
  projectLocation: string;
  demobilizationDate: Date | null;
  demobilizationReason: DemobilizationReason;
  drugTests: DrugTest[];
  lastDrugTestDate: Date | null;
  lastDrugTestResult: DrugTestResult;
  assignedCustomers: Customer[];
  assignedContracts: string[];
  createdAt: Date;
  createdBy: string;
  updatedAt: Date;
  updatedBy: string;
}
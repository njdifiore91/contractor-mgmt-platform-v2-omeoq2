/**
 * Application-wide constants for API endpoints, validation rules, status codes, and enums
 * @version 1.0.0
 */

// API Endpoints for all backend routes
export const API_ENDPOINTS = {
  ADMIN: {
    QUICK_LINKS: '/api/admin/quick-links',
    CODE_TYPES: '/api/admin/code-types',
    USERS: '/api/admin/users'
  },
  CUSTOMER: {
    BASE: '/api/customers',
    SEARCH: '/api/customers/search',
    CONTRACTS: '/api/customers/:id/contracts',
    CONTACTS: '/api/customers/:id/contacts'
  },
  EQUIPMENT: {
    LIST: '/api/equipment',
    ASSIGN: '/api/equipment/assign',
    RETURN: '/api/equipment/return'
  },
  INSPECTOR: {
    DRUG_TESTS: '/api/inspectors/:id/drug-tests',
    SEARCH: '/api/inspectors/search',
    MOBILIZE: '/api/inspectors/:id/mobilize',
    DEMOBILIZE: '/api/inspectors/:id/demobilize',
    FILES: '/api/inspectors/:id/files',
    CLASS_CHANGE: '/api/inspectors/:id/class-change'
  }
} as const;

// Equipment condition status constants
export const EQUIPMENT_CONDITIONS = {
  NEW: 'New',
  GOOD: 'Good',
  FAIR: 'Fair',
  POOR: 'Poor'
} as const;

// Inspector status constants
export const INSPECTOR_STATUS = {
  ACTIVE: 'Active',
  INACTIVE: 'Inactive',
  MOBILIZED: 'Mobilized',
  DEMOBILIZED: 'Demobilized'
} as const;

// Drug test type constants
export const DRUG_TEST_TYPES = {
  PRE_EMPLOYMENT: 'Pre-Employment',
  RANDOM: 'Random',
  POST_INCIDENT: 'Post-Incident'
} as const;

// Inspector hire type constants
export const HIRE_TYPES = {
  FULL_TIME: 'Full-Time',
  CONTRACT: 'Contract',
  TEMPORARY: 'Temporary'
} as const;

// Common validation patterns
export const VALIDATION_RULES = {
  EMAIL_PATTERN: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
  PHONE_PATTERN: /^\+?[1-9]\d{1,14}$/,
  ZIP_CODE_PATTERN: /^\d{5}(-\d{4})?$/
} as const;

// Date format constants for display and API
export const DATE_FORMATS = {
  DISPLAY_DATE: 'MM/DD/YYYY',
  DISPLAY_DATETIME: 'MM/DD/YYYY HH:mm:ss',
  API_DATE: 'YYYY-MM-DD'
} as const;

// Default values for table configurations
export const TABLE_DEFAULTS = {
  PAGE_SIZES: [10, 25, 50, 100] as const,
  DEFAULT_PAGE_SIZE: 25,
  VIRTUAL_SCROLL_BUFFER: 10
} as const;

// Type assertions to ensure type safety
type ApiEndpoints = typeof API_ENDPOINTS;
type EquipmentConditions = typeof EQUIPMENT_CONDITIONS;
type InspectorStatus = typeof INSPECTOR_STATUS;
type DrugTestTypes = typeof DRUG_TEST_TYPES;
type HireTypes = typeof HIRE_TYPES;
type ValidationRules = typeof VALIDATION_RULES;
type DateFormats = typeof DATE_FORMATS;
type TableDefaults = typeof TABLE_DEFAULTS;

// Export types for use in other files
export type {
  ApiEndpoints,
  EquipmentConditions,
  InspectorStatus,
  DrugTestTypes,
  HireTypes,
  ValidationRules,
  DateFormats,
  TableDefaults
};
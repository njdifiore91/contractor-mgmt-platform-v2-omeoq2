/**
 * @file Common TypeScript interfaces and types shared across the application
 * Defines base entities, field configurations, and validation structures
 * @version 1.0.0
 */

/**
 * Base interface for all entity types with comprehensive tracking fields
 * Provides common fields for entity tracking and auditing
 */
export interface BaseEntity {
  /** Unique identifier for the entity */
  id: number;
  
  /** Timestamp when the entity was created */
  createdAt: Date;
  
  /** User ID who created the entity */
  createdBy: number;
  
  /** Last modification timestamp, null if never modified */
  updatedAt: Date | null;
  
  /** User ID who last modified the entity, null if never modified */
  updatedBy: number | null;
  
  /** Flag indicating if the entity is currently active */
  isActive: boolean;
}

/**
 * Comprehensive validation rule interface for form fields
 * Supports multiple validation types including regex, length, value ranges and custom validation
 */
export interface ValidationRule {
  /** Regular expression pattern for text validation */
  pattern: RegExp | null;
  
  /** Minimum length requirement for text fields */
  minLength: number | null;
  
  /** Maximum length requirement for text fields */
  maxLength: number | null;
  
  /** Minimum value for numeric fields */
  minValue: number | null;
  
  /** Maximum value for numeric fields */
  maxValue: number | null;
  
  /** Custom validation function that returns boolean */
  customValidator: ((value: any) => boolean) | null;
  
  /** Error message to display when validation fails */
  errorMessage: string;
}

/**
 * Enhanced interface for list view field configuration
 * Defines structure for displaying data in tables and list views
 */
export interface ListField {
  /** Field identifier/key in the data object */
  field: string;
  
  /** Display label for the field */
  label: string;
  
  /** Indicates if the field can be sorted */
  sortable: boolean;
  
  /** Controls field visibility in the list */
  visible: boolean;
  
  /** Column width specification (e.g., '100px', '10%') */
  width: string;
  
  /** Optional formatter function for value display */
  formatter: ((value: any) => string) | null;
  
  /** Column content alignment */
  align: 'left' | 'center' | 'right';
}

/**
 * Enhanced interface for dialog form field configuration
 * Defines structure for input fields in forms and dialogs
 */
export interface DialogField {
  /** Field identifier/key in the data object */
  field: string;
  
  /** Display label for the field */
  label: string;
  
  /** Input field type */
  type: 'text' | 'number' | 'date' | 'select' | 'multiselect' | 'boolean' | 'textarea';
  
  /** Indicates if the field is required */
  required: boolean;
  
  /** Array of validation rules for the field */
  validation: ValidationRule[];
  
  /** Placeholder text for the input field */
  placeholder: string;
  
  /** Default value for the field */
  defaultValue: any;
}
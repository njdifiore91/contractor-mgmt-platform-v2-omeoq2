/**
 * @file Equipment-related TypeScript interfaces and types
 * Defines data structures for equipment management, assignments, and tracking
 * @version 1.0.0
 */

import { BaseEntity, ListField, DialogField } from '../types/common';

/**
 * Enumerated type for equipment condition states
 */
export type EquipmentCondition = 'NEW' | 'GOOD' | 'FAIR' | 'POOR';

/**
 * Comprehensive interface for equipment data structure
 * Extends BaseEntity for common tracking fields
 */
export interface Equipment extends BaseEntity {
  /** Equipment model identifier */
  model: string;
  
  /** Unique serial number for the equipment */
  serialNumber: string;
  
  /** Detailed description of the equipment */
  description: string;
  
  /** Current condition of the equipment */
  condition: EquipmentCondition;
  
  /** Flag indicating if equipment is currently assigned */
  isOut: boolean;
  
  /** ID of inspector currently assigned to the equipment */
  assignedToInspectorId: number | null;
  
  /** Name of inspector currently assigned to the equipment */
  assignedToInspectorName: string | null;
  
  /** Company ID owning the equipment */
  companyId: number;
  
  /** Date when equipment was assigned */
  assignedDate: Date | null;
  
  /** Condition when equipment was assigned */
  assignedCondition: EquipmentCondition | null;
  
  /** Date when equipment was returned */
  returnedDate: Date | null;
  
  /** Condition when equipment was returned */
  returnedCondition: EquipmentCondition | null;
}

/**
 * Interface for equipment assignment records
 */
export interface EquipmentAssignment {
  /** ID of the equipment being assigned */
  equipmentId: number;
  
  /** ID of the inspector receiving the equipment */
  inspectorId: number;
  
  /** Condition of equipment at assignment */
  condition: EquipmentCondition;
  
  /** Date of assignment */
  assignmentDate: Date;
}

/**
 * Interface for equipment return records
 */
export interface EquipmentReturn {
  /** ID of the equipment being returned */
  equipmentId: number;
  
  /** Condition of equipment upon return */
  returnCondition: EquipmentCondition;
  
  /** Date of return */
  returnDate: Date;
}

/**
 * Type definition for equipment list view fields
 * Based on PRD List Fields requirements
 */
export type EquipmentListFields = {
  model: ListField<string>;
  serialNumber: ListField<string>;
  description: ListField<string>;
  out: ListField<boolean>;
  condition: ListField<EquipmentCondition>;
  returnedDate?: ListField<Date>;
  returnedCondition?: ListField<EquipmentCondition>;
};

/**
 * Type definition for equipment dialog form fields
 * Based on PRD Dialog Fields requirements
 */
export type EquipmentDialogFields = {
  model: DialogField<string>;
  serialNumber: DialogField<string>;
  description: DialogField<string>;
  condition: DialogField<EquipmentCondition>;
};

/**
 * Type for equipment assignment dialog fields
 */
export type EquipmentAssignmentDialogFields = {
  equipment: DialogField<number>;
  outCondition: DialogField<EquipmentCondition>;
  outDate: DialogField<Date>;
};

/**
 * Type for equipment return dialog fields
 */
export type EquipmentReturnDialogFields = {
  equipment: DialogField<number>;
  inCondition: DialogField<EquipmentCondition>;
  inDate: DialogField<Date>;
};
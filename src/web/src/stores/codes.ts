/**
 * @file Pinia store for managing code types and codes
 * @version 1.0.0
 */

import { defineStore } from 'pinia'; // ^2.1.0
import { CodeType, Code } from '../types/admin';
import { usePermissions } from '../composables/usePermissions';
import axios from 'axios';

// Constants
const CACHE_DURATION = 5 * 60 * 1000; // 5 minutes
const API_ENDPOINTS = {
  CODE_TYPES: '/api/admin/code-types',
  CODES: '/api/admin/codes'
} as const;

interface ValidationErrors {
  [key: string]: string[];
}

interface CodesState {
  codeTypes: CodeType[];
  loading: boolean;
  validationErrors: ValidationErrors;
  lastUpdated: Date | null;
  hasEditPermission: boolean;
  cacheTimeout: number;
}

export const useCodesStore = defineStore('codes', {
  state: (): CodesState => ({
    codeTypes: [],
    loading: false,
    validationErrors: {},
    lastUpdated: null,
    hasEditPermission: false,
    cacheTimeout: CACHE_DURATION
  }),

  getters: {
    /**
     * Returns a code type by its ID
     */
    getCodeTypeById: (state) => (id: number): CodeType | undefined => {
      return state.codeTypes.find(type => type.id === id);
    },

    /**
     * Returns codes for a specific code type
     */
    getCodesByType: (state) => (typeName: string): Code[] => {
      const codeType = state.codeTypes.find(type => type.name === typeName);
      return codeType?.codes || [];
    },

    /**
     * Returns only active code types
     */
    getActiveCodeTypes: (state): CodeType[] => {
      return state.codeTypes.filter(type => type.isActive);
    },

    /**
     * Returns all expirable codes across types
     */
    getExpirableCodes: (state): Code[] => {
      return state.codeTypes.flatMap(type => 
        type.codes.filter(code => code.isExpireable)
      );
    },

    /**
     * Returns active codes for a specific type
     */
    getActiveCodesByType: (state) => (typeId: number): Code[] => {
      const codeType = state.codeTypes.find(type => type.id === typeId);
      return codeType?.codes.filter(code => code.isActive) || [];
    }
  },

  actions: {
    /**
     * Loads all code types from the API with permission check
     */
    async loadCodeTypes(): Promise<void> {
      const { checkPermission } = usePermissions();
      this.hasEditPermission = checkPermission('edit_codes');

      // Check cache validity
      if (
        this.lastUpdated && 
        Date.now() - this.lastUpdated.getTime() < this.cacheTimeout
      ) {
        return;
      }

      this.loading = true;
      this.validationErrors = {};

      try {
        const response = await axios.get<CodeType[]>(API_ENDPOINTS.CODE_TYPES);
        this.codeTypes = response.data;
        this.lastUpdated = new Date();
      } catch (error) {
        console.error('Failed to load code types:', error);
        throw error;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates a code type and its codes with validation
     */
    async updateCodeType(codeType: CodeType): Promise<void> {
      if (!this.hasEditPermission) {
        throw new Error('Permission denied: Cannot edit codes');
      }

      if (!this.validateCodeType(codeType)) {
        throw new Error('Validation failed');
      }

      this.loading = true;
      try {
        const response = await axios.put(
          `${API_ENDPOINTS.CODE_TYPES}/${codeType.id}`,
          codeType
        );
        
        // Update local state
        const index = this.codeTypes.findIndex(type => type.id === codeType.id);
        if (index !== -1) {
          this.codeTypes[index] = response.data;
        }
        
        this.lastUpdated = new Date();
      } catch (error) {
        console.error('Failed to update code type:', error);
        throw error;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Validates code type data
     */
    validateCodeType(codeType: CodeType): boolean {
      this.validationErrors = {};
      let isValid = true;

      // Validate required fields
      if (!codeType.name?.trim()) {
        this.validationErrors.name = ['Name is required'];
        isValid = false;
      }

      // Validate name uniqueness
      const existingType = this.codeTypes.find(
        type => type.name === codeType.name && type.id !== codeType.id
      );
      if (existingType) {
        this.validationErrors.name = ['Code type name must be unique'];
        isValid = false;
      }

      // Validate codes
      if (codeType.codes?.length) {
        codeType.codes.forEach((code, index) => {
          if (!this.validateCode(code)) {
            this.validationErrors[`codes[${index}]`] = ['Invalid code data'];
            isValid = false;
          }
        });
      }

      return isValid;
    },

    /**
     * Validates individual code data
     */
    validateCode(code: Code): boolean {
      let isValid = true;

      // Validate required fields
      if (!code.value?.trim()) {
        this.validationErrors.value = ['Code value is required'];
        isValid = false;
      }

      // Validate expiration date if expirable
      if (code.isExpireable && code.expiresAt) {
        const expirationDate = new Date(code.expiresAt);
        if (isNaN(expirationDate.getTime()) || expirationDate < new Date()) {
          this.validationErrors.expirationDate = ['Invalid expiration date'];
          isValid = false;
        }
      }

      return isValid;
    },

    /**
     * Archives a code within a code type
     */
    async archiveCode(typeId: number, codeId: number): Promise<void> {
      if (!this.hasEditPermission) {
        throw new Error('Permission denied: Cannot archive codes');
      }

      this.loading = true;
      try {
        await axios.patch(`${API_ENDPOINTS.CODES}/${codeId}/archive`);
        
        // Update local state
        const codeType = this.codeTypes.find(type => type.id === typeId);
        if (codeType) {
          const code = codeType.codes.find(code => code.id === codeId);
          if (code) {
            code.isActive = false;
          }
        }
        
        this.lastUpdated = new Date();
      } catch (error) {
        console.error('Failed to archive code:', error);
        throw error;
      } finally {
        this.loading = false;
      }
    }
  }
});
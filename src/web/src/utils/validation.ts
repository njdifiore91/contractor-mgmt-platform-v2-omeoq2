/**
 * @file Comprehensive validation utilities for form fields and data structures
 * @version 1.0.0
 */

import { memoize } from 'lodash'; // ^4.17.21
import { i18n } from 'vue-i18n'; // ^9.0.0
import { DialogField } from '@/types/common';

/**
 * Interface for validation result containing validation status and error details
 */
interface ValidationResult {
  isValid: boolean;
  errorMessage: string;
  asyncState?: {
    isPending: boolean;
    isResolved: boolean;
  };
}

/**
 * Sanitizes input to prevent XSS attacks
 */
const sanitizeInput = (value: any): any => {
  if (typeof value === 'string') {
    return value.replace(/[<>]/g, '');
  }
  return value;
};

/**
 * Validates a single form field value against its validation rules
 */
export const validateFormField = memoize(
  async (value: any, field: DialogField, isAsync = false): Promise<ValidationResult> => {
    const sanitizedValue = sanitizeInput(value);

    // Check required field
    if (field.required && (sanitizedValue === null || sanitizedValue === undefined || sanitizedValue === '')) {
      return {
        isValid: false,
        errorMessage: i18n.t('validation.required', { field: field.label })
      };
    }

    // Skip further validation if field is empty and not required
    if (!field.required && !sanitizedValue) {
      return { isValid: true, errorMessage: '' };
    }

    // Apply validation rules
    if (field.validation && field.validation.length > 0) {
      for (const rule of field.validation) {
        // Pattern validation
        if (rule.pattern && !rule.pattern.test(String(sanitizedValue))) {
          return {
            isValid: false,
            errorMessage: rule.errorMessage
          };
        }

        // Length validation
        if (typeof sanitizedValue === 'string') {
          if (rule.minLength && sanitizedValue.length < rule.minLength) {
            return {
              isValid: false,
              errorMessage: i18n.t('validation.minLength', { min: rule.minLength })
            };
          }
          if (rule.maxLength && sanitizedValue.length > rule.maxLength) {
            return {
              isValid: false,
              errorMessage: i18n.t('validation.maxLength', { max: rule.maxLength })
            };
          }
        }

        // Value range validation
        if (typeof sanitizedValue === 'number') {
          if (rule.minValue !== null && sanitizedValue < rule.minValue) {
            return {
              isValid: false,
              errorMessage: i18n.t('validation.minValue', { min: rule.minValue })
            };
          }
          if (rule.maxValue !== null && sanitizedValue > rule.maxValue) {
            return {
              isValid: false,
              errorMessage: i18n.t('validation.maxValue', { max: rule.maxValue })
            };
          }
        }

        // Custom validator
        if (rule.customValidator) {
          const isValid = rule.customValidator(sanitizedValue);
          if (!isValid) {
            return {
              isValid: false,
              errorMessage: rule.errorMessage
            };
          }
        }
      }
    }

    // Handle async validation if required
    if (isAsync) {
      return {
        isValid: true,
        errorMessage: '',
        asyncState: {
          isPending: false,
          isResolved: true
        }
      };
    }

    return { isValid: true, errorMessage: '' };
  }
);

/**
 * Validates email format using RFC 5322 standard
 */
export const validateEmail = memoize((email: string): ValidationResult => {
  const sanitizedEmail = sanitizeInput(email);

  // RFC 5322 compliant email regex
  const emailRegex = /^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])$/i;

  if (!emailRegex.test(sanitizedEmail)) {
    return {
      isValid: false,
      errorMessage: i18n.t('validation.email.invalid')
    };
  }

  const [localPart, domain] = sanitizedEmail.split('@');

  // Check local part length
  if (localPart.length > 64) {
    return {
      isValid: false,
      errorMessage: i18n.t('validation.email.localPartTooLong')
    };
  }

  // Check domain length
  if (domain.length > 255) {
    return {
      isValid: false,
      errorMessage: i18n.t('validation.email.domainTooLong')
    };
  }

  return { isValid: true, errorMessage: '' };
});

/**
 * Validates phone number format with international support
 */
export const validatePhoneNumber = memoize((phoneNumber: string, countryCode: string): ValidationResult => {
  const sanitizedPhone = sanitizeInput(phoneNumber).replace(/\D/g, '');

  const phonePatterns: { [key: string]: RegExp } = {
    'US': /^1?\d{10}$/,
    'CA': /^1?\d{10}$/,
    'GB': /^44\d{10}$/,
    'AU': /^61\d{9}$/
  };

  const pattern = phonePatterns[countryCode];
  if (!pattern) {
    return {
      isValid: false,
      errorMessage: i18n.t('validation.phone.unsupportedCountry')
    };
  }

  if (!pattern.test(sanitizedPhone)) {
    return {
      isValid: false,
      errorMessage: i18n.t('validation.phone.invalid')
    };
  }

  return { isValid: true, errorMessage: '' };
});

/**
 * Validates postal codes with multi-country support
 */
export const validateZipCode = memoize((postalCode: string, countryCode: string): ValidationResult => {
  const sanitizedCode = sanitizeInput(postalCode).toUpperCase();

  const postalPatterns: { [key: string]: RegExp } = {
    'US': /^\d{5}(-\d{4})?$/,
    'CA': /^[ABCEGHJ-NPRSTVXY]\d[ABCEGHJ-NPRSTV-Z]\s?\d[ABCEGHJ-NPRSTV-Z]\d$/,
    'GB': /^[A-Z]{1,2}\d[A-Z\d]?\s?\d[A-Z]{2}$/,
    'AU': /^\d{4}$/
  };

  const pattern = postalPatterns[countryCode];
  if (!pattern) {
    return {
      isValid: false,
      errorMessage: i18n.t('validation.postalCode.unsupportedCountry')
    };
  }

  if (!pattern.test(sanitizedCode)) {
    return {
      isValid: false,
      errorMessage: i18n.t('validation.postalCode.invalid')
    };
  }

  return { isValid: true, errorMessage: '' };
});
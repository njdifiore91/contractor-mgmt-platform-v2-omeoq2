/**
 * @file Utility functions for consistent data formatting across the application
 * @version 1.0.0
 */

// date-fns v2.30.0 - Date formatting and manipulation library
import { format, isValid, parseISO } from 'date-fns';
import { enUS } from 'date-fns/locale';

// Define Address interface since it's not in common.ts but needed for formatting
interface Address {
  line1: string;
  line2?: string;
  line3?: string;
  city: string;
  state: string;
  zip: string;
  country: string;
}

/**
 * Formats a date string or Date object into a consistent display format
 * @param date - Date to format
 * @param formatStr - Format string (defaults to 'MM/dd/yyyy')
 * @param timezone - Timezone (defaults to 'UTC')
 * @returns Formatted date string or empty string if invalid
 */
export const formatDate = (
  date: Date | string | null | undefined,
  formatStr: string = 'MM/dd/yyyy',
  timezone: string = 'UTC'
): string => {
  if (!date) return '';
  
  try {
    const dateObj = typeof date === 'string' ? parseISO(date) : date;
    if (!isValid(dateObj)) return '';
    
    return format(dateObj, formatStr, {
      locale: enUS,
      timeZone: timezone
    });
  } catch {
    return '';
  }
};

/**
 * Formats a phone number with optional extension and country code
 * @param phoneNumber - Raw phone number string
 * @param extension - Optional extension
 * @param countryCode - Country code (defaults to 'US')
 * @returns Formatted phone number string
 */
export const formatPhoneNumber = (
  phoneNumber: string,
  extension?: string,
  countryCode: string = 'US'
): string => {
  if (!phoneNumber) return '';

  // Remove all non-numeric characters
  const cleaned = phoneNumber.replace(/\D/g, '');
  
  if (countryCode === 'US') {
    if (cleaned.length !== 10) return phoneNumber; // Return original if not valid
    
    const areaCode = cleaned.slice(0, 3);
    const prefix = cleaned.slice(3, 6);
    const lineNumber = cleaned.slice(6);
    
    const formatted = `(${areaCode}) ${prefix}-${lineNumber}`;
    return extension ? `${formatted} ext. ${extension}` : formatted;
  }
  
  // Default international format if not US
  return extension ? `${phoneNumber} ext. ${extension}` : phoneNumber;
};

/**
 * Formats an address object into a string representation
 * @param address - Address object to format
 * @param multiline - Whether to return multiline format
 * @param locale - Locale for formatting (defaults to 'en-US')
 * @returns Formatted address string
 */
export const formatAddress = (
  address: Address,
  multiline: boolean = false,
  locale: string = 'en-US'
): string => {
  if (!address) return '';

  const parts = [
    address.line1,
    address.line2,
    address.line3,
    `${address.city}, ${address.state} ${address.zip}`,
    address.country
  ].filter(Boolean);

  return multiline ? parts.join('\n') : parts.join(', ');
};

/**
 * Formats name components into a full name string
 * @param firstName - First name
 * @param lastName - Last name
 * @param middleName - Optional middle name
 * @param suffix - Optional name suffix
 * @param locale - Locale for formatting (defaults to 'en-US')
 * @returns Formatted full name
 */
export const formatName = (
  firstName: string,
  lastName: string,
  middleName?: string,
  suffix?: string,
  locale: string = 'en-US'
): string => {
  if (!firstName && !lastName) return '';

  const parts = [
    firstName,
    middleName,
    lastName,
    suffix && suffix.length ? `, ${suffix}` : ''
  ].filter(Boolean);

  return parts.join(' ').trim();
};

/**
 * Formats a number as a currency string
 * @param amount - Numeric amount to format
 * @param currency - Currency code (defaults to 'USD')
 * @param locale - Locale for formatting (defaults to 'en-US')
 * @returns Formatted currency string
 */
export const formatCurrency = (
  amount: number,
  currency: string = 'USD',
  locale: string = 'en-US'
): string => {
  if (typeof amount !== 'number') return '';

  try {
    return new Intl.NumberFormat(locale, {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(amount);
  } catch {
    // Fallback to basic formatting if Intl fails
    return `${currency} ${amount.toFixed(2)}`;
  }
};
// @ts-check
import axios, { AxiosError } from 'axios'; // ^1.3.0

// Constants for configuration and validation
const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB max file size
const ALLOWED_FILE_TYPES = ['.pdf', '.doc', '.docx', '.jpg', '.png'];
const REQUEST_TIMEOUT = 30000; // 30 seconds
const MAX_RETRY_ATTEMPTS = 3;

// RFC 5322 compliant email regex
const EMAIL_REGEX = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;

// Create axios instance with default configuration
const emailClient = axios.create({
  timeout: REQUEST_TIMEOUT,
  headers: {
    'Content-Type': 'multipart/form-data',
  },
});

/**
 * Validates email address format and domain
 * @param {string} email - Email address to validate
 * @returns {boolean} True if email is valid
 */
const validateEmail = (email: string): boolean => {
  if (!email || typeof email !== 'string') {
    return false;
  }
  
  if (!EMAIL_REGEX.test(email)) {
    return false;
  }

  const [, domain] = email.split('@');
  // Additional domain validation could be implemented here
  return domain.includes('.');
};

/**
 * Validates file attachments for size and type
 * @param {File[]} attachments - Array of files to validate
 * @returns {Promise<boolean>} True if all attachments are valid
 */
const validateAttachments = async (attachments: File[]): Promise<boolean> => {
  if (!attachments || !Array.isArray(attachments)) {
    return true;
  }

  for (const file of attachments) {
    if (file.size > MAX_FILE_SIZE) {
      throw new Error(`File ${file.name} exceeds maximum size of ${MAX_FILE_SIZE / 1024 / 1024}MB`);
    }

    const fileExtension = `.${file.name.split('.').pop()?.toLowerCase()}`;
    if (!ALLOWED_FILE_TYPES.includes(fileExtension)) {
      throw new Error(`File type ${fileExtension} is not allowed`);
    }
  }

  return true;
};

/**
 * Sends a regular email with optional attachments
 * @param {string} to - Recipient email address
 * @param {string} subject - Email subject
 * @param {string} body - Email body content
 * @param {File[]} attachments - Optional file attachments
 * @returns {Promise<boolean>} True if email sent successfully
 */
export const sendEmail = async (
  to: string,
  subject: string,
  body: string,
  attachments: File[] = []
): Promise<boolean> => {
  try {
    if (!validateEmail(to)) {
      throw new Error('Invalid recipient email address');
    }

    await validateAttachments(attachments);

    const formData = new FormData();
    formData.append('to', to);
    formData.append('subject', subject);
    formData.append('body', body);

    attachments.forEach((file, index) => {
      formData.append(`attachment${index}`, file);
    });

    let attempts = 0;
    while (attempts < MAX_RETRY_ATTEMPTS) {
      try {
        const response = await emailClient.post('/api/email/send', formData);
        return response.status === 200;
      } catch (error) {
        attempts++;
        if (attempts === MAX_RETRY_ATTEMPTS) {
          throw error;
        }
        await new Promise(resolve => setTimeout(resolve, 1000 * attempts));
      }
    }

    return false;
  } catch (error) {
    console.error('Error sending email:', error);
    throw error;
  }
};

/**
 * Sends an email using a predefined template
 * @param {string} to - Recipient email address
 * @param {string} templateName - Name of the email template
 * @param {object} templateData - Dynamic data for template
 * @param {File[]} attachments - Optional file attachments
 * @returns {Promise<boolean>} True if email sent successfully
 */
export const sendTemplatedEmail = async (
  to: string,
  templateName: string,
  templateData: Record<string, unknown>,
  attachments: File[] = []
): Promise<boolean> => {
  try {
    if (!validateEmail(to)) {
      throw new Error('Invalid recipient email address');
    }

    if (!templateName || typeof templateName !== 'string') {
      throw new Error('Invalid template name');
    }

    await validateAttachments(attachments);

    const formData = new FormData();
    formData.append('to', to);
    formData.append('templateName', templateName);
    formData.append('templateData', JSON.stringify(templateData));

    attachments.forEach((file, index) => {
      formData.append(`attachment${index}`, file);
    });

    let attempts = 0;
    while (attempts < MAX_RETRY_ATTEMPTS) {
      try {
        const response = await emailClient.post('/api/email/send-template', formData);
        return response.status === 200;
      } catch (error) {
        attempts++;
        if (attempts === MAX_RETRY_ATTEMPTS) {
          throw error;
        }
        await new Promise(resolve => setTimeout(resolve, 1000 * attempts));
      }
    }

    return false;
  } catch (error) {
    console.error('Error sending templated email:', error);
    throw error;
  }
};

/**
 * Sends a notification email when an inspector is mobilized
 * @param {string} inspectorEmail - Inspector's email address
 * @param {string} projectName - Name of the project
 * @param {string} customerName - Name of the customer
 * @param {Date} mobilizationDate - Date of mobilization
 * @returns {Promise<boolean>} True if notification sent successfully
 */
export const sendMobilizationNotification = async (
  inspectorEmail: string,
  projectName: string,
  customerName: string,
  mobilizationDate: Date
): Promise<boolean> => {
  try {
    if (!validateEmail(inspectorEmail)) {
      throw new Error('Invalid inspector email address');
    }

    if (!projectName || !customerName) {
      throw new Error('Project name and customer name are required');
    }

    if (!(mobilizationDate instanceof Date) || isNaN(mobilizationDate.getTime())) {
      throw new Error('Invalid mobilization date');
    }

    const templateData = {
      projectName,
      customerName,
      mobilizationDate: mobilizationDate.toISOString(),
    };

    return await sendTemplatedEmail(
      inspectorEmail,
      'inspector-mobilization',
      templateData
    );
  } catch (error) {
    console.error('Error sending mobilization notification:', error);
    throw error;
  }
};
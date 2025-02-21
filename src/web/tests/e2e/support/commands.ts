/// <reference types="cypress" />
import { BaseEntity } from '../../../src/types/common';

// Extend Cypress namespace for custom commands
declare global {
  namespace Cypress {
    interface Chainable {
      login(permissions: string[], credentials?: { username?: string; password?: string }): Chainable<void>;
      searchInTable(selector: string, searchTerm: string, options?: SearchTableOptions): Chainable<void>;
      fillForm(formData: Record<string, any>, validationRules?: Record<string, any>): Chainable<void>;
      verifyTableData(selector: string, expectedData: any[], options?: VerifyTableOptions): Chainable<void>;
    }
  }
}

interface SearchTableOptions {
  debounceMs?: number;
  timeout?: number;
  scrollBehavior?: 'virtual' | 'pagination';
  checkLoadingState?: boolean;
}

interface VerifyTableOptions {
  timeout?: number;
  scrollBehavior?: 'virtual' | 'pagination';
  compareFields?: string[];
  strictOrder?: boolean;
}

/**
 * Performs secure user authentication with permission validation
 * @version 12.0.0 (cypress)
 */
Cypress.Commands.add('login', (permissions: string[], credentials = {}) => {
  const defaultCredentials = {
    username: Cypress.env('AUTH_USERNAME'),
    password: Cypress.env('AUTH_PASSWORD')
  };

  const finalCredentials = { ...defaultCredentials, ...credentials };

  // Intercept auth-related requests
  cy.intercept('POST', '**/api/auth/login').as('loginRequest');
  cy.intercept('GET', '**/api/auth/permissions').as('permissionsRequest');

  cy.visit('/login', { timeout: 10000 });

  // Perform login
  cy.get('[data-cy=username-input]').type(finalCredentials.username);
  cy.get('[data-cy=password-input]').type(finalCredentials.password, { log: false });
  cy.get('[data-cy=login-submit]').click();

  // Verify authentication
  cy.wait('@loginRequest').its('response.statusCode').should('eq', 200);
  cy.wait('@permissionsRequest').then((interception) => {
    const userPermissions = interception.response?.body || [];
    permissions.forEach(permission => {
      expect(userPermissions).to.include(permission);
    });
  });

  // Verify successful navigation post-login
  cy.url().should('not.include', '/login');
});

/**
 * Performs search operations in virtual scrolling tables
 * @version 12.0.0 (cypress)
 */
Cypress.Commands.add('searchInTable', (selector: string, searchTerm: string, options: SearchTableOptions = {}) => {
  const {
    debounceMs = 300,
    timeout = 10000,
    scrollBehavior = 'virtual',
    checkLoadingState = true
  } = options;

  // Wait for table to be ready
  cy.get(selector, { timeout }).should('exist');

  // Clear existing search and type new term
  cy.get(`${selector} [data-cy=search-input]`)
    .clear()
    .type(searchTerm);

  // Handle loading states
  if (checkLoadingState) {
    cy.get(`${selector} [data-cy=loading-indicator]`).should('exist');
    cy.get(`${selector} [data-cy=loading-indicator]`).should('not.exist');
  }

  // Virtual scroll handling
  if (scrollBehavior === 'virtual') {
    cy.get(`${selector} .virtual-scroll-viewport`).scrollTo('bottom', { duration: 1000 });
    cy.wait(debounceMs); // Allow for data loading
  }

  // Verify search results
  cy.get(`${selector} [data-cy=no-results]`).should('not.exist');
  cy.get(`${selector} [data-cy=table-row]`).should('have.length.gt', 0);
});

/**
 * Fills form fields with comprehensive validation
 * @version 12.0.0 (cypress)
 */
Cypress.Commands.add('fillForm', (formData: Record<string, any>, validationRules: Record<string, any> = {}) => {
  Object.entries(formData).forEach(([field, value]) => {
    const selector = `[data-cy=form-field-${field}]`;
    
    // Handle different input types
    cy.get(selector).then($el => {
      const inputType = $el.attr('type') || $el.prop('tagName').toLowerCase();
      
      switch(inputType) {
        case 'select':
          cy.get(selector).select(value);
          break;
        case 'checkbox':
          if (value) cy.get(selector).check();
          else cy.get(selector).uncheck();
          break;
        case 'radio':
          cy.get(`${selector}[value="${value}"]`).check();
          break;
        default:
          cy.get(selector).clear().type(value.toString());
      }
    });

    // Apply validation if rules exist
    if (validationRules[field]) {
      cy.get(`${selector}-error`).should('not.exist');
    }
  });
});

/**
 * Verifies table data with virtual scrolling support
 * @version 12.0.0 (cypress)
 */
Cypress.Commands.add('verifyTableData', (selector: string, expectedData: any[], options: VerifyTableOptions = {}) => {
  const {
    timeout = 10000,
    scrollBehavior = 'virtual',
    compareFields = [],
    strictOrder = true
  } = options;

  // Wait for table initialization
  cy.get(selector, { timeout }).should('exist');

  if (scrollBehavior === 'virtual') {
    // Handle virtual scrolling verification
    cy.get(`${selector} .virtual-scroll-viewport`).then($viewport => {
      const viewportHeight = $viewport.height();
      const rowHeight = 48; // Assumed default row height
      const visibleRows = Math.floor(viewportHeight / rowHeight);

      // Verify initial visible rows
      cy.get(`${selector} [data-cy=table-row]`)
        .should('have.length.at.least', Math.min(visibleRows, expectedData.length));
    });
  }

  // Verify data content
  expectedData.forEach((expectedItem, index) => {
    if (compareFields.length > 0) {
      compareFields.forEach(field => {
        cy.get(`${selector} [data-cy=table-row]:nth-child(${index + 1}) [data-cy=cell-${field}]`)
          .should('contain', expectedItem[field]);
      });
    } else {
      cy.get(`${selector} [data-cy=table-row]:nth-child(${index + 1})`)
        .should('exist');
    }
  });
});

// Export commands module
export {};
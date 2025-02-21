/// <reference types="cypress" />

import { login, searchInTable, fillForm, verifyTableData } from '../../support/commands';

describe('Equipment Assignment Page', () => {
  const selectors = {
    virtualScroll: {
      viewport: '[data-cy=virtual-scroll-viewport]',
      item: '[data-cy=virtual-scroll-item]',
      loading: '[data-cy=virtual-scroll-loading]'
    },
    form: {
      equipmentSelect: '[data-cy=equipment-select]',
      inspectorSelect: '[data-cy=inspector-select]',
      conditionInput: '[data-cy=condition-input]',
      dateInput: '[data-cy=assignment-date]',
      submitButton: '[data-cy=submit-button]'
    },
    feedback: {
      errorMessage: '[data-cy=error-message]',
      successMessage: '[data-cy=success-message]',
      loadingIndicator: '[data-cy=loading-indicator]'
    }
  };

  beforeEach(() => {
    // Login with required permissions
    login(['Edit Equipment']);

    // Visit equipment assignment page
    cy.visit('/equipment/assign');

    // Intercept API calls
    cy.intercept('GET', '/api/equipment*').as('getEquipment');
    cy.intercept('GET', '/api/inspectors*').as('getInspectors');
    cy.intercept('POST', '/api/equipment/assign').as('assignEquipment');
    cy.intercept('GET', '/api/audit-log*').as('getAuditLog');

    // Wait for initial data load
    cy.wait(['@getEquipment', '@getInspectors']);
  });

  it('Should enforce Edit Equipment permission for assignment', () => {
    // Test with user lacking permissions
    login([]);
    cy.visit('/equipment/assign');
    
    // Verify form elements are disabled
    cy.get(selectors.form.equipmentSelect).should('be.disabled');
    cy.get(selectors.form.inspectorSelect).should('be.disabled');
    cy.get(selectors.form.submitButton).should('be.disabled');
  });

  it('Should handle virtual scrolling in equipment and inspector lists', () => {
    // Test equipment list virtual scrolling
    cy.get(selectors.virtualScroll.viewport).first().should('be.visible');
    cy.get(selectors.virtualScroll.item).should('have.length.gt', 0);

    // Scroll and verify dynamic loading
    cy.get(selectors.virtualScroll.viewport).scrollTo('bottom', { duration: 1000 });
    cy.get(selectors.virtualScroll.loading).should('not.exist');
    cy.get(selectors.virtualScroll.item).should('have.length.gt', 10);
  });

  it('Should successfully assign equipment with all required fields', () => {
    const testData = {
      equipment: 'Test Equipment Model-123',
      inspector: 'John Doe',
      condition: 'Good',
      date: '2024-01-20'
    };

    // Search and select equipment
    searchInTable(selectors.form.equipmentSelect, testData.equipment, {
      scrollBehavior: 'virtual'
    });

    // Search and select inspector
    searchInTable(selectors.form.inspectorSelect, testData.inspector, {
      scrollBehavior: 'virtual'
    });

    // Fill assignment details
    fillForm({
      condition: testData.condition,
      date: testData.date
    });

    // Submit assignment
    cy.get(selectors.form.submitButton).click();

    // Verify success
    cy.wait('@assignEquipment').its('response.statusCode').should('eq', 200);
    cy.get(selectors.feedback.successMessage).should('be.visible');
    cy.wait('@getAuditLog');
  });

  it('Should validate all required fields before submission', () => {
    // Try submitting without required fields
    cy.get(selectors.form.submitButton).click();

    // Verify validation messages
    cy.get(`${selectors.form.equipmentSelect}-error`).should('be.visible');
    cy.get(`${selectors.form.inspectorSelect}-error`).should('be.visible');
    cy.get(`${selectors.form.conditionInput}-error`).should('be.visible');
    cy.get(`${selectors.form.dateInput}-error`).should('be.visible');
  });

  it('Should handle API errors with proper user feedback', () => {
    // Mock API error
    cy.intercept('POST', '/api/equipment/assign', {
      statusCode: 500,
      body: { error: 'Internal Server Error' }
    }).as('assignmentError');

    // Attempt assignment
    fillForm({
      equipment: 'Test Equipment',
      inspector: 'John Doe',
      condition: 'Good',
      date: '2024-01-20'
    });

    cy.get(selectors.form.submitButton).click();

    // Verify error handling
    cy.get(selectors.feedback.errorMessage)
      .should('be.visible')
      .and('contain', 'Failed to assign equipment');
  });

  it('Should maintain virtual scroll position after operations', () => {
    // Scroll to specific position
    cy.get(selectors.virtualScroll.viewport).scrollTo('50%');
    const scrollPosition = cy.get(selectors.virtualScroll.viewport).invoke('scrollTop');

    // Perform operation
    cy.get(selectors.virtualScroll.item).first().click();

    // Verify scroll position maintained
    cy.get(selectors.virtualScroll.viewport).invoke('scrollTop').should('eq', scrollPosition);
  });

  it('Should create audit log entries for assignments', () => {
    // Perform assignment
    fillForm({
      equipment: 'Test Equipment',
      inspector: 'John Doe',
      condition: 'Good',
      date: '2024-01-20'
    });

    cy.get(selectors.form.submitButton).click();

    // Verify audit log
    cy.wait('@getAuditLog').then((interception) => {
      const auditLog = interception.response?.body;
      expect(auditLog).to.have.property('action', 'EQUIPMENT_ASSIGNED');
      expect(auditLog).to.have.property('userId');
      expect(auditLog).to.have.property('timestamp');
    });
  });
});
/// <reference types="cypress" />

// @version cypress ^12.0.0
import { login } from '../support/commands';

describe('Inspector Mobilization', () => {
  const testData = {
    employeeName: 'Test Inspector',
    primaryEmail: 'test@example.com',
    phone: '1234567890',
    dateOfBirth: '1990-01-01',
    mobDate: '2023-01-01',
    hireType: 'Full-Time',
    hirePack: 'Standard',
    training: true,
    project: 'Test Project',
    customer: 'Test Customer',
    contract: 'Test Contract',
    department: 'Testing',
    function: 'Inspector',
    type: 'Regular',
    location: 'Test Location',
    classification: 'Level 1',
    certRequired: true,
    certsRequired: ['Cert1', 'Cert2'],
    addressType: 'Home',
    shipOpt: 'Express',
    projectContact: 'Project Manager',
    invoiceContact: 'Finance Manager'
  };

  beforeEach(() => {
    // Login with required permissions
    cy.login(['Edit Users']);

    // Intercept API calls
    cy.intercept('GET', '**/api/inspectors*').as('getInspectors');
    cy.intercept('POST', '**/api/inspectors/mobilize').as('mobilizeInspector');
    cy.intercept('POST', '**/api/notifications/email').as('emailNotification');

    // Navigate to inspector list
    cy.visit('/inspectors');
    cy.wait('@getInspectors');

    // Select test inspector and open mobilization dialog
    cy.searchInTable('[data-cy=inspectors-table]', testData.employeeName);
    cy.get('[data-cy=mobilize-button]').first().click();
  });

  it('should validate all required mobilization fields', () => {
    // Attempt submission with empty form
    cy.get('[data-cy=submit-mobilization]').click();

    // Verify validation messages for required fields
    const requiredFields = [
      'employeeName',
      'primaryEmail',
      'dateOfBirth',
      'mobDate',
      'hireType',
      'project',
      'customer',
      'contract',
      'department',
      'function',
      'type',
      'location',
      'classification'
    ];

    requiredFields.forEach(field => {
      cy.get(`[data-cy=form-field-${field}-error]`)
        .should('exist')
        .and('contain', 'This field is required');
    });

    // Test field-specific validations
    cy.get('[data-cy=form-field-primaryEmail]')
      .type('invalid-email')
      .blur();
    cy.get('[data-cy=form-field-primaryEmail-error]')
      .should('contain', 'Invalid email format');

    cy.get('[data-cy=form-field-phone]')
      .type('abc')
      .blur();
    cy.get('[data-cy=form-field-phone-error]')
      .should('contain', 'Invalid phone number format');

    // Test date validations
    const pastDate = new Date();
    pastDate.setDate(pastDate.getDate() - 1);
    cy.get('[data-cy=form-field-mobDate]')
      .type(pastDate.toISOString().split('T')[0])
      .blur();
    cy.get('[data-cy=form-field-mobDate-error]')
      .should('contain', 'Mobilization date must be in the future');
  });

  it('should handle successful mobilization workflow', () => {
    // Fill all required fields
    cy.fillForm(testData);

    // Submit form
    cy.get('[data-cy=submit-mobilization]').click();

    // Verify API call
    cy.wait('@mobilizeInspector').then((interception) => {
      expect(interception.response?.statusCode).to.equal(200);
      expect(interception.request.body).to.deep.include(testData);
    });

    // Verify success notification
    cy.get('[data-cy=success-notification]')
      .should('exist')
      .and('contain', 'Inspector mobilized successfully');

    // Verify email notification
    cy.wait('@emailNotification');

    // Verify navigation back to inspector list
    cy.url().should('include', '/inspectors');
  });

  it('should verify email notification content', () => {
    // Fill and submit form
    cy.fillForm(testData);
    cy.get('[data-cy=submit-mobilization]').click();

    // Verify email notification request
    cy.wait('@emailNotification').then((interception) => {
      const emailRequest = interception.request.body;
      
      expect(emailRequest).to.deep.include({
        to: testData.primaryEmail,
        template: 'INSPECTOR_MOBILIZATION',
        data: {
          inspectorName: testData.employeeName,
          project: testData.project,
          customer: testData.customer,
          mobilizationDate: testData.mobDate,
          location: testData.location
        }
      });
    });

    // Verify email preview in UI
    cy.get('[data-cy=email-preview]').should('exist');
    cy.get('[data-cy=email-preview-recipient]')
      .should('contain', testData.primaryEmail);
    cy.get('[data-cy=email-preview-subject]')
      .should('contain', 'Mobilization Confirmation');
  });
});
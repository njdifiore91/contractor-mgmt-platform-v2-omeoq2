import { QuickLink } from '@/types/admin';
import { faker } from '@faker-js/faker';

describe('Quick Links Management', () => {
  // Helper function to generate mock quick links
  const mockQuickLinks = (count: number): QuickLink[] => {
    return Array.from({ length: count }, (_, index) => ({
      id: index + 1,
      label: faker.word.noun(),
      link: faker.internet.url(),
      order: index + 1,
      isActive: true,
      createdAt: new Date(),
      createdBy: 1,
      updatedAt: null,
      updatedBy: null
    }));
  };

  // Helper function to mock user configuration
  const mockUserConfig = (config = {}) => ({
    quickLinks: {
      enabled: true,
      visibleLinks: [],
      customOrder: [],
      ...config
    }
  });

  beforeEach(() => {
    // Intercept API requests
    cy.intercept('GET', '/api/admin/quicklinks', { statusCode: 200, body: [] }).as('getQuickLinks');
    cy.intercept('GET', '/api/user/config', { statusCode: 200, body: mockUserConfig() }).as('getUserConfig');
    cy.intercept('PUT', '/api/admin/quicklinks/*', { statusCode: 200 }).as('updateQuickLink');

    // Visit quick links admin page
    cy.visit('/admin/quick-links');
    cy.wait('@getQuickLinks');
    cy.wait('@getUserConfig');
  });

  it('displays quick links in correct order', () => {
    const mockLinks = mockQuickLinks(3);
    cy.intercept('GET', '/api/admin/quicklinks', { statusCode: 200, body: mockLinks }).as('getQuickLinks');

    cy.visit('/admin/quick-links');
    cy.wait('@getQuickLinks');

    // Verify table headers
    cy.get('th').should('contain', 'Label');
    cy.get('th').should('contain', 'Link');

    // Verify links are displayed in correct order
    cy.get('tbody tr').should('have.length', 3);
    mockLinks.forEach((link, index) => {
      cy.get(`tbody tr:nth-child(${index + 1})`).within(() => {
        cy.get('td').eq(0).should('contain', link.label);
        cy.get('td').eq(1).should('contain', link.link);
      });
    });
  });

  it('validates link opens in new tab', () => {
    const mockLinks = mockQuickLinks(1);
    cy.intercept('GET', '/api/admin/quicklinks', { statusCode: 200, body: mockLinks }).as('getQuickLinks');

    cy.visit('/admin/quick-links');
    cy.wait('@getQuickLinks');

    // Verify link attributes
    cy.get('tbody tr:first-child a')
      .should('have.attr', 'target', '_blank')
      .and('have.attr', 'rel', 'noopener noreferrer');
  });

  it('respects user-specific configurations', () => {
    const mockLinks = mockQuickLinks(3);
    const userConfig = mockUserConfig({
      visibleLinks: [mockLinks[0].id, mockLinks[2].id],
      customOrder: [mockLinks[2].id, mockLinks[0].id]
    });

    cy.intercept('GET', '/api/admin/quicklinks', { statusCode: 200, body: mockLinks }).as('getQuickLinks');
    cy.intercept('GET', '/api/user/config', { statusCode: 200, body: userConfig }).as('getUserConfig');

    cy.visit('/admin/quick-links');
    cy.wait(['@getQuickLinks', '@getUserConfig']);

    // Verify only configured links are visible and in correct order
    cy.get('tbody tr').should('have.length', 2);
    cy.get('tbody tr:nth-child(1)').should('contain', mockLinks[2].label);
    cy.get('tbody tr:nth-child(2)').should('contain', mockLinks[0].label);
  });

  it('validates all required fields when editing', () => {
    const mockLink = mockQuickLinks(1)[0];
    cy.intercept('GET', '/api/admin/quicklinks', { statusCode: 200, body: [mockLink] }).as('getQuickLinks');

    cy.visit('/admin/quick-links');
    cy.wait('@getQuickLinks');

    // Click edit button
    cy.get('[data-cy="edit-quick-link"]').first().click();

    // Test empty label validation
    cy.get('[data-cy="quick-link-label"]').clear();
    cy.get('[data-cy="save-quick-link"]').click();
    cy.get('[data-cy="label-error"]').should('be.visible');

    // Test invalid URL validation
    cy.get('[data-cy="quick-link-link"]').clear().type('invalid-url');
    cy.get('[data-cy="save-quick-link"]').click();
    cy.get('[data-cy="link-error"]').should('be.visible');

    // Test valid submission
    cy.get('[data-cy="quick-link-label"]').type('Valid Label');
    cy.get('[data-cy="quick-link-link"]').clear().type('https://valid-url.com');
    cy.get('[data-cy="save-quick-link"]').click();
    cy.wait('@updateQuickLink');
    cy.get('[data-cy="edit-dialog"]').should('not.exist');
  });

  it('checks edit links permission', () => {
    // Mock user without edit permission
    cy.intercept('GET', '/api/user/permissions', {
      statusCode: 200,
      body: { editLinks: false }
    }).as('getPermissions');

    cy.visit('/admin/quick-links');
    cy.wait('@getPermissions');

    // Verify edit buttons are not visible
    cy.get('[data-cy="edit-quick-link"]').should('not.exist');
    cy.get('[data-cy="add-quick-link"]').should('not.exist');
  });
});
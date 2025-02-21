import { Customer } from '@/types/customer'; // v1.0.0
import { BaseEntity } from '@/types/common'; // v1.0.0
import 'cypress'; // ^12.0.0

describe('Customer Management', () => {
  beforeEach(() => {
    // Visit customer list page and ensure clean test environment
    cy.visit('/customers');
    cy.intercept('GET', '/api/customers*').as('getCustomers');
    cy.intercept('POST', '/api/customers').as('createCustomer');
    
    // Wait for initial page load
    cy.wait('@getCustomers');
    
    // Verify virtual scroll container is initialized
    cy.get("[data-test='virtual-scroll-container']").should('exist');
    cy.get("[data-test='virtual-scroll-container']").scrollTo(0, 0);
  });

  // Helper function to create test customer
  const createTestCustomer = async (customerData: Partial<Customer>): Promise<void> => {
    cy.get("[data-test='add-customer-btn']").click();
    
    // Fill customer form
    cy.get("[data-test='customer-name-input']").type(customerData.name || '');
    cy.get("[data-test='customer-code-input']").type(customerData.code || '');
    
    // Submit form
    cy.get("[data-test='submit-btn']").click();
    
    // Wait for creation and verify
    cy.wait('@createCustomer');
    cy.get("[data-test='success-message']").should('be.visible');
  };

  // Helper function to test virtual scrolling
  const simulateVirtualScroll = async (scrollDistance: number): Promise<void> => {
    cy.get("[data-test='virtual-scroll-container']")
      .scrollTo(0, scrollDistance, { duration: 500 });
    
    // Wait for new items to load
    cy.wait('@getCustomers');
    
    // Verify scroll position
    cy.get("[data-test='virtual-scroll-container']")
      .invoke('scrollTop')
      .should('be.approximately', scrollDistance, 10);
  };

  it('should display customer list page with virtual scroll', () => {
    // Verify page elements
    cy.get("[data-test='customer-search-form']").should('be.visible');
    cy.get("[data-test='customer-table']").should('be.visible');
    cy.get("[data-test='virtual-scroll-container']").should('be.visible');
    
    // Verify initial items are loaded
    cy.get("[data-test='scroll-item']").should('have.length.greaterThan', 0);
  });

  it('should handle virtual scrolling', () => {
    // Load test dataset
    const testCustomers = Array.from({ length: 100 }, (_, i) => ({
      id: i + 1,
      name: `Test Customer ${i + 1}`,
      code: `TC${i + 1}`
    }));
    
    cy.intercept('GET', '/api/customers*', {
      body: testCustomers,
      headers: { 'x-total-count': '100' }
    }).as('getLargeCustomerList');

    // Verify initial render
    cy.get("[data-test='scroll-item']")
      .should('have.length.lessThan', testCustomers.length);

    // Test progressive scrolling
    cy.wrap([500, 1000, 1500]).each((scrollPos) => {
      simulateVirtualScroll(scrollPos);
      cy.get("[data-test='loading-indicator']").should('not.exist');
    });
  });

  it('should validate all required fields', () => {
    // Click add without filling required fields
    cy.get("[data-test='add-customer-btn']").click();
    cy.get("[data-test='submit-btn']").click();

    // Verify validation messages
    cy.get("[data-test='validation-message']").should('have.length.at.least', 2);
    cy.get("[data-test='validation-message']").should('contain', 'Name is required');
    cy.get("[data-test='validation-message']").should('contain', 'Code is required');

    // Fill required fields
    cy.get("[data-test='customer-name-input']").type('Test Customer');
    cy.get("[data-test='customer-code-input']").type('TC001');

    // Verify validation messages clear
    cy.get("[data-test='validation-message']").should('not.exist');

    // Submit valid form
    cy.get("[data-test='submit-btn']").click();
    cy.wait('@createCustomer');
    cy.get("[data-test='success-message']").should('be.visible');
  });

  it('should search for customers', () => {
    const searchTerm = 'Test Customer';
    
    // Perform search
    cy.get("[data-test='customer-search-form']")
      .find('input')
      .type(searchTerm);
    
    cy.wait('@getCustomers');

    // Verify search results
    cy.get("[data-test='scroll-item']").each(($el) => {
      cy.wrap($el).should('contain', searchTerm);
    });
  });

  it('should edit existing customer', () => {
    // Create test customer first
    const testCustomer = {
      name: 'Edit Test Customer',
      code: 'ETC001'
    };
    
    createTestCustomer(testCustomer);

    // Click edit button
    cy.get("[data-test='edit-customer-btn']").first().click();

    // Modify customer data
    cy.get("[data-test='customer-name-input']")
      .clear()
      .type('Updated Customer Name');

    // Submit changes
    cy.get("[data-test='submit-btn']").click();
    
    // Verify update
    cy.get("[data-test='success-message']").should('be.visible');
    cy.get("[data-test='scroll-item']")
      .first()
      .should('contain', 'Updated Customer Name');
  });
});
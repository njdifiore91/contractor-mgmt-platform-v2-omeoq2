// System v6.0.0
using System;
// System.Collections.Generic v6.0.0
using System.Collections.Generic;
// System.Threading.Tasks v6.0.0
using System.Threading.Tasks;
using Backend.Core.Entities;

namespace Backend.Core.Interfaces.Repositories
{
    /// <summary>
    /// Defines the contract for customer data access operations.
    /// Provides comprehensive methods for managing customer entities including CRUD operations,
    /// search functionality, and related data management for contacts and contracts.
    /// </summary>
    public interface ICustomerRepository
    {
        /// <summary>
        /// Retrieves a customer by their unique identifier including all related contacts and contracts.
        /// </summary>
        /// <param name="id">The unique identifier of the customer to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The customer with the specified ID including all related data, or null if not found.</returns>
        Task<Customer> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all customers with their related contacts and contracts.
        /// </summary>
        /// <param name="includeInactive">When true, includes inactive customers in the result set.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>Collection of all customers with their related data.</returns>
        Task<IEnumerable<Customer>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for customers by name or code with support for partial matches.
        /// </summary>
        /// <param name="searchTerm">The search term to match against customer name or code.</param>
        /// <param name="exactMatch">When true, performs exact matching instead of partial matching.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>Collection of customers matching the search criteria including related data.</returns>
        Task<IEnumerable<Customer>> SearchAsync(string searchTerm, bool exactMatch = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new customer to the database with validation for required fields.
        /// </summary>
        /// <param name="customer">The customer entity to add. Must include required fields (Name, Code).</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The newly created customer with generated ID and timestamps.</returns>
        /// <exception cref="ArgumentNullException">Thrown when customer is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when customer validation fails.</exception>
        Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing customer and their related data in the database.
        /// </summary>
        /// <param name="customer">The customer entity with updated information.</param>
        /// <param name="updateRelatedData">When true, updates associated contacts and contracts.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The updated customer with refreshed related data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when customer is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when customer validation fails.</exception>
        Task<Customer> UpdateAsync(Customer customer, bool updateRelatedData = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft deletes a customer and optionally their related data from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the customer to delete.</param>
        /// <param name="deleteRelatedData">When true, also deletes associated contacts and contracts.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>True if customer was deleted, false if not found.</returns>
        Task<bool> DeleteAsync(int id, bool deleteRelatedData = false, CancellationToken cancellationToken = default);
    }
}
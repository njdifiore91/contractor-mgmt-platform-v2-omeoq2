// Microsoft.EntityFrameworkCore v6.0.0
using Microsoft.EntityFrameworkCore;
// Microsoft.Extensions.Logging v6.0.0
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Core.Entities;
using Backend.Core.Interfaces.Repositories;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Repositories
{
    /// <summary>
    /// Implements the ICustomerRepository interface providing optimized data access operations
    /// for customer entities with comprehensive validation and error handling.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomerRepository> _logger;

        public CustomerRepository(ApplicationDbContext context, ILogger<CustomerRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Customer> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving customer with ID: {CustomerId}", id);

                return await _context.Customers
                    .AsNoTracking()
                    .Include(c => c.Contacts.Where(contact => !contact.IsDeleted))
                    .Include(c => c.Contracts.Where(contract => !contract.IsDeleted))
                    .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer with ID: {CustomerId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving all customers. Including inactive: {IncludeInactive}", includeInactive);

                var query = _context.Customers
                    .AsNoTracking()
                    .Include(c => c.Contacts.Where(contact => !contact.IsDeleted))
                    .Include(c => c.Contracts.Where(contract => !contract.IsDeleted))
                    .Where(c => !c.IsDeleted);

                if (!includeInactive)
                {
                    query = query.Where(c => c.IsActive);
                }

                return await query.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all customers");
                throw;
            }
        }

        public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm, bool exactMatch = false, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Searching customers with term: {SearchTerm}, ExactMatch: {ExactMatch}", searchTerm, exactMatch);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Array.Empty<Customer>();
                }

                var query = _context.Customers
                    .AsNoTracking()
                    .Include(c => c.Contacts.Where(contact => !contact.IsDeleted))
                    .Include(c => c.Contracts.Where(contract => !contract.IsDeleted))
                    .Where(c => !c.IsDeleted && c.IsActive);

                if (exactMatch)
                {
                    query = query.Where(c => 
                        c.Name.Equals(searchTerm) || 
                        c.Code.Equals(searchTerm));
                }
                else
                {
                    var searchTermLower = searchTerm.ToLower();
                    query = query.Where(c => 
                        c.Name.ToLower().Contains(searchTermLower) || 
                        c.Code.ToLower().Contains(searchTermLower));
                }

                return await query
                    .OrderBy(c => c.Name)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with term: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            try
            {
                if (customer == null)
                {
                    throw new ArgumentNullException(nameof(customer));
                }

                _logger.LogInformation("Adding new customer: {CustomerName}", customer.Name);

                // Validate unique code
                if (await _context.Customers.AnyAsync(c => c.Code == customer.Code && !c.IsDeleted, cancellationToken))
                {
                    throw new InvalidOperationException($"Customer with code {customer.Code} already exists");
                }

                customer.ConcurrencyStamp = Guid.NewGuid().ToString();
                await _context.Customers.AddAsync(customer, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding customer: {CustomerName}", customer?.Name);
                throw;
            }
        }

        public async Task<Customer> UpdateAsync(Customer customer, bool updateRelatedData = true, CancellationToken cancellationToken = default)
        {
            try
            {
                if (customer == null)
                {
                    throw new ArgumentNullException(nameof(customer));
                }

                _logger.LogInformation("Updating customer: {CustomerId}", customer.Id);

                var existingCustomer = await _context.Customers
                    .Include(c => c.Contacts)
                    .Include(c => c.Contracts)
                    .FirstOrDefaultAsync(c => c.Id == customer.Id && !c.IsDeleted, cancellationToken);

                if (existingCustomer == null)
                {
                    throw new InvalidOperationException($"Customer with ID {customer.Id} not found");
                }

                // Validate concurrency
                if (existingCustomer.ConcurrencyStamp != customer.ConcurrencyStamp)
                {
                    throw new DbUpdateConcurrencyException("Customer has been modified by another user");
                }

                // Validate unique code if changed
                if (existingCustomer.Code != customer.Code)
                {
                    if (await _context.Customers.AnyAsync(c => c.Code == customer.Code && c.Id != customer.Id && !c.IsDeleted, cancellationToken))
                    {
                        throw new InvalidOperationException($"Customer with code {customer.Code} already exists");
                    }
                }

                // Update main properties
                existingCustomer.Name = customer.Name;
                existingCustomer.Code = customer.Code;
                existingCustomer.IsActive = customer.IsActive;
                existingCustomer.ConcurrencyStamp = Guid.NewGuid().ToString();

                if (updateRelatedData)
                {
                    // Update contacts
                    _context.Entry(existingCustomer)
                        .Collection(c => c.Contacts)
                        .Load();

                    // Update contracts
                    _context.Entry(existingCustomer)
                        .Collection(c => c.Contracts)
                        .Load();
                }

                await _context.SaveChangesAsync(cancellationToken);

                return existingCustomer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer: {CustomerId}", customer?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, bool deleteRelatedData = false, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting customer: {CustomerId}", id);

                var customer = await _context.Customers
                    .Include(c => c.Contacts)
                    .Include(c => c.Contracts)
                    .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

                if (customer == null)
                {
                    return false;
                }

                if (deleteRelatedData)
                {
                    foreach (var contact in customer.Contacts)
                    {
                        contact.IsDeleted = true;
                    }

                    foreach (var contract in customer.Contracts)
                    {
                        contract.IsDeleted = true;
                    }
                }

                customer.IsDeleted = true;
                await _context.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer: {CustomerId}", id);
                throw;
            }
        }
    }
}
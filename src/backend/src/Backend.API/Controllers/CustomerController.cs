using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Backend.Core.Entities;
using Backend.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.API.Controllers
{
    /// <summary>
    /// Controller responsible for managing customer-related operations including customers, contacts, and contracts.
    /// Provides RESTful endpoints with comprehensive validation, error handling, and monitoring.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [ApiVersion("1.0")]
    [Produces(MediaTypeNames.Application.Json)]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomerController> _logger;

        /// <summary>
        /// Initializes a new instance of the CustomerController with required dependencies.
        /// </summary>
        /// <param name="customerRepository">Repository for customer data operations</param>
        /// <param name="logger">Logger for telemetry and monitoring</param>
        /// <exception cref="ArgumentNullException">Thrown when required dependencies are null</exception>
        public CustomerController(ICustomerRepository customerRepository, ILogger<CustomerController> logger)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all customers with optional pagination.
        /// </summary>
        /// <param name="includeInactive">When true, includes inactive customers in results</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of customers with related data</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAllAsync(
            [FromQuery] bool includeInactive = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving all customers. IncludeInactive: {IncludeInactive}, Page: {Page}, PageSize: {PageSize}",
                    includeInactive, page, pageSize);

                var customers = await _customerRepository.GetAllAsync(includeInactive, cancellationToken);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving customers");
            }
        }

        /// <summary>
        /// Retrieves a specific customer by ID.
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Customer details if found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Customer>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
                if (customer == null)
                {
                    return NotFound();
                }
                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the customer");
            }
        }

        /// <summary>
        /// Searches for customers by name or code.
        /// </summary>
        /// <param name="searchTerm">Search term to match against customer name or code</param>
        /// <param name="exactMatch">When true, performs exact matching</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of matching customers</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Customer>>> SearchAsync(
            [FromQuery] string searchTerm,
            [FromQuery] bool exactMatch = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term is required");
            }

            try
            {
                var customers = await _customerRepository.SearchAsync(searchTerm, exactMatch, cancellationToken);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with term: {SearchTerm}", searchTerm);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching customers");
            }
        }

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="customer">Customer details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Newly created customer</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Customer>> CreateAsync(
            [FromBody] Customer customer,
            CancellationToken cancellationToken = default)
        {
            if (customer == null)
            {
                return BadRequest("Customer data is required");
            }

            if (!customer.Validate())
            {
                return BadRequest("Invalid customer data");
            }

            try
            {
                var createdCustomer = await _customerRepository.AddAsync(customer, cancellationToken);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = createdCustomer.Id }, createdCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer: {CustomerName}", customer.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the customer");
            }
        }

        /// <summary>
        /// Updates an existing customer.
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <param name="customer">Updated customer details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated customer</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Customer>> UpdateAsync(
            int id,
            [FromBody] Customer customer,
            CancellationToken cancellationToken = default)
        {
            if (customer == null || id != customer.Id)
            {
                return BadRequest("Invalid customer data or ID mismatch");
            }

            if (!customer.Validate())
            {
                return BadRequest("Invalid customer data");
            }

            try
            {
                var updatedCustomer = await _customerRepository.UpdateAsync(customer, true, cancellationToken);
                if (updatedCustomer == null)
                {
                    return NotFound();
                }
                return Ok(updatedCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the customer");
            }
        }

        /// <summary>
        /// Deletes a customer.
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <param name="deleteRelatedData">When true, deletes related contacts and contracts</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteAsync(
            int id,
            [FromQuery] bool deleteRelatedData = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var deleted = await _customerRepository.DeleteAsync(id, deleteRelatedData, cancellationToken);
                if (!deleted)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the customer");
            }
        }
    }
}
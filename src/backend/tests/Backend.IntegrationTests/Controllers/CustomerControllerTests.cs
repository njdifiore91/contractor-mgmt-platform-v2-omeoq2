using Backend.Core.DTOs.Customer;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Backend.IntegrationTests.Controllers
{
    /// <summary>
    /// Integration tests for CustomerController endpoints verifying complete request-response cycle
    /// including database operations, authorization, validation, and error handling.
    /// </summary>
    [Collection("Database")]
    public class CustomerControllerTests : IAsyncLifetime
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;
        private readonly HttpClient _authenticatedClient;
        private readonly ITestOutputHelper _output;
        private readonly JsonSerializerOptions _jsonOptions;

        public CustomerControllerTests(
            CustomWebApplicationFactory<Startup> factory,
            ITestOutputHelper output)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _output = output ?? throw new ArgumentNullException(nameof(output));

            // Initialize HTTP clients
            _client = _factory.CreateClient();
            _authenticatedClient = _factory.CreateAuthenticatedClient();

            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            _client.Dispose();
            _authenticatedClient.Dispose();
            await _factory.CleanupTestDatabase();
        }

        [Fact]
        public async Task GetAllCustomers_ReturnsSuccessAndCustomersList()
        {
            // Arrange
            var requestUri = "/api/customers?pageSize=10&pageNumber=1&sortBy=name&sortDirection=asc";

            // Act
            var response = await _authenticatedClient.GetAsync(requestUri);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Headers.Should().ContainKey("X-Pagination");

            var customers = await response.Content.ReadFromJsonAsync<List<CustomerDto>>(_jsonOptions);
            customers.Should().NotBeNull();
            customers.Should().AllBeOfType<CustomerDto>();
            
            // Verify pagination headers
            var paginationHeader = response.Headers.GetValues("X-Pagination");
            paginationHeader.Should().NotBeNull();

            // Verify customer data completeness
            foreach (var customer in customers)
            {
                customer.Id.Should().BeGreaterThan(0);
                customer.Name.Should().NotBeNullOrWhiteSpace();
                customer.Code.Should().NotBeNullOrWhiteSpace();
                customer.CreatedAt.Should().BeBefore(DateTime.UtcNow);
                customer.CreatedBy.Should().NotBeNullOrWhiteSpace();
            }
        }

        [Fact]
        public async Task GetCustomerById_WithValidId_ReturnsCustomer()
        {
            // Arrange
            var customerId = 1; // Using test customer from factory seed data
            var requestUri = $"/api/customers/{customerId}?includeContacts=true&includeContracts=true";

            // Act
            var response = await _authenticatedClient.GetAsync(requestUri);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
            
            customer.Should().NotBeNull();
            customer.Id.Should().Be(customerId);
            customer.Name.Should().NotBeNullOrWhiteSpace();
            customer.Code.Should().NotBeNullOrWhiteSpace();
            customer.Contacts.Should().NotBeNull();
            customer.Contracts.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateCustomer_WithValidData_ReturnsCreatedCustomer()
        {
            // Arrange
            var newCustomer = new CustomerDto
            {
                Name = "Test Customer",
                Code = "TEST001",
                IsActive = true,
                CreatedBy = "IntegrationTest",
                Contacts = new List<ContactDto>
                {
                    new ContactDto
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        JobTitle = "Manager",
                        Emails = new List<EmailDto>
                        {
                            new EmailDto
                            {
                                IsPrimary = true,
                                EmailAddress = "john.doe@test.com"
                            }
                        }
                    }
                }
            };

            // Act
            var response = await _authenticatedClient.PostAsJsonAsync("/api/customers", newCustomer);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            var createdCustomer = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
            createdCustomer.Should().NotBeNull();
            createdCustomer.Id.Should().BeGreaterThan(0);
            createdCustomer.Name.Should().Be(newCustomer.Name);
            createdCustomer.Code.Should().Be(newCustomer.Code);
            createdCustomer.Contacts.Should().HaveCount(1);
        }

        [Fact]
        public async Task UpdateCustomer_WithValidData_ReturnsUpdatedCustomer()
        {
            // Arrange
            var customerId = 1;
            var updateData = new CustomerDto
            {
                Id = customerId,
                Name = "Updated Customer Name",
                Code = "UPD001",
                IsActive = true
            };

            // Act
            var response = await _authenticatedClient.PutAsJsonAsync($"/api/customers/{customerId}", updateData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedCustomer = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
            
            updatedCustomer.Should().NotBeNull();
            updatedCustomer.Id.Should().Be(customerId);
            updatedCustomer.Name.Should().Be(updateData.Name);
            updatedCustomer.Code.Should().Be(updateData.Code);
        }

        [Fact]
        public async Task SearchCustomers_WithValidCriteria_ReturnsMatchingCustomers()
        {
            // Arrange
            var searchTerm = "Test";
            var requestUri = $"/api/customers/search?term={searchTerm}&pageSize=10&pageNumber=1";

            // Act
            var response = await _authenticatedClient.GetAsync(requestUri);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var searchResults = await response.Content.ReadFromJsonAsync<List<CustomerDto>>(_jsonOptions);
            
            searchResults.Should().NotBeNull();
            searchResults.Should().AllSatisfy(customer =>
            {
                (customer.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                 customer.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .Should().BeTrue();
            });
        }

        [Fact]
        public async Task DeleteCustomer_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var customerId = 2; // Using test customer from factory seed data

            // Act
            var response = await _authenticatedClient.DeleteAsync($"/api/customers/{customerId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify customer is deleted
            var getResponse = await _authenticatedClient.GetAsync($"/api/customers/{customerId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetCustomerById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = 99999;

            // Act
            var response = await _authenticatedClient.GetAsync($"/api/customers/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateCustomer_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidCustomer = new CustomerDto
            {
                // Missing required Name and Code
                IsActive = true
            };

            // Act
            var response = await _authenticatedClient.PostAsJsonAsync("/api/customers", invalidCustomer);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationErrors = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>(_jsonOptions);
            validationErrors.Should().ContainKey("Name");
            validationErrors.Should().ContainKey("Code");
        }

        [Fact]
        public async Task GetAllCustomers_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/customers");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
using System.Threading.Tasks;
using System.Data;
using Backend.Core.Interfaces.Repositories;

namespace Backend.Core.Interfaces
{
    /// <summary>
    /// Defines a Unit of Work interface that coordinates operations across multiple repositories
    /// and ensures transactional consistency. Provides a unified interface for managing database
    /// operations with enhanced security and monitoring capabilities.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Repository for managing customer-related operations
        /// </summary>
        ICustomerRepository Customers { get; }

        /// <summary>
        /// Repository for managing equipment-related operations
        /// </summary>
        IEquipmentRepository Equipment { get; }

        /// <summary>
        /// Repository for managing inspector-related operations
        /// </summary>
        IInspectorRepository Inspectors { get; }

        /// <summary>
        /// Repository for managing user-related operations
        /// </summary>
        IUserRepository Users { get; }

        /// <summary>
        /// Saves all changes made in this unit of work to the database with enhanced validation
        /// and error handling. Ensures atomic operations across multiple repositories.
        /// </summary>
        /// <returns>The number of state entries written to the database</returns>
        /// <exception cref="InvalidOperationException">Thrown when validation fails or transaction is in invalid state</exception>
        /// <exception cref="DbUpdateException">Thrown when database update fails</exception>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Begins a new database transaction with configurable isolation level.
        /// Initializes transaction monitoring and sets up retry policies.
        /// </summary>
        /// <param name="isolationLevel">The isolation level for the transaction</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="InvalidOperationException">Thrown when a transaction is already in progress</exception>
        Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// Commits the current transaction with enhanced validation.
        /// Performs pre-commit checks and records transaction metrics.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="InvalidOperationException">Thrown when no transaction is in progress or validation fails</exception>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rolls back the current transaction with comprehensive cleanup.
        /// Records rollback reason and updates monitoring metrics.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="InvalidOperationException">Thrown when no transaction is in progress</exception>
        Task RollbackTransactionAsync();
    }
}
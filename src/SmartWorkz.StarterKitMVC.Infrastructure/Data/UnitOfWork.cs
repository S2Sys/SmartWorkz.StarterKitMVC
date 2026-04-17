using System.Data;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Data;

/// <summary>
/// Unit of Work pattern implementation for coordinating Dapper repositories.
/// Provides a single point for managing all repository interactions and transactions.
/// </summary>
public class UnitOfWork : IDisposable
{
    private readonly IDbConnection _connection;
    private readonly ILoggerFactory _loggerFactory;
    private IDbTransaction? _transaction;

    // Lazy-loaded repositories
    private ILookupRepository? _lookupRepository;
    private IConfigurationRepository? _configurationRepository;
    private IRoleRepository? _roleRepository;
    private IPermissionRepository? _permissionRepository;
    private IBlogPostRepository? _blogPostRepository;
    private INotificationRepository? _notificationRepository;
    private IAuditLogRepository? _auditLogRepository;
    private ICountryRepository? _countryRepository;
    private ITenantRepository? _tenantRepository;
    private ICustomPageRepository? _customPageRepository;
    private IUserRepository? _userRepository;
    private IProductRepository? _productRepository;
    private ICategoryRepository? _categoryRepository;
    private IEmailQueueRepository? _emailQueueRepository;

    public UnitOfWork(IDbConnection connection, ILoggerFactory loggerFactory)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    #region Repository Properties

    /// <summary>Repository for lookup values (hierarchical lists)</summary>
    public ILookupRepository Lookups =>
        _lookupRepository ??= new LookupRepository(_connection, _loggerFactory.CreateLogger<LookupRepository>());

    /// <summary>Repository for configuration settings</summary>
    public IConfigurationRepository Configurations =>
        _configurationRepository ??= new ConfigurationRepository(_connection, _loggerFactory.CreateLogger<ConfigurationRepository>());

    /// <summary>Repository for roles</summary>
    public IRoleRepository Roles =>
        _roleRepository ??= new RoleRepository(_connection, _loggerFactory.CreateLogger<RoleRepository>());

    /// <summary>Repository for permissions</summary>
    public IPermissionRepository Permissions =>
        _permissionRepository ??= new PermissionRepository(_connection, _loggerFactory.CreateLogger<PermissionRepository>());

    /// <summary>Repository for blog posts</summary>
    public IBlogPostRepository BlogPosts =>
        _blogPostRepository ??= new BlogPostRepository(_connection, _loggerFactory.CreateLogger<BlogPostRepository>());

    /// <summary>Repository for notifications</summary>
    public INotificationRepository Notifications =>
        _notificationRepository ??= new NotificationRepository(_connection, _loggerFactory.CreateLogger<NotificationRepository>());

    /// <summary>Repository for audit logs</summary>
    public IAuditLogRepository AuditLogs =>
        _auditLogRepository ??= new AuditLogRepository(_connection, _loggerFactory.CreateLogger<AuditLogRepository>());

    /// <summary>Repository for countries</summary>
    public ICountryRepository Countries =>
        _countryRepository ??= new CountryRepository(_connection, _loggerFactory.CreateLogger<CountryRepository>());

    /// <summary>Repository for tenants (must be injected from DI)</summary>
    public ITenantRepository? Tenants => _tenantRepository;
    public void SetTenantRepository(ITenantRepository repository) => _tenantRepository = repository;

    /// <summary>Repository for custom pages</summary>
    public ICustomPageRepository CustomPages =>
        _customPageRepository ??= new CustomPageRepository(_connection, _loggerFactory.CreateLogger<CustomPageRepository>());

    /// <summary>Repository for users (must be injected from DI)</summary>
    public IUserRepository? Users => _userRepository;
    public void SetUserRepository(IUserRepository repository) => _userRepository = repository;

    /// <summary>Repository for products (must be injected from DI)</summary>
    public IProductRepository? Products => _productRepository;
    public void SetProductRepository(IProductRepository repository) => _productRepository = repository;

    /// <summary>Repository for categories (must be injected from DI)</summary>
    public ICategoryRepository? Categories => _categoryRepository;
    public void SetCategoryRepository(ICategoryRepository repository) => _categoryRepository = repository;

    /// <summary>Repository for email queue (must be injected from DI)</summary>
    public IEmailQueueRepository? EmailQueue => _emailQueueRepository;
    public void SetEmailQueueRepository(IEmailQueueRepository repository) => _emailQueueRepository = repository;

    #endregion

    #region Transaction Management

    /// <summary>Begin a database transaction</summary>
    public void BeginTransaction()
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        _transaction = _connection.BeginTransaction();
    }

    /// <summary>Commit the current transaction</summary>
    public void CommitTransaction()
    {
        try
        {
            _transaction?.Commit();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    /// <summary>Rollback the current transaction</summary>
    public void RollbackTransaction()
    {
        try
        {
            _transaction?.Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    #endregion

    #region IDisposable

    /// <summary>Dispose resources</summary>
    public void Dispose()
    {
        try
        {
            _transaction?.Dispose();
        }
        finally
        {
            if (_connection?.State == ConnectionState.Open)
                _connection?.Close();
            _connection?.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    #endregion
}

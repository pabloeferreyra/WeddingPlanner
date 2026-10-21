using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<GenericRepository<T>> _logger;

    public GenericRepository(ApplicationDbContext context, ILogger<GenericRepository<T>> logger)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _logger = logger;
    }

    protected virtual string EntityName => typeof(T).Name;

    public virtual async Task<T> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting {Entity} with id {Id}", EntityName, id);

        var entity = await _dbSet.FindAsync(id);

        if (entity is null)
        {
            _logger.LogWarning("{Entity} with id {Id} was not found", EntityName, id);
        }
        return entity;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        _logger.LogDebug("Getting all {Entity} records", EntityName);

        var entities = await _dbSet.ToListAsync();

        _logger.LogDebug("Retrieved {Count} {Entity} records", entities.Count, EntityName);

        return entities;
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        try
        {
            _logger.LogInformation("Adding new {Entity}", EntityName);

            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("{Entity} {Id} was persisted successfully", EntityName, GetId(entity));

            return entity;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to add {Entity}", EntityName);
            throw;
        }
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        try
        {
            _logger.LogInformation("Updating {Entity} {Id}", EntityName, GetId(entity));

            _dbSet.Update(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("{Entity} {Id} was updated successfully", EntityName, GetId(entity));

            return entity;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update {Entity} {Id}", EntityName, GetId(entity));
            throw;
        }
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting {Entity} with id {Id}", EntityName, id);

            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("{Entity} {Id} was deleted successfully", EntityName, id);
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to delete {Entity} with id {Id}", EntityName, id);
            throw;
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        var exists = await _dbSet.FindAsync(id) != null;

        _logger.LogDebug("{Entity} {Id} exists: {Exists}", EntityName, id, exists);

        return exists;
    }

    public virtual IQueryable<T> AsQueryable() => _dbSet.AsQueryable();

    private static object GetId(T entity)
    {
        const string idPropertyName = "Id";
        var idProperty = typeof(T).GetProperty(idPropertyName);

        return idProperty?.GetValue(entity) ?? "unknown";
    }
}
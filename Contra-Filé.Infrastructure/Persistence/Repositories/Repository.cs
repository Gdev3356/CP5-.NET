using Contra_Filé.Application.Repositories;
using Contra_Filé.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Contra_Filé.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core genérica de <see cref="IRepository{T}"/>.
/// Cobre o CRUD mínimo para qualquer agregado derivado de <see cref="BaseEntity"/>.
/// </summary>
public class Repository<T>(ContraFiléContext context) : IRepository<T>
    where T : BaseEntity
{
    protected ContraFiléContext Context { get; } = context;
    private readonly DbSet<T> _set = context.Set<T>();

    /// <inheritdoc/>
    public IReadOnlyList<T> GetAll() =>
        _set.AsNoTracking().OrderBy(e => e.CreatedAt).ToList();

    /// <inheritdoc/>
    public T? GetById(Guid id) => _set.Find(id);

    /// <inheritdoc/>
    public T Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _set.Add(entity);
        Context.SaveChanges();
        return entity;
    }

    /// <inheritdoc/>
    public bool Delete(Guid id)
    {
        var entity = GetById(id);
        if (entity is null) return false;
        _set.Remove(entity);
        Context.SaveChanges();
        return true;
    }

    /// <inheritdoc/>
    public bool ExistsById(Guid id) => _set.Any(e => e.Id == id);
}
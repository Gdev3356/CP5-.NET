using Contra_Filé.Domain.Common;

namespace Contra_Filé.Application.Repositories;

/// <summary>
/// Contrato genérico de persistência para qualquer entidade derivada de <see cref="BaseEntity"/>.
/// </summary>
/// <typeparam name="T">Tipo da entidade.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>Retorna todas as entidades.</summary>
    IReadOnlyList<T> GetAll();

    /// <summary>
    /// CP5: retorna uma página de entidades cortada no banco (Skip/Take no IQueryable, ordenado
    /// por CreatedAt) junto com o total de itens existentes (sem paginação).
    /// </summary>
    (IReadOnlyList<T> Items, int Total) GetPaged(int pageNumber, int pageSize);

    /// <summary>Busca pelo identificador. Retorna null se não encontrado.</summary>
    T? GetById(Guid id);

    /// <summary>Persiste uma nova entidade e retorna ela após salvar.</summary>
    T Add(T entity);

    /// <summary>Remove pelo id. Retorna false se não existir.</summary>
    bool Delete(Guid id);

    /// <summary>Verifica existência pelo id sem carregar a entidade.</summary>
    bool ExistsById(Guid id);
}
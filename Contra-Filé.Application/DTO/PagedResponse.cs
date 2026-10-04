namespace Contra_Filé.Application.DTO;

/// <summary>
/// CP5: envelope padrão de listagens paginadas (contrato v2).
/// </summary>
public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    /// <summary>Total de páginas com o pageSize atual.</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    /// <summary>Indica se existe página anterior.</summary>
    public bool HasPrevious => Page > 1;

    /// <summary>Indica se existe próxima página.</summary>
    public bool HasNext => Page < TotalPages;
}
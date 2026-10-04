namespace Contra_Filé.Application.DTO;

/// <summary>
/// CP5: parâmetros de paginação por offset (page / pageSize) para a listagem v2.
/// </summary>
public class PaginationQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>Número da página (1-based).</summary>
    public int Page { get; set; } = 1;

    /// <summary>Quantidade de itens por página (máximo 100).</summary>
    public int PageSize { get; set; } = DefaultPageSize;

    /// <summary>Valida os limites de page e pageSize.</summary>
    public bool TryGetError(out string? message)
    {
        if (Page < 1)
        {
            message = "page deve ser >= 1.";
            return true;
        }

        if (PageSize is < 1 or > 100)
        {
            message = $"pageSize deve estar entre 1 e {MaxPageSize}.";
            return true;
        }

        message = null;
        return false;
    }
}
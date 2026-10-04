using Contra_Filé.Domain.Entities;

namespace Contra_Filé.Application.DTO;

/// <summary>Resposta de prato do cardápio.</summary>
public record PratoResponse(Guid Id, string Name, decimal Price, int TimePrepareMinutes, string Description)
{
    /// <summary>Mapeia <see cref="Prato"/> para DTO.</summary>
    public static PratoResponse FromDomain(Prato p) =>
        new(p.Id, p.Name, p.Price, p.TimePrepareMinutes, p.Description);
}
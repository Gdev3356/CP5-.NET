using Contra_Filé.Domain.Entities;

namespace Contra_Filé.Application.DTO;

/// <summary>Resposta de mesa sem dados internos sensíveis.</summary>
public record MesaResponse(Guid Id, int Number, bool IsOccupied)
{
    /// <summary>Mapeia <see cref="Mesa"/> para DTO.</summary>
    public static MesaResponse FromDomain(Mesa m) => new(m.Id, m.Number, m.IsOccupied);
}
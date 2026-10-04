using Contra_Filé.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Contra_Filé.Application.DTO;

/// <summary>Dados para criação de uma mesa.</summary>
public record MesaRequest(
    [Range(1, 999, ErrorMessage = "O número da mesa deve ser positivo.")]
    int Number,
    bool IsOccupied)
{
    /// <summary>Constrói a entidade <see cref="Mesa"/>.</summary>
    public Mesa ToDomain() => new Mesa(Number, IsOccupied);
}
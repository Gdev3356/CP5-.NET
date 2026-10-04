using Contra_Filé.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Contra_Filé.Application.DTO;

/// <summary>Dados para abertura de um pedido.</summary>
public record PedidoRequest(
    [Required] Guid MesaId,
    [Required] Guid UserId,
    List<Guid>? PratoIds)
{
    /// <summary>Constrói a entidade <see cref="Pedido"/>.</summary>
    public Pedido ToDomain() => new Pedido(0m, MesaId, UserId);
}
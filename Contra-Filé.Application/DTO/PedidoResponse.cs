using Contra_Filé.Domain.Entities;

namespace Contra_Filé.Application.DTO;

public record PedidoResponse(Guid Id, decimal TotalValue, Guid MesaId, Guid UserId)
{
    public static PedidoResponse FromDomain(Pedido p) => 
        new(p.Id, p.TotalValue, p.MesaId, p.UserId);
}
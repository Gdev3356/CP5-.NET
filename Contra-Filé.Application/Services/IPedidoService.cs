using Contra_Filé.Application.DTO;
using Contra_Filé.Domain.Entities;

namespace Contra_Filé.Application.Services;

public interface IPedidoService
{
    /// <summary>Lista todos os pedidos (contrato antigo, sem paginação).</summary>
    IReadOnlyList<PedidoResponse> GetAll();

    /// <summary>[v2]: listagem paginada, cortada no banco.</summary>
    (IReadOnlyList<Pedido> Items, int Total) GetPaged(int page, int pageSize);

    PedidoResponse? GetById(Guid id);
    PedidoResponse Create(PedidoRequest request);
    bool Delete(Guid id);
}
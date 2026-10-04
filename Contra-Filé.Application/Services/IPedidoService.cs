using Contra_Filé.Application.DTO;

namespace Contra_Filé.Application.Services;

public interface IPedidoService
{
    IReadOnlyList<PedidoResponse> GetAll();
    PedidoResponse? GetById(Guid id);
    PedidoResponse Create(PedidoRequest request);
    bool Delete(Guid id);
}
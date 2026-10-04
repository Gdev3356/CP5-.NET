using Contra_Filé.Application.DTO;
using Contra_Filé.Application.Repositories;
using Contra_Filé.Domain.Entities;

namespace Contra_Filé.Application.Services;

/// <summary>Orquestra os casos de uso de pedidos.</summary>
/// <remarks>
/// Valida se Mesa e Usuário referenciados existem antes de criar o pedido (KeyNotFoundException,
/// já mapeada pelo GlobalExceptionHandler para 404). GetPaged delega direto ao repositório, a
/// v1 e a v2 do controller chamam o MESMO serviço, só a action HTTP muda o contrato de saída.
/// </remarks>
public class PedidoService : IPedidoService
{
    private readonly IRepository<Pedido> _repo;
    private readonly IRepository<Mesa> _mesaRepo;
    private readonly IRepository<User> _userRepo;

    public PedidoService(IRepository<Pedido> repo, IRepository<Mesa> mesaRepo, IRepository<User> userRepo)
    {
        _repo = repo;
        _mesaRepo = mesaRepo;
        _userRepo = userRepo;
    }

    /// <inheritdoc/>
    public IReadOnlyList<PedidoResponse> GetAll() =>
        _repo.GetAll().Select(PedidoResponse.FromDomain).ToList();

    /// <inheritdoc/>
    public (IReadOnlyList<Pedido> Items, int Total) GetPaged(int page, int pageSize) =>
        _repo.GetPaged(page, pageSize);

    /// <inheritdoc/>
    public PedidoResponse? GetById(Guid id)
    {
        var pedido = _repo.GetById(id);
        return pedido is null ? null : PedidoResponse.FromDomain(pedido);
    }

    /// <inheritdoc/>
    public PedidoResponse Create(PedidoRequest request)
    {
        if (!_mesaRepo.ExistsById(request.MesaId))
            throw new KeyNotFoundException($"Mesa '{request.MesaId}' não encontrada.");

        if (!_userRepo.ExistsById(request.UserId))
            throw new KeyNotFoundException($"Usuário '{request.UserId}' não encontrado.");

        var pedido = _repo.Add(request.ToDomain());
        return PedidoResponse.FromDomain(pedido);
    }

    /// <inheritdoc/>
    public bool Delete(Guid id) => _repo.Delete(id);
}
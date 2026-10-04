using Contra_Filé.Application.DTO;
using Contra_Filé.Application.Repositories;
using Contra_Filé.Domain.Entities;

namespace Contra_Filé.Application.Services;

/// <summary>Orquestra os casos de uso de pedidos.</summary>
/// <remarks>
/// Agora valida se Mesa e Usuário referenciados existem antes de criar o pedido, necessário
/// para o cenário de teste "dependência ausente" pedido no CP4. Lança <see cref="KeyNotFoundException"/>
/// (já mapeada pelo GlobalExceptionHandler do CP3 para 404) quando a mesa ou o usuário não existem.
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
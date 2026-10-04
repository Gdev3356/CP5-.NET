using Contra_Filé.Application.DTO;
using Contra_Filé.Application.Repositories;
using Contra_Filé.Domain.Entities;

namespace Contra_Filé.Application.Services;

/// <summary>Orquestra os casos de uso de mesas.</summary>
public class MesaService : IMesaService
{
    private readonly IRepository<Mesa> _repo;
    public MesaService(IRepository<Mesa> repo) => _repo = repo;

    /// <inheritdoc/>
    public IReadOnlyList<MesaResponse> GetAll() =>
        _repo.GetAll().Select(MesaResponse.FromDomain).ToList();

    /// <inheritdoc/>
    public MesaResponse? GetById(Guid id)
    {
        var mesa = _repo.GetById(id);
        return mesa is null ? null : MesaResponse.FromDomain(mesa);
    }

    /// <inheritdoc/>
    public MesaResponse Create(MesaRequest request)
    {
        var mesa = _repo.Add(request.ToDomain());
        return MesaResponse.FromDomain(mesa);
    }

    /// <inheritdoc/>
    public bool Delete(Guid id) => _repo.Delete(id);
}
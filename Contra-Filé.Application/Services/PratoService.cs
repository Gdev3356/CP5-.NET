using Contra_Filé.Application.DTO;
using Contra_Filé.Application.Repositories;
using Contra_Filé.Domain.Entities;

namespace Contra_Filé.Application.Services;

/// <summary>Orquestra os casos de uso de pratos do cardápio.</summary>
public class PratoService : IPratoService
{
    private readonly IRepository<Prato> _repo;
    public PratoService(IRepository<Prato> repo) => _repo = repo;

    /// <inheritdoc/>
    public IReadOnlyList<PratoResponse> GetAll() =>
        _repo.GetAll().Select(PratoResponse.FromDomain).ToList();

    /// <inheritdoc/>
    public PratoResponse? GetById(Guid id)
    {
        var prato = _repo.GetById(id);
        return prato is null ? null : PratoResponse.FromDomain(prato);
    }

    /// <inheritdoc/>
    public PratoResponse Create(PratoRequest request)
    {
        var prato = _repo.Add(request.ToDomain());
        return PratoResponse.FromDomain(prato);
    }

    /// <inheritdoc/>
    public bool Delete(Guid id) => _repo.Delete(id);
}
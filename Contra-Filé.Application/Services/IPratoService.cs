using Contra_Filé.Application.DTO;

namespace Contra_Filé.Application.Services;

/// <summary>Casos de uso de pratos do cardápio.</summary>
public interface IPratoService
{
    IReadOnlyList<PratoResponse> GetAll();
    PratoResponse? GetById(Guid id);
    PratoResponse Create(PratoRequest request);
    bool Delete(Guid id);
}
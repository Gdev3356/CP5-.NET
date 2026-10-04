using Contra_Filé.Application.DTO;

namespace Contra_Filé.Application.Services;

/// <summary>Casos de uso de mesas.</summary>
public interface IMesaService
{
    IReadOnlyList<MesaResponse> GetAll();
    MesaResponse? GetById(Guid id);
    MesaResponse Create(MesaRequest request);
    bool Delete(Guid id);
}
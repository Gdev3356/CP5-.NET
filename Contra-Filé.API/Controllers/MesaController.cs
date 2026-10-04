using Contra_Filé.Application.DTO;
using Contra_Filé.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Contra_Filé.Controllers;

/// <summary>
/// Gerenciamento de mesas do restaurante.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class MesaController(IMesaService mesaService) : ControllerBase
{
    /// <summary>Lista todas as mesas.</summary>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MesaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll() => Ok(mesaService.GetAll());

    /// <summary>Obtém uma mesa pelo id.</summary>
    /// <param name="id">Identificador único da mesa.</param>
    /// <response code="200">Mesa encontrada.</response>
    /// <response code="404">Mesa não encontrada.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MesaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var mesa = mesaService.GetById(id);
        return mesa is null ? NotFound() : Ok(mesa);
    }

    /// <summary>Cadastra uma nova mesa.</summary>
    /// <param name="request">Número e status de ocupação da mesa.</param>
    /// <response code="201">Mesa criada.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(MesaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] MesaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var mesa = mesaService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = mesa.Id }, mesa);
    }

    /// <summary>Remove uma mesa pelo id.</summary>
    /// <param name="id">Identificador único da mesa.</param>
    /// <response code="204">Removida com sucesso.</response>
    /// <response code="404">Mesa não encontrada.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        mesaService.Delete(id) ? NoContent() : NotFound();
}